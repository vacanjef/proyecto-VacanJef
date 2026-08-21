# VacanJef Web — ASP.NET Core MVC + SQL Server

Aplicación web para la guardería canina VacanJef: login real contra base de datos,
registro de mascotas (CU-01), reservas de estadía/paseo (CU-02), monitoreo en
tiempo real simulado (CU-03), facturación y pagos (CU-04), y reportes básicos
para el administrador (CU-05).

## ⚠️ Importante: este proyecto no se compiló en el entorno de chat

Aquí no hay SDK de .NET instalado ni acceso a NuGet, así que el código se
escribió directamente sin poder ejecutar `dotnet build`. Está hecho con cuidado
siguiendo el esquema real de tu base de datos, pero **antes de darlo por
terminado, ejecútalo localmente y revisa la consola por si aparece algún
error de compilación menor** (typo, usign faltante, etc.) — es lo normal en
un proyecto de este tamaño escrito sin compilador a la mano.

## 1. Requisitos previos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) o superior
- SQL Server (Express, Developer o LocalDB) con SQL Server Management Studio (SSMS)
- Visual Studio 2022, VS Code o `dotnet` CLI

## 2. Crear y preparar la base de datos

En SSMS, sobre tu instancia de SQL Server, ejecuta en este orden:

1. `sql/01_vacanjef_corregido.sql` — crea la base `VacanJef` y todas las tablas
   originales, con los datos de ejemplo. **Se corrigió el catálogo
   `Tipos_Servicio`**: solo traía un servicio ("Estadia") pero los datos de
   ejemplo de `Reservas` y `Facturas` ya referenciaban un segundo servicio
   ("Paseo") inexistente, lo que rompía la llave foránea al insertar. También
   se ajustó `precio_base` de Estadia a tarifa diaria (50000), porque así es
   como realmente escalan los montos en las facturas de ejemplo (4 días =
   200000, 5 días = 250000, 7 días = 350000).
2. `sql/02_auth_usuarios.sql` — agrega la tabla `Usuarios` (no existía en el
   script original) con 5 cuentas de demostración y contraseñas ya cifradas
   con bcrypt.

## 3. Configurar la cadena de conexión

Edita `appsettings.json` con los datos de tu instancia:

```json
"ConnectionStrings": {
  "VacanJefDb": "Server=TU_SERVIDOR;Database=VacanJef;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Si usas autenticación SQL (usuario/contraseña) en vez de Windows, cambia por:
`Server=TU_SERVIDOR;Database=VacanJef;User Id=sa;Password=TU_PASSWORD;TrustServerCertificate=True;`

## 4. Restaurar paquetes y ejecutar

```bash
cd VacanJefWeb
dotnet restore
dotnet run
```

Abre la URL que indique la consola (algo como `https://localhost:5001`).

## 5. Cuentas de demostración

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | admin@vacanjef.com | admin123 |
| Empleado (cuidador) | jorge.salinas@vacanjef.com | emp2026 |
| Empleado (recepción) | patricia.luna@vacanjef.com | emp2026 |
| Cliente | carlos.ramirez@gmail.com | cliente123 |
| Cliente | lucia.gomez@hotmail.com | cliente123 |

## 6. Estructura del proyecto

```
VacanJefWeb/
  Controllers/      Cuenta (login), Dashboard, Mascotas, Reservas, Facturas, Monitoreo
  Models/            Entidades EF Core mapeadas 1:1 al esquema SQL existente
  Models/ViewModels/ Formularios y datos de pantalla
  Data/              VacanJefContext (EF Core, sin migraciones automáticas)
  Hubs/              MonitoreoHub (SignalR) para el monitoreo en vivo
  Services/          Servicio en segundo plano que simula métricas de cámaras
  Views/             Razor views, con el mismo tema "Playa · Sol · Arena" del login original
  wwwroot/           CSS y JS compartidos
  sql/               Scripts SQL Server (esquema + autenticación)
```

EF Core está mapeado por **Fluent API** (no Data Annotations) contra los
nombres de tabla y columna exactos de tu script (`snake_case`), así que no
hace falta generar migraciones: las tablas ya existen tal cual.

## 7. Decisiones importantes que tomé

- **Autenticación real**: el script original no traía contraseñas en
  `Clientes` ni `Empleados`. Agregué una tabla `Usuarios` aparte (sin tocar las
  tablas existentes) que enlaza por `id_cliente` / `id_empleado` y guarda solo
  el hash bcrypt de la contraseña (cumple RNF03).
- **Monitoreo en tiempo real (CU-03)**: no hay cámaras físicas conectadas a
  este chat, así que las métricas (actividad, temperatura, última comida) se
  generan cada 4 segundos en `Services/MonitoreoSimuladoService.cs` y se
  envían por SignalR. El único punto que necesitas tocar para conectar
  cámaras/sensores reales es ese archivo — el resto del flujo (hub, vista,
  JS) ya queda funcionando igual.
- **Precio de Estadia vs. Paseo**: Estadia se cobra por día (`precio_base ×
  noches`), Paseo es tarifa fija por visita. Esto se calcula en
  `FacturasController.Generar` y se estima en vivo en el formulario de
  reserva con JavaScript.

## 8. Si el sitio se cierra solo mientras está corriendo

Corregí un bug real: si `MonitoreoSimuladoService` (el que simula las métricas
de cámaras cada 4 segundos) encontraba un error de conexión a SQL Server —
por ejemplo un timeout pasajero — ese error no estaba capturado y, por
comportamiento por defecto de ASP.NET Core, **apagaba toda la aplicación**,
no solo ese servicio. Ahora cualquier error ahí se registra en el log y el
ciclo simplemente reintenta 4 segundos después, sin afectar el resto del
sitio. Si el sitio se te sigue cerrando solo, revisa la ventana "Salida" en
Visual Studio (la entrada `info: VacanJefWeb.Services.MonitoreoSimuladoService`)
para ver el motivo exacto.

## 9. Posibles próximos pasos

- Exportar facturas/reportes a PDF o Excel.
- **Notificaciones por correo** al confirmar/cancelar una reserva (RF10).
- Reemplazar el monitoreo simulado por cámaras IP reales (RTSP) o WebRTC.
- Agregar registro de nuevos clientes desde la propia web (hoy los clientes
  deben existir primero en la tabla `Clientes`).

## 10. Página pública de bienvenida

La raíz del sitio (`/`) ya no es el login: ahora es una página pública
(`HomeController` → `Views/Home/Index.cshtml`) con servicios y precios (traídos
en vivo desde `Tipos_Servicio`), sección "Sobre nosotros", una galería con
bloques de muestra y un bloque de contacto. El botón principal lleva a
"Iniciar sesión" si no hay sesión activa, o a "Ir a mi panel" si ya iniciaste
sesión.

**Datos de contacto con marcador de posición**: el teléfono (`300 000 0000`)
y el correo (`contacto@vacanjef.com`) en esa vista son de ejemplo — reemplázalos
por los datos reales de tu guardería en `Views/Home/Index.cshtml`, sección
`id="contacto"`. Lo mismo con los íconos de la galería: están pensados para
sustituirse por fotos reales (`<img>` en lugar de los `<div class="gallery-ph">`).

