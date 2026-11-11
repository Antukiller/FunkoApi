# 20 — Pruebas HTTP con Bruno

## 1. Antes de ejecutar

1. Inicia Docker Desktop.
2. Desde la raíz, ejecuta `.\Start-Development.ps1` y deja esa terminal abierta.
3. Comprueba que la API responde en `http://localhost:5798`.
4. Instala Bruno CLI si aún no está disponible: `bru --version`.

La colección está en `Bruno/` y el entorno `Local` apunta a
`http://localhost:5798`. Para otro puerto, usa `Test-All.ps1 -BaseUrl` o cambia
la URL del entorno local.

## 2. Ejecutar colección Bruno

Desde la raíz, establece credenciales ADMIN solo para esta sesión PowerShell,
ejecuta y luego quítalas:

```powershell
$env:FUNKO_ADMIN_EMAIL = "admin@admin.com"
$env:FUNKO_ADMIN_PASSWORD = "Admin123!"
.\Run-Bruno.ps1
Remove-Item Env:FUNKO_ADMIN_EMAIL, Env:FUNKO_ADMIN_PASSWORD
```

La cuenta ADMIN anterior es una cuenta de muestra que el seeder crea en
Development. Si has cambiado su contraseña, utiliza la nueva. `Run-Bruno.ps1`
no pide credenciales: requiere las dos variables de entorno. No guardes
contraseñas en archivos `.bru`.

También se puede ejecutar solo la colección con Bruno CLI:

```powershell
Push-Location .\Bruno
bru run -r --env-file .\environments\Local.bru
Pop-Location
```

La ejecución requiere las mismas variables `FUNKO_ADMIN_EMAIL` y
`FUNKO_ADMIN_PASSWORD`.

## 3. Suite completa

Para ejecutar primero .NET y luego Bruno:

```powershell
.\Test-All.ps1
```

Requiere Docker Desktop, la API Development activa en el puerto del parámetro
`BaseUrl` y Bruno CLI. Si no hay credenciales en el entorno, el script las pide
sin escribirlas en la colección. Para un puerto distinto:

```powershell
.\Test-All.ps1 -BaseUrl http://localhost:5800
```

Las pruebas de integración .NET usan Testcontainers. Si una compilación informa
que `FunkoApiV.2.dll` está bloqueada, detén la instancia de la API con `Ctrl+C`,
vuelve a ejecutar las pruebas y arranca la API de nuevo antes de correr solo
Bruno.

## 4. Contenido de la colección

| Carpeta | Cobertura |
|---|---|
| `0 - Setup` | Inicio de sesión ADMIN, creación de proveedor de prueba y acceso protegido sin token (401). |
| `1 - Categorias` | Listado, alta, actualización, eliminación, validación y errores 404/409. |
| `2 - Funkos` | Listado, lectura protegida, CRUD, filtros, imagen, validación y errores 404. |
| `3 - Proveedores` | Listado, lectura, actualización, validación y error 404. |
| `90 - Teardown` | Eliminación del proveedor de prueba. |

La colección usa IDs guardados por el flujo de setup. Ejecútala completa y
ordenada con `bru run -r`; ejecutar archivos aislados puede fallar si no existen
las variables que producen las peticiones previas. En la ejecución de referencia
actual hay 34 peticiones y 80 comprobaciones; la cantidad puede cambiar al
ampliar la colección.

## 5. Seguridad y limpieza

- Los tokens se obtienen durante el login; no están escritos en la colección.
- La contraseña se pasa mediante variables de proceso.
- `Local.bru` contiene la URL local y variables no secretas.
- Las pruebas crean y borran registros de prueba. No ejecutes la colección
  apuntando a producción ni a una base de datos con datos que deban conservarse.

Para el orden general de trabajo, consulta
[la guía práctica](00-guia-practica.md); para revisar archivos de la colección,
consulta [`Bruno/README.md`](../Bruno/README.md).
