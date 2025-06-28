# Social Media API

A comprehensive Social Media REST API built with ASP.NET Core following Clean Architecture principles. This API provides full social media functionality including user authentication, posts, comments, reactions, and follow/following system.

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

## 🏗️ Architecture

The project follows Clean Architecture principles with the following layers:

```
src/
├── SocialMedia.WebApi/          # Presentation Layer
│   ├── Controllers/             # API Controllers
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
- **Email Service**: SMTP with Gmail
- **File Storage**: Server storage + Supabase
- **Architecture**: Clean Architecture
- **Patterns**: Repository Pattern, Unit of Work

## 📋 Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code
- Git

## ⚙️ Installation & Setup

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd SocialMedia
   ```

2. **Update Connection String**

   Update the connection string in `src/SocialMedia.WebApi/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "sqlServerConnectionString": "Your_SQL_Server_Connection_String"
     }
   }
   ```

3. **Configure Email Settings**

   Update email configuration in `appsettings.json`:

   ```json
   {
     "email": {
       "Smtp": "smtp.gmail.com",
       "Port": 587,
       "Username": "your-email@gmail.com",
       "Password": "your-app-password",
       "From": "your-email@gmail.com"
     }
   }
   ```

4. **Configure JWT Settings**

   Update JWT configuration:

   ```json
   {
     "jwt": {
       "Issuer": "issuer-url",
       "Audience": "audience-url",
       "Lifetime": 15,
       "RefreshTokenLifetime": 120,
       "SigningKey": "your-secret-signing-key"
     }
   }
   ```

5. **Run Database Migrations**

   ```bash
   cd src/SocialMedia.WebApi
   dotnet ef database update
   ```

6. **Build and Run**
   ```bash
   dotnet build
   dotnet run
   ```

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

### Enumerations

- **ReactType**: Like, Love, Laugh, Angry, Sad
- **AttachmentType**: Image, Video, Document
- **StorageProvider**: Server, Supabase
