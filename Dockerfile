FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY Directory.Packages.props .
COPY Directory.Build.props .
COPY ./src/SocialMedia.WebApi/*.csproj ./src/SocialMedia.WebApi/
COPY ./src/SocialMedia.Application/*.csproj ./src/SocialMedia.Application/
COPY ./src/SocialMedia.Core/*.csproj ./src/SocialMedia.Core/
COPY ./src/SocialMedia.Infrastructure/*.csproj ./src/SocialMedia.Infrastructure/


RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore ./src/SocialMedia.WebApi/SocialMedia.WebApi.csproj

COPY . .

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 5000
ENTRYPOINT ["dotnet", "SocialMedia.WebApi.dll"]
