
# 🚀 Docker + Kubernetes Command Cheat Sheet

This guide summarizes the most commonly used commands for **Docker**, **Kubernetes (kubectl)**, and **Minikube**.

---

## 🐳 Docker Commands

### 🔧 Images
```bash
docker build -t myapp:latest .      # Build image from Dockerfile
docker images                       # List local images
docker rmi myapp:latest             # Remove image
docker pull nginx:latest            # Download image from Docker Hub
docker tag myapp:latest myrepo/myapp:v1   # Tag image for registry
docker push myrepo/myapp:v1         # Push image to registry
```

### 📦 Containers
```bash
docker run -d -p 8080:80 myapp      # Run container (map host:container port)
docker ps                           # List running containers
docker ps -a                        # List all containers (including stopped)
docker stop <container_id>          # Stop container
docker rm <container_id>            # Remove container
docker restart <container_id>       # Restart container
docker exec -it <container_id> sh   # Get shell inside running container
```

### 📂 Volumes & Networks
```bash
docker volume ls                    # List volumes
docker volume rm myvolume           # Remove volume
docker network ls                   # List networks
docker network create my-net        # Create network
docker network inspect my-net       # Inspect network details
docker run -d --network=my-net nginx   # Run container in custom network
```

### 🛑 Clean up
```bash
docker system prune -a              # Remove all unused containers/images/networks
docker image prune -a               # Remove dangling & unused images
docker container prune              # Remove stopped containers
docker volume prune                 # Remove unused volumes
```

---

## ☸️ Kubernetes (`kubectl`) Commands

### 🌐 Cluster & Contexts
```bash
kubectl version --client            # Check kubectl version
kubectl config view                 # Show kubeconfig
kubectl config get-contexts         # List contexts
kubectl config use-context minikube # Switch context
kubectl cluster-info                # Show cluster info
kubectl get nodes                   # List nodes in cluster
kubectl get namespaces              # List namespaces
```

### 📦 Workloads (Pods, Deployments)
```bash
kubectl get pods                    # List pods in default namespace
kubectl get pods -A                 # List pods in all namespaces
kubectl describe pod <pod_name>     # Show detailed info of a pod
kubectl logs <pod_name>             # Show logs of a pod
kubectl exec -it <pod_name> -- sh   # Open shell in a pod

kubectl get deployments             # List deployments
kubectl describe deployment myapp   # Deployment details
kubectl rollout status deployment/myapp   # Check rollout
kubectl rollout undo deployment/myapp     # Rollback deployment
kubectl scale deployment myapp --replicas=3   # Scale deployment
```

### 🛠 Services & Networking
```bash
kubectl get svc                     # List services
kubectl expose deployment myapp --type=NodePort --port=80   # Expose deployment
kubectl get ingress                 # List ingress rules
kubectl describe ingress my-ingress # Inspect ingress
kubectl get networkpolicy           # List network policies
```

### ⚙️ Apply & Delete Manifests
```bash
kubectl apply -f deployment.yaml    # Apply resources from file
kubectl delete -f deployment.yaml   # Delete resources from file
kubectl apply -k overlays/dev       # Apply using Kustomize
```

### 🔍 Debugging
```bash
kubectl describe pod <pod_name>     # Detailed pod info (events, errors)
kubectl logs -f <pod_name>          # Stream logs
kubectl top pod                     # Show pod resource usage (if metrics enabled)
kubectl get events --sort-by=.metadata.creationTimestamp   # Show recent events
```

---

## 🚀 Minikube Commands

### 🟢 Start & Stop Cluster
```bash
minikube start --driver=docker      # Start Minikube with Docker driver
minikube status                     # Show cluster status
minikube stop                       # Stop Minikube (keeps data)
minikube delete --all --purge       # Delete all Minikube clusters and configs
```

### 🌍 Networking & Dashboard
```bash
minikube service myapp              # Open exposed service in browser
minikube dashboard                  # Launch Kubernetes dashboard
minikube tunnel                     # Create tunnel for LoadBalancer services
```

### 📦 Add-ons
```bash
minikube addons list                # List available addons
minikube addons enable metrics-server   # Enable metrics server
minikube addons enable ingress          # Enable ingress controller
```

---

## ✅ Workflow Example (Docker → Kubernetes with Minikube)

1. Build Docker image:
   ```bash
   docker build -t myapp:latest .
   ```

2. Load image into Minikube:
   ```bash
   minikube image load myapp:latest
   ```

3. Apply Kubernetes manifests:
   ```bash
   kubectl apply -f k8s/deployment.yaml
   kubectl apply -f k8s/service.yaml
   ```

4. Check pods and services:
   ```bash
   kubectl get pods
   kubectl get svc
   ```

5. Open service in browser:
   ```bash
   minikube service myapp
   ```

---

📌 With these commands, you can:
- Build and run containers with Docker.  
- Deploy and manage workloads in Kubernetes with `kubectl`.  
- Control your local cluster using Minikube.  
