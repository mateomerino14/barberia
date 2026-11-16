# Qué hace el sistema

Barbería es un sistema de reservas para una barbería: los clientes sacan turno desde el celular y el equipo administra la agenda y los horarios. Cada barbería que lo usa tiene su propia instalación, con su nombre, su logo y su base de datos.

## Personas que lo usan

| Persona | Entra | Para qué |
|---|---|---|
| Cliente | Por el enlace público, sin crear cuenta | Reservar, cambiar o cancelar su turno |
| Barbero | Con usuario y contraseña | Ver su agenda, marcar turnos y bloquear horas |
| Dueño | Con usuario y contraseña | Todo lo del barbero, más servicios, equipo, horarios y ajustes |

## Módulos

### 1. Acceso

- Inicio de sesión del dueño y de los barberos con correo y contraseña.
- Dos roles: `owner` (dueño) y `barber` (barbero). El dueño también puede atender como barbero.
- El dueño crea las cuentas de los barberos; no hay registro abierto.
- La sesión dura una jornada y se cierra sola.

### 2. Barbería

- Nombre, dirección, teléfono, logo y mensaje de bienvenida.
- Horario de atención por día de la semana (puede haber días cerrados, como el lunes).
- Días cerrados puntuales: feriados, vacaciones.
- Reglas de reserva: cuánto antes se puede reservar como mínimo (por ejemplo, 1 hora), hasta cuántos días adelante (por ejemplo, 14) y hasta cuándo se puede cancelar (por ejemplo, 2 horas antes).
- Intervalo de turnos: cada cuántos minutos puede empezar un turno (15, 20, 30 o 60). Cada turno ocupa la duración real de su servicio; el intervalo solo define las horas de inicio.

### 3. Servicios y barberos

- Servicios con nombre, duración y precio en bolivianos. Se pueden ocultar sin borrarlos.
- Barberos con nombre, foto o iniciales y qué servicios hacen.
- Horario semanal de cada barbero, que no puede salirse del horario de la barbería.
- Ausencias de un barbero por uno o varios días.

### 4. Agenda

- **Horas libres:** para un servicio, un barbero (o cualquiera) y un día, el sistema calcula las horas en las que entra el servicio completo. Una hora está libre si:
  - cae dentro del horario de la barbería y del barbero;
  - el servicio termina antes de que cierre;
  - no choca con otro turno ni con un bloqueo;
  - respeta la anticipación mínima y el límite de días.

  Las horas de inicio se ofrecen según el intervalo que eligió la barbería (cada 15 minutos por defecto, así se aprovechan los huecos que dejan servicios de distinta duración).
- **Días llenos:** la tira de días marca **Lleno** los días sin ninguna hora libre. Antes de elegir servicio se usa el más corto de cada barbero (si no entra ni ese, el día está lleno para todo); con servicio y barbero elegidos muestra cuántas horas le quedan a cada día. La API lo calcula para todos los días en una sola petición (`GET /api/availability/days`).
- **Reservar:** el cliente deja nombre y celular (el correo es opcional). El turno recibe un número correlativo y un enlace secreto para cambiarlo o cancelarlo.
- **"Cualquiera":** si el cliente no elige barbero, el turno va al barbero libre con menos turnos ese día.
- **Cambiar y cancelar:** desde el enlace del cliente, dentro del plazo permitido, o desde el panel en cualquier momento.
- **Estados del turno:** `booked` (reservado), `attended` (atendido), `no_show` (no vino) y `cancelled` (cancelado).
- **Bloqueos:** el barbero marca horas en las que no atiende (almuerzo, un trámite).
- **Recordar datos:** el cliente puede guardar su nombre y celular en su propio teléfono para la próxima reserva; no se crea ninguna cuenta.
- **Fotos:** el dueño sube el logo, las fotos del local y la foto de cada barbero. La web las achica en el teléfono antes de subirlas y la API solo acepta JPG, PNG o WebP de hasta 2 MB, revisando el contenido real del archivo.
- **Contraseñas:** cada uno cambia la suya en "Mi cuenta". Si un barbero olvida la suya, el dueño le pone una nueva desde "Barberos". Al cambiarla se cierran las sesiones abiertas.
- **Colores:** el dueño elige entre cinco paletas y cada persona puede pasar a modo oscuro con el botón de sol o luna.
- **Turnos sin reserva:** el panel anota a quien llega sin turno para que la agenda quede completa.
- **Choques:** dos personas nunca pueden quedarse con la misma hora, aunque reserven al mismo tiempo. La base de datos lo garantiza con una restricción, no solo el código.

### 5. Avisos

- Confirmación por correo al reservar, cambiar o cancelar, si el cliente dejó correo.
- Recuperar la contraseña del panel por correo: un enlace de un solo uso que vence en 30 minutos. Solo se ofrece si hay correos configurados.
- Los correos salen con el nombre de la barbería como remitente, tomado de "Mi barbería".
- Recordatorio por correo unas horas antes del turno (3 por defecto), una sola vez; si cambia la hora, se vuelve a recordar.
- Botón de WhatsApp en la agenda que abre el chat con el recordatorio listo.
- Si un aviso falla (por ejemplo, el servicio de correo no responde), la reserva igual se guarda y el error queda registrado.
- Los canales son intercambiables: más adelante se puede sumar la API de WhatsApp Business sin tocar la agenda.

## Pantallas

| Pantalla | Quién | Qué muestra |
|---|---|---|
| Presentación | Cliente | Portada con el poste en 3D, precios, equipo, horario y ubicación, en `/` |
| Reservar | Cliente | Servicio, barbero, día y hora, datos y confirmación |
| Mi turno | Cliente | El turno como talón de ticket, con cambiar, cancelar y agregar al calendario |
| Agenda | Barbero y dueño | El día por columnas de barbero, la hora actual y el siguiente turno |
| Servicios | Dueño | Lista y edición de servicios |
| Barberos | Dueño | Equipo, sus servicios, horarios y ausencias |
| Horarios | Dueño | Horario de la barbería y días cerrados |
| Mi barbería | Dueño | Datos de la barbería, reglas de reserva, paleta, logo y hasta 6 fotos del local |
| Mi cuenta | Dueño y barberos | Cambiar la propia contraseña |
| Ingresar | Barbero y dueño | Inicio de sesión |

## Fuera de esta versión

Caja y cobros, comisiones, reportes, historial de clientes, inventario, puntos de fidelidad, pago en línea y varias barberías en una misma instalación. La arquitectura deja lugar para agregarlos como módulos nuevos.

## Reglas generales

- Todas las horas se guardan en UTC y se muestran en la hora de Bolivia (`America/La_Paz`, UTC−4, sin horario de verano).
- Los precios se guardan en centavos para no perder decimales.
- Los textos que ve la gente están en español y hablan de tú.
