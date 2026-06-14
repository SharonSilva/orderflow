# Stage 1: build 
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first, restore (cached layer)
COPY Orders.Api/Orders.Api.csproj Orders.Api/
COPY OrderFlow.ServiceDefaults/OrderFlow.ServiceDefaults.csproj OrderFlow.ServiceDefaults/
COPY OrderFlow.Contracts/OrderFlow.Contracts.csproj OrderFlow.Contracts/
RUN dotnet restore Orders.Api/Orders.Api.csproj

# Copy source code and publish
COPY Orders.Api/ Orders.Api/
COPY OrderFlow.ServiceDefaults/ OrderFlow.ServiceDefaults/
COPY OrderFlow.Contracts/ OrderFlow.Contracts/
RUN dotnet publish Orders.Api/Orders.Api.csproj -c Release -o /app/publish --no-restore

#Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime 
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Orders.Api.dll"]