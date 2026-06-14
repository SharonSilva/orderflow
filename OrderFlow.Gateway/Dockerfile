# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY OrderFlow.Gateway/OrderFlow.Gateway.csproj OrderFlow.Gateway/
COPY OrderFlow.ServiceDefaults/OrderFlow.ServiceDefaults.csproj OrderFlow.ServiceDefaults/
RUN dotnet restore OrderFlow.Gateway/OrderFlow.Gateway.csproj

COPY OrderFlow.Gateway/ OrderFlow.Gateway/
COPY OrderFlow.ServiceDefaults/ OrderFlow.ServiceDefaults/
RUN dotnet publish OrderFlow.Gateway/OrderFlow.Gateway.csproj -c Release -o /app/publish --no-restore

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "OrderFlow.Gateway.dll"]
