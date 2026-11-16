# Despliegue

Guía para publicar una instalación de Barbería con planes gratuitos: la base en Supabase, la API en Render y la web en Vercel. Cada barbería que use el sistema tiene su propia instalación con estos tres pasos.

Las cuentas en Supabase, Render, Vercel y Brevo se crean a nombre del dueño del proyecto. Las claves nunca se suben al repositorio: van solo en las variables de entorno de cada servicio.

## 1. Base de datos en Supabase

1. Crea un proyecto nuevo en Supabase. Conviene la misma región que la API: si la API va en Render Virginia, el proyecto va en **us-east-1**.
2. En el **SQL Editor**, crea un usuario solo para la aplicación, con una contraseña larga y aleatoria, y la extensión que impide los choques de horario:

   ```sql
   CREATE EXTENSION IF NOT EXISTS btree_gist WITH SCHEMA public;
   CREATE ROLE barberia_app LOGIN PASSWORD 'UNA_CONTRASEÑA_LARGA';
   GRANT USAGE, CREATE ON SCHEMA public TO barberia_app;
   ```

   Así la aplicación no usa el usuario administrador, y las tablas que crea no quedan expuestas en la API automática de Supabase (los roles `anon` y `authenticated` no reciben permisos sobre ellas).
3. En **Connect**, mira el host del **Session pooler** (puerto 5432), que funciona desde servicios que solo tienen IPv4, como Render. Arma la cadena en el formato de .NET, con el usuario del paso anterior seguido de un punto y el identificador del proyecto:

   ```
   Host=aws-1-us-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=barberia_app.TU_PROYECTO;Password=UNA_CONTRASEÑA_LARGA;SSL Mode=Require
   ```

No hace falta crear tablas: la API aplica las migraciones sola al arrancar.

En el plan gratuito, Supabase pausa el proyecto si pasa una semana sin uso; se reactiva desde su panel.

## 2. API en Render

1. En Render, crea un **Web Service** desde el repositorio, con **Root Directory** `backend` y **Runtime** Docker. El `Dockerfile` ya está listo.
2. Agrega las variables de entorno:

   | Variable | Valor |
   |---|---|
   | `ConnectionStrings__Barberia` | La cadena del paso 1 |
   | `Jwt__Key` | Una clave larga y aleatoria, de al menos 32 caracteres |
   | `Setup__ShopName` | Nombre de la barbería |
   | `Setup__OwnerEmail` | Correo del dueño |
   | `Setup__OwnerPassword` | Contraseña inicial del dueño (al menos 8 caracteres) |
   | `Cors__Origins__0` | La dirección de la web del paso 3, por ejemplo `https://barberia.vercel.app` |
   | `Proxy__TrustForwardedHeaders` | `true` en Render, para que los límites de intentos cuenten por cliente y no por el proxy |
   | `Setup__DemoData` | `true` solo para una demostración: carga servicios, barberos y horarios de ejemplo |

3. Comprueba que `https://TU-API.onrender.com/api/health` responda `{"status":"ok",...}`.

En el plan gratuito, Render duerme el servicio tras unos minutos sin visitas y la primera petición después tarda cerca de un minuto. Para una barbería que paga, conviene el plan más chico de pago.

## 3. Web en Vercel

1. En Vercel, importa el repositorio con **Root Directory** `frontend`. Detecta SvelteKit solo.
2. Agrega la variable `VITE_API_URL` con la dirección de la API, por ejemplo `https://TU-API.onrender.com`.
3. En **Settings → Deployment Protection**, deja la protección solo para las vistas previas: si cubre producción, los clientes tendrían que tener cuenta en Vercel para ver la página.
4. Publica. La presentación de la barbería queda en la raíz, la reserva en `/reservar` y el panel en `/ingresar`.
5. En Render, pon esa dirección en `Cors__Origins__0` para que la web pueda hablar con la API.

Si cambias `VITE_API_URL`, vuelve a publicar: la dirección queda fija al compilar.

## 4. Correos (opcional)

Con los correos configurados salen las confirmaciones y los recordatorios de los turnos, y en Ingresar aparece **¿Olvidaste tu contraseña?** para el dueño y los barberos.

1. Crea una cuenta en Brevo y verifica el correo remitente.
2. Genera una clave de API (SMTP & API → API Keys).
3. Agrega en Render:

   | Variable | Valor |
   |---|---|
   | `Email__BrevoApiKey` | La clave de Brevo |
   | `Email__SenderEmail` | El remitente verificado |
   | `Email__PublicWebUrl` | La dirección de la web, para el enlace del turno y el de la contraseña nueva |
   | `Reminders__HoursBefore` | Opcional. Horas antes del turno en que llega el recordatorio (por defecto 3; 0 lo apaga) |

   **Recordatorios en el plan gratis de Render:** la API se duerme tras 15 minutos sin visitas y, dormida, no manda recordatorios. Para que lleguen, mantenla despierta en horario de la barbería (sección 5) o pasa la API a un plan pago.

Sin estas variables el sistema funciona igual; solo no envía correos ni ofrece recuperar la contraseña por correo. Los recordatorios por WhatsApp salen desde la agenda con un botón y no necesitan configuración.

## 5. Mantener la API despierta en horario de la barbería (opcional)

En el plan gratis, Render apaga la API tras 15 minutos sin visitas: la primera visita después tarda alrededor de un minuto y los recordatorios no salen mientras está apagada. Render da 750 horas gratis al mes **para todos los servicios gratis de la cuenta juntos**; si una API queda despierta las 24 horas usa unas 720, y al acabarse las horas Render apaga todos los servicios gratis hasta el mes siguiente.

Por eso el repositorio trae el flujo `Mantener la API despierta` (`.github/workflows/keep_api_awake.yml`), que visita `/api/health` cada 10 minutos solo de 06:00 a 21:00 en La Paz: unas 450 horas al mes. De noche la API duerme.

**Viene apagado.** Para encenderlo:

1. En GitHub, **Settings → Secrets and variables → Actions → Variables**, crea `API_URL` con la dirección de la API, por ejemplo `https://TU-API.onrender.com`.
2. En **Actions**, elige **Mantener la API despierta** y toca **Enable workflow**.

Para apagarlo, **Disable workflow** en la misma pantalla. GitHub pausa los flujos programados de un repositorio público si pasan 60 días sin actividad; en ese caso se vuelve a activar igual.

Con una barbería real conviene más un plan pago de Render para la API: no se duerme y no gasta las horas gratis de otros proyectos.

## Después de publicar

- Entra a `/ingresar` con la cuenta del dueño, revisa **Mi barbería** y **Horarios**, cambia la contraseña en **Mi cuenta** y carga servicios y barberos.
- Comparte el enlace de la raíz de la web en Instagram, WhatsApp o un código QR en el local.
