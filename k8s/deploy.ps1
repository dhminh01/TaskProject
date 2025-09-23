# TaskProject Kubernetes Deployment Script (PowerShell)
# This script deploys the TaskProject application to a Kubernetes cluster

param(
    [switch]$SkipBuild,
    [switch]$Help
)

if ($Help) {
    Write-Host "TaskProject Kubernetes Deployment Script" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage: .\deploy.ps1 [OPTIONS]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  -SkipBuild    Skip Docker image building"
    Write-Host "  -Help         Show this help message"
    exit 0
}

# Colors for output
$ErrorActionPreference = "Stop"

function Write-Status {
    param($Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
    param($Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning {
    param($Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param($Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

Write-Host "🚀 Starting TaskProject Kubernetes deployment..." -ForegroundColor Green

# Check if kubectl is installed
try {
    kubectl version --client --short | Out-Null
} catch {
    Write-Error "kubectl is not installed. Please install kubectl first."
    exit 1
}

# Check if we can connect to the cluster
try {
    kubectl cluster-info | Out-Null
} catch {
    Write-Error "Cannot connect to Kubernetes cluster. Please check your kubeconfig."
    exit 1
}

$currentContext = kubectl config current-context
Write-Status "Connected to Kubernetes cluster: $currentContext"

# Build Docker images (unless skipped)
if (-not $SkipBuild) {
    Write-Status "Building Docker images..."

    # Build Task Service
    Write-Status "Building Task Service..."
    docker build -t taskproject/task-service:latest -f src/Services/TaskService/TaskService.API/Dockerfile .

    # Build Email Service
    Write-Status "Building Email Service..."
    docker build -t taskproject/email-service:latest -f src/Services/EmailService/EmailService.API/Dockerfile .

    # Build Gateway Service
    Write-Status "Building Gateway Service..."
    docker build -t taskproject/gateway-service:latest -f src/Gateway/Gateway.API/Dockerfile .

    # Build Frontend
    Write-Status "Building Frontend..."
    docker build -t taskproject/frontend:latest -f src/Web/Dockerfile .

    Write-Success "All Docker images built successfully!"
} else {
    Write-Warning "Skipping Docker image build..."
}

# Deploy to Kubernetes
Write-Status "Deploying to Kubernetes..."

# Create namespace
Write-Status "Creating namespace..."
kubectl apply -f k8s/namespace.yaml

# Apply ConfigMaps and Secrets
Write-Status "Applying ConfigMaps and Secrets..."
kubectl apply -f k8s/configmap-secrets.yaml

# Deploy infrastructure (databases, message queues)
Write-Status "Deploying infrastructure components..."
kubectl apply -f k8s/infrastructure/

# Wait for infrastructure to be ready
Write-Status "Waiting for infrastructure to be ready..."
kubectl wait --for=condition=ready pod -l app=sqlserver -n taskproject --timeout=300s
kubectl wait --for=condition=ready pod -l app=rabbitmq -n taskproject --timeout=300s

# Deploy services
Write-Status "Deploying microservices..."
kubectl apply -f k8s/services/
kubectl apply -f k8s/gateway/

# Wait for services to be ready
Write-Status "Waiting for services to be ready..."
kubectl wait --for=condition=ready pod -l app=task-service -n taskproject --timeout=300s
kubectl wait --for=condition=ready pod -l app=email-service -n taskproject --timeout=300s
kubectl wait --for=condition=ready pod -l app=gateway-service -n taskproject --timeout=300s

# Deploy web frontend
Write-Status "Deploying frontend..."
kubectl apply -f k8s/web/

# Wait for frontend to be ready
kubectl wait --for=condition=ready pod -l app=frontend -n taskproject --timeout=300s

# Deploy ingress and HPA
Write-Status "Deploying ingress and autoscaling..."
kubectl apply -f k8s/ingress.yaml
kubectl apply -f k8s/hpa.yaml

Write-Success "TaskProject deployed successfully!"

# Display deployment status
Write-Status "Deployment Status:"
kubectl get pods -n taskproject
Write-Host ""
kubectl get services -n taskproject
Write-Host ""
kubectl get ingress -n taskproject

Write-Success "🎉 TaskProject is now running on Kubernetes!"
Write-Status "Access the application at: http://taskproject.local"
Write-Warning "Make sure to add 'taskproject.local' to your hosts file pointing to your ingress IP"

# Display useful commands
Write-Host ""
Write-Status "Useful commands:"
Write-Host "  View pods: kubectl get pods -n taskproject" -ForegroundColor Cyan
Write-Host "  View services: kubectl get svc -n taskproject" -ForegroundColor Cyan
Write-Host "  View logs: kubectl logs -f deployment/<service-name> -n taskproject" -ForegroundColor Cyan
Write-Host "  Scale service: kubectl scale deployment <service-name> --replicas=<number> -n taskproject" -ForegroundColor Cyan
Write-Host "  Port forward: kubectl port-forward svc/<service-name> <local-port>:<service-port> -n taskproject" -ForegroundColor Cyan