# TaskProject Kubernetes Deployment

This directory contains Kubernetes manifests and deployment scripts for the TaskProject application.

## 📁 Directory Structure

```
k8s/
├── namespace.yaml                 # Kubernetes namespace
├── configmap-secrets.yaml        # Configuration and secrets
├── ingress.yaml                   # Ingress controller configuration
├── hpa.yaml                       # Horizontal Pod Autoscalers
├── deploy.sh                      # Deployment script (Linux/macOS)
├── deploy.ps1                     # Deployment script (Windows PowerShell)
├── cleanup.sh                     # Cleanup script (Linux/macOS)
├── cleanup.ps1                    # Cleanup script (Windows PowerShell)
├── infrastructure/
│   ├── sqlserver.yaml            # SQL Server deployment
│   └── rabbitmq.yaml             # RabbitMQ deployment
├── services/
│   ├── task-service.yaml         # Task Service deployment
│   └── email-service.yaml        # Email Service deployment
├── gateway/
│   └── gateway-service.yaml      # API Gateway deployment
└── web/
    └── frontend.yaml              # Frontend deployment
```

## 🚀 Quick Start

### Prerequisites

1. **Kubernetes Cluster**: Running Kubernetes cluster (minikube, kind, AKS, EKS, GKE, etc.)
2. **kubectl**: Kubernetes command-line tool
3. **Docker**: For building container images
4. **Ingress Controller**: NGINX Ingress Controller (optional, for external access)

### Installation

#### Option 1: Using PowerShell Script (Windows)

```powershell
# Navigate to the project root
cd "C:\Users\MinhDuongHong\OneDrive - NASHTECH\Documents\Project\ABP\TaskProject"

# Run the deployment script
.\k8s\deploy.ps1
```

#### Option 2: Using Bash Script (Linux/macOS/WSL)

```bash
# Navigate to the project root
cd /path/to/TaskProject

# Make script executable
chmod +x k8s/deploy.sh

# Run the deployment script
./k8s/deploy.sh
```

#### Option 3: Manual Deployment

```bash
# Create namespace
kubectl apply -f k8s/namespace.yaml

# Apply configuration and secrets
kubectl apply -f k8s/configmap-secrets.yaml

# Deploy infrastructure
kubectl apply -f k8s/infrastructure/

# Wait for infrastructure to be ready
kubectl wait --for=condition=ready pod -l app=sqlserver -n taskproject --timeout=300s
kubectl wait --for=condition=ready pod -l app=rabbitmq -n taskproject --timeout=300s

# Deploy services
kubectl apply -f k8s/services/
kubectl apply -f k8s/gateway/
kubectl apply -f k8s/web/

# Deploy ingress and autoscaling
kubectl apply -f k8s/ingress.yaml
kubectl apply -f k8s/hpa.yaml
```

## 🏗️ Architecture Overview

### Components

| Component           | Replicas | Port       | Description                     |
| ------------------- | -------- | ---------- | ------------------------------- |
| **SQL Server**      | 1        | 1433       | Primary database                |
| **RabbitMQ**        | 1        | 5672/15672 | Message broker                  |
| **Task Service**    | 2-10     | 5008       | Task management microservice    |
| **Email Service**   | 2-8      | 5009       | Email notification microservice |
| **Gateway Service** | 2-10     | 5000       | API Gateway                     |
| **Frontend**        | 2-6      | 80         | React frontend                  |

### Networking

- **Internal Communication**: Services communicate via ClusterIP services
- **External Access**: Through Ingress Controller or LoadBalancer
- **Service Discovery**: Kubernetes DNS (service-name.namespace.svc.cluster.local)

### Storage

- **SQL Server**: 10Gi persistent volume
- **RabbitMQ**: 5Gi persistent volume

### Auto-scaling

- **CPU Threshold**: 70% utilization
- **Memory Threshold**: 80% utilization
- **Scale Down Delay**: 5 minutes
- **Scale Up**: Immediate

## 🔧 Configuration

### Environment Variables

The application uses ConfigMaps and Secrets for configuration:

#### ConfigMap (`taskproject-config`)

- Database connection settings
- RabbitMQ configuration
- Service URLs
- ASP.NET Core environment

#### Secrets (`taskproject-secrets`)

- Database password
- RabbitMQ password
- Gmail API credentials

### Customization

To customize the deployment:

1. **Resource Limits**: Edit resource requests/limits in deployment manifests
2. **Replica Counts**: Modify `spec.replicas` in deployment files
3. **Environment Variables**: Update `configmap-secrets.yaml`
4. **Ingress Rules**: Modify `ingress.yaml` for different routing
5. **Auto-scaling**: Adjust HPA settings in `hpa.yaml`

## 🌐 Accessing the Application

### Local Development (minikube)

