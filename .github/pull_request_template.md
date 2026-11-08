## Qué cambia

<!-- Una o dos frases. -->

## Referencias

<!-- HU, ADR, regla del cliente o cambio de OpenSpec. Ej.: HU-15.1, ADR-03, R11 -->

- 

## Tipo

- [ ] Funcionalidad
- [ ] Corrección
- [ ] Documentación (HU, DDD, ADR)
- [ ] Pruebas
- [ ] Configuración / CI

## Cómo se probó

<!-- Pruebas unitarias, de integración o pasos manuales. -->

## Lista de control

- [ ] La rama sale de `main` actualizada.
- [ ] Las reglas de negocio están en el dominio, no en handlers ni controladores.
- [ ] No se lee ni se escribe el schema de otro servicio.
- [ ] Toda decisión nueva deja `RegistroDecision` (actor, fecha, justificación).
- [ ] Hay pruebas para lo nuevo y pasan.
- [ ] No hay secretos ni credenciales en el cambio.
- [ ] Si cambia una decisión de arquitectura o del modelo, se actualizó el ADR o el DDD.
