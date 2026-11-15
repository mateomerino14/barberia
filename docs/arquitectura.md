# Arquitectura

Un solo repositorio con dos aplicaciones: la API en .NET 10 con arquitectura hexagonal y la web en SvelteKit con Atomic Design. La base de datos es PostgreSQL: en desarrollo corre en Docker y en producción en Supabase, usado solo como PostgreSQL.

```
barberia/
├── backend/            API .NET 10 (hexagonal)
├── frontend/           Web SvelteKit (Atomic Design + features)
├── docs/               Sistema, arquitectura y diseño
├── .github/workflows/  CI
├── docker-compose.yml  PostgreSQL local
└── reglas.md           Convenciones del proyecto
```

## Despliegue

| Pieza | Producción | Desarrollo |
|---|---|---|
| Web | Vercel | `npm run dev` |
| API | Render (Docker) | `dotnet run` |
| Base de datos | Supabase (solo PostgreSQL) | `docker compose up` |

La API se conecta a la base con una cadena de conexión normal. Cambiar de proveedor es cambiar esa variable: el código no usa la autenticación ni la API automática de Supabase.

## Backend: puertos y adaptadores

```
backend/
├── src/
│   ├── Barberia.Domain/          Núcleo: entidades, value objects, reglas y eventos
│   ├── Barberia.Application/     Casos de uso y puertos (interfaces que el núcleo necesita)
│   ├── Barberia.Infrastructure/  Adaptadores de salida: PostgreSQL, correo, seguridad, reloj
│   └── Barberia.Api/             Adaptador de entrada HTTP y raíz de composición
└── tests/
    ├── Barberia.Domain.Tests/        Reglas puras
    ├── Barberia.Application.Tests/   Casos de uso con puertos falsos
    ├── Barberia.Api.Tests/           Rutas HTTP de punta a punta con PostgreSQL real
    └── Barberia.Architecture.Tests/  Dependencias entre capas y reglas de estilo
```

### Regla de dependencias

```
Api ──► Infrastructure ──► Application ──► Domain
 └──────────────────────────────┘
```

- **Domain** no depende de nada: ni de EF Core, ni de ASP.NET, ni de otra capa.
- **Application** depende solo de Domain. Define los puertos (`IBookingRepository`, `INotifier`, `IClock`…) que los adaptadores implementan.
- **Infrastructure** implementa los puertos. No conoce la API.
- **Api** traduce HTTP a casos de uso y arma el contenedor de dependencias. No usa la base de datos directamente.

`Barberia.Architecture.Tests` verifica estas reglas en cada compilación del CI: si una capa de adentro empieza a depender de una de afuera, las pruebas fallan.

### Organización por módulo

Dentro de cada capa el código se agrupa por módulo del sistema, no por tipo de archivo:

```
Barberia.Domain/
├── Common/       Entity, Result, Error, BusinessTime, DailyHours, EmailAddress, Text, Limits
├── Access/       StaffAccount, StaffRole
├── Shops/        Shop, BookingRules, ClosedDay, ShopPalette
├── Catalog/      Service, Money
├── Staff/        Barber, TimeOff
├── Scheduling/   Booking, Customer, Block, TimeRange, AvailabilityCalculator
└── Media/        StoredImage

Barberia.Application/
├── Ports/        Repositorios, IUnitOfWork, IClock, INotifier, IPasswordHasher, ITokenIssuer
└── Features/
    ├── Access/      Login, CheckSession, Passwords (cambiar la propia, acceso de barberos)
    ├── Shops/       InitializeShop, GetShop, UpdateShop, SetOpeningHours, ClosedDays, SeedDemo
    ├── Catalog/     ListServices, SaveService
    ├── Staff/       ListBarbers, SaveBarber, SetBarberSchedule, TimeOffChanges
    ├── Bookings/    GetAvailability, CreateBooking, ManageBooking, SendReminders
    ├── Agenda/      GetAgenda, StaffBookings, Blocks
    └── Media/       Images (subir, ver, logo, fotos del local y del barbero)

Barberia.Infrastructure/
├── Persistence/    EF Core, configuraciones, repositorios y migraciones
├── Notifications/  Correo con Brevo y tarea de fondo de recordatorios
├── Security/       Contraseñas (PBKDF2) y pases JWT
└── Time/           Reloj del sistema
```

