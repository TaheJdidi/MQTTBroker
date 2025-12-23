# MQTT Broker

A full-featured MQTT V5 broker implementation using .NET 10 as a Windows service with an Angular 21 frontend for management and monitoring.

## Features

- **MQTT V5 Protocol Support**: Full support for MQTT version 5.0 using the MQTTnet library
- **Windows Service**: Runs as a Windows service for automatic startup and background operation
- **REST API**: RESTful API for broker management (start, stop, configure)
- **Angular 21 Frontend**: Modern web interface for monitoring and configuration
- **Real-time Monitoring**: View connected clients and broker status in real-time
- **Configuration Management**: Easily configure broker settings through the web UI

## Project Structure

```
MQTTBroker/
├── src/
│   ├── MQTTBrokerService/           # .NET 10 MQTT Broker Service
│   │   └── MQTTBrokerService/
│   │       ├── Controllers/         # REST API Controllers
│   │       ├── Models/              # Data models
│   │       ├── Services/            # MQTT Broker management service
│   │       ├── Program.cs           # Application entry point
│   │       └── Worker.cs            # Background service worker
│   └── MQTTBrokerService.Tests/     # Unit tests
└── frontend/                        # Angular 21 Frontend
    └── src/
        └── app/
            ├── components/          # UI Components
            ├── models/              # TypeScript models
            └── services/            # API services
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/)
- [Angular CLI 21](https://angular.dev/)

## Getting Started

### Backend (.NET Service)

1. Navigate to the backend project:
   ```bash
   cd src/MQTTBrokerService/MQTTBrokerService
   ```

2. Restore dependencies and build:
   ```bash
   dotnet restore
   dotnet build
   ```

3. Run the service:
   ```bash
   dotnet run
   ```

The API will be available at `http://localhost:5000` and the MQTT broker will listen on port 1883.

### Frontend (Angular)

1. Navigate to the frontend project:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   ng serve
   ```

The frontend will be available at `http://localhost:4200`.

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/broker/status` | GET | Get broker status |
| `/api/broker/clients` | GET | List connected clients |
| `/api/broker/configuration` | GET | Get broker configuration |
| `/api/broker/configuration` | PUT | Update broker configuration |
| `/api/broker/start` | POST | Start the broker |
| `/api/broker/stop` | POST | Stop the broker |
| `/api/broker/clients/{clientId}/disconnect` | POST | Disconnect a client |

## Configuration

The broker can be configured via `appsettings.json`:

```json
{
  "MqttBroker": {
    "Port": 1883,
    "MaxPendingConnections": 100,
    "EnableAuthentication": false,
    "EnableVerboseLogging": false,
    "CommunicationTimeout": 30,
    "AutoStart": true
  }
}
```

## Installing as Windows Service

To install the broker as a Windows service:

```powershell
sc.exe create "MQTTBrokerService" binPath="path\to\MQTTBrokerService.exe"
sc.exe start "MQTTBrokerService"
```

To uninstall:

```powershell
sc.exe stop "MQTTBrokerService"
sc.exe delete "MQTTBrokerService"
```

## Running Tests

### Backend Tests
```bash
cd src/MQTTBrokerService.Tests/MQTTBrokerService.Tests
dotnet test
```

### Frontend Tests
```bash
cd frontend
ng test
```

## Technologies Used

- **Backend**:
  - .NET 10
  - ASP.NET Core
  - MQTTnet 5.0 (MQTT V5 support)
  - Windows Services support

- **Frontend**:
  - Angular 21
  - TypeScript
  - SCSS
  - RxJS

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
