# Despliegue

Guía para publicar una instalación de Barbería con planes gratuitos: la base en Supabase, la API en Render y la web en Vercel. Cada barbería que use el sistema tiene su propia instalación con estos tres pasos.

Las cuentas en Supabase, Render, Vercel y Brevo se crean a nombre del dueño del proyecto. Las claves nunca se suben al repositorio: van solo en las variables de entorno de cada servicio.

## 1. Base de datos en Supabase

1. Crea un proyecto nuevo en Supabase, en la región más cercana (São Paulo para Bolivia).
2. En **Connect**, copia la cadena de conexión del **Session pooler** (puerto 5432). Es la que funciona desde servicios que solo tienen IPv4, como Render.
3. Pásala al formato de .NET:

   ```
   Host=aws-0-sa-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.TU_PROYECTO;Password=TU_CONTRASEÑA;SSL Mode=Require
   ```

No hace falta crear tablas: la API aplica las migraciones sola al arrancar, incluida la extensión `btree_gist` que impide los choques de horario.

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
3. Publica. La presentación de la barbería queda en la raíz, la reserva en `/reservar` y el panel en `/ingresar`.

Si cambias `VITE_API_URL`, vuelve a publicar: la dirección queda fija al compilar.

## 4. Correos de confirmación (opcional)

1. Crea una cuenta en Brevo y verifica el correo remitente.
2. Genera una clave de API (SMTP & API → API Keys).
3. Agrega en Render:

   | Variable | Valor |
   |---|---|
   | `Email__BrevoApiKey` | La clave de Brevo |
   | `Email__SenderEmail` | El remitente verificado |
   | `Email__SenderName` | Nombre de la barbería |
   | `Email__PublicWebUrl` | La dirección de la web, para el enlace del turno |
   | `Reminders__HoursBefore` | Opcional. Horas antes del turno en que llega el recordatorio (por defecto 3; 0 lo apaga) |

   **Recordatorios en el plan gratis de Render:** la API se duerme tras 15 minutos sin visitas y, dormida, no manda recordatorios. Para que lleguen siempre, usa un plan pago o un monitor gratuito (por ejemplo UptimeRobot) que visite `/api/health` cada 10 minutos.

Sin estas variables el sistema funciona igual; solo no envía correos. Los recordatorios por WhatsApp salen desde la agenda con un botón y no necesitan configuración.

## Después de publicar

- Entra a `/ingresar` con la cuenta del dueño, revisa **Ajustes** y **Horarios**, y carga servicios y barberos.
- Comparte el enlace de la raíz de la web en Instagram, WhatsApp o un código QR en el local.
