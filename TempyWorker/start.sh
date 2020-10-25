#!/bin/bash
if [ -e /config/appsettings.json ]; then
    echo -n "Copying configuration file: "
    cp -v /config/appsettings.json .
fi
echo "Tempy API target: $TEMPY_API_TARGET"


echo "Going into loop.."
while true; do sleep 60; done
# dotnet run

