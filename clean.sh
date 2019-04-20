#!/bin/bash

echo -n "Cleaning up log files..."
find . -type f -name "*.log" -exec rm {} \;
echo "done."

echo "Cleaning up Release files"
find . -type d -name "Release" -exec rm -rfdv {} \;
echo "Cleaning up debug files"
find . -type d -name "Debug" -exec rm -rfdv {} \;
