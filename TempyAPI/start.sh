#!/bin/bash
if [ -e /config/appsettings.json ]; then
    echo -n "Copying configuration file: "
    cp -v /config/appsettings.json .
fi
dotnet run
