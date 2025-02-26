# TGB.AccountBE

The backend of The Gifted Battlefield's SSO system.

## How to run?

### Requirements:

- Docker (and Docker Compose)
- .NET SDK (v8.0)
- dotnet-ef tool

### Steps:

1. Copy the `./TGB.AccountBE.API/appsettings.Example.json` to `./TGB.AccountBE.API/appsettings.json`
   and modify it to match desired settings.
2. Config the environment files inside the `./Environments` folder.
3. Copy the X.509 OpenIddict certificate to the `./Secrets/cert.pfx` file.
4. Migrate the database if needed (scroll to the [Migrations](#migrations) section for more
   information).
5. Run with Docker Compose:

```shell
docker compose up -d
```

## Migrations

```shell
dotnet ef migrations script --idempotent --project TGB.AccountBE.API -o ./InitDB/init.sql
```

## Documentations

TBD as we're in alpha stage.

## License:

TGB.AccountBE is licensed under the AGPLv3 license. Copyright &copy; 2025 The Gifted Battlefield IID
team.
