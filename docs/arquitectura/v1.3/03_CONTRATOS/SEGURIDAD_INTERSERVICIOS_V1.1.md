# Fronteras de seguridad y consistencia de APIs (v1.1)

`openapi.yaml` contiene **solo rutas públicas** que YARP expone; `openapi-interno.yaml` especifica servicios privados no incluidos en rutas YARP. Los service clients usan OAuth2 client_credentials (Keycloak) con audiencias y scopes propios. Un token de usuario final nunca autoriza reservar/liberar ni consumir OTP en la API privada.

**Reserva:** S4 recibe una referencia a postulación seleccionada (S3) y solicita a S2, por puerto privado, una reserva con `(postulacionId,cicloId,programaId,operationId)`; S2 obtiene `Programa.MontoPorPeriodo`, calcula el horizonte aplicable y verifica cupos y presupuesto de forma atómica. No recibe monto desde el navegador. Una única reserva activa por clave de negocio; si falla el guardado de S4, compensación idempotente.

**OTP:** el usuario envía `challengeId + código` al endpoint público de la acción. El handler comprueba autenticación, rol, alcance y estado, y solo entonces llama `IValidadorOtp`→S1 privado con usuario/recurso/operación/operationId. S1 consume atómicamente y responde resultado. S4 confirma dominio y Outbox; si el commit falla, el desafío consumido no puede reutilizarse. El comando es idempotente por `operationId`. Cubrir pago, resolución de pago, datos bancarios, revocación y consecuencia revocatoria HU-25.2.

**HU-01.2:** S3 usa réplica de eventos para vista preliminar, pero antes de recibir postulación hace confirmación autoritativa interna S4 (clave cédula + periodo); si S4 no responde, falla de forma segura.

**Universidades:** Keycloak emite tokens de client_credentials, un cliente por universidad. `codigoEstudiante` pertenece a la universidad y el ACL resuelve `becaId` interno; no se confía solo en `universidadId` de la URL.

**Pruebas diseñadas:** (a) JWT de postulante sobre internal = 401/403; (b) liberar reserva sin scope = 403; (c) dos ofertas mismo par postulación/ciclo = una sola; (d) OTP replay = denegado; (e) réplica académica/costos vencida = rechazo seguro; (f) compensación repetida = sin doble liberación. Sin código, aún no se han ejecutado.


**v1.2:** la consulta privada de beneficios usa `personaId` UUID, nunca cédula en ruta. El control de permisos se aplica por recurso y por token client_credentials.
