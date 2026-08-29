# GUÍA DEVOPS — CLUB CANINO VACANJEF
## Presentación SENA · Programa DevOps y Contenedores (Docker)

---

## ANTES DE LA PRESENTACIÓN — REQUISITOS

### Instalar Docker Desktop
1. Ir a https://www.docker.com/products/docker-desktop
2. Descargar para Windows y ejecutar el instalador
3. Reiniciar el equipo cuando lo pida
4. Verificar que funciona: abrir PowerShell y ejecutar:
   ```
   docker --version
   docker compose version
   ```

### Subir el proyecto a GitHub (necesario para mostrar Actions)
```bash
git init
git add .
git commit -m "feat: VacanJef Club Canino - proyecto completo"
git remote add origin https://github.com/TU_USUARIO/vacanjef.git
git push -u origin main
```

---

## ARQUITECTURA DEL PROYECTO (para explicar en la presentación)

```
USUARIO (navegador)
       │
       ↓ http://localhost:8080
┌─────────────────────────────────┐
│   vacanjef-app                  │
│   ASP.NET Core 8                │
│   Puerto: 8080                  │
│   Imagen: mcr.microsoft.com/    │
│           dotnet/aspnet:8.0     │
└─────────────┬───────────────────┘
              │ Red: vacanjef-network
              │ (comunicación interna)
       ↓ Puerto 1433
┌─────────────────────────────────┐
│   vacanjef-sqlserver            │
│   SQL Server 2022 Express       │
│   Puerto: 1433                  │
│   Imagen: mcr.microsoft.com/    │
│           mssql/server:2022     │
└─────────────┬───────────────────┘
              │
       ↓ Volumen Docker
┌─────────────────────────────────┐
│   vacanjef-sqldata              │
│   Volumen persistente           │
│   /var/opt/mssql                │
└─────────────────────────────────┘

PIPELINE CI/CD (GitHub Actions):
GitHub push → Build .NET → Docker Build → Validar Compose
```

---

## PASO A PASO — DEMO EN VIVO (30 minutos)

### FASE 1: INTRODUCCIÓN (5 minutos)

Decir al evaluador:

> "Desarrollamos **Club Canino VacanJef**, un sistema web completo para la gestión de una guardería canina en Medellín. El sistema resuelve el problema de manejar reservas, clientes, mascotas, facturación y monitoreo en tiempo real de manera manual. Lo implementamos con ASP.NET Core 8, SQL Server, y lo contenerizamos con Docker usando una arquitectura de dos servicios comunicados por una red bridge."

**Tecnologías utilizadas:**
- Backend: ASP.NET Core 8 MVC + Entity Framework Core
- Base de datos: SQL Server 2022 (contenedor Docker)
- Tiempo real: SignalR (métricas en vivo de mascotas)
- Contenedores: Docker + Docker Compose
- CI/CD: GitHub Actions
- Control de versiones: Git + GitHub

---

### FASE 2: ARQUITECTURA (10 minutos)

**Mostrar y explicar estos 4 archivos:**

1. **Dockerfile** — abrir en VS Code y explicar:
   - Etapa 1 (build): usa `sdk:8.0` para compilar → más grande pero tiene todas las herramientas
   - Etapa 2 (final): usa `aspnet:8.0` → imagen liviana solo con el runtime
   - Esto es "multi-stage build": reduce el tamaño de la imagen final

2. **docker-compose.yml** — explicar:
   - Dos servicios: `vacanjef-web` y `vacanjef-db`
   - `depends_on` con `healthcheck`: la app espera que la BD esté lista
   - `networks`: los dos contenedores se hablan por nombre (`vacanjef-db`)
   - `volumes`: los datos de SQL Server persisten aunque el contenedor se detenga

3. **.github/workflows/vacanjef-ci.yml** — explicar:
   - Se activa automáticamente en cada `git push`
   - Job 1: compila el proyecto .NET
   - Job 2: construye la imagen Docker
   - Job 3: valida el docker-compose.yml

4. **Flujo de red** — dibujar en la pizarra o mostrar el diagrama de arriba

---

### FASE 3: DEMOSTRACIÓN EN VIVO (10 minutos)

#### PASO 1 — Mostrar el código en GitHub
```
Abrir: https://github.com/TU_USUARIO/vacanjef
Mostrar: commits, branches, carpeta .github/workflows
```

#### PASO 2 — Mostrar el pipeline CI/CD corriendo
```
GitHub → Actions → Ver el workflow "VacanJef CI/CD Pipeline"
Mostrar: los 3 jobs (Build, Docker Build, Validate Compose)
```

#### PASO 3 — Construir los contenedores en vivo
```powershell
# Abrir PowerShell en la carpeta del proyecto
cd C:\Users\El Sarra\Desktop\proyecto\VacanJefWeb

# Construir las imágenes y arrancar los contenedores
docker compose up --build -d

# Ver que los contenedores están corriendo
docker compose ps
```
**Explicar:** "El flag `--build` construye la imagen desde el Dockerfile. El flag `-d` corre en segundo plano."

