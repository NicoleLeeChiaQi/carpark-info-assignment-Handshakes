# Carpark Information API

This repository contains a .NET 8 Web API designed to manage and query HDB carpark information. The solution is built as a take-home assignment, focusing on backend development principles including database design, batch data processing, and API development.

The application processes a CSV file containing carpark data, stores it in a normalized SQLite database, and exposes RESTful endpoints for filtering carparks and managing user favorites.

## Tech Stack

*   **.NET 8:** The underlying framework for the application.
*   **ASP.NET Core Web API:** For building the RESTful API endpoints.
*   **Entity Framework Core:** As the Object-Relational Mapper (ORM) for database interactions.
*   **SQLite:** A lightweight, serverless, file-based database for data persistence.
*   **CsvHelper:** A library for reading and processing the input CSV data.
*   **Swagger (OpenAPI):** For interactive API documentation and testing.

## Database Schema

The database is designed with normalization in mind to reduce redundancy and improve data integrity.

*   `CarParks`: The central table storing essential carpark details like `car_park_no`, `address`, coordinates, number of decks, gantry height, and basement information. It serves as the primary entity.
*   `ParkingPolicy`: A one-to-one relationship with `CarParks`, holding policy details such as `short_term_parking`, `free_parking`, and `night_parking` availability.
*   `CarParkType`: A lookup table for different carpark types (e.g., "MULTI-STOREY CAR PARK", "BASEMENT CAR PARK").
*   `ParkingSystem`: A lookup table for parking system types (e.g., "ELECTRONIC PARKING", "COUPON PARKING").
*   `UserFavorite`: A table to manage the many-to-many relationship between users and their favorite carparks, linking a `UserId` to a `CarParkNo`.

Indexes have been added to columns frequently used in filtering (`GantryHeight`, `FreeParking`, `OffersNightParking`) to optimize query performance.

## Features

*   **Transactional Batch CSV Import:** An atomic, transactional batch service that processes a delta CSV file. It performs an "upsert" operation: new carparks are added, and existing ones are updated. If any record fails during processing, the entire transaction is rolled back to maintain data consistency.
*   **Carpark Filtering API:** A flexible endpoint that allows users to find carparks based on various criteria:
    *   Availability of free parking.
    *   Availability of night parking.
    *   Vehicle height requirements (gantry height).
*   **User Favorites:** An endpoint enabling users to save and manage a list of their favorite carparks.

## Getting Started

### Prerequisites

*   [.NET 8 SDK]

### Setup & Run

1.  **Clone the repository:**
    git clone https://github.com/nicoleleechiaqi/carpark-info-assignment-Handshakes.git

2.  **Navigate to the project directory:**
    cd carpark-info-assignment-Handshakes/CarParkAPI

3.  **Run the application:**
    The project is configured to use Entity Framework Core migrations to set up the database schema. The `carparks.db` file is included, but you can generate it from scratch. The application will automatically create and seed the database if it doesn't exist upon startup.
    dotnet run


4.  **Access the API:**
    The API will be running on `http://localhost:5052`. You can access the interactive Swagger UI documentation to test the endpoints at:
    [http://localhost:5052/swagger](http://localhost:5052/swagger)

## API Endpoints

The following endpoints are available:

### Car Park Management

*   **`POST /api/CarPark/import-delta`**
    *   Imports and processes carpark data from a CSV string provided in the request body. The entire operation is transactional.
    *   **Body (JSON):**
        {
          "data": "\"car_park_no\",\"address\",\"x_coord\",\"y_coord\",...\" \n\"ACB\",\"BLK 270/271 ALBERT CENTRE...\",..."
        }

*   **`GET /api/CarPark/filter`**
    *   Filters carparks based on query parameters.
    *   **Query Parameters:**
        *   `free` (boolean, optional): Set to `true` to find carparks that offer free parking.
        *   `night` (boolean, optional): Set to `true` to find carparks offering night parking.
        *   `height` (decimal, optional): Find carparks with a gantry height greater than or equal to the specified value.
    *   **Example Request:** `GET /api/CarPark/filter?free=true&height=2.0`

### User Favorites

*   **`POST /api/CarPark/favorites`**
    *   Adds a specified carpark to a user's list of favorites.
    *   **Query Parameters:**
        *   `userId` (string, required): The identifier for the user.
        *   `carParkNo` (string, required): The unique number of the carpark to favorite.
    *   **Example Request:** `POST /api/CarPark/favorites?userId=testuser&carParkNo=ACB`