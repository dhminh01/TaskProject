# TaskProject Kubernetes Cleanup Script (PowerShell)
# This script removes the TaskProject application from Kubernetes cluster

param(
    [switch]$Force,
    [switch]$Help
)

if ($Help) {
    Write-Host "TaskProject Kubernetes Cleanup Script" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage: .\cleanup.ps1 [OPTIONS]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  -Force        Skip confirmation prompt"
    Write-Host "  -Help         Show this help message"
    exit 0
}

$ErrorActionPreference = "Continue"

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

Write-Host "🧹 Starting TaskProject Kubernetes cleanup..." -ForegroundColor Green

# Check if kubectl is installed
try {
    kubectl version --client --short | Out-Null
} catch {
    Write-Error "kubectl is not installed."
    exit 1
}

# Check if namespace exists
try {
    kubectl get namespace taskproject | Out-Null
} catch {
    Write-Warning "TaskProject namespace does not exist. Nothing to clean up."
    exit 0
}

if (-not $Force) {
    Write-Warning "This will delete all TaskProject resources from Kubernetes!"
    $confirmation = Read-Host "Are you sure you want to continue? (y/N)"
    if ($confirmation -ne "y" -and $confirmation -ne "Y") {
        Write-Status "Cleanup cancelled."
        exit 0
    }
}

Write-Status "Cleaning up TaskProject from Kubernetes..."

# Delete HPA
Write-Status "Removing autoscalers..."
kubectl delete -f k8s/hpa.yaml --ignore-not-found=true

# Delete ingress
Write-Status "Removing ingress..."
kubectl delete -f k8s/ingress.yaml --ignore-not-found=true

# Delete web frontend
Write-Status "Removing frontend..."
kubectl delete -f k8s/web/ --ignore-not-found=true

# Delete services
Write-Status "Removing microservices..."
kubectl delete -f k8s/services/ --ignore-not-found=true
kubectl delete -f k8s/gateway/ --ignore-not-found=true

# Delete infrastructure
Write-Status "Removing infrastructure..."
kubectl delete -f k8s/infrastructure/ --ignore-not-found=true

# Delete ConfigMaps and Secrets
Write-Status "Removing ConfigMaps and Secrets..."
kubectl delete -f k8s/configmap-secrets.yaml --ignore-not-found=true

# Delete namespace (this will delete everything else)
Write-Status "Removing namespace..."
kubectl delete -f k8s/namespace.yaml --ignore-not-found=true

Write-Success "TaskProject cleanup completed!"
Write-Status "All resources have been removed from the cluster."