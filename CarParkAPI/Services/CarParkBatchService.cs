using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using CarParkAPI.Data;
using CarParkAPI.Models;

namespace CarParkAPI.Services;

public class CarParkBatchService
{
    private readonly DataContext _context;

    public CarParkBatchService(DataContext context)
    {
        _context = context;
    }

    public async Task ProcessDeltaFileAsync(string csvContent)
    {
        // Enforce basic expectation requirement: File operation must execute as an atomic transaction block
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            using var reader = new StringReader(csvContent);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            });

            // Read the records dynamically to parse lookups cleanly
            var records = csv.GetRecords<dynamic>().ToList();

            // Pre-fetch existing reference tables to optimize parsing performance
            var currentTypes = await _context.CarParkTypes.ToDictionaryAsync(t => t.Name.ToUpper(), t => t.Id);
            var currentSystems = await _context.ParkingSystems.ToDictionaryAsync(s => s.Name.ToUpper(), s => s.Id);

            foreach (var record in records)
            {
                var dict = (IDictionary<string, object>)record;

                string carParkNo = dict["car_park_no"].ToString()!;
                string typeName = dict["car_park_type"].ToString()!;
                string systemName = dict["type_of_parking_system"].ToString()!;

                // Dynamically build missing lookup data structures inline
                if (!currentTypes.TryGetValue(typeName.ToUpper(), out int typeId))
                {
                    var newType = new CarParkType { Name = typeName };
                    _context.CarParkTypes.Add(newType);
                    await _context.SaveChangesAsync();
                    typeId = newType.Id;
                    currentTypes[typeName.ToUpper()] = typeId;
                }

                if (!currentSystems.TryGetValue(systemName.ToUpper(), out int systemId))
                {
                    var newSystem = new ParkingSystem { Name = systemName };
                    _context.ParkingSystems.Add(newSystem);
                    await _context.SaveChangesAsync();
                    systemId = newSystem.Id;
                    currentSystems[systemName.ToUpper()] = systemId;
                }

                // Check if the entry already exists to allow update deltas
                var existingCarPark = await _context.CarParks
                    .Include(c => c.ParkingPolicy)
                    .FirstOrDefaultAsync(c => c.CarParkNo == carParkNo);

                decimal.TryParse(dict["gantry_height"].ToString(), out decimal parsedHeight);

                if (existingCarPark != null)
                {
                    // Core Properties Update Mapping
                    existingCarPark.Address = dict["address"].ToString()!;
                    existingCarPark.XCoord = double.Parse(dict["x_coord"].ToString()!);
                    existingCarPark.YCoord = double.Parse(dict["y_coord"].ToString()!);
                    existingCarPark.CarParkTypeId = typeId;
                    existingCarPark.ParkingSystemId = systemId;
                    existingCarPark.CarParkDecks = int.Parse(dict["car_park_decks"].ToString()!);
                    existingCarPark.GantryHeight = parsedHeight;
                    existingCarPark.HasBasement = dict["car_park_basement"].ToString() == "Y";

                    // Policy Data Updates
                    existingCarPark.ParkingPolicy.ShortTermParking = dict["short_term_parking"].ToString()!;
                    existingCarPark.ParkingPolicy.FreeParking = dict["free_parking"].ToString()!;
                    existingCarPark.ParkingPolicy.OffersNightParking = dict["night_parking"].ToString() == "YES";
                }
                else
                {
                    // Construct a new item chain
                    var newCarPark = new CarPark
                    {
                        CarParkNo = carParkNo,
                        Address = dict["address"].ToString()!,
                        XCoord = double.Parse(dict["x_coord"].ToString()!),
                        YCoord = double.Parse(dict["y_coord"].ToString()!),
                        CarParkTypeId = typeId,
                        ParkingSystemId = systemId,
                        CarParkDecks = int.Parse(dict["car_park_decks"].ToString()!),
                        GantryHeight = parsedHeight,
                        HasBasement = dict["car_park_basement"].ToString() == "Y",
                        ParkingPolicy = new ParkingPolicy
                        {
                            ShortTermParking = dict["short_term_parking"].ToString()!,
                            FreeParking = dict["free_parking"].ToString()!,
                            OffersNightParking = dict["night_parking"].ToString() == "YES"
                        }
                    };
                    _context.CarParks.Add(newCarPark);
                }
            }

            // Save our combined operations to disk
            await _context.SaveChangesAsync();
            
            // Commit our execution transaction safely
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            // CRITICAL: Any failures force an immediate data rollback on the database context state!
            await transaction.RollbackAsync();
            throw;
        }
    }
}