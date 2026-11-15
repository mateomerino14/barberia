# Estándares de desarrollo

Barbería: convenciones de ramas, commits, codificación y pruebas para la API en .NET y la web en SvelteKit.

---

## ESTÁNDARES DE RAMAS

| Rama | Uso |
|---|---|
| `main` | Versión estable, solo con aprobación |
| `develop` | Integración de los módulos terminados |
| `feat/<modulo>` | Un módulo o funcionalidad, en `snake_case` (ej. `feat/horas_libres`) |
| `fix/<tema>` | Corrección puntual sobre `develop` |
| `style/<tema>` | Cambio visual sin tocar la lógica |
| `refactor/<tema>` | Reestructuración sin cambiar el comportamiento |
| `test/<tema>` | Pruebas nuevas o ajustes de pruebas |
| `docs/<tema>` | Documentación |
| `chore/<tema>` | Configuración, CI y herramientas |

### Flujo

1. Cada módulo se desarrolla en su rama, creada desde `develop`.
2. Al terminarlo se abre un Pull Request hacia `develop`. GitHub Actions debe pasar antes del merge.
3. Cuando `develop` reúne una entrega completa y se aprueba, se abre un Pull Request de `develop` hacia `main`.
4. Después del merge la rama de trabajo se borra; en el repositorio solo quedan `main` y `develop`.

---

## ESTÁNDARES DE COMMITS

Se usa Conventional Commits. Todos los commits van a nombre del autor del repositorio, sin coautores ni líneas agregadas.

| Tipo | Uso |
|---|---|
| `feat` | Nueva funcionalidad |
| `fix` | Corrección de errores |
| `style` | Cambios visuales sin afectar la lógica |
| `refactor` | Reestructuración sin modificar el comportamiento |
| `docs` | Documentación |
| `test` | Creación o actualización de pruebas |
| `chore` | Configuración del proyecto, CI y herramientas |
| `build` | Empaquetado y despliegue |

```
<tipo>: <Descripción>
```

```
feat: Calcula las horas libres de un barbero para un servicio
fix: Impide reservar una hora que ya pasó
test: Prueba que dos reservas no puedan tomar la misma hora
```

- El mensaje comienza con mayúscula después del prefijo.
- Describe la funcionalidad, no los archivos modificados.
- Un commit representa una única funcionalidad, corrección o mejora.
- Evitar mensajes genéricos: `cambios`, `correcciones`, `actualización`, `prueba`.

---

## ESTÁNDARES DE CODIFICACIÓN

### Idioma

- El código se escribe en inglés: clases, variables, funciones, componentes, rutas de la API, tablas y columnas.
- Los textos que ve la gente, los comentarios y la documentación van en español.

### Documentación

- **Backend:** cada clase, record, interfaz y método lleva un `/// <summary>` de una sola línea que dice su propósito. Las propiedades públicas también, porque el compilador lo exige.
- **Frontend:** cada componente empieza con un comentario HTML de una línea que dice qué es, y cada función lleva un JSDoc de una línea.
- Dentro de las funciones solo se dejan avisos importantes, también de una línea.
- No se deja código comentado.

```csharp
/// <summary>Calcula las horas en las que entra un servicio completo en la agenda de un barbero.</summary>
public IReadOnlyList<TimeRange> FreeSlots(Service service, Barber barber, DateOnly day)
```

```svelte
<!-- Botón de texto de la identidad: rojo para la acción principal, contorno para la secundaria. -->
```

### Sintaxis

- **Sin operadores ternarios**, ni en C# ni en TypeScript ni en las plantillas de Svelte. Se usan bloques `if`/`else` o `{#if}`.
- **Llaves siempre**, incluso en bloques de una línea. En C# van en línea nueva (Allman); en TypeScript, `else` y `catch` van en línea nueva después de la llave de cierre.
- **Sin valores mágicos:** los números y textos con significado se declaran como constantes con nombre. En las pruebas también.
- **Tipado estricto:** `Nullable` activado en C# y `strict` en TypeScript. En C# no se usa `var`: el tipo siempre se escribe.
- **Sin código muerto:** nada sin uso, ni privados, ni parámetros, ni `using`, ni exportaciones, ni dependencias.

