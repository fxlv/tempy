#!/bin/bash
dotnet run &
echo "Tempy API target: $TEMPY_API_TARGET"
sleep 1
tail -f worker*.log
