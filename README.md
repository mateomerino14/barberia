# Barbería

Sistema de reservas para barberías: los clientes conocen la barbería en una portada con un poste en 3D y sacan turno desde el celular, sin crear cuenta, y el equipo administra la agenda, los horarios y los servicios desde un panel.

## Capturas

| Reservar (celular) | Turno del cliente | Agenda del panel |
|---|---|---|
| ![Página de reservas](docs/capturas/reservar.png) | ![Turno del cliente](docs/capturas/turno.png) | ![Agenda del día](docs/capturas/agenda.png) |

Más capturas en [`docs/capturas`](docs/capturas): ingresar, servicios, barberos y horarios.

## Estado

Los siete días de trabajo están completos: los clientes reservan desde el celular y manejan su turno con un enlace, y el equipo maneja la agenda, los horarios, los servicios y los avisos desde el panel.

| Entrega | Contenido | Estado |
|---|---|---|
| 1. Base | Documentos, reglas, esqueleto de API y web, CI | Lista |
| 2. Barbería y equipo | Acceso, datos de la barbería, servicios y barberos | Lista |
| 3. Horas libres | Cálculo de disponibilidad y reservas | Lista |
| 4. Reserva pública | Página para el cliente, cambiar y cancelar | Lista |
| 5. Agenda | Panel del barbero y del dueño, bloqueos y avisos | Lista |
| 6. Pulido | Detalles visuales, celular y datos de demostración | Lista |
| 7. Publicación | Pruebas de punta a punta, despliegue y capturas | Lista |

## Documentos

- [Qué hace el sistema](docs/sistema.md)
- [Arquitectura](docs/arquitectura.md)
- [Reglas de desarrollo](reglas.md)
- [Despliegue](docs/despliegue.md)

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
| `Cors__Origins__0` | Dirección de la web que puede llamar a la API |
| `Email__BrevoApiKey` | Clave de Brevo para mandar las confirmaciones por correo; sin ella no se envían correos |
| `Email__SenderEmail` y `Email__SenderName` | Remitente de los correos, verificado en Brevo |
| `Email__PublicWebUrl` | Dirección pública de la web, para el enlace del turno en el correo |

En desarrollo ya vienen valores locales en `appsettings.Development.json`: el dueño es `dueno@barberia.local` con la contraseña `barberia-local`. Para empezar con servicios, barberos y horarios de ejemplo, arranca la API con `Setup__DemoData=true`.

Reservar, cambiar turnos e iniciar sesión tienen un límite de peticiones por dirección IP (`RateLimits__PermitsPerMinute`, 20 por minuto por defecto).

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
