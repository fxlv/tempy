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

### Using with Docker

If you use Docker then there are few Dockerfiles you could use for testing or even deploying the project.
You'll find them in the `Docker` directory.

To test that the solution builds you can use 
```
docker build -f Dockerfile.build --no-cache .
```
And if you'd like to have the components running in containers, you could build separate images for API and Worker:

```
docker build -f Dockerfile.API -t tempy.api --no-cache .
docker build -f Dockerfile.Worker -t tempy.worker --no-cache .
```

Now that you have the images, you can spin un the containers like so:

```
docker run -t -p 5000:5000 -i tempy.api:latest
docker run -e TEMPY_API_TARGET=${DOCKER_IP}:5000 -t -i tempy.worker:latest
```
Observe that you can provide the IP for your docker host via an environment variable `TEMPY_API_TARGET`.  
You can also leave it out, and `localhost` will be assumed.


## Testing

```
dotnet test
```

## Running
Running is as simple as doing `dotnet run` in the respective directories of `TempyAPI` and `TempyWorker`. 
You can also use the provided `start.sh` for both.

Currently this is all for debugging purposes only, so it is preferred that you run Debug versions and performance is not an object. 


