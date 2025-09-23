# Proyecto SISIE Taller II - Configuración

## Requisitos
- SQL Server Express 2022 (o versión compatible)
- Visual Studio 2022
- .NET 8 SDK

## Configuración de la base de datos

El proyecto utiliza **Entity Framework Core** y requiere una conexión a SQL Server.

1. Copiar el archivo `appsettings.json.example` y renombrarlo como `appsettings.json`.
2. Editar la cadena de conexión según tu servidor de SQL Server:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=TU_SERVIDOR;Database=proyecto_taller;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;"
}
