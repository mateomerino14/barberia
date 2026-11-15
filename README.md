# Barbería

Sistema de reservas para barberías: los clientes sacan turno desde el celular, sin crear cuenta, y el equipo administra la agenda, los horarios y los servicios desde un panel.

## Estado

Día 1 de 7: documentación, identidad visual, estructura del proyecto y CI. Los módulos se suman en este orden:

| Entrega | Contenido | Estado |
|---|---|---|
| 1. Base | Documentos, reglas, esqueleto de API y web, CI | Lista |
| 2. Barbería y equipo | Acceso, datos de la barbería, servicios y barberos | En curso: API lista |
| 3. Horas libres | Cálculo de disponibilidad y reservas | Pendiente |
| 4. Reserva pública | Página para el cliente, cambiar y cancelar | Pendiente |
| 5. Agenda | Panel del barbero y del dueño, bloqueos y avisos | Pendiente |
| 6. Pulido | Detalles visuales, celular y datos de demostración | Pendiente |
| 7. Publicación | Pruebas de punta a punta, despliegue y capturas | Pendiente |

## Documentos

- [Qué hace el sistema](docs/sistema.md)
- [Arquitectura](docs/arquitectura.md)
- [Reglas de desarrollo](reglas.md)

## Tecnologías

| Parte | Tecnología |
|---|---|
| API | .NET 10, ASP.NET Core, EF Core |
| Web | SvelteKit con TypeScript |
| Base de datos | PostgreSQL (Supabase en producción) |
| Pruebas | xUnit, NetArchTest, Testcontainers, Vitest y Testing Library |
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

En desarrollo ya vienen valores locales en `appsettings.Development.json`: el dueño es `dueno@barberia.local` con la contraseña `barberia-local`.

## Desarrollo

Requisitos: .NET SDK 10, Node 22 y Docker.

```bash
# Base de datos local
docker compose up -d

# API en http://localhost:5000
cd backend
dotnet run --project src/Barberia.Api

# Web en http://localhost:5173
cd frontend
npm install
npm run dev
```

| Tarea | Backend (`backend/`) | Frontend (`frontend/`) |
|---|---|---|
| Pruebas | `dotnet test` | `npm test` |
| Formato y estilo | `dotnet format` | `npm run lint:fix` |
| Código sin uso | se revisa al compilar | `npm run lint:deadcode` |
| Tipos | `dotnet build` | `npm run check` |
