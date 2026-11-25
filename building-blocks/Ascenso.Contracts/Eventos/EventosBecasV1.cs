namespace Ascenso.Contracts.Eventos;

public sealed record OfertaEmitidaV1(
    Guid OfertaId,
    Guid ReservaId,
    Guid PostulacionId,
    Guid PostulanteId,
    Guid ProgramaId,
    Guid CicloId,
    string CodigoPeriodo,
    decimal MontoTotalReservado,
    int DuracionPeriodos,
    string Moneda,
    DateOnly FechaLimite);

public sealed record OfertaAceptadaV1(
    Guid OfertaId,
    Guid ReservaId,
    Guid PostulacionId,
    Guid PostulanteId,
    Guid ProgramaId,
    Guid CicloId,
    string CodigoPeriodo,
    decimal MontoTotal,
    string Moneda);

public sealed record OfertaRechazadaV1(
    Guid OfertaId,
    Guid ReservaId,
    Guid PostulacionId,
    Guid ProgramaId,
    Guid CicloId,
    decimal MontoLiberado,
    string Moneda);

public sealed record OfertaExpiradaV1(
    Guid OfertaId,
    Guid ReservaId,
    Guid PostulacionId,
    Guid ProgramaId,
    Guid CicloId,
    decimal MontoLiberado,
    string Moneda);

public sealed record BecaActivadaV1(
    Guid BecaId,
    Guid PostulanteId,
    Guid ProgramaId,
    string CodigoPeriodo);

public sealed record BecaSuspendidaV1(
    Guid BecaId,
    Guid PostulanteId,
    Guid ProgramaId,
    string TipoSuspension,
    DateOnly? FechaRevision);

public sealed record BecaRevocadaV1(
    Guid BecaId,
    Guid PostulanteId,
    Guid ProgramaId,
    decimal MontoNoDesembolsado,
    string Moneda);

public sealed record BecaInactivadaV1(
    Guid BecaId,
    Guid PostulanteId,
    Guid ProgramaId,
    string Motivo);

public sealed record PagoConfirmadoV1(
    Guid BecaId,
    Guid ProgramaId,
    Guid DesembolsoId,
    Guid PagoId,
    decimal Monto,
    string Moneda,
    string Destinatario,
    DateOnly FechaPago);

public sealed record PagoFallidoV1(
    Guid BecaId,
    Guid DesembolsoId,
    Guid PagoId,
    string Motivo);

public sealed record PagoDevueltoV1(
    Guid BecaId,
    Guid DesembolsoId,
    Guid PagoId,
    string Motivo);