### Patrones

| Patrón | Dónde | Para qué |
|---|---|---|
| Puertos y adaptadores | Application/Ports ↔ Infrastructure | El núcleo no sabe cómo se guarda ni cómo se avisa |
| Repository | Puertos de persistencia | Guardar y leer agregados sin exponer EF Core |
| Unit of Work | `IUnitOfWork` | Confirmar un caso de uso en una sola transacción |
| Value Object | `TimeRange`, `Money`, `DailyHours`, `EmailAddress`, `Customer` | Datos que se validan al crearse y nunca existen inválidos |
| Agregado | `Booking`, `Barber`, `Shop` | Cada uno cuida sus propias reglas |
| Servicio de dominio | `AvailabilityCalculator` | Las reglas de horas libres (anticipación, choque, horario) en un solo lugar puro y probado |
| Strategy | `INotifier` | Sumar un canal de aviso sin tocar la agenda: `BookingNotifications` avisa por todos los registrados |
| Command / Query | Un manejador por caso de uso | Cada acción en un archivo, fácil de probar |
| Result | Errores de negocio | "Hora ocupada" es un resultado, no una excepción |
| Tarea de fondo | `ReminderWorker` | Los recordatorios salen solos, fuera de las peticiones |

No se usan MediatR ni AutoMapper: desde 2025 tienen licencia comercial, y para este tamaño los manejadores y mapeos propios son más claros.

### SOLID

- **Responsabilidad única:** un caso de uso por manejador; una regla por política.
- **Abierto/cerrado:** un canal de aviso nuevo es una clase nueva que implementa `INotifier`.
- **Sustitución de Liskov:** cualquier adaptador de un puerto cumple el mismo contrato; las pruebas de aplicación usan dobles en su lugar.
- **Segregación de interfaces:** puertos chicos y específicos (`IClock`, `INotifier`), no un "servicio" gigante.
- **Inversión de dependencias:** el núcleo define las interfaces y la infraestructura las implementa.

## Frontend: Atomic Design + features

```
frontend/src/
├── lib/
│   ├── ui/                  Sistema de diseño (Atomic Design): solo presentación
│   │   ├── atoms/           Button, FileButton, Input, Select, Checkbox, Badge, Notice, PoleStripe
│   │   ├── molecules/       FormField, PageTitle, StepTitle, HoursRow, ImagePicker, ThemeToggle
│   │   ├── organisms/       LandingHero, BarberPole3D, DayAgenda, SlotGrid, Ticket, PalettePicker…
│   │   └── templates/       PanelLayout
│   ├── features/            Lógica por módulo: agenda, auth, barbers, booking, landing, media, services, shop
│   │   └── <modulo>/
│   │       ├── api.ts           Adaptador HTTP del módulo
│   │       ├── state.svelte.ts  Estado y validación con runas
│   │       └── types.ts         Tipos del módulo
│   ├── core/                Cliente HTTP, sesión, tema, fechas en hora de Bolivia, imágenes, enlaces
│   └── styles/              Tokens de diseño (paletas y modo oscuro) y estilos base
└── routes/                  /, /reservar, /turno/[token], /ingresar, /panel/…
```

| Capa | Puede | No puede |
|---|---|---|
| `ui/` | Mostrar datos recibidos por props y emitir eventos | Llamar a la API o importar `features/` |
| `features/` | Llamar a su `api.ts`, guardar estado, validar | Contener marcado |
| `routes/` | Usar `features/` y componer componentes de `ui/` | Llamar a la API sin pasar por `features/` |

Así el mismo `DayAgenda` sirve en el panel del dueño y en el del barbero, y el calendario de horas es el mismo en la página pública y al anotar un turno desde el panel.

## Datos

- Tablas y columnas en inglés y `snake_case`, igual que el código.
- Todas las fechas con hora en `timestamptz` (UTC); el dominio las convierte a la hora de Bolivia.
- Las imágenes se guardan en la tabla `images` como bytes, ya achicadas por la web; se sirven con caché de un año.
- Los choques de turnos se impiden en la base con una restricción de exclusión sobre el rango de horas de cada barbero (`EXCLUDE USING gist`).
- Las migraciones de EF Core viven en `Infrastructure/Persistence/Migrations` y nunca se editan una vez subidas.
