# Despliegue local

Aquí irá el `docker-compose` que levanta el entorno completo (ADR-02): gateway, los seis servicios, Keycloak, RabbitMQ, PostgreSQL (un schema por servicio), MongoDB (proyecciones de lectura), servidor de correo de pruebas, emulador de Firebase y frontend.

Las credenciales van en un `.env` local (ignorado por git). Se documentarán en un `.env.example` sin valores reales.
