# 📄 doc/18 — Estrategia de Testing

---

## 1. Stack de testing

| Paquete | Versión | Uso |
|---------|---------|-----|
| NUnit | 4.6.1 | Framework de tests |
| Moq | 4.21.0 | Mocks |
| FluentAssertions | 7.2.2 | Aserciones |
| Testcontainers.PostgreSql | 4.15.0 | Integración con BD real |
| coverlet.collector | 10.0.1 | Cobertura de código |

---

## 2. Estructura de tests

```
FunkoApiV2Test/
├── Unit/
│   ├── Controllers/     → FunkoControllerTests, CategoriaControllerTests
│   ├── Services/        → servicios de dominio, autenticación y caché
│   ├── Cache/           → RedisServiceTests
│   ├── Config/          → FunkoConfigTests
│   ├── Infrastructure/ → IdentityConfigTests
│   ├── Mappers/         → FunkoMapperTests, CategoriaMapperTests
│   ├── Validators/      → FunkoCreateValidatorTests, CategoriaValidatorTests
│   └── Entity/          → AppDbContextModelTests
└── Integration/
    ├── Repository/
    │   ├── Postgres/    → FunkoRepositoryTests, CategoriaRepositoryTests
    │   └── Mongo/       → ProveedorRepositoryBaseTest
    └── Services/
        ├── FunkoServiceIntegrationTests.cs
        └── CategoriaServiceIntegrationTests.cs
```

---

## 3. Estilo de test (estilo profe)

```csharp
[TestFixture]
[Category("Unit")]
public class FunkoServiceTests
{
    private Mock<IFunkoRepository> _repo = null!;
    private FunkoService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IFunkoRepository>();
        _service = new FunkoService(_repo.Object, ...);
    }

    [Test]
    public async Task Add_ConDatosValidos_CreaFunko()
    {
        // Arrange
        var request = new FunkoCreateRequest { Name = "Test", ... };

        // Act
        var resultado = await _service.Add(request);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Name.Should().Be("Test");
    }
}
```

**Reglas:**
- Nombre: `{Método}_{Escenario}_{ResultadoEsperado}` en español
- Estructura: `// Arrange` / `// Act` / `// Assert`
- Aserciones: FluentAssertions (`.Should()`), nunca `Assert.AreEqual`
- Mocks: `private Mock<T> _mock = null!;`
- Un test = un comportamiento

---

## 4. Categorías de tests

```csharp
public const string Unit = "Unit";
public const string Integration = "Integration";
public const string Repository = "Repository";
public const string Service = "Service";
public const string Controller = "Controller";
public const string Validator = "Validator";
public const string Cache = "Cache";
public const string Docker = "Docker";
```

---

## 5. Comandos de ejecución

```powershell
# Todos los tests
dotnet test .\FunkoApiV2Test\FunkoApiV2Test.csproj

# Solo unitarios
dotnet test .\FunkoApiV2Test\FunkoApiV2Test.csproj --filter "FullyQualifiedName~Unit"

# Solo integración (requiere Docker)
dotnet test .\FunkoApiV2Test\FunkoApiV2Test.csproj --filter "FullyQualifiedName~Integration"

# Con cobertura
dotnet test .\FunkoApiV2Test\FunkoApiV2Test.csproj `
  --settings .\FunkoApiV2Test\coverage.runsettings `
  --collect:"XPlat Code Coverage"

# Regenerar el informe HTML con el XML Cobertura más reciente
reportgenerator `
  "-reports:.\FunkoApiV2Test\TestResults\*\coverage.cobertura.xml" `
  "-targetdir:.\FunkoApiV2Test\coveragereport" `
  "-reporttypes:Html"
```

`coverage.runsettings` excluye `obj/**/*.cs` porque son archivos generados durante
la compilación y no código mantenido por el proyecto.

---

## 6. Tests de integración (TestContlements)

Requieren Docker. Usan PostgreSQL real en contenedor:

```csharp
private readonly PostgreSqlContainer _postgres =
    new PostgreSqlBuilder("postgres:16-alpine").Build();

[OneTimeSetUp]
public async Task ClassSetUp() => await _postgres.StartAsync();
```