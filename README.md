# Tempy
Store and display weather data


The project consists of 4 components:

- NetatmoLib is the library that is used to communicate with the netatmo API
- NetatmoCLI is the CLI tool that show the temperature status
- TempyAPI is as web API that talks to CosmosDB to retrieve and store temperature measurements
- TempyWorker runs in a loop and collects data from NetatmoAPI using the netatmolib and posts it to the TempyAPI

## Building

```
dotnet restore
dotnet build
```

### Testing with Docker

If you use Docker then the quickest way to test the build is to run
```
docker build --no-cache .
```

## Testing

```
dotnet test
```

## Running

...

