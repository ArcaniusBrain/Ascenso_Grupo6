# Especificación: Frontend Programas, Presupuesto y Donantes (RF-11, RF-12, RF-13)

## Purpose

Implementar en `frontend/` las pantallas de gestión de programas de becas (`/programas` y `/programas/[id]`) y directorio de donantes e historial de aportes (`/donantes`), conectadas a `S2` exclusivamente mediante el BFF de Next.js (`ADR-28`) y fieles a la pantalla `08 · Coordinador — Programa: presupuesto y cupo` de Figma.

## Requirements

### Requirement: Gestion de programas y vista de presupuesto y cupos (RF-11 y RF-12)

El frontend MUST permitir listar, crear y editar programas de becas, configurar elegibilidad y condiciones de permanencia, y consultar el desglose autoritativo de presupuesto (`Aportes − Otorgado − Ofertas pendientes − Renovaciones pendientes = Disponible`) y cupos directamente desde `S2` vía BFF.

#### Scenario: Consulta de detalle de programa muestra formula de presupuesto y regla R27

- **WHEN** un coordinador o directivo autenticado accede a `/programas/[id]`
- **THEN** la interfaz obtiene el detalle y presupuesto desde `/api/v1/programas/{id}` y `/api/v1/programas/{id}/presupuesto` a través del BFF
- **AND** muestra la franja de fórmula de presupuesto, los aportes por donante, los cupos y la aclaración de que una beca Inactiva mantiene ocupados su cupo y presupuesto mientras una revocada los libera

#### Scenario: Creacion de programa completo deshabilita acumulacion en el formulario

- **WHEN** el usuario selecciona alcance `COMPLETO` al crear o editar un programa
- **THEN** la interfaz fija `permiteAcumulacion = false` y bloquea el selector de acumulación en coherencia con `RF-11`

### Requirement: Directorio de donantes y ledger de aportes filtrable por periodo (RF-13)

El frontend MUST permitir registrar donantes (organizaciones o personas, sin crear cuentas de acceso), consultar su historial de aportes filtrable por periodo y registrar nuevos aportes hacia programas existentes.

#### Scenario: Registro de aporte actualiza presupuesto e historial inmutable

- **WHEN** el usuario registra un aporte desde `/programas/[id]` o `/donantes`
- **THEN** la solicitud se envía vía `/api/bff/api/v1/...` a `S2`
- **AND** al confirmarse, la interfaz recarga el presupuesto del programa o el historial de aportes del donante desde el servidor
