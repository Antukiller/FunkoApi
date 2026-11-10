# 📄 doc/19 — Docker y Docker Compose

---

## 1. Servicios en `compose.yaml`

```yaml
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: funko
      POSTGRES_USER: admin
      POSTGRES_PASSWORD: admin
    ports:
      - "5432:5432"

  mongodb:
    image: mongo:7
    environment:
      MONGO_INITDB_DATABASE: funko
    ports:
      - "27017:27017"

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  api:
    build: .
    ports:
      - "8080:8080"
    environment:
      ConnectionStrings__DefaultConnection: Host=postgres;Database=funko;Username=admin;Password=admin
      ConnectionStrings__Mongo: mongodb://mongodb:27017
      Cache__Redis: redis:6379
    depends_on:
      - postgres
      - mongodb
      - redis
```

---

## 2. Dockerfile multi-stage

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["FunkoApiV.2/FunkoApiV.2.csproj", "FunkoApiV.2/"]
RUN dotnet restore "FunkoApiV.2/FunkoApiV.2.csproj"
COPY . .
RUN dotnet publish "FunkoApiV.2/FunkoApiV.2.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN adduser --system --uid 1654 appuser
COPY --from=build /app/publish .
USER appuser
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "FunkoApiV.2.dll"]
```

---

## 3. Comandos útiles

```powershell
# Levantar todo el stack
docker-compose -f compose.yaml up -d

# Detener y borrar volúmenes
docker-compose -f compose.yaml down -v

# Reconstruir y levantar
docker-compose -f compose.yaml up -d --build

# Ver logs
docker-compose -f compose.yaml logs -f api
```

---

## 4. Health checks

Cada servicio tiene healthcheck para verificar disponibilidad antes de iniciar la API.