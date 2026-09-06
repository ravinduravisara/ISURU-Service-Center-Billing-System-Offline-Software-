# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ServiceStationBillingWeb/ServiceStationBillingWeb.csproj ServiceStationBillingWeb/
COPY ServiceStationBillingApp/Database.cs ServiceStationBillingApp/Database.cs
RUN dotnet restore ServiceStationBillingWeb/ServiceStationBillingWeb.csproj

COPY ServiceStationBillingWeb/ ServiceStationBillingWeb/
RUN dotnet publish ServiceStationBillingWeb/ServiceStationBillingWeb.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV BILLING_DB_PATH=/data/billing.db
EXPOSE 8080
VOLUME ["/data"]
ENTRYPOINT ["dotnet", "ServiceStationBillingWeb.dll"]