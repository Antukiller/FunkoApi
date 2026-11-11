# 18 — Pruebas .NET y estrategia de testing

## 1. Qué ejecutar según lo que quieras comprobar

| Necesidad | Comando |
|---|---|
| Suite completa .NET + Bruno | `.\Test-All.ps1` |
| Tests .NET unitarios e integración | `dotnet test .\FunkoApiV2Test\FunkoApiV2Test.csproj` |
| Solo unitarios | `dotnet test .\FunkoApiV2Test\FunkoApiV2Test.csproj --filter "FullyQualifiedName~Unit"` |
| Solo integración | `dotnet test .\FunkoApiV2Test\FunkoApiV2Test.csproj --filter "FullyQualifiedName~Integration"` |
| Colección Bruno | `.\Run-Bruno.ps1` |

Ejecuta estos comandos desde la raíz, donde están los scripts y el archivo
`FunkoApiV.2.slnx`.

## 2. Requisitos

- SDK de .NET 10.
- Docker Desktop iniciado para pruebas de integración.
- API en Development y Bruno CLI instalado para la suite completa.
- Cuenta ADMIN de Development para Bruno. `Test-All.ps1` pide el email y la
  contraseña si las variables `FUNKO_ADMIN_EMAIL` y `FUNKO_ADMIN_PASSWORD` no
  están definidas.

Para arrancar dependencias y API en desarrollo en otra terminal:

```powershell
.\Start-Development.ps1
```

El puerto predeterminado es `5798`; se puede cambiar con `-Port`. Si se usa otro
puerto, indícalo también a `Test-All.ps1`:

```powershell
.\Test-All.ps1 -BaseUrl http://localhost:5800
```

`Test-All.ps1` valida la API, ejecuta `dotnet test` y luego Bruno. En Windows,
si la compilación falla con MSB3021/MSB3027 porque `FunkoApiV.2.dll` está en uso,
detén la API con `Ctrl+C` y vuelve a compilar. Para ejecutar después Bruno,
arranca de nuevo la API.

## 3. Estructura real de los tests

```text
FunkoApiV2Test/
├── Unit/          # Controladores, servicios, caché, GraphQL, SignalR,
│                  # infraestructura, mapeadores, seeders, almacenamiento y validadores
├── Integration/   # PostgreSQL, MongoDB, Cassandra y servicios
└── GraphQL/       # Pruebas de integración GraphQL
```

El proyecto usa NUnit, Moq, FluentAssertions y Testcontainers. Las pruebas de
integración arrancan contenedores propios; no necesitan que la API de desarrollo
esté activa, pero sí Docker Desktop.

## 4. Cobertura

```powershell
dotnet test .\FunkoApiV2Test\FunkoApiV2Test.csproj `
  --settings .\coverage.runsettings `
  --collect:"XPlat Code Coverage"
```

El XML de cobertura se genera dentro de
`FunkoApiV2Test\TestResults`. `coverage.runsettings` excluye código generado de
`obj`.

## 5. Estilo de tests

Los tests siguen el patrón Arrange / Act / Assert y separan comportamientos.
Usa las categorías y convenciones ya presentes en `FunkoApiV2Test`; no supongas
que cada clase o test usa exactamente el mismo patrón si el código existente
marca una excepción.

## 6. Diagnóstico rápido

- **Docker no disponible:** inicia Docker Desktop y repite.
- **Bruno no encontrado:** comprueba `bru --version` o instala Bruno CLI desde
  la página oficial.
- **API no responde:** arráncala con `.\Start-Development.ps1` y comprueba
  `http://localhost:5798/api/funkos?page=0&size=1`.
- **401 en Bruno:** verifica las credenciales de una cuenta ADMIN de Development.
- **DLL bloqueada:** detén la instancia de la API antes de compilar el proyecto.

Para el flujo detallado de la colección y credenciales, consulta
[Testing con Bruno](20-bruno-testing.md). Para el paso a paso general, consulta
[la guía práctica](00-guia-practica.md).
