# reportes-impacto-donantes Specification

## Purpose
Establecer el agregado `ConsentimientoDonante`, la capa única de agregación y anonimización, y la generación asíncrona de reportes de impacto por donante en `S6` (`RF-33`: `HU-33.1` y `HU-33.2`, `R36/A6`).

## Requirements

### Requirement: Registro de consentimiento irrevocable del becario por donante (HU-33.2 y R36/A6)

Un becario SHALL poder registrar explícitamente su consentimiento para ser presentado de forma individual a un donante específico, conservando un único registro vigente e irrevocable por `(becario, donante)` con su fecha original. Ningún otro rol MUST poder registrar el consentimiento en nombre del becario.

#### Scenario: Becario acepta una vez y reconfirmar no duplica ni altera la fecha (HU-33.2 Escenarios 1, 2 y 4)
- **GIVEN** que Carlos es becario y no ha registrado consentimiento para "Fundación Horizonte", y Ana nunca registró consentimiento
- **WHEN** Carlos confirma su aceptación el 01-oct-2026 y vuelve a confirmar días después
- **THEN** el sistema conserva un único registro vigente con fecha 01-oct-2026 sin duplicarlo ni permitir retiro
- **AND** Carlos queda habilitado para presentación individual ante "Fundación Horizonte", mientras Ana permanece solo en datos agregados

#### Scenario: Otro rol no puede registrar consentimiento por el becario (HU-33.2 Escenario 3)
- **GIVEN** que Ana es becaria
- **WHEN** un coordinador, Dirección u otro becario intenta registrar la aceptación en nombre de Ana
- **THEN** el sistema niega la operación (`403 Forbidden`) sin crear ningún registro

### Requirement: Generación asíncrona del reporte de impacto con anonimización por diseño (HU-33.1)

El personal interno autorizado (`DIRECCION` y `COORDINADOR` sobre sus programas) SHALL poder solicitar en segundo plano el reporte de impacto de un donante para un periodo registrado del calendario. El reporte MUST producirse mediante una única capa de anonimización sin datos socioeconómicos individuales, contando solo desembolsos confirmados.

#### Scenario: Reporte de uno o varios programas con anonimización estricta y consentimiento selectivo (HU-33.1 Escenarios 1, 3, 4, 5, 6 y 10)
- **GIVEN** que "Fundación Horizonte" financia "Becas Ingeniería" (con 4.000 USD confirmados y 500 USD fallidos) y "Becas Salud" en 2026-1, donde Carlos aceptó ser presentado y Ana no, y ambos declararon ingresos familiares distintos
- **WHEN** el coordinador genera el reporte de impacto de "Fundación Horizonte" para 2026-1
- **THEN** el reporte incluye ambos programas, muestra 4.000 USD ejecutados (excluyendo los 500 USD fallidos), nunca expone ingresos familiares, composición familiar ni cédula de ningún becario
- **AND** presenta de forma individual solo a Carlos (datos no socioeconómicos) y mantiene a Ana únicamente dentro de las cifras agregadas

#### Scenario: Validaciones de periodo inexistente, donante sin aportes, sin datos académicos y acceso denegado (HU-33.1 Escenarios 2, 7, 8 y 11)
- **GIVEN** que existe el calendario proyectado en `S6` y un evaluador intenta generar reportes
- **WHEN** el evaluador solicita un reporte, o el coordinador solicita un periodo inexistente "2027-1", un donante sin aportes en "2026-1", o un programa sin reportes académicos en "2026-1"
- **THEN** el evaluador recibe acceso denegado (`403 Forbidden`)
- **AND** el periodo inexistente devuelve "El periodo 2027-1 no existe en el calendario académico."
- **AND** el donante sin aportes devuelve "Este donante no tiene aportes registrados en el periodo 2026-1."
- **AND** el programa sin reportes académicos muestra "Aún no hay datos académicos para este periodo."

#### Scenario: Preparación asíncrona en segundo plano sin detener la operación diaria (HU-33.1 Escenarios 9 y 12)
- **GIVEN** que el coordinador solicita generar el reporte de impacto de un donante
- **WHEN** el reporte aún se está preparando mientras otras consultas o solicitudes se atienden en paralelo
- **THEN** el sistema responde de inmediato indicando "El reporte se está preparando." y permite continuar operando sin esperar a que finalice la agregación
- **AND** al completar en segundo plano, el reporte queda disponible para su consulta y entrega al donante
