# codigos-un-solo-uso Specification

## Purpose
Establecer el servicio único transversal de códigos de verificación de un solo uso (OTP) por operación sensible en ASCENSO (`ADR-10`, `DDD-10`, `R4`–`R6`, `R42`), utilizado para confirmar desembolsos (`HU-20.1`), resolver desembolsos fallidos (`HU-20.2`), registrar o modificar datos de pago (`HU-20.3`), revocar becas (`HU-22.1`) y aplicar consecuencias revocatorias (`HU-25.2`), de forma independiente del segundo factor de inicio de sesión.

## Requirements

### Requirement: Emisión de desafío OTP vinculado a usuario, operación y recurso (HU-20.1, HU-20.3, HU-22.1)

Un usuario autenticado SHALL poder solicitar un código de un solo uso para una operación sensible y un recurso específico eligiendo su canal de entrega (correo o SMS). El sistema MUST generar un código con vigencia de 15 minutos sin revelarlo en la respuesta de solicitud y devolver únicamente el identificador del desafío y su fecha de vencimiento.

#### Scenario: Solicitar desafío OTP por correo o SMS (HU-20.1, HU-20.3)
- **GIVEN** que un usuario autenticado va a confirmar un desembolso o registrar sus datos de pago
- **WHEN** solicita un desafío OTP indicando la operación sensible, el recurso específico y el canal elegido (correo o SMS)
- **THEN** el sistema envía el código por el canal elegido con vigencia de 15 minutos
- **AND** responde únicamente con el identificador del desafío y su instante de vencimiento sin exponer el código

#### Scenario: Solicitar reenvío invalida el código anterior del mismo caso (HU-20.1)
- **GIVEN** que el usuario ya solicitó un código OTP activo para una operación y recurso
- **WHEN** solicita un reenvío de código para la misma operación y recurso
- **THEN** el sistema emite un nuevo desafío con 15 minutos de vigencia e invalida el código anterior

#### Scenario: Fallo del canal de envío impide emitir un desafío utilizable (HU-20.1)
- **GIVEN** que el canal de correo o SMS no logra entregar el mensaje
- **WHEN** el usuario solicita un desafío OTP
- **THEN** el sistema informa que el servicio de envío no está disponible y no registra un desafío usable

### Requirement: Validación estricta y consumo único atómico en frontera privada (HU-20.1, HU-22.1)

El consumo de un código OTP MUST ejecutarse exclusivamente desde un servicio interno autorizado (nunca desde el navegador ni con credenciales de usuario final). El código SHALL validarse contra el usuario, la operación sensible, el recurso y el identificador del desafío, sirviendo una sola vez de forma atómica.

#### Scenario: Consumo exitoso del código vinculado correctamente (HU-20.1 Escenario 2)
- **GIVEN** que existe un desafío OTP vigente para un usuario, operación sensible y recurso determinados
- **WHEN** el servicio autorizado solicita validar y consumir ese desafío con el código correcto y los mismos datos de usuario, operación y recurso
- **THEN** el sistema marca el desafío como consumido de forma atómica y confirma la validación

#### Scenario: Código reutilizado o usado para otro recurso u operación es rechazado (HU-20.1 Escenario 4)
- **GIVEN** que un código OTP ya fue consumido para un desembolso, o fue emitido para otro recurso, otra operación u otro usuario
- **WHEN** se intenta validar ese mismo código para confirmar una operación diferente o por segunda vez
- **THEN** el sistema rechaza el consumo sin autorizar la operación

#### Scenario: Consumo concurrente del mismo código solo permite un ganador (HU-20.1 Escenario 7)
- **GIVEN** que existe un único desafío OTP vigente
- **WHEN** dos solicitudes concurrentes con distinto identificador de operación intentan consumir simultáneamente el mismo código
- **THEN** exactamente una solicitud logra consumir el desafío y la otra es rechazada

#### Scenario: Reintento de red idempotente versus nuevo intento tras fallo de negocio (R42)
- **GIVEN** que un desafío OTP ya fue consumido por una operación identificada por su clave de idempotencia
- **WHEN** el mismo servicio reintenta por corte de red con idéntica clave de idempotencia, y luego llega otro intento con una clave de operación nueva tras un fallo de negocio
- **THEN** el reintento con la misma clave de idempotencia confirma de forma segura que el desafío fue consumido por esa operación
- **AND** el intento con una clave de operación nueva es rechazado exigiendo solicitar un código nuevo

#### Scenario: Token de usuario final no puede invocar el consumo privado de OTP (ADR-10)
- **GIVEN** que un postulante o usuario final autenticado intenta invocar directamente el consumo privado de OTP
- **WHEN** envía la solicitud sin credenciales de servicio autorizadas para consumo de OTP
- **THEN** el sistema deniega el acceso y el desafío permanece sin consumirse

### Requirement: Vigencia de 15 minutos y bloqueo de 1 hora tras 3 intentos fallidos (HU-20.1)

Un código OTP MUST expirar exactamente a los 15 minutos de su emisión. Tras 3 intentos fallidos consecutivos de código sobre la misma operación y recurso, el sistema MUST bloquear nuevos intentos y nuevas solicitudes para esa operación y recurso durante 1 hora completa.

#### Scenario: Código expirado después de 15 minutos es rechazado (HU-20.1 Escenario 12)
- **GIVEN** que han transcurrido más de 15 minutos desde el envío del código OTP
- **WHEN** se intenta validar ese código
- **THEN** el sistema rechaza el código y no autoriza la operación

#### Scenario: Tercer intento fallido bloquea la operación durante una hora (HU-20.1 Escenario 13)
- **GIVEN** que el usuario ya erró dos veces el código OTP para un mismo desembolso u operación
- **WHEN** ingresa un tercer código incorrecto
- **THEN** el sistema rechaza la validación y bloquea nuevas solicitudes o validaciones OTP para ese usuario, operación y recurso durante 1 hora
- **AND** una vez transcurrida la hora vuelve a permitir solicitar un código nuevo
