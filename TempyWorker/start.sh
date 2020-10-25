#!/bin/bash
if [ -e /config/appsettings.json ]; then
    echo -n "Copying configuration file: "
    cp -v /config/appsettings.json .
fi
echo "Tempy API target: $TEMPY_API_TARGET"
dotnet run
