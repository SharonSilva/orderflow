# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Payments.Api/Payments.Api.csproj Payments.Api/
COPY OrderFlow.ServiceDefaults/OrderFlow.ServiceDefaults.csproj OrderFlow.ServiceDefaults/
COPY OrderFlow.Contracts/OrderFlow.Contracts.csproj OrderFlow.Contracts/
RUN dotnet restore Payments.Api/Payments.Api.csproj

COPY Payments.Api/ Payments.Api/
COPY OrderFlow.ServiceDefaults/ OrderFlow.ServiceDefaults/
COPY OrderFlow.Contracts/ OrderFlow.Contracts/
RUN dotnet publish Payments.Api/Payments.Api.csproj -c Release -o /app/publish --no-restore

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Payments.Api.dll"]
