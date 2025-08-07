# Social Media API

A comprehensive Social Media REST API built with ASP.NET Core following Clean Architecture principles. This API provides full social media functionality including user authentication, posts, comments, reactions, following system, and real-time communication.

## 🚀 Features

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

## 🏗️ Architecture

The project follows Clean Architecture principles with the following layers:

```
src/
├── SocialMedia.WebApi/          # Presentation Layer
│   ├── Controllers/             # API Controllers
│   ├── Hubs/                   # SignalR Hubs for real-time communication
│   ├── Program.cs              # Application entry point
│   └── appsettings.json        # Configuration
├── SocialMedia.Application/     # Application Layer
│   ├── Services/               # Business logic services
│   ├── ServiceContracts/       # Service interfaces
│   ├── Dtos/                   # Data Transfer Objects
│   └── CustomValidations/      # Custom validation attributes
├── SocialMedia.Core/           # Domain Layer
│   ├── Entities/               # Domain entities
│   ├── Enumerations/           # Domain enums
│   └── RepositoryContracts/    # Repository interfaces
└── SocialMedia.Infrastructure/ # Infrastructure Layer
    ├── Database/               # Entity Framework DbContext
    ├── Repositories/           # Repository implementations
    ├── Auth/                   # JWT & Password services
    ├── Email/                  # Email services
    └── FileUploading/          # File upload services
```

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Real-time Communication**: SignalR
- **Email Service**: SMTP with Gmail
- **File Storage**: Server storage + Supabase
- **Architecture**: Clean Architecture
- **Patterns**: Repository Pattern, Unit of Work
- **Containerization**: Docker & Docker Compose

## 📋 Prerequisites

- Docker & Docker Compose
- Git

## 🐳 Installation & Deployment

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
     "EmailVerificationTokenExpiryMinutes": 30,
     "PasswordResetTokenExpiryMinutes": 30
   }
   ```

3. **Run with Docker Compose**

   ```bash
   docker-compose up -d
   ```

4. **Verify Services**
   - **API Service**: Available at `http://localhost:5001`
   - **SQL Server**: Available at `localhost:1234`
   - **Database**: Automatically created and migrated on startup

### Docker Configuration Details

- **API Port**: 5001 (external) → 5000 (internal)
- **Database Port**: 1234 (external) → 1433 (internal)
- **Database Credentials**: SA user with password `YourStrong@Passw0rd`
- **Persistent Volumes**:
  - Database data persisted in `db_data` volume
  - Uploaded files persisted in `api_wwwroot` volume
- **Environment**: Production environment by default
- **Auto-migration**: Database migrations run automatically on container startup

## 📊 Database Schema

### Core Entities

- **User**: User profiles with authentication data
- **Post**: User posts with content and metadata
- **Comment**: Comments on posts with nested replies support
- **PostReact**: Reactions on posts
- **CommentReact**: Reactions on comments
- **FollowerFollowing**: Many-to-many relationship for user connections
- **Avatar**: User profile pictures
- **PostAttachment**: File attachments for posts
- **Message**: Real-time chat messages
- **MessageStatus**: Message delivery status tracking
- **Group**: Chat groups for real-time communication
- **UserConnection**: Active SignalR connections for users

### Enumerations

- **ReactType**: Like, Love, Laugh, Angry, Sad
- **AttachmentType**: Image, Video, Document
- **StorageProvider**: Server, Supabase
- **MessageStatusType**: Sent, Delivered, Read
- **GroupType**: Private, Group chat types
