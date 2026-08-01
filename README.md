# AirportApp - Reserva y seleccion de asientos

Aplicacion web de operacion aeroportuaria desarrollada con ASP.NET Core MVC. Corresponde al Tipo 2 Reserva y seleccion de asientos y se integra con PayPal Sandbox y PayPhone como pasarelas de pago en ambiente de pruebas.

## Descripcion

El sistema permite al usuario autenticado buscar un vuelo en la base de datos de operacion aeroportuaria, consultar la aeronave asignada, ver los asientos disponibles por clase, crear una reserva temporal de 10 minutos, generar una orden de pago y completarla con una pasarela de pruebas. El servidor recalcula el precio en cada paso y registra pagos y transacciones en PostgreSQL.

## Funcionalidades

- Inicio y cierre de sesion con ASP.NET Core Identity.
- Roles Administrador y Cliente con rutas protegidas.
- Consulta de aeropuertos, aerolineas, aeronaves, pasajeros y vuelos.
- Busqueda por texto, filtros y ordenamiento en los listados.
- Paginacion fisica con Skip y Take desde PostgreSQL.
- Seleccion de asientos con bloqueo de asientos ocupados.
- Filtro de asientos por clase First Business y Economy.
- Reserva temporal de 10 minutos con vencimiento automatico.
- Creacion de ordenes con estado pendiente y detalle de la orden.
- Pago con PayPal Sandbox o link de pago PayPhone.
- Verificacion del pago en el backend y confirmacion del asiento.
- Historial de operaciones por usuario.
- Panel de administracion con KPIs y reportes por pasarela y por mes.
- Registro de pagos y bitacora de transacciones.

## Tecnologias

- ASP.NET Core MVC con .NET 10
- Entity Framework Core con Npgsql
- PostgreSQL
- ASP.NET Core Identity con autenticacion por cookies
- Consultas LINQ
- Paginacion fisica desde PostgreSQL
- PayPal Sandbox
- PayPhone ambiente de pruebas
- Bootstrap 5
- Razor Pages para el area de Identity

## Requisitos

- .NET SDK 10 o superior
- PostgreSQL 18
- Base de datos Flughafen aprobada por el docente
- Visual Studio 2022 o superior opcional

## Base de datos

Se utiliza la base de datos Flughafen de la operacion aeroportuaria. La base se instalo con la herramienta psql y las tablas originales se conservan en el esquema public. La aplicacion usa la base de datos `airportdb_examen` con el usuario `airportuser`.

Las tablas de Airport se leen con Database First y no se modifican. Las tablas propias de la aplicacion se crean con migraciones de Entity Framework Core.

## Configuracion

Las credenciales se almacenan fuera del repositorio con Secret Manager. Ejecutar en la carpeta del proyecto:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=airportdb_examen;Username=airportuser;Password=TU_CLAVE"
dotnet user-secrets set "PayPal:ClientId" "TU_CLIENT_ID_SANDBOX"
dotnet user-secrets set "PayPal:ClientSecret" "TU_CLIENT_SECRET_SANDBOX"
dotnet user-secrets set "PayPal:BaseUrl" "https://api-m.sandbox.paypal.com"
dotnet user-secrets set "PayPal:ReturnUrl" "http://localhost:5043/Reservas/PagoExitoso"
dotnet user-secrets set "PayPal:CancelUrl" "http://localhost:5043/Reservas/PagoCancelado"
dotnet user-secrets set "PayPhone:Token" "TU_TOKEN_PAYPHONE"
dotnet user-secrets set "PayPhone:StoreId" "TU_STORE_ID"
```

Tambien se puede copiar el archivo de ejemplo y completar los valores:

```bash
copy appsettings.Example.json appsettings.json
```

El archivo `appsettings.json` no se sube al repositorio. El repositorio solo contiene `appsettings.Example.json` sin credenciales reales.

## Migraciones

Las migraciones crean las tablas de Identity y las tablas propias. Aplicarlas con:

```bash
dotnet ef database update --context ApplicationDbContext
```

Las migraciones de la aplicacion son:

- InicialIdentity e IdentityInicial crean las tablas de Identity y los roles.
- TablasComercio crea Seats SeatReservations Orders OrderDetails Payments y TransactionHistory.
- CamposPasarelaPago agrega los campos de la pasarela a Payments.
- IndiceUnicoTransaccionPago agrega el indice unico sobre Gateway y ExternalTransactionId.

El contexto AirportContext lee la base de datos Flughafen con Database First y no requiere migraciones.

## Usuarios de prueba

| Rol | Correo | Contrasena |
|---|---|---|
| Administrador | admin@espe.edu.ec | Admin123* |
| Cliente | cliente@espe.edu.ec | Cliente123* |

Los usuarios que se registran reciben automaticamente el rol Cliente. El administrador y el cliente de prueba se crean al iniciar la aplicacion por primera vez con el seeder.

## Estructura del proyecto

```
AirportApp/
|-- AirportApp.slnx
|-- AirportApp.csproj
|-- Program.cs
|-- appsettings.Example.json
|-- Controllers/
|   |-- FlightsController.cs
|   |-- AirportsController.cs
|   |-- AirlinesController.cs
|   |-- AirplanesController.cs
|   |-- PassengersController.cs
|   |-- ReservasController.cs
|   `-- AdminController.cs
|-- Models/
|   |-- Airline.cs, Airplane.cs, Airport.cs, Flight.cs, Passenger.cs
|   |-- Resto de modelos de Airport generados con Database First
|   `-- Commerce/
|       |-- Order.cs
|       |-- OrderDetail.cs
|       |-- Payment.cs
|       |-- Seat.cs
|       |-- SeatReservation.cs
|       `-- TransactionHistory.cs
|-- Views/
|   |-- Home/
|   |-- Flights/
|   |-- Airports/
|   |-- Airlines/
|   |-- Airplanes/
|   |-- Passengers/
|   |-- Reservas/
|   |-- Admin/
|   `-- Shared/
|-- Areas/Identity/       Razor Pages de Identity
|-- Data/
|   |-- AirportContext.cs
|   |-- ApplicationDbContext.cs
|   |-- IdentitySeeder.cs
|   `-- MigrationsIdentity/
|-- Migrations/Identity/
|-- Services/
|   |-- SeatService.cs
|   `-- Payments/
|       |-- PayPalService.cs
|       |-- PayPhoneApiLinkService.cs
|       `-- PayPhoneLinkRequest.cs
|-- Settings/
|   |-- PayPalSettings.cs
|   `-- PayPhoneSettings.cs
|-- ViewModels/
|   |-- AsientosViewModel.cs
|   `-- PaginacionViewModel.cs
|-- SQL/
|   `-- generar_asientos.sql
|-- wwwroot/
`-- README.md
```

