# Despliegue local y base de servidor

Contiene la infraestructura reproducible del proyecto (`ADR-02`, `ADR-05`, `ADR-07`, `ADR-09`, `ADR-13`, `ADR-20`):

- **`docker-compose.yml`**: levanta PostgreSQL 16 (con 6 schemas y roles aislados `s1_identidad..s6_comunicaciones` vía `postgres/init-schemas.sql`), MongoDB 7 (CQRS), RabbitMQ 3 (con consola en `:15672`), Keycloak 26 dev (`:8085`) y Mailpit (`:8025` para correo de pruebas).
- **`.env.example`**: plantilla de variables de entorno sin secretos reales. Copiar a `deploy/.env` antes de levantar.

## Cómo levantar en local

```bash
cd deploy
cp .env.example .env
docker compose up -d
```

## Portabilidad a servidor

Los microservicios `.NET 10` leen todas las cadenas de conexión desde variables de entorno (`IConfiguration`), sin acoplarse a `localhost` ni a Docker: pueden desplegarse tanto sobre un servidor Linux con Docker (añadiendo un `docker-compose.prod.yml` con credenciales reales y TLS) como apuntando a instancias externas gestionadas.
