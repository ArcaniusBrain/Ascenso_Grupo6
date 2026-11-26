using Ascenso.Contracts.Eventos;
using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using MediatR;

namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Presupuesto;

public sealed record ReservarOfertaInternaCommand(
    Guid ProgramaId,
    Guid PostulacionId,
    Guid CicloId,
    Guid OperationId,
    string? CodigoPeriodo = null) : IRequest<ReservaInternaResponseDto>;

public sealed class ReservarOfertaInternaHandler
    : IRequestHandler<ReservarOfertaInternaCommand, ReservaInternaResponseDto>
{
    private readonly IProgramaRepository _programaRepo;
    private readonly IPresupuestoProgramaRepository _presupuestoRepo;
    private readonly ICalendarioAcademicoRepository _calendarioRepo;
    private readonly IEjecutorConcurrenciaPresupuesto _ejecutor;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public ReservarOfertaInternaHandler(
        IProgramaRepository programaRepo,
        IPresupuestoProgramaRepository presupuestoRepo,
        ICalendarioAcademicoRepository calendarioRepo,
        IEjecutorConcurrenciaPresupuesto ejecutor,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _programaRepo = programaRepo;
        _presupuestoRepo = presupuestoRepo;
        _calendarioRepo = calendarioRepo;
        _ejecutor = ejecutor;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public Task<ReservaInternaResponseDto> Handle(
        ReservarOfertaInternaCommand request,
        CancellationToken cancellationToken) =>
        _ejecutor.EjecutarConReintentoAsync(async ct =>
        {
            var programa = await _programaRepo.ObtenerPorIdAsync(request.ProgramaId, ct)
                ?? throw new ExcepcionDeDominio("El programa de becas indicado no está registrado.", "PROGRAMA_NO_ENCONTRADO");

            var ahoraUtc = _reloj.AhoraUtc;
            var presupuesto = await _presupuestoRepo.ObtenerPorProgramaIdAsync(programa.Id, ct);
            if (presupuesto is null)
            {
                presupuesto = PresupuestoPrograma.Crear(programa.Id, programa.CupoBecarios, programa.MontoPorPeriodo.Moneda, ahoraUtc);
                await _presupuestoRepo.AgregarAsync(presupuesto, ct);
            }

            // S2 determina el periodo y el monto total autoritativamente (R32/A2: toda la duración de la beca).
            string codigoPeriodo;
            if (!string.IsNullOrWhiteSpace(request.CodigoPeriodo))
            {
                codigoPeriodo = new CodigoPeriodo(request.CodigoPeriodo).Valor;
            }
            else
            {
                var calendario = await _calendarioRepo.ObtenerInstitucionalAsync(ct);
                codigoPeriodo = calendario?.PeriodoActual()?.Codigo.Valor ?? programa.PeriodoInicio.Valor;
            }

            var montoTotal = new Dinero(
                programa.MontoPorPeriodo.Cantidad * programa.DuracionPeriodos,
                programa.MontoPorPeriodo.Moneda);

            var reserva = presupuesto.ReservarOferta(
                request.PostulacionId,
                request.CicloId,
                codigoPeriodo,
                montoTotal,
                programa.DuracionPeriodos,
                request.OperationId,
                ahoraUtc);

            await _unidadTrabajo.GuardarCambiosAsync(ct);

            return MapearRespuesta(reserva);
        }, cancellationToken);

    internal static ReservaInternaResponseDto MapearRespuesta(ReservaPresupuesto r) =>
        new(
            r.Id,
            r.EstadoContratoInterno(),
            r.Monto.Cantidad,
            r.DuracionPeriodos,
            r.Monto.Moneda);
}

public sealed record ReservarRenovacionInternaCommand(
    Guid ProgramaId,
    Guid BecaId,
    string CodigoPeriodo,
    decimal MontoSolicitado,
    string Moneda,
    Guid OperationId) : IRequest<ReservaInternaResponseDto>;

public sealed class ReservarRenovacionInternaHandler
    : IRequestHandler<ReservarRenovacionInternaCommand, ReservaInternaResponseDto>
{
    private readonly IProgramaRepository _programaRepo;
    private readonly IPresupuestoProgramaRepository _presupuestoRepo;
    private readonly IEjecutorConcurrenciaPresupuesto _ejecutor;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public ReservarRenovacionInternaHandler(
        IProgramaRepository programaRepo,
        IPresupuestoProgramaRepository presupuestoRepo,
        IEjecutorConcurrenciaPresupuesto ejecutor,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _programaRepo = programaRepo;
        _presupuestoRepo = presupuestoRepo;
        _ejecutor = ejecutor;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public Task<ReservaInternaResponseDto> Handle(
        ReservarRenovacionInternaCommand request,
        CancellationToken cancellationToken) =>
        _ejecutor.EjecutarConReintentoAsync(async ct =>
        {
            var programa = await _programaRepo.ObtenerPorIdAsync(request.ProgramaId, ct)
                ?? throw new ExcepcionDeDominio("El programa de becas indicado no está registrado.", "PROGRAMA_NO_ENCONTRADO");

            var codigoPeriodo = new CodigoPeriodo(request.CodigoPeriodo);
            var ahoraUtc = _reloj.AhoraUtc;

            var presupuesto = await _presupuestoRepo.ObtenerPorProgramaIdAsync(programa.Id, ct);
            if (presupuesto is null)
            {
                presupuesto = PresupuestoPrograma.Crear(programa.Id, programa.CupoBecarios, programa.MontoPorPeriodo.Moneda, ahoraUtc);
                await _presupuestoRepo.AgregarAsync(presupuesto, ct);
            }

            if (request.MontoSolicitado <= 0m)
            {
                throw new ExcepcionDeDominio("El monto de la renovación debe ser mayor que cero.", "MONTO_RESERVA_INVALIDO");
            }

            var moneda = string.IsNullOrWhiteSpace(request.Moneda) ? programa.MontoPorPeriodo.Moneda : request.Moneda;
            var reserva = presupuesto.ReservarRenovacion(
                request.BecaId,
                codigoPeriodo,
                new Dinero(request.MontoSolicitado, moneda),
                request.OperationId,
                ahoraUtc);

            await _unidadTrabajo.GuardarCambiosAsync(ct);

            return ReservarOfertaInternaHandler.MapearRespuesta(reserva);
        }, cancellationToken);
}

public sealed record LiberarReservaInternaCommand(
    Guid ReservaId,
    Guid OperationId,
    MotivoLiberacionReserva Motivo,
    decimal? MontoNoDesembolsadoRevocacion = null) : IRequest<ReservaInternaResponseDto>;

public sealed class LiberarReservaInternaHandler
    : IRequestHandler<LiberarReservaInternaCommand, ReservaInternaResponseDto>
{
    public const string NombreEventoCupoLiberado = "CupoLiberado.v1";

    private readonly IPresupuestoProgramaRepository _presupuestoRepo;
    private readonly IEscritorOutboxCatalogo _outbox;
    private readonly IEjecutorConcurrenciaPresupuesto _ejecutor;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public LiberarReservaInternaHandler(
        IPresupuestoProgramaRepository presupuestoRepo,
        IEscritorOutboxCatalogo outbox,
        IEjecutorConcurrenciaPresupuesto ejecutor,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _presupuestoRepo = presupuestoRepo;
        _outbox = outbox;
        _ejecutor = ejecutor;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public Task<ReservaInternaResponseDto> Handle(
        LiberarReservaInternaCommand request,
        CancellationToken cancellationToken) =>
        _ejecutor.EjecutarConReintentoAsync(async ct =>
        {
            if (request.OperationId == Guid.Empty)
            {
                throw new ExcepcionDeDominio("Indica el operationId de idempotencia.", "OPERATION_ID_INVALIDO");
            }

            if (!Enum.IsDefined(request.Motivo))
            {
                throw new ExcepcionDeDominio("Indica un motivo válido de liberación.", "MOTIVO_LIBERACION_INVALIDO");
            }

            var presupuesto = await _presupuestoRepo.ObtenerPorReservaIdAsync(request.ReservaId, ct)
                ?? throw new ExcepcionDeDominio("La reserva indicada no está registrada.", "RESERVA_NO_ENCONTRADA");

            var ahoraUtc = _reloj.AhoraUtc;
            var resultado = presupuesto.LiberarReserva(
                request.ReservaId,
                request.Motivo,
                ahoraUtc,
                request.MontoNoDesembolsadoRevocacion);

            if (!resultado.YaEstabaLiberada)
            {
                if (resultado.SeLiberoCupo)
                {
                    var evento = new CupoLiberadoV1(
                        presupuesto.ProgramaId,
                        resultado.Reserva.CicloId,
                        resultado.Reserva.Id,
                        request.Motivo.ToString(),
                        resultado.MontoLiberado,
                        presupuesto.Moneda,
                        ahoraUtc);

                    var correlationId = Guid.NewGuid();
                    await _outbox.RegistrarEventoAsync(
                        Guid.NewGuid(),
                        NombreEventoCupoLiberado,
                        schemaVersion: 1,
                        evento,
                        correlationId: correlationId,
                        causationId: request.OperationId,
                        occurredAtUtc: ahoraUtc,
                        ct: ct);
                }

                await _unidadTrabajo.GuardarCambiosAsync(ct);
            }

            return ReservarOfertaInternaHandler.MapearRespuesta(resultado.Reserva);
        }, cancellationToken);
}

public sealed record VincularReservaCommand(
    Guid ReservaId,
    Guid? RecursoId = null) : IRequest<ReservaInternaResponseDto>;

public sealed class VincularReservaHandler : IRequestHandler<VincularReservaCommand, ReservaInternaResponseDto>
{
    private readonly IPresupuestoProgramaRepository _presupuestoRepo;
    private readonly IEjecutorConcurrenciaPresupuesto _ejecutor;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public VincularReservaHandler(
        IPresupuestoProgramaRepository presupuestoRepo,
        IEjecutorConcurrenciaPresupuesto ejecutor,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _presupuestoRepo = presupuestoRepo;
        _ejecutor = ejecutor;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public Task<ReservaInternaResponseDto> Handle(VincularReservaCommand request, CancellationToken cancellationToken) =>
        _ejecutor.EjecutarConReintentoAsync(async ct =>
        {
            var presupuesto = await _presupuestoRepo.ObtenerPorReservaIdAsync(request.ReservaId, ct)
                ?? throw new ExcepcionDeDominio("La reserva indicada no está registrada.", "RESERVA_NO_ENCONTRADA");

            var reserva = presupuesto.VincularReserva(request.ReservaId, request.RecursoId, _reloj.AhoraUtc);
            await _unidadTrabajo.GuardarCambiosAsync(ct);
            return ReservarOfertaInternaHandler.MapearRespuesta(reserva);
        }, cancellationToken);
}

public sealed record ConfirmarReservaCommand(Guid ReservaId) : IRequest<ReservaInternaResponseDto>;

public sealed class ConfirmarReservaHandler : IRequestHandler<ConfirmarReservaCommand, ReservaInternaResponseDto>
{
    private readonly IPresupuestoProgramaRepository _presupuestoRepo;
    private readonly IEjecutorConcurrenciaPresupuesto _ejecutor;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public ConfirmarReservaHandler(
        IPresupuestoProgramaRepository presupuestoRepo,
        IEjecutorConcurrenciaPresupuesto ejecutor,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _presupuestoRepo = presupuestoRepo;
        _ejecutor = ejecutor;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public Task<ReservaInternaResponseDto> Handle(ConfirmarReservaCommand request, CancellationToken cancellationToken) =>
        _ejecutor.EjecutarConReintentoAsync(async ct =>
        {
            var presupuesto = await _presupuestoRepo.ObtenerPorReservaIdAsync(request.ReservaId, ct)
                ?? throw new ExcepcionDeDominio("La reserva indicada no está registrada.", "RESERVA_NO_ENCONTRADA");

            var existente = presupuesto.Reservas.First(r => r.Id == request.ReservaId);
            var reserva = existente.Tipo == TipoReservaPresupuesto.OFERTA_NUEVA
                ? presupuesto.ConfirmarOtorgamiento(request.ReservaId, _reloj.AhoraUtc)
                : presupuesto.ConfirmarRenovacion(request.ReservaId, _reloj.AhoraUtc);

            await _unidadTrabajo.GuardarCambiosAsync(ct);
            return ReservarOfertaInternaHandler.MapearRespuesta(reserva);
        }, cancellationToken);
}

public sealed record RegistrarCupoCicloCommand(
    Guid ProgramaId,
    Guid CicloId,
    int Limite) : IRequest<Unit>;

public sealed class RegistrarCupoCicloHandler : IRequestHandler<RegistrarCupoCicloCommand, Unit>
{
    private readonly IProgramaRepository _programaRepo;
    private readonly IPresupuestoProgramaRepository _presupuestoRepo;
    private readonly IEjecutorConcurrenciaPresupuesto _ejecutor;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public RegistrarCupoCicloHandler(
        IProgramaRepository programaRepo,
        IPresupuestoProgramaRepository presupuestoRepo,
        IEjecutorConcurrenciaPresupuesto ejecutor,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _programaRepo = programaRepo;
        _presupuestoRepo = presupuestoRepo;
        _ejecutor = ejecutor;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public Task<Unit> Handle(RegistrarCupoCicloCommand request, CancellationToken cancellationToken) =>
        _ejecutor.EjecutarConReintentoAsync(async ct =>
        {
            var programa = await _programaRepo.ObtenerPorIdAsync(request.ProgramaId, ct)
                ?? throw new ExcepcionDeDominio("El programa de becas indicado no está registrado.", "PROGRAMA_NO_ENCONTRADO");

            var ahoraUtc = _reloj.AhoraUtc;
            var presupuesto = await _presupuestoRepo.ObtenerPorProgramaIdAsync(programa.Id, ct);
            if (presupuesto is null)
            {
                presupuesto = PresupuestoPrograma.Crear(programa.Id, programa.CupoBecarios, programa.MontoPorPeriodo.Moneda, ahoraUtc);
                await _presupuestoRepo.AgregarAsync(presupuesto, ct);
            }

            presupuesto.RegistrarCupoCiclo(request.CicloId, request.Limite, ahoraUtc);
            await _unidadTrabajo.GuardarCambiosAsync(ct);
            return Unit.Value;
        }, cancellationToken);
}

public sealed record LiberarBecaRevocadaCommand(
    Guid ProgramaId,
    decimal MontoNoDesembolsado,
    string Moneda,
    Guid? CicloId,
    Guid ReferenciaId) : IRequest<Unit>;

public sealed class LiberarBecaRevocadaHandler : IRequestHandler<LiberarBecaRevocadaCommand, Unit>
{
    private readonly IPresupuestoProgramaRepository _presupuestoRepo;
    private readonly IEscritorOutboxCatalogo _outbox;
    private readonly IEjecutorConcurrenciaPresupuesto _ejecutor;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public LiberarBecaRevocadaHandler(
        IPresupuestoProgramaRepository presupuestoRepo,
        IEscritorOutboxCatalogo outbox,
        IEjecutorConcurrenciaPresupuesto ejecutor,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _presupuestoRepo = presupuestoRepo;
        _outbox = outbox;
        _ejecutor = ejecutor;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public Task<Unit> Handle(LiberarBecaRevocadaCommand request, CancellationToken cancellationToken) =>
        _ejecutor.EjecutarConReintentoAsync(async ct =>
        {
            var presupuesto = await _presupuestoRepo.ObtenerPorProgramaIdAsync(request.ProgramaId, ct)
                ?? throw new ExcepcionDeDominio("El presupuesto del programa indicado no está registrado.", "PROGRAMA_NO_ENCONTRADO");

            var ahoraUtc = _reloj.AhoraUtc;
            var monto = new Dinero(request.MontoNoDesembolsado, request.Moneda);
            presupuesto.LiberarBecaRevocada(monto, request.CicloId, request.ReferenciaId, ahoraUtc);

            var evento = new CupoLiberadoV1(
                presupuesto.ProgramaId,
                request.CicloId,
                request.ReferenciaId,
                MotivoLiberacionReserva.REVOCACION.ToString(),
                monto.Cantidad,
                presupuesto.Moneda,
                ahoraUtc);

            var correlationId = Guid.NewGuid();
            await _outbox.RegistrarEventoAsync(
                Guid.NewGuid(),
                LiberarReservaInternaHandler.NombreEventoCupoLiberado,
                schemaVersion: 1,
                evento,
                correlationId: correlationId,
                causationId: request.ReferenciaId,
                occurredAtUtc: ahoraUtc,
                ct: ct);

            await _unidadTrabajo.GuardarCambiosAsync(ct);
            return Unit.Value;
        }, cancellationToken);
}

public sealed record RegistrarDesembolsoPresupuestoCommand(
    Guid ProgramaId,
    decimal Monto,
    string Moneda) : IRequest<Unit>;

public sealed class RegistrarDesembolsoPresupuestoHandler : IRequestHandler<RegistrarDesembolsoPresupuestoCommand, Unit>
{
    private readonly IPresupuestoProgramaRepository _presupuestoRepo;
    private readonly IEjecutorConcurrenciaPresupuesto _ejecutor;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public RegistrarDesembolsoPresupuestoHandler(
        IPresupuestoProgramaRepository presupuestoRepo,
        IEjecutorConcurrenciaPresupuesto ejecutor,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _presupuestoRepo = presupuestoRepo;
        _ejecutor = ejecutor;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public Task<Unit> Handle(RegistrarDesembolsoPresupuestoCommand request, CancellationToken cancellationToken) =>
        _ejecutor.EjecutarConReintentoAsync(async ct =>
        {
            var presupuesto = await _presupuestoRepo.ObtenerPorProgramaIdAsync(request.ProgramaId, ct)
                ?? throw new ExcepcionDeDominio("El presupuesto del programa indicado no está registrado.", "PROGRAMA_NO_ENCONTRADO");

            presupuesto.RegistrarDesembolso(new Dinero(request.Monto, request.Moneda), _reloj.AhoraUtc);
            await _unidadTrabajo.GuardarCambiosAsync(ct);
            return Unit.Value;
        }, cancellationToken);
}
