# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Inventory.Api/Inventory.Api.csproj Inventory.Api/
COPY OrderFlow.ServiceDefaults/OrderFlow.ServiceDefaults.csproj OrderFlow.ServiceDefaults/
COPY OrderFlow.Contracts/OrderFlow.Contracts.csproj OrderFlow.Contracts/
RUN dotnet restore Inventory.Api/Inventory.Api.csproj

COPY Inventory.Api/ Inventory.Api/
COPY OrderFlow.ServiceDefaults/ OrderFlow.ServiceDefaults/
COPY OrderFlow.Contracts/ OrderFlow.Contracts/
RUN dotnet publish Inventory.Api/Inventory.Api.csproj -c Release -o /app/publish --no-restore

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Inventory.Api.dll"]