## Flujo de reserva y pago

1. El usuario inicia sesion.
2. Consulta los vuelos disponibles con filtros y paginacion.
3. Selecciona un vuelo y consulta la aeronave y los asientos.
4. Filtra por clase y selecciona un asiento libre.
5. Se crea una reserva temporal con vencimiento en 10 minutos.
6. El servidor vuelve a consultar el asiento y recalcula el precio.
7. Se crea la orden con estado Pendiente y se registran los detalles.
8. El usuario elige PayPal Sandbox o genera un link de pago PayPhone.
9. Se crea la solicitud de pago en la pasarela.
10. El usuario completa el pago en el ambiente de pruebas.
11. El backend verifica el resultado y captura el pago.
12. La orden pasa a Aprobado y el asiento queda confirmado.
13. Se registra la transaccion en TransactionHistory.
14. La operacion aparece en el historial del usuario.

Los asientos se generan con el servicio SeatService a partir de la capacidad de la aeronave. El script `SQL/generar_asientos.sql` permite precargar los asientos de un vuelo con la clase y el precio correspondientes.

## Estados de orden y pago

| Estado | Descripcion |
|---|---|
| Pendiente | Orden o pago creado y a la espera de confirmacion |
| Aprobado | Pago verificado y orden confirmada |
| Cancelado | Pago cancelado por el usuario |
| Rechazado | Pago rechazado por la pasarela |
| Fallido | Error al procesar el pago |

La tabla Payments tiene un indice unico sobre Gateway y ExternalTransactionId que impide registrar dos veces la misma transaccion.

## Tablas adicionales

| Tabla | Descripcion |
|---|---|
| Seats | Asientos por vuelo con clase y precio |
| SeatReservations | Reservas temporales del usuario |
| Orders | Ordenes de pago por usuario |
| OrderDetails | Detalle de cada orden |
| Payments | Pagos por pasarela con transaccion externa |
| TransactionHistory | Bitacora de operaciones |

## Ejecucion

```bash
dotnet restore
dotnet ef database update --context ApplicationDbContext
dotnet run
```

La aplicacion inicia en `http://localhost:5043`. El usuario de administracion se crea automaticamente en el primer arranque.

## Seguridad

- Credenciales fuera del repositorio con Secret Manager.
- `appsettings.json` excluido del repositorio con .gitignore.
- `appsettings.Example.json` con valores de ejemplo sin credenciales reales.
- Rutas protegidas con Authorize.
- Panel de administracion restringido al rol Administrador.
- Cada cliente consulta unicamente sus propias ordenes y pagos.
- Antiforgery token en los formularios POST.
- El precio se calcula siempre en el servidor y no se confia en el navegador.
- El asiento se confirma solo cuando el pago fue verificado como aprobado.
