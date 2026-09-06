# ISURU-Service-Center-Billing-System-Offline-Software-

## Deploy the web application with Docker

The browser version is in `ServiceStationBillingWeb/` and uses the existing SQLite
schema. Build and run it from the repository root:

```sh
docker build -t service-station-web .
docker run --rm -p 8080:8080 -v service-station-data:/data service-station-web
```

Open `http://localhost:8080`. The database is stored at `/data/billing.db`; mount a
persistent disk at `/data` in production. Render should use the Dockerfile in this
repository and a persistent disk mounted at `/data`.

The original WinForms application remains in `ServiceStationBillingApp/` while the
web migration is completed.
"# ISURU-Service-Center-Billing-System-Offline-Software-" 
