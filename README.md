# Exam Submission Platform

A comprehensive microservices-based exam submission platform built with .NET 9, featuring API Gateway, Authentication, Exam Management, Submission Processing, and Notification services.

## 🏗️ Architecture Overview

- **API Gateway**: Entry point with reverse proxy, rate limiting, and authentication
- **Auth Service**: User authentication and JWT token management
- **Exam Service**: Exam creation, management, and scheduling
- **Submission Service**: Exam submission processing and validation
- **Notification Service**: Event-driven notifications and messaging
- **Common Infrastructure**: Shared libraries for resilience, caching, messaging, and data access

## 📋 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [SQL Server 2022](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or use Docker container)
- [Redis](https://redis.io/download) (or use Docker container)
- [RabbitMQ](https://www.rabbitmq.com/download.html) (or use Docker container)

## 🚀 Quick Start

### Docker Compose

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd ExamSubmissionPlatform
   ```

2. **Start all services**
   ```bash
   docker-compose up --build
   ```

3. **Start in background**
   ```bash
   docker-compose up -d --build
   ```

4. **View logs**
   ```bash
   docker-compose logs -f
   ```

5. **Stop services**
   ```bash
   docker-compose down
   ```

6. **Stop and remove data**
   ```bash
   docker-compose down -v
   ```

## 🔗 Service Endpoints

| Service | Port | Swagger UI | Health Check |
|---------|------|------------|--------------|
| API Gateway | 8080 | http://localhost:8080/swagger | http://localhost:8080/health |
| Auth Service | 8001 | http://localhost:8001/swagger | http://localhost:8001/health |
| Exam Service | 8002 | http://localhost:8002/swagger | http://localhost:8002/health |
| Submission Service | 8003 | http://localhost:8003/swagger | http://localhost:8003/health |
| Notification Service | 8004 | http://localhost:8004/swagger | http://localhost:8004/health |

### External Services
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **MailHog UI**: http://localhost:8025
- **Redis**: localhost:6379
- **SQL Server**: localhost:1433

## 🧪 Running Tests

### Unit Tests

```bash
# Run all unit tests
dotnet test

# Run specific service tests
dotnet test tests/AuthService.Tests/
dotnet test tests/ExamService.Tests/
dotnet test tests/SubmissionService.Tests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Integration Tests

```bash
# Run integration tests (requires Docker)
dotnet test tests/IntegrationTests/

# Run with verbose output
dotnet test tests/IntegrationTests/ -v normal

# Run specific test
dotnet test tests/IntegrationTests/ --filter "CompleteUserJourney"
```

**Note**: Integration tests use Testcontainers and require Docker to be running.

### Test Structure

- **Unit Tests**: Fast, isolated tests for individual components
    - Domain logic testing
    - Use case testing
    - Controller testing with mocks
    - Service testing

- **Integration Tests**: End-to-end testing with real infrastructure
    - Complete user journeys
    - Database integration
    - Message broker integration
    - Service-to-service communication

## 📖 API Usage Examples

### 1. Register a User
```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "student@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "password": "SecurePass123!"
  }'
```

### 2. Login
```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "student@example.com",
    "password": "SecurePass123!"
  }'
```

### 3. Create an Exam
```bash
curl -X POST http://localhost:8080/api/exam \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "title": "Mathematics Final Exam",
    "description": "Final examination for Mathematics course",
    "startTime": "2024-06-15T09:00:00Z",
    "endTime": "2024-06-15T12:00:00Z",
    "timeZone": "UTC",
    "durationMinutes": 180,
    "maxAttempts": 2
  }'
```

### 4. Submit an Exam
```bash
curl -X POST http://localhost:8080/api/submission/submit \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "examId": "YOUR_EXAM_ID",
    "content": "My exam answers...",
    "startedAt": "2024-06-15T09:00:00Z"
  }'
```

## 🗂️ Project Structure

```
ExamSubmissionPlatform/
├── src/
│   ├── ApiGateway/
│   │   └── ApiGateway.Api/
│   ├── Services/
│   │   ├── AuthService/
│   │   │   ├── AuthService.Api/
│   │   │   ├── AuthService.Application/
│   │   │   ├── AuthService.Domain/
│   │   │   └── AuthService.Infrastructure/
│   │   ├── ExamService/
│   │   ├── SubmissionService/
│   │   └── NotificationService/
│   └── Shared/
│       ├── Common.Application/
│       ├── Common.Contracts/
│       ├── Common.Domain/
│       └── Common.Infrastructure/
├── tests/
│   ├── Unit/
│   │   ├── AuthService.Tests/
│   │   ├── ExamService.Tests/
│   │   └── SubmissionService.Tests/
│   └── Integration/
│       └── IntegrationTests/
├── docker-compose.yml
├── docker-compose.override.yml
└── README.md
```

## 🔧 Configuration

### Environment Variables

Key environment variables for Docker deployment:

```env
# Database
CONNECTIONSTRINGS__DEFAULTCONNECTION=Server=sql-server;Database=ExamDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;

# Message Broker
CONNECTIONSTRINGS__MESSAGEBROKER=amqp://guest:guest@rabbitmq:5672/

# Redis Cache
CONNECTIONSTRINGS__REDIS=redis:6379

# JWT Settings
JWTSETTINGS__SECRETKEY=your-secret-key
JWTSETTINGS__ISSUER=ExamPlatform
JWTSETTINGS__AUDIENCE=ExamPlatform
```

### Development Settings

Each service has its own `appsettings.json` and `appsettings.Development.json` for local development configuration.

## 🐛 Troubleshooting

### Common Issues

1. **Services not starting**
    - Ensure Docker is running
    - Check port conflicts
    - Verify connection strings

2. **Database connection issues**
    - Wait for SQL Server container to be ready
    - Check firewall settings
    - Verify credentials

3. **Authentication failures**
    - Ensure JWT secrets match across services
    - Check token expiration
    - Verify issuer/audience settings

4. **Message broker issues**
    - Ensure RabbitMQ is running
    - Check queue declarations
    - Verify connection strings

### Logs

```bash
# View all service logs
docker-compose logs

# View specific service logs
docker-compose logs auth-service
docker-compose logs exam-service

# Follow logs in real-time
docker-compose logs -f
```

## 🚧 Areas for Improvement

### Security & Authentication
- [ ] **Implement proper OAuth2/OpenID Connect provider** (Duende IdentityServer, Auth0, or Azure AD)
- [ ] **Certificate-based JWT signing** instead of shared secret keys
- [ ] **API rate limiting per user/IP** beyond basic gateway limits
- [ ] **Input validation and sanitization** at API boundaries
- [ ] **Implement proper RBAC** (Role-Based Access Control) with granular permissions
- [ ] **Add API versioning strategy** for backward compatibility
- [ ] **Implement CSRF protection** for web clients
- [ ] **Add security headers** (HSTS, CSP, X-Frame-Options)

### Testing & Quality
- [ ] **Increase unit test coverage** to >90% across all services
- [ ] **Implement contract testing** (Pact) between services
- [ ] **Add performance/load testing** with tools like NBomber
- [ ] **Implement chaos engineering** for resilience testing

### Architecture & Infrastructure
- [ ] **Separate repositories per microservice** for true independence
- [ ] **Implement proper service mesh** (Istio, Linkerd) for production
- [ ] **Add distributed tracing** (OpenTelemetry, Jaeger)
- [ ] **Implement proper monitoring** (Prometheus, Grafana)
- [ ] **Add structured logging** with correlation IDs across services (correlation Ids already assigned)
- [ ] **Event sourcing implementation** for audit trails
- [ ] **CQRS pattern enhancement** with separate read/write databases
- [ ] **Add backup and disaster recovery** strategies

### DevOps & Deployment
- [ ] **Kubernetes deployment manifests** with Helm charts
- [ ] **CI/CD pipelines** with GitHub Actions/Azure DevOps
- [ ] **Infrastructure as Code** (Terraform, ARM templates)
- [ ] **Automated database migrations** in deployment pipeline
- [ ] **Secret management** (Azure Key Vault, HashiCorp Vault)
- [ ] **Multi-environment configuration** (dev, staging, prod)

### Performance & Scalability
- [ ] **Database optimization** and indexing strategies
- [ ] **Implement read replicas** for query performance
- [ ] **Implement response caching** strategies
- [ ] **Database sharding** for large-scale deployments
- [ ] **Optimize Docker images** (multi-stage builds, Alpine Linux)
- [ ] **Memory and CPU profiling** optimization

### User Experience
- [ ] **Real-time notifications** via SignalR/WebSockets
- [ ] **Progressive Web App** features
- [ ] **Internationalization** (i18n) support

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

**Note**: This is a demonstration project showcasing microservices architecture patterns. For production use, implement the security and scalability improvements listed above.