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
- **Reservar:** el cliente deja nombre y celular (el correo es opcional). El turno recibe un número correlativo y un enlace secreto para cambiarlo o cancelarlo.
- **"Cualquiera":** si el cliente no elige barbero, el turno va al barbero libre con menos turnos ese día.
- **Cambiar y cancelar:** desde el enlace del cliente, dentro del plazo permitido, o desde el panel en cualquier momento.
- **Estados del turno:** `booked` (reservado), `attended` (atendido), `no_show` (no vino) y `cancelled` (cancelado).
- **Bloqueos:** el barbero marca horas en las que no atiende (almuerzo, un trámite).
- **Turnos sin reserva:** el panel anota a quien llega sin turno para que la agenda quede completa.
- **Choques:** dos personas nunca pueden quedarse con la misma hora, aunque reserven al mismo tiempo. La base de datos lo garantiza con una restricción, no solo el código.

### 5. Avisos

- Confirmación por correo al reservar, cambiar o cancelar, si el cliente dejó correo.
- Botón de WhatsApp en el panel que abre el chat con el mensaje listo: confirmación o recordatorio.
- Los canales son intercambiables: más adelante se puede sumar la API de WhatsApp Business sin tocar la agenda.

## Pantallas

| Pantalla | Quién | Qué muestra |
|---|---|---|
| Reservar | Cliente | Servicio, barbero, día y hora, datos y confirmación |
| Mi turno | Cliente | El turno como talón de ticket, con cambiar, cancelar y agregar al calendario |
| Agenda | Barbero y dueño | El día por columnas de barbero, la hora actual y el siguiente turno |
| Servicios | Dueño | Lista y edición de servicios |
| Barberos | Dueño | Equipo, sus servicios, horarios y ausencias |
| Horarios | Dueño | Horario de la barbería y días cerrados |
| Ajustes | Dueño | Datos de la barbería y reglas de reserva |
| Ingresar | Barbero y dueño | Inicio de sesión |

## Fuera de esta versión

Caja y cobros, comisiones, reportes, historial de clientes, inventario, puntos de fidelidad, pago en línea y varias barberías en una misma instalación. La arquitectura deja lugar para agregarlos como módulos nuevos.

## Reglas generales

- Todas las horas se guardan en UTC y se muestran en la hora de Bolivia (`America/La_Paz`, UTC−4, sin horario de verano).
- Los precios se guardan en centavos para no perder decimales.
- Los textos que ve la gente están en español y hablan de tú.
