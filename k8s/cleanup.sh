#!/bin/bash

# TaskProject Kubernetes Cleanup Script
# This script removes the TaskProject application from Kubernetes cluster

set -e

echo "🧹 Starting TaskProject Kubernetes cleanup..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

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
    print_error "kubectl is not installed."
    exit 1
fi

# Check if namespace exists
if ! kubectl get namespace taskproject &> /dev/null; then
    print_warning "TaskProject namespace does not exist. Nothing to clean up."
    exit 0
fi

print_warning "This will delete all TaskProject resources from Kubernetes!"
read -p "Are you sure you want to continue? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    print_status "Cleanup cancelled."
    exit 0
fi

print_status "Cleaning up TaskProject from Kubernetes..."

# Delete HPA
print_status "Removing autoscalers..."
kubectl delete -f k8s/hpa.yaml --ignore-not-found=true

# Delete ingress
print_status "Removing ingress..."
kubectl delete -f k8s/ingress.yaml --ignore-not-found=true

# Delete web frontend
print_status "Removing frontend..."
kubectl delete -f k8s/web/ --ignore-not-found=true

# Delete services
print_status "Removing microservices..."
kubectl delete -f k8s/services/ --ignore-not-found=true
kubectl delete -f k8s/gateway/ --ignore-not-found=true

# Delete infrastructure
print_status "Removing infrastructure..."
kubectl delete -f k8s/infrastructure/ --ignore-not-found=true

# Delete ConfigMaps and Secrets
print_status "Removing ConfigMaps and Secrets..."
kubectl delete -f k8s/configmap-secrets.yaml --ignore-not-found=true

# Delete namespace (this will delete everything else)
print_status "Removing namespace..."
kubectl delete -f k8s/namespace.yaml --ignore-not-found=true

print_success "TaskProject cleanup completed!"
print_status "All resources have been removed from the cluster."