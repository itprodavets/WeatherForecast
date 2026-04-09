# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props nuget.config ./
COPY WeatherForecast.sln ./
COPY src/WeatherForecast.Domain/WeatherForecast.Domain.csproj src/WeatherForecast.Domain/
COPY src/WeatherForecast.Application/WeatherForecast.Application.csproj src/WeatherForecast.Application/
COPY src/WeatherForecast.Infrastructure/WeatherForecast.Infrastructure.csproj src/WeatherForecast.Infrastructure/
COPY src/WeatherForecast.Api/WeatherForecast.Api.csproj src/WeatherForecast.Api/

RUN dotnet restore src/WeatherForecast.Api/WeatherForecast.Api.csproj

COPY src/ src/
RUN dotnet publish src/WeatherForecast.Api/WeatherForecast.Api.csproj -c Release -o /app --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app .

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "WeatherForecast.Api.dll"]
