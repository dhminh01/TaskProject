#!/bin/bash

# TaskProject Kubernetes Deployment Script
# This script deploys the TaskProject application to a Kubernetes cluster

set -e

echo "🚀 Starting TaskProject Kubernetes deployment..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if kubectl is installed
if ! command -v kubectl &> /dev/null; then
    print_error "kubectl is not installed. Please install kubectl first."
    exit 1
fi

# Check if we can connect to the cluster
if ! kubectl cluster-info &> /dev/null; then
    print_error "Cannot connect to Kubernetes cluster. Please check your kubeconfig."
    exit 1
fi

print_status "Connected to Kubernetes cluster: $(kubectl config current-context)"

# Build Docker images (optional - comment out if images are already built)
print_status "Building Docker images..."

# Build Task Service
print_status "Building Task Service..."
docker build -t taskproject/task-service:latest -f src/Services/TaskService/TaskService.API/Dockerfile .

# Build Email Service
print_status "Building Email Service..."
docker build -t taskproject/email-service:latest -f src/Services/EmailService/EmailService.API/Dockerfile .

# Build Gateway Service
print_status "Building Gateway Service..."
docker build -t taskproject/gateway-service:latest -f src/Gateway/Gateway.API/Dockerfile .

# Build Frontend
print_status "Building Frontend..."
docker build -t taskproject/frontend:latest -f src/Web/Dockerfile .

print_success "All Docker images built successfully!"

# Deploy to Kubernetes
print_status "Deploying to Kubernetes..."

# Create namespace
print_status "Creating namespace..."
kubectl apply -f k8s/namespace.yaml

# Apply ConfigMaps and Secrets
print_status "Applying ConfigMaps and Secrets..."
kubectl apply -f k8s/configmap-secrets.yaml

# Deploy infrastructure (databases, message queues)
print_status "Deploying infrastructure components..."
kubectl apply -f k8s/infrastructure/

# Wait for infrastructure to be ready
print_status "Waiting for infrastructure to be ready..."
kubectl wait --for=condition=ready pod -l app=sqlserver -n taskproject --timeout=300s
kubectl wait --for=condition=ready pod -l app=rabbitmq -n taskproject --timeout=300s

# Deploy services
print_status "Deploying microservices..."
kubectl apply -f k8s/services/
kubectl apply -f k8s/gateway/

# Wait for services to be ready
print_status "Waiting for services to be ready..."
kubectl wait --for=condition=ready pod -l app=task-service -n taskproject --timeout=300s
kubectl wait --for=condition=ready pod -l app=email-service -n taskproject --timeout=300s
kubectl wait --for=condition=ready pod -l app=gateway-service -n taskproject --timeout=300s

# Deploy web frontend
print_status "Deploying frontend..."
kubectl apply -f k8s/web/

# Wait for frontend to be ready
kubectl wait --for=condition=ready pod -l app=frontend -n taskproject --timeout=300s

# Deploy ingress and HPA
print_status "Deploying ingress and autoscaling..."
kubectl apply -f k8s/ingress.yaml
kubectl apply -f k8s/hpa.yaml

print_success "TaskProject deployed successfully!"

# Display deployment status
print_status "Deployment Status:"
kubectl get pods -n taskproject
echo ""
kubectl get services -n taskproject
echo ""
kubectl get ingress -n taskproject

print_success "🎉 TaskProject is now running on Kubernetes!"
print_status "Access the application at: http://taskproject.local"
print_warning "Make sure to add 'taskproject.local' to your /etc/hosts file pointing to your ingress IP"

# Display useful commands
echo ""
print_status "Useful commands:"
echo "  View pods: kubectl get pods -n taskproject"
echo "  View services: kubectl get svc -n taskproject"
echo "  View logs: kubectl logs -f deployment/<service-name> -n taskproject"
echo "  Scale service: kubectl scale deployment <service-name> --replicas=<number> -n taskproject"
echo "  Port forward: kubectl port-forward svc/<service-name> <local-port>:<service-port> -n taskproject"