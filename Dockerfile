FROM mcr.microsoft.com/dotnet/aspnet:7.0-bullseye-slim AS base
WORKDIR /app

COPY _#{Build.Repository.Name}#/Artifact-#{Build.Repository.Name}#  .

ENTRYPOINT ["dotnet", "OrderInvoice.dll"]