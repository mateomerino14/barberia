# Seguridad

Qué protege el sistema, cómo lo hace y qué queda en manos de quien lo instala.

## Quién entra a qué

| Dirección | Quién | Cómo se protege |
|---|---|---|
| `/`, `/reservar` | Cualquiera | Solo muestra datos públicos: nombre, dirección, precios, barberos activos y horario |
| `/turno/{clave}` | Quien tiene el enlace | La clave es aleatoria de 192 bits (48 caracteres); no se puede adivinar ni recorrer |
| `/ingresar` | El equipo | Correo y contraseña, con límite de intentos |
| `/panel/...` | Dueño | Todas las secciones |
| `/panel/agenda` | Barbero | Solo su propia agenda; si escribe otra dirección del panel, vuelve a ella |

La web solo decide qué pantalla mostrar. **Quien decide de verdad es la API**: cada ruta del panel exige el pase de sesión y el rol, y cada acción sobre un turno o bloqueo revisa que el barbero sea el dueño de esa agenda.

## Sesiones

- El pase de sesión está firmado (JWT, HMAC-SHA256) y dura 12 horas.
- En cada petición la API revisa que la cuenta siga existiendo, que el barbero no haya sido dado de baja y que la contraseña no haya cambiado. Si algo de eso pasó, el pase deja de valer al instante y la web vuelve a `/ingresar`.
- El inicio de sesión responde igual, y tarda lo mismo, exista o no el correo: no revela qué cuentas hay.
- Las contraseñas se guardan con el hash de ASP.NET Identity (PBKDF2), nunca en texto.

## Límites de intentos (por IP, por minuto)

| Qué | Límite | Variable para ajustarlo |
|---|---|---|
| Iniciar sesión | 5 | `RateLimits__LoginsPerMinute` |
| Reservar, cambiar o cancelar | 20 | `RateLimits__PermitsPerMinute` |
| Horas libres, días libres, imágenes y ver un turno por enlace | 120 | `RateLimits__ReadsPerMinute` |

Detrás del proxy de Render hay que poner `Proxy__TrustForwardedHeaders=true`; si no, todos los clientes se verían con la misma IP y compartirían el límite. Solo se toma el último salto de `X-Forwarded-For`, el que agrega Render, así que un cliente no puede inventarse otra IP.

## Cabeceras del navegador

- **Web:** política de contenido (CSP) que solo permite scripts propios con nonce, conexiones a la propia web y a la API, y nada de incrustar la página en otro sitio. Además `X-Frame-Options: DENY`, `nosniff`, `Referrer-Policy: no-referrer` (el enlace del turno nunca viaja a WhatsApp o Google Maps) y `Permissions-Policy` sin cámara, micrófono ni ubicación.
- **API:** las mismas protecciones, `Cache-Control: no-store` para que ningún turno o dato del panel quede guardado en cachés, y HSTS fuera de desarrollo.
- **CORS:** la API solo acepta llamadas del navegador desde las direcciones de `Cors__Origins__N`.

## Datos

- El cliente deja nombre, celular y, si quiere, correo. No crea cuenta.
- "Recordar mis datos" guarda nombre y celular solo en el teléfono del cliente.
- Los textos que escribe el cliente se escapan en la web y en el correo de confirmación.
- Los errores en producción no muestran detalles internos.
- La imagen Docker de la API corre sin permisos de administrador.

## Lo que queda en manos de quien instala

- Usar una `Jwt__Key` larga y aleatoria y una contraseña fuerte para el dueño, y cambiar la inicial.
- No activar `Setup__DemoData` en una barbería real.
- Guardar las claves solo como variables de entorno del hosting, nunca en el repositorio.
