# ChicLink

ChicLink is a modern dating application that connects people based on their interests and preferences. The application provides a platform for users to create profiles, send messages, and like each other’s profiles to foster meaningful connections.

## Table of Contents

- [Features](#features)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Configuration](#configuration)
- [API Documentation](#api-documentation)
- [License](#license)

## Features

- User registration and authentication
- Profile creation and management
- Sending and receiving messages
- Liking and matching with other users
- Profile photo uploads and updates

## Getting Started

### Prerequisites

Ensure you have the following software installed on your machine:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or another compatible database system

### Installation

1. Clone the repository:
    ```bash
    git clone https://github.com/yourusername/chiclink.git
    cd chiclink
    ```

2. Restore the dependencies:
    ```bash
    dotnet restore
    ```

3. Build the project:
    ```bash
    dotnet build
    ```

### Configuration

1. Update the `appsettings.json` file with your database connection string and JWT settings:
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "YourConnectionStringHere"
      },
      "JwtSettings": {
        "SecretKey": "YourSecretKeyHere",
        "ValidIssuer": "YourIssuerHere",
        "ValidAudience": "YourAudienceHere"
      }
    }
    ```

2. Apply the database migrations:
    ```bash
    dotnet ef database update
    ```

3. Run the application:
    ```bash
    dotnet run
    ```

## API Documentation

The API documentation is available via Swagger. Once the application is running, you can access it at:

### Available Endpoints

#### Users
- `GET /api/users`: Get a list of all users.
- `GET /api/users/{id}`: Get details of a specific user.
- `PUT /api/users/{id}`: Update a user's information.
- `DELETE /api/users/{id}`: Delete a user.

#### Profiles
- `GET /api/profiles/by-profile-id/{profileId}`: Get a profile by profile ID.
- `GET /api/profiles/by-user-id/{userId}`: Get a profile by user ID.
- `POST /api/profiles/create-profile-for-user/{userId}`: Create a profile for a user.
- `PUT /api/profiles/update-profile/{profileId}`: Update a profile.
- `PUT /api/profiles/update-profile-photos/{profileId}`: Update profile photos.

#### Likes
- `POST /api/likes`: Add a like.
- `GET /api/likes/{userId}`: Get likes received by a user.
- `DELETE /api/likes/{senderId}/{receiverId}`: Delete a like.

#### Messages
- `GET /api/messages/{userId}/sentMessages`: Get messages sent by a user.
- `GET /api/messages/{userId}/receivedMessages`: Get messages received by a user.
- `POST /api/messages`: Send a message.

## License

This project is licensed under the Apache License. See the [LICENSE](LICENSE) file for details.