```csharp
// Correcto
if (booking.IsCancelled)
{
    return BookingErrors.AlreadyCancelled;
}

return booking.Cancel(clock.Now);

// Incorrecto
return booking.IsCancelled ? BookingErrors.AlreadyCancelled : booking.Cancel(clock.Now);
```

```typescript
const minPhoneDigits = 8;
const payload = {name, phone};
```

- **TypeScript:** punto y coma al final, comillas simples, `const` por defecto y objetos literales sin espacios internos (`{name}`).

### Herramientas que lo verifican

| Regla | Backend | Frontend |
|---|---|---|
| Formato y estilo | `dotnet format --verify-no-changes` y `.editorconfig` como errores de compilación | `npm run lint` (ESLint) |
| Sin ternarios | `Barberia.Architecture.Tests` | regla `no-ternary` |
| Documentación de una línea | `Barberia.Architecture.Tests` y `CS1591` | regla `jsdoc/require-jsdoc` |
| Valores mágicos | Revisión en el PR | regla `no-magic-numbers` |
| Código muerto | Analizadores `IDE0051`, `IDE0052`, `IDE0060`, `IDE0005` | `npm run lint:deadcode` (knip) |
| Tipos | Compilación con advertencias como errores | `npm run check` (svelte-check) |

---

## BACKEND

La estructura y las reglas de dependencia están en [docs/arquitectura.md](docs/arquitectura.md).

- Un caso de uso por manejador, con su comando en el mismo archivo: `CreateBookingCommand` + `CreateBookingHandler` en `Features/Bookings/CreateBooking.cs`. Los casos que son uno el inverso del otro (agregar y quitar) pueden compartir archivo.
- Los errores de negocio previstos se devuelven como `Result`, nunca como excepción. Las excepciones quedan para fallas reales.
- Las entidades se crean con métodos de fábrica que validan (`Booking.Create(...)`); no tienen constructores públicos ni setters públicos.
- Las rutas HTTP se agrupan por módulo en `Api/Endpoints/` y solo traducen la petición al caso de uso y el resultado a la respuesta.
- La autorización se declara en la ruta, no dentro del caso de uso.
- La API valida todo: una validación solo en la web no protege los datos.
- Los errores salen en formato `ProblemDetails` con un mensaje en español para la persona.
- Ninguna credencial en el repositorio: se usan variables de entorno.

## FRONTEND

La estructura está en [docs/arquitectura.md](docs/arquitectura.md).

- Ningún componente de `lib/ui/` llama a la API ni importa de `lib/features/`.
- Los estilos van en el bloque `<style>` del componente y usan solo los tokens de `lib/styles/tokens.css`: ningún color, fuente ni medida suelta.
- Los botones con texto no llevan ícono; los botones de solo ícono llevan `aria-label`.
- Los botones miden al menos 44 px de alto y el texto no se parte en dos líneas.
- Las horas ocupadas se distinguen por algo más que el color (tachado, borde punteado).
- Las animaciones respetan "reducir movimiento" del sistema.
- Antes de un cambio visual se presenta una maqueta para aprobarla.

## BASE DE DATOS

- Tablas y columnas en inglés y `snake_case`.
- Toda fecha con hora usa `timestamptz` en UTC.
- Los precios se guardan en centavos (`integer`).
- Los cambios de esquema son migraciones nuevas; nunca se edita una migración ya subida.
- Los índices se declaran sobre las columnas que se usan para filtrar o unir.

## PRUEBAS

- Las reglas de la agenda (horas libres, choques, anticipación, horarios) se prueban en el dominio con casos límite: el primer y el último turno del día, el cierre, el cambio de día y las reservas simultáneas.
- Cada caso de uso tiene pruebas con puertos falsos.
- Cada ruta de la API tiene pruebas de integración contra PostgreSQL real.
- Cada átomo, molécula y organismo tiene pruebas con Testing Library.
- Los recorridos principales (reservar, cancelar, ver la agenda) se prueban de punta a punta.
- Un módulo está terminado cuando su CI pasa en verde y su documentación está al día.
