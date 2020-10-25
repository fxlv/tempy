#!/bin/bash
echo "Deploying API"
kubectl apply -f deployments/tempy-api-deploy.yaml
echo "Deploying API Service"
kubectl apply -f services/tempy-api-service.yaml
echo "Deploying Worker"
kubectl apply -f deployments/tempy-worker-deploy.yaml
echo "deployment complete"