1. **Enable Ingress**:

   ```bash
   minikube addons enable ingress
   ```

2. **Get Minikube IP**:

   ```bash
   minikube ip
   ```

3. **Update hosts file**:

   ```
   # Add to /etc/hosts (Linux/macOS) or C:\Windows\System32\drivers\etc\hosts (Windows)
   <minikube-ip> taskproject.local
   ```

4. **Access Application**:
   - Frontend: http://taskproject.local
   - API Gateway: http://taskproject.local/api
   - RabbitMQ Management: http://taskproject.local/rabbitmq

### Cloud Deployment

For cloud deployments, the LoadBalancer service will provide an external IP:

```bash
kubectl get svc taskproject-loadbalancer -n taskproject
```

### Port Forwarding (Development)

```bash
# Frontend
kubectl port-forward svc/frontend-service 8080:80 -n taskproject

# API Gateway
kubectl port-forward svc/gateway-service 5000:5000 -n taskproject

# Task Service (direct access)
kubectl port-forward svc/task-service 5008:5008 -n taskproject

# RabbitMQ Management
kubectl port-forward svc/rabbitmq-service 15672:15672 -n taskproject
```

## 📊 Monitoring and Troubleshooting

### Useful Commands

```bash
# Check pod status
kubectl get pods -n taskproject

# Check service endpoints
kubectl get endpoints -n taskproject

# View pod logs
kubectl logs -f deployment/task-service -n taskproject

# Describe pod for troubleshooting
kubectl describe pod <pod-name> -n taskproject

# Check resource usage
kubectl top pods -n taskproject

# Check HPA status
kubectl get hpa -n taskproject

# Check ingress status
kubectl get ingress -n taskproject
```

### Common Issues

1. **ImagePullBackOff**: Ensure Docker images are built and available
2. **Pending Pods**: Check resource constraints and node capacity
3. **Service Unreachable**: Verify service selectors and pod labels
4. **Database Connection**: Ensure SQL Server is ready and connection string is correct
5. **RabbitMQ Connection**: Check RabbitMQ service status and credentials

### Health Checks

All services include readiness and liveness probes:

- **Readiness**: Service is ready to receive traffic
- **Liveness**: Service is healthy and should not be restarted

## 🔒 Security Considerations

### Secrets Management

- All sensitive data stored in Kubernetes Secrets
- Base64 encoded (not encrypted by default)
- Consider using external secret management (Azure Key Vault, AWS Secrets Manager, etc.)

### Network Policies

Consider implementing network policies for additional security:

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: taskproject-network-policy
  namespace: taskproject
spec:
  podSelector: {}
  policyTypes:
    - Ingress
    - Egress
  # Define specific ingress/egress rules
```

### RBAC

Implement Role-Based Access Control for fine-grained permissions:

```yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: taskproject-role
  namespace: taskproject
rules:
# Define specific permissions
```

## 🔄 CI/CD Integration

### GitLab CI/CD Example

```yaml
deploy:
  stage: deploy
  script:
    - kubectl apply -f k8s/
  only:
    - main
```

### GitHub Actions Example

```yaml
- name: Deploy to Kubernetes
  run: |
    kubectl apply -f k8s/
  env:
    KUBECONFIG: ${{ secrets.KUBECONFIG }}
```

## 🧹 Cleanup

### Using Scripts

**PowerShell**:

```powershell
.\k8s\cleanup.ps1
```

**Bash**:

```bash
./k8s/cleanup.sh
```

### Manual Cleanup

```bash
# Delete all resources
kubectl delete namespace taskproject

# Or delete specific components
kubectl delete -f k8s/
```

## 📈 Scaling

### Manual Scaling

```bash
# Scale task service
kubectl scale deployment task-service --replicas=5 -n taskproject

# Scale email service
kubectl scale deployment email-service --replicas=3 -n taskproject
```

### Auto-scaling

The HPA automatically scales based on CPU and memory usage. Monitor with:

```bash
kubectl get hpa -n taskproject -w
```

## 🔧 Maintenance

### Updates

1. **Application Updates**: Build new images and update deployment
2. **Configuration Changes**: Update ConfigMaps/Secrets and restart pods
3. **Kubernetes Updates**: Follow cluster upgrade procedures

### Backup

- **Database**: Regular SQL Server backups
- **Configuration**: Version control all Kubernetes manifests
- **Persistent Volumes**: Backup according to storage provider recommendations

## 📚 Additional Resources

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [NGINX Ingress Controller](https://kubernetes.github.io/ingress-nginx/)
- [Horizontal Pod Autoscaler](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/)
- [Kubernetes Security Best Practices](https://kubernetes.io/docs/concepts/security/)

---

For questions or issues, please refer to the main project documentation or create an issue in the repository.
