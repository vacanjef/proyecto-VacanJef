# Etapa 1: compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY VacanJefWeb.csproj .
RUN dotnet restore VacanJefWeb.csproj
COPY . .
RUN dotnet publish VacanJefWeb.csproj -c Release -o /app/publish --no-restore

# Etapa 2: solo el runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "VacanJefWeb.dll"]
