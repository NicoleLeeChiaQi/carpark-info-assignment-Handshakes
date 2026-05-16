using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarParkAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarParkTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarParkTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSystems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSystems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CarParks",
                columns: table => new
                {
                    CarParkNo = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    XCoord = table.Column<double>(type: "REAL", nullable: false),
                    YCoord = table.Column<double>(type: "REAL", nullable: false),
                    CarParkTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParkingSystemId = table.Column<int>(type: "INTEGER", nullable: false),
                    CarParkDecks = table.Column<int>(type: "INTEGER", nullable: false),
                    GantryHeight = table.Column<decimal>(type: "TEXT", nullable: false),
                    HasBasement = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarParks", x => x.CarParkNo);
                    table.ForeignKey(
                        name: "FK_CarParks_CarParkTypes_CarParkTypeId",
                        column: x => x.CarParkTypeId,
                        principalTable: "CarParkTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarParks_ParkingSystems_ParkingSystemId",
                        column: x => x.ParkingSystemId,
                        principalTable: "ParkingSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParkingPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CarParkNo = table.Column<string>(type: "TEXT", nullable: false),
                    ShortTermParking = table.Column<string>(type: "TEXT", nullable: false),
                    FreeParking = table.Column<string>(type: "TEXT", nullable: false),
                    OffersNightParking = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkingPolicies_CarParks_CarParkNo",
                        column: x => x.CarParkNo,
                        principalTable: "CarParks",
                        principalColumn: "CarParkNo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFavorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    CarParkNo = table.Column<string>(type: "TEXT", nullable: false),
                    CarParkNo1 = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFavorites_CarParks_CarParkNo1",
                        column: x => x.CarParkNo1,
                        principalTable: "CarParks",
                        principalColumn: "CarParkNo");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarParks_CarParkTypeId",
                table: "CarParks",
                column: "CarParkTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CarParks_GantryHeight",
                table: "CarParks",
                column: "GantryHeight");

            migrationBuilder.CreateIndex(
                name: "IX_CarParks_ParkingSystemId",
                table: "CarParks",
                column: "ParkingSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingPolicies_CarParkNo",
                table: "ParkingPolicies",
                column: "CarParkNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParkingPolicies_FreeParking",
                table: "ParkingPolicies",
                column: "FreeParking");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingPolicies_OffersNightParking",
                table: "ParkingPolicies",
                column: "OffersNightParking");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavorites_CarParkNo1",
                table: "UserFavorites",
                column: "CarParkNo1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParkingPolicies");

            migrationBuilder.DropTable(
                name: "UserFavorites");

            migrationBuilder.DropTable(
                name: "CarParks");

            migrationBuilder.DropTable(
                name: "CarParkTypes");

            migrationBuilder.DropTable(
                name: "ParkingSystems");
        }
    }
}
