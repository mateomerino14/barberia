# Barbería

Sistema de reservas para barberías: los clientes conocen la barbería en una portada con un poste en 3D y sacan turno desde el celular, sin crear cuenta, y el equipo administra la agenda, los horarios y los servicios desde un panel.

**Demo en línea:** [barberia-mat13.vercel.app](https://barberia-mat13.vercel.app). La API está en el plan gratis de Render: si nadie entró en un rato, la primera carga tarda alrededor de un minuto.

## Capturas

<p>
  <img src="docs/manual/01_inicio.webp" width="240" alt="Portada de la barbería">
  <img src="docs/manual/04_reservar_hora.webp" width="240" alt="Elegir día y hora">
  <img src="docs/manual/06_turno.webp" width="240" alt="Talón del turno">
</p>

![Agenda del panel](docs/manual/12_agenda_turno.webp)

Todas las pantallas, paso a paso, en el [manual de usuario](docs/manual.md).

## Estado

| Versión | Contenido |
|---|---|
| 1.0 | Reserva desde el celular, turno con enlace para cambiar o cancelar, agenda del panel, servicios, barberos, horarios, bloqueos y avisos por correo |
| 1.1 | Presentación con poste en 3D, paletas y modo oscuro, recordar datos del cliente, cambio de contraseñas, logo y fotos, recordatorio antes del turno y revisión de seguridad |
| 1.2 | Días llenos marcados en la reserva con las horas libres de cada día, e intervalo de turnos configurable (15, 20, 30 o 60 minutos) |

## Documentos

- [Manual de usuario](docs/manual.md)
- [Qué hace el sistema](docs/sistema.md)
- [Arquitectura](docs/arquitectura.md)
- [Reglas de desarrollo](reglas.md)
- [Despliegue](docs/despliegue.md)
- [Seguridad](docs/seguridad.md)

## Tecnologías

| Parte | Tecnología |
|---|---|
| API | .NET 10, ASP.NET Core, EF Core |
| Web | SvelteKit con TypeScript |
| Base de datos | PostgreSQL (Supabase en producción) |
| Pruebas | xUnit, NetArchTest, Testcontainers, Vitest, Testing Library y Playwright |
| Calidad | `dotnet format`, ESLint, knip y pruebas de arquitectura |

## Configuración

La API lee su configuración de `appsettings.json` o de variables de entorno (con `__` en lugar de `:`):

| Variable | Para qué |
|---|---|
| `ConnectionStrings__Barberia` | Cadena de conexión a PostgreSQL |
| `Jwt__Key` | Clave secreta de al menos 32 caracteres para firmar las sesiones |
| `Setup__ShopName` | Nombre de la barbería al arrancar por primera vez |
| `Setup__OwnerEmail` y `Setup__OwnerPassword` | Cuenta del dueño que se crea al primer arranque |
| `Cors__Origins__0`, `Cors__Origins__1`… | Direcciones de la web que pueden llamar a la API |
| `Proxy__TrustForwardedHeaders` | `true` detrás del proxy de Render, para contar los límites por cliente real |
| `Email__BrevoApiKey` | Clave de Brevo para mandar los correos; sin ella no se envían |
| `Email__SenderEmail` | Correo remitente, verificado en Brevo; el nombre del remitente es el de la barbería |
| `Email__PublicWebUrl` | Dirección pública de la web, para el enlace del turno en el correo |
| `Reminders__HoursBefore` | Horas antes del turno en que sale el recordatorio (3 por defecto; 0 lo apaga) |

En desarrollo ya vienen valores locales en `appsettings.Development.json`: el dueño es `dueno@barberia.local` con la contraseña `barberia-local`. Para empezar con servicios, barberos y horarios de ejemplo, arranca la API con `Setup__DemoData=true`.

Las rutas públicas tienen límites de peticiones por cliente: 5 por minuto para iniciar sesión, 20 para reservar, cambiar o cancelar y 120 para las consultas. Se ajustan con `RateLimits__*`; el detalle está en [Seguridad](docs/seguridad.md).

## Desarrollo

Requisitos: .NET SDK 10, Node 22 y Docker.

```bash
# Base de datos local
docker compose up -d

# API en http://localhost:5000
cd backend
dotnet run --project src/Barberia.Api

# Web en http://localhost:5173 (reservas en /, panel en /ingresar)
cd frontend
npm install
npm run dev
```

La web busca la API en `VITE_API_URL`; si no está definida usa `http://localhost:5000`.

| Tarea | Backend (`backend/`) | Frontend (`frontend/`) |
|---|---|---|
| Pruebas | `dotnet test` | `npm test` |
| Formato y estilo | `dotnet format` | `npm run lint:fix` |
| Código sin uso | se revisa al compilar | `npm run lint:deadcode` |
| Tipos | `dotnet build` | `npm run check` |
| Punta a punta | (con la API y la web levantadas) | `npm run test:e2e` |
