using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S1.Identidad.Domain.Eventos;

/// <summary>
/// Evento de dominio registrado al dar de alta una cuenta de postulante (DDD v3.3 §13, DDD-11, flujo 16.4f).
/// </summary>
public sealed record UsuarioRegistradoEvento(
    Guid UsuarioId,
    Cedula Cedula,
    DateTimeOffset Fecha) : IEventoDeDominio;
