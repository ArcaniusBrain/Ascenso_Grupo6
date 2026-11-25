namespace Ascenso.Contracts.Eventos;

public sealed record CicloPublicadoV1(
    Guid CicloId,
    Guid ProgramaId,
    string CodigoPeriodo,
    int CupoNuevos,
    DateOnly Apertura,
    DateOnly Cierre);

public sealed record PeriodoEvaluacionAbiertoV1(
    Guid CicloId,
    Guid ProgramaId,
    string CodigoPeriodo);

public sealed record PostulacionSeleccionadaV1(
    Guid PostulacionId,
    Guid PostulanteId,
    Guid ProgramaId,
    Guid CicloId,
    string CodigoPeriodo,
    string OrigenSeleccion);

public sealed record SugerenciaListaEsperaV1(
    Guid CicloId,
    Guid ProgramaId,
    Guid PostulacionSugeridaId,
    int PuestoListaEspera);