#### PASO 4 — Cargar la base de datos
```powershell
# Esperar ~30 segundos hasta que SQL Server esté listo, luego:
docker exec -it vacanjef-sqlserver /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "VacanJef2026!" `
  -i /scripts/vacanjef_v2_completo.sql `
  -No
```
**Explicar:** "Con `docker exec` ejecutamos un comando dentro del contenedor de SQL Server. Le pasamos el script SQL que crea toda la base de datos con datos de prueba."

#### PASO 5 — Mostrar los contenedores activos
```powershell
# Listar todos los contenedores corriendo
docker ps

# Ver los logs de la aplicación web
docker logs vacanjef-app --tail 20

# Ver los logs de SQL Server
docker logs vacanjef-sqlserver --tail 10
```

#### PASO 6 — Mostrar redes Docker
```powershell
# Listar redes
docker network ls

# Ver detalles de la red de VacanJef
docker network inspect vacanjef-network
```
**Explicar:** "La red `vacanjef-network` es de tipo bridge. Los dos contenedores se comunican por esta red usando el nombre del servicio como hostname — la app web se conecta a `vacanjef-db:1433` sin necesidad de IPs."

#### PASO 7 — Mostrar volúmenes Docker
```powershell
# Listar volúmenes
docker volume ls

# Ver detalles del volumen de datos
docker volume inspect vacanjef-sqldata
```
**Explicar:** "El volumen `vacanjef-sqldata` guarda los datos de SQL Server en el host. Aunque eliminemos el contenedor, los datos persisten — eso es persistencia con volúmenes Docker."

#### PASO 8 — Mostrar las imágenes Docker
```powershell
# Ver todas las imágenes
docker images

# Ver el tamaño: la imagen final es ~300MB, no 800MB gracias al multi-stage build
docker inspect vacanjef-web:latest | findstr Size
```

#### PASO 9 — Abrir la aplicación en el navegador
```
Abrir: http://localhost:8080

Demostrar:
✓ Página principal (hero, galería, slider infinito, cámaras)
✓ Login como Admin: admin@vacanjef.com / admin123
✓ Panel de bienvenida con KPIs
✓ Dashboard con gráficas
✓ Módulo de Reservas (crear una reserva desde el empleado)
✓ Módulo de Clientes con filtros de fecha
✓ Módulo de Facturación → botón "Ver recibo" (tirilla)
✓ Módulo de Cámaras (confirmar una reserva → ver el token generado)
✓ Módulo de Empleados (calendario de turnos)
✓ Cerrar sesión → Login como Empleado y como Cliente
```

#### PASO 10 — Probar que los datos persisten (bonus impresionante)
```powershell
# Detener y eliminar los contenedores
docker compose down

# Volver a levantar SIN el flag --build (no reconstruye)
docker compose up -d

# Los datos de la BD siguen ahí gracias al volumen
# Abrir http://localhost:8080 → todo funciona igual
```

---

### FASE 4: CONCLUSIONES (5 minutos)

**Dificultades encontradas:**
- La cadena de conexión de SQL Server cambia entre desarrollo local (Windows Auth) y Docker (SQL Auth con usuario SA)
- Solución: variables de entorno en docker-compose.yml sobreescriben el appsettings.json

- SQL Server en Docker tarda ~30 segundos en estar listo
- Solución: healthcheck en el compose — la app espera hasta que la BD responda

- El `@` de Razor colisiona con el `@` de correos en JavaScript
- Solución: escapar como `@@` dentro de bloques Razor

**Aprendizajes adquiridos:**
- Multi-stage build reduce imágenes de 800MB a ~300MB
- Docker Compose simplifica el despliegue de arquitecturas multicontenedor
- Variables de entorno permiten configuraciones distintas sin cambiar código
- healthcheck garantiza que los servicios arranquen en el orden correcto

**Mejoras futuras:**
- Publicar en la nube (Azure App Service + Azure SQL)
- Agregar contenedor de Nginx como reverse proxy
- Configurar HTTPS dentro del contenedor
- Integrar cámaras RTSP reales reemplazando la simulación de SignalR

---

## COMANDOS RÁPIDOS DE REFERENCIA

```powershell
# Arrancar todo
docker compose up --build -d

# Ver estado
docker compose ps

# Ver logs en tiempo real
docker compose logs -f

# Parar sin borrar datos
docker compose stop

# Parar y eliminar contenedores (datos persisten en el volumen)
docker compose down

# Parar, eliminar contenedores Y el volumen (borra la BD)
docker compose down -v

# Entrar al contenedor de la app
docker exec -it vacanjef-app bash

# Entrar al contenedor de SQL Server
docker exec -it vacanjef-sqlserver bash

# Ver uso de recursos
docker stats
```

---

## CREDENCIALES DE PRUEBA..

| Rol | Correo | Contraseña |
|-----|--------|------------|
| Admin | admin@vacanjef.com | admin123 |
| Empleado | jorge.salinas@vacanjef.com | emp2026 |
| Cliente | carlos.ramirez@gmail.com | cliente123 |
| Cliente | lucia.gomez@hotmail.com | cliente123 |

**URL en Docker:** http://localhost:8080

---

*Club Canino VacanJef · SENA CTMA · Programa DevOps y Contenedores · 2026*
