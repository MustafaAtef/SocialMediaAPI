# Social Media API

A comprehensive Social Media REST API built with ASP.NET Core following Clean Architecture, CQRS, and MediatR, separating read and write models using table projections populated through domain events, the Outbox pattern, and background services. This API provides full social media functionality including user authentication, posts, comments, reactions, following system, and real-time communication.

## Features

### Authentication & Authorization

- User registration and login
- JWT-based authentication
- Email verification system
- Password reset functionality
- Refresh token support

### User Management

- User profiles with avatars
- Follow/unfollow system
- Follower and following lists
- User posts management

### Posts

- Create, update, and view posts
- File attachments support (images, videos)
- Post reactions (like, love, laugh, angry, sad)
- Comments and replies system

### Comments

- Nested comments (one level of replies)
- Comment reactions
- Edit and manage comments
- Paginated comment loading

### File Storage

- Multiple storage providers support
- Server-based file storage
- Supabase integration for cloud storage

### Real-time Communication

- SignalR hub for instant messaging
- Real-time message delivery status updates
- Connection management for online/offline status
- Message status tracking (sent, delivered, read)

## Architecture

The project follows Clean Architecture principles and uses physical CQRS with a clear separation between write models (EF Core) and read models (Dapper projections). Write operations are handled via commands, while read operations use optimized projection tables. Domain events, the Outbox pattern, and background services coordinate consistent side effects and populate read models.

Project layout:

```
src/
├── SocialMedia.WebApi/          # Presentation Layer
│   ├── Controllers/             # API Controllers
│   ├── Hubs/                    # SignalR Hubs for real-time communication
│   ├── Middlewares/             # Request pipeline and logging
│   ├── Filters/                 # API filters
│   ├── Program.cs               # Application entry point
│   └── appsettings.json         # Configuration
├── SocialMedia.Application/     # Application Layer
│   ├── Abstractions/            # Cross-cutting contracts
│   ├── Auth/                    # Auth commands, queries, and responses
│   ├── Comments/                # Comment commands, queries, and responses
│   ├── Posts/                   # Post commands, queries, and responses
│   ├── Reacts/                  # React commands, queries, and responses
│   ├── Users/                   # User commands, queries, and responses
│   ├── Behaviors/               # MediatR pipeline behaviors
│   ├── Dtos/                    # Cross-layer DTOs
│   └── Options/                 # Options and configuration models
├── SocialMedia.Core/            # Domain Layer
│   ├── Entities/                # Domain entities
│   ├── Enumerations/            # Domain enums
│   ├── Events/                  # Domain events
│   └── RepositoryContracts/     # Repository interfaces
└── SocialMedia.Infrastructure/  # Infrastructure Layer
  ├── Database/                # Entity Framework DbContext
  ├── Data/                    # Dapper read models and projections
  ├── Repositories/            # Repository implementations
  ├── Auth/                    # JWT & Password services
  ├── Email/                   # Email services
  ├── FileUploading/           # File upload services
  └── Outbox/                  # Outbox processing

tests/
├── SocialMedia.Application.UnitTests/
│   ├── Auth/
│   ├── Posts/
│   ├── Reacts/
│   ├── Users/
│   └── Comments/
└── SocialMedia.IntegrationTests/
  ├── Auth/
  ├── Posts/
  ├── Reacts/
  ├── Users/
  └── Comments/

Docker/
├── Dockerfile.dev
├── Dockerfile.prod
└── Dockerfile.test

docker-compose/
└── dev.yaml

```

## Technology Stack

- **Framework**: ASP.NET Core 10.0
- **Database**: SQL Server with Entity Framework Core
- **Read Models**: Dapper
- **Authentication**: JWT Bearer tokens
- **Real-time Communication**: SignalR
- **Email Service**: SMTP with Gmail
- **File Storage**: Server storage + Supabase
- **Mediation & Validation**: MediatR + FluentValidation
- **Observability**: Serilog + Seq
- **Architecture**: Clean Architecture
- **Patterns**: CQRS, Repository Pattern, Outbox, Domain Events
- **Package Management**: Central Package Management (Directory.Packages.props)
- **Containerization**: Docker & Docker Compose

## Prerequisites

- Docker & Docker Compose
- Git

## Installation & Deployment

The application is designed to run using Docker Compose, which provides a complete containerized environment with all necessary services.

### Quick Start

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd SocialMedia
   ```

2. **Configure Application Settings**

   Update the configuration in `src/SocialMedia.WebApi/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "sqlServerConnectionString": "Data Source=sqlserver-db;Initial Catalog=SocialMedia;User ID=sa;Password=YourStrong@Passw0rd;Connect Timeout=30;Trust Server Certificate=True;Authentication=SqlPassword;"
     },
     "jwt": {
       "Issuer": "SocialMedia.WebApi",
       "Audience": "SocialMedia.Frontend",
       "Lifetime": 60,
       "RefreshTokenLifetime": 120,
       "SigningKey": "your-secret-signing-key-min-32-characters-long"
     },
     "email": {
       "Smtp": "smtp.gmail.com",
       "Port": 587,
       "Username": "your-email@gmail.com",
       "Password": "your-app-password",
       "From": "your-email@gmail.com",
       "EmailVerificationUrl": "https://localhost:5001/api/auth/verify-email?token=",
       "PasswordResetUrl": "https://localhost:5001/api/auth/reset-password?token="
     },
     "supabase": {
       "Url": "https://your-supabase-url.supabase.co",
       "Key": "your-supabase-anon-key"
     },
     "durations": {
       "EmailVerificationTokenExpiryMinutes": 30,
       "PasswordResetTokenExpiryMinutes": 30
     },
     "FileUpload": {
       "Provider": "Server"
     }
   }
   ```

3. **Run with Docker Compose**

   ```bash
   docker compose -f docker-compose/dev.yaml up -d
   ```

4. **Verify Services**

- **API Service**: Available at `http://localhost:5039`
- **SQL Server**: Available at `localhost:1234`
- **Seq UI**: Available at `http://localhost:8081`
- **Database**: Automatically created and migrated on startup

### Docker Configuration Details

- **API Port**: 5039 (external) → 5000 (internal)
- **Database Port**: 1234 (external) → 1433 (internal)
- **Seq UI Port**: 8081 (external) → 80 (internal)
- **Seq Ingestion Port**: 5341 (external) → 5341 (internal)
- **Database Credentials**: SA user with password `YourStrong@Passw0rd`
- **Persistent Volumes**:
  - Database data persisted in `db_data` volume
  - Uploaded files persisted in `api_wwwroot` volume
- **Auto-migration**: Database migrations run automatically on container startup

## Testing & CI

- **Tests**: Unit and integration coverage across the codebase, with integration tests using real dependencies spun up via Testcontainers.
- **CI/CD**: GitHub Actions pipeline runs unit and integration tests and publishes results.
- **Deploy**: Automated Docker Hub image build and push after successful pipeline runs.
- **Containerization**: Docker + Docker Compose with environment-specific Dockerfiles for development and production.
