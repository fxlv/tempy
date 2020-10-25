#!/bin/bash


echo
echo "Building Docker images"
echo
sudo docker build -t fxlvacr.azurecr.io/testing/tempybuild -f Dockerfile.build --no-cache .
sudo docker build -t fxlvacr.azurecr.io/testing/tempyapi -f Dockerfile.build --no-cache .
sudo docker build -t fxlvacr.azurecr.io/testing/tempyworker -f Dockerfile.Worker --no-cache .
  

echo
echo "Pushing docker images"
echo 
sudo docker push fxlvacr.azurecr.io/testing/tempybuild
sudo docker push fxlvacr.azurecr.io/testing/tempyapi
sudo docker push fxlvacr.azurecr.io/testing/tempyworker

echo
echo "all done"
echo
