# 📄 doc/01 — Configuración de Proyectos .NET

Este documento describe la configuración de la solución FunkoApi siguiendo el estándar del profesor José Luis González Sánchez (`joseluisgs`).

---

## 1. Solución (`.slnx`)

Se utiliza el formato moderno de solución `.slnx` en lugar del legacy `.sln`:

```xml
<Solution>
  <Configurations>
    <Platform Name="Any CPU" />
    <Platform Name="x64" />
    <Platform Name="x86" />
  </Configurations>
  <Project Path="FunkoApiV.2/FunkoApiV.2.csproj" />
  <Project Path="FunkoApiV2Test/FunkoApiV2Test.csproj" />
</Solution>
```

---

## 2. `global.json`

Fija la versión del SDK de .NET:

```json
{
  "sdk": {
    "version": "10.0.401",
    "rollForward": "latestFeature"
  }
}
```

---

## 3. `Directory.Build.props`

Configuración global para todos los proyectos de la solución:

```xml
<Project>
  <PropertyGroup>
    <LangVersion>14</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

> ⚠️ `TreatWarningsAsErrors=true` es innegociable. Cualquier warning rompe el build.

---

## 4. `Directory.Packages.props` (Central Package Management)

Todas las versiones de NuGet se centralizan aquí. Los `.csproj` no llevan versión en los `PackageReference`.

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="CSharpFunctionalExtensions" Version="3.7.0" />
    <PackageVersion Include="FluentValidation.AspNetCore" Version="11.3.1" />
    <PackageVersion Include="AutoMapper" Version="16.2.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.14" />
    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.3" />
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.12" />
    <PackageVersion Include="BCrypt.Net-Next" Version="4.2.0" />
    <PackageVersion Include="Swashbuckle.AspNetCore" Version="10.2.3" />
    <PackageVersion Include="Serilog.AspNetCore" Version="10.0.0" />
    <PackageVersion Include="StackExchange.Redis" Version="3.3.1" />
    <PackageVersion Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="10.0.12" />
    <PackageVersion Include="MongoDB.Driver" Version="3.13.0" />
    <PackageVersion Include="MongoDB.EntityFrameworkCore" Version="10.0.4" />
  </ItemGroup>
</Project>
```

---

## 5. `.editorconfig`

Estilo de código C# aplicado a toda la solución:

```ini
root = true

[*]
charset = utf-8
end_of_line = lf
indent_style = space
indent_size = 4
insert_final_newline = true
trim_trailing_whitespace = true

[*.cs]
csharp_new_line_before_open_brace = all
csharp_style_namespace_declarations = file_scoped:suggestion
dotnet_sort_system_directives_first = true
csharp_style_var_for_built_in_types = false:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_prefer_braces = true:suggestion
```

> 📌 **LF, no CRLF**. En Windows configurar Rider/VS para usar LF.

---

## 6. Versiones exactas utilizadas

| Paquete | Versión |
|---------|---------|
| .NET SDK | 10.0.401 |
| C# / LangVersion | 14 |
| Microsoft.EntityFrameworkCore | 10.0.14 |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 |
| MongoDB.EntityFrameworkCore | 10.0.4 |
| MongoDB.Driver | 3.13.0 |
| StackExchange.Redis | 3.3.1 |
| CSharpFunctionalExtensions | 3.7.0 |
| FluentValidation.AspNetCore | 11.3.1 |
| AutoMapper | 16.2.0 |
| Swashbuckle.AspNetCore | 10.2.3 |
| Serilog.AspNetCore | 10.0.0 |

---

## 7. Regla de nombrado de proyectos

- API: `FunkoApiV.2.csproj` (namespace `FunkoApiV._2`)
- Tests: `FunkoApiV2Test.csproj` (namespace `FunkoApiV2Test`)

> ⚠️ Los puntos y guiones bajos en el namespace son inusuales pero se mantienen por consistencia con el proyecto existente.