#!/bin/bash
if [ -e /config/appsettings.json ]; then
    echo "Running in a Kubernetes environment"
    echo -n "Copying configuration file: "
    cp -v /config/appsettings.json .
    export TEMPY_API_TARGET=tempy-api:5000
fi
echo "Tempy API target: $TEMPY_API_TARGET"
dotnet run
