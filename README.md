# Task Management System

This project is a Task Management system built with the following technologies:

- [**.NET 8**](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- **Microservice Architecture**
- [**Microsoft SQL Server**](https://www.microsoft.com/en-us/sql-server)
- [**gRPC & Protobuf**](https://grpc.io/)
- [**CQRS & MediatR**](https://github.com/jbogard/MediatR)
- [**MassTransit & RabbitMQ**](https://masstransit.io/)
- [**Gmail API**](https://developers.google.com/gmail/api)
- [**React & Typescript**](https://react.dev/)
- [**Ant Design**](https://ant.design/)
- [**Docker & Kubernetes**](https://kubernetes.io/) - Container orchestration

## 🚀 Deployment Options

### Docker Compose (Development)

```bash
docker-compose up -d
```

### Kubernetes (Production)

```bash
# Quick deployment
.\k8s\deploy.ps1

# Or manual deployment
kubectl apply -f k8s/
```

For detailed Kubernetes setup instructions, see [k8s/README.md](k8s/README.md).
