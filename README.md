# 🏦 TransferHub - Money Transfer Microservices

![CI](https://github.com/YOUR_USERNAME/YOUR_REPO/actions/workflows/ci.yml/badge.svg)

A money transfer system built with .NET 9.0, Clean Architecture, and microservices patterns.

## 🚀 Quick Start

### Single Command Deployment

```bash
docker-compose up -d
```

**That's it!** Full stack with 6 services running.

### Access Points

- **BFF API:** http://localhost:5002/swagger
- **Account API:** http://localhost:5000/swagger
- **MoneyTransfer API:** http://localhost:5001/swagger
- **RabbitMQ Management:** http://localhost:15672 (rabbitmq/rabbitmq123)

---

## ✨ Key Features

### 🏗️ Architecture
- ✅ **Clean Architecture** (Domain, Application, Infrastructure, API)
- ✅ **CQRS** pattern with MediatR
- ✅ **Saga** orchestration with MassTransit
- ✅ **Repository** pattern with Unit of Work
- ✅ **Domain-Driven Design** (Aggregates, Value Objects, Domain Events)

### 🔐 Security
- ✅ **JWT Authentication** on all endpoints
- ✅ **Input Validation** with FluentValidation
- ✅ **Sensitive Data Hashing** in logs

### 🔄 Reliability
- ✅ **Idempotency** (Redis-backed, prevents duplicate transfers)
- ✅ **Balance Control** (saga orchestration, prevents insufficient funds)
- ✅ **Transactional Outbox** (MassTransit, reliable message delivery)

### 📊 Observability
- ✅ **Distributed Tracing** (OpenTelemetry + Jaeger)
- ✅ **Metrics** (Prometheus + Grafana)
- ✅ **Structured Logging** (Serilog + Seq)
- ✅ **Correlation ID** (end-to-end request tracking)

### 🎯 API Features
- ✅ **API Versioning** (v1.0)
- ✅ **Swagger/OpenAPI** documentation
- ✅ **Health Checks** (ready for k8s)
- ✅ **CORS** configured
- ✅ **SignalR** real-time updates (not fully implemented)

### 🚀 CI/CD
- ✅ **GitHub Actions** - Automated build and test pipeline
- ✅ **Automated Testing** - Unit and integration tests on every push
- ✅ **Code Coverage** - Coverage reports generated automatically
- ✅ **Docker Validation** - Multi-stage builds verified
- ✅ **Multi-branch Support** - Runs on main and develop branches


## 🔧 Development

### Local Development (without Docker)

```bash
# Start infrastructure only
docker-compose up -d postgres redis rabbitmq

# Update appsettings.Development.json connection strings
# Run APIs locally
cd src/Services/Account/Account.API && dotnet run
cd src/Services/MoneyTransfer/MoneyTransfer.API && dotnet run
cd src/Services/BFF/BFF.API && dotnet run
```

### Build

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test
```

### CI/CD Pipeline

The project includes a complete CI/CD pipeline using **GitHub Actions**:

**Automated on every push/PR:**
- 🔨 Build all .NET services
- ✅ Run all unit and integration tests
- 📊 Generate code coverage reports
- 🎨 Build and validate Frontend (Next.js)
- 🐳 Validate Docker images (on main branch)

**View pipeline status:**
- Go to **Actions** tab in GitHub
- See detailed build logs and test results
- Download coverage reports as artifacts

**Documentation:**
- [`docs/CI_CD_PIPELINE.md`](docs/CI_CD_PIPELINE.md) - Complete technical guide
- [`docs/CI_CD_QUICKSTART.md`](docs/CI_CD_QUICKSTART.md) - Quick start guide

---

## 🎯 Technical Stack

### Backend
- **.NET 9.0** - Latest LTS
- **ASP.NET Core** - Web API framework
- **Entity Framework Core 9.0** - ORM
- **PostgreSQL** - Primary database
- **Redis** - Caching & idempotency
- **RabbitMQ** - Message broker

### Patterns & Libraries
- **MediatR** - CQRS implementation
- **MassTransit** - Saga orchestration & messaging
- **FluentValidation** - Input validation
- **YARP** - Reverse proxy (BFF)

### Observability
- **Serilog** - Structured logging
- **OpenTelemetry** - Distributed tracing
- **Prometheus** - Metrics
- **Seq** - Log aggregation (optional)
- **Jaeger** - Trace visualization (optional)

---

## 📊 Observability Stack (Optional)

For full observability with Jaeger, Prometheus, Grafana, and Seq:

```bash
docker-compose -f docker-compose.observability.yml up -d
```

**Access:**
- Jaeger: http://localhost:16686
- Prometheus: http://localhost:9090
- Grafana: http://localhost:3000 (admin/admin)
- Seq: http://localhost:5341

---

## 🔑 Default Credentials

### Services
- **PostgreSQL:** postgres / postgres123
- **Redis:** redis123
- **RabbitMQ:** rabbitmq / rabbitmq123

### API Authentication
- **JWT Secret:** (configured in environment variables)
- **Test User:** test@example.com / test123

---

## 📈 Production Ready

### Features
✅ Clean Architecture  
✅ CQRS & Event Sourcing  
✅ Saga Pattern  
✅ Idempotency  
✅ Distributed Tracing  
✅ Health Checks  
✅ API Versioning  
✅ Comprehensive Logging  
✅ Message Deduplication  
✅ Transactional Outbox  
✅ Balance Control  
✅ Error Handling  
✅ Input Validation  
✅ CI/CD Pipeline (GitHub Actions)

### Performance
- Async/await throughout
- CancellationToken support
- Database connection pooling
- Redis caching
- Batch message processing

### Reliability
- Saga orchestration for distributed transactions
- Idempotency for critical endpoints
- Retry policies
- Health checks
- Automated testing on every commit

## 🎉 Summary

**TransferHub** is a money transfer system featuring:

- ✅ Microservices architecture
- ✅ Clean Architecture principles
- ✅ CQRS and Saga patterns
- ✅ Full observability
- ✅ Docker containerization
- ✅ Idempotency & reliability
- ✅ Comprehensive documentation
- ✅ CI/CD with GitHub Actions

**Start now:**
```bash
docker-compose up -d
```

**Status:** ✅ Build successful | ✅ 0 Errors | 🚀 Production Ready

---