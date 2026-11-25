using Ascenso.S1.Identidad.Domain.Eventos;
using Ascenso.SharedKernel.Seguridad;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S1.Identidad.Domain.Agregados;

/// <summary>
/// Raíz de agregado Usuario en S1 (DDD v3.3 §13, R1, ADR-30).
/// Protege unicidad de cédula (con índice único en S1), nunca se borra (solo se desactiva)
/// y mantiene SelloSeguridad para forzar nuevo inicio de sesión obligatorio al cambiar roles o permisos (C14).
/// </summary>
public sealed class Usuario : RaizDeAgregado<Guid>
{
    public const string MensajeRolObligatorio = "Asigna un rol al usuario.";

    private readonly HashSet<Guid> _tipoUsuarioIds = new();
    private readonly HashSet<Guid> _programasAsignados = new();

    public Cedula Cedula { get; private set; } = null!;
    public string NombreCompleto { get; private set; } = string.Empty;
    public Contacto Contacto { get; private set; } = null!;
    public CanalCodigo CanalPreferido { get; private set; }
    public EstadoUsuario Estado { get; private set; }
    public bool EsEvaluadorExterno { get; private set; }
    public Guid? UniversidadId { get; private set; }
    public string? KeycloakUserId { get; private set; }
    public Guid SelloSeguridad { get; private set; }
    public DateTimeOffset? Bloqueado2FaHastaUtc { get; private set; }

    public IReadOnlyCollection<Guid> TipoUsuarioIds => _tipoUsuarioIds;
    public IReadOnlyCollection<Guid> ProgramasAsignados => _programasAsignados;

    private Usuario()
    {
    }

    private Usuario(
        Guid id,
        Cedula cedula,
        string nombreCompleto,
        Contacto contacto,
        CanalCodigo canalPreferido,
        IEnumerable<Guid> tipoUsuarioIds,
        IEnumerable<Guid> programasAsignados,
        Guid? universidadId,
        bool esEvaluadorExterno,
        string? keycloakUserId) : base(id)
    {
        Cedula = cedula;
        NombreCompleto = nombreCompleto;
        Contacto = contacto;
        CanalPreferido = esEvaluadorExterno ? CanalCodigo.CORREO : canalPreferido;
        Estado = EstadoUsuario.ACTIVO;
        EsEvaluadorExterno = esEvaluadorExterno;
        UniversidadId = universidadId;
        KeycloakUserId = string.IsNullOrWhiteSpace(keycloakUserId) ? null : keycloakUserId.Trim();
        SelloSeguridad = Guid.NewGuid();

        foreach (var tipoId in tipoUsuarioIds)
        {
            if (tipoId != Guid.Empty)
            {
                _tipoUsuarioIds.Add(tipoId);
            }
        }

        foreach (var programaId in programasAsignados)
        {
            if (programaId != Guid.Empty)
            {
                _programasAsignados.Add(programaId);
            }
        }
    }

    public static Usuario Crear(
        Cedula cedula,
        string? nombreCompleto,
        Contacto contacto,
        IEnumerable<Guid>? tipoUsuarioIds,
        CanalCodigo canalPreferido = CanalCodigo.CORREO,
        IEnumerable<Guid>? programasAsignados = null,
        Guid? universidadId = null,
        bool esEvaluadorExterno = false,
        string? keycloakUserId = null,
        bool emitirEventoRegistroPostulante = false,
        DateTimeOffset? ahoraUtc = null,
        Guid? id = null)
    {
        ArgumentNullException.ThrowIfNull(cedula);
        ArgumentNullException.ThrowIfNull(contacto);

        var nombreLimpio = (nombreCompleto ?? string.Empty).Trim();
        if (nombreLimpio.Length < 2)
        {
            throw new ExcepcionDeDominio("El nombre completo del usuario es obligatorio.", "USUARIO_NOMBRE_OBLIGATORIO");
        }

        var tipos = (tipoUsuarioIds ?? Array.Empty<Guid>())
            .Where(t => t != Guid.Empty)
            .Distinct()
            .ToList();

        if (tipos.Count == 0)
        {
            throw new ExcepcionDeDominio(MensajeRolObligatorio, "USUARIO_ROL_OBLIGATORIO");
        }

        var usuario = new Usuario(
            id ?? Guid.NewGuid(),
            cedula,
            nombreLimpio,
            contacto,
            canalPreferido,
            tipos,
            programasAsignados ?? Array.Empty<Guid>(),
            universidadId,
            esEvaluadorExterno,
            keycloakUserId);

        if (emitirEventoRegistroPostulante)
        {
            usuario.Registrar(new UsuarioRegistradoEvento(
                usuario.Id,
                usuario.Cedula,
                (ahoraUtc ?? DateTimeOffset.UtcNow).ToUniversalTime()));
        }

        return usuario;
    }

    public bool RequiereSegundoFactor(IEnumerable<TipoUsuario> tiposAsignados)
    {
        ArgumentNullException.ThrowIfNull(tiposAsignados);
        return tiposAsignados.Any(t => _tipoUsuarioIds.Contains(t.Id) && t.ExigeSegundoFactor());
    }

    /// <summary>
    /// Resuelve el canal efectivo del código 2FA (HU-37.2 E3, E8 y E12; R4a, R4e y R4f).
    /// - Recuperación: siempre CORREO (R4e).
    /// - Evaluador externo: siempre CORREO (R4f).
    /// - Resto de usuarios: el canal elegido por la persona (CORREO o SMS, R4a).
    /// </summary>
    public CanalCodigo ResolverCanalSegundoFactor(bool esRecuperacion = false)
    {
        if (esRecuperacion || EsEvaluadorExterno)
        {
            return CanalCodigo.CORREO;
        }

        return CanalPreferido;
    }

    public void CambiarCanalPreferido(CanalCodigo nuevoCanal)
    {
        if (EsEvaluadorExterno && nuevoCanal != CanalCodigo.CORREO)
        {
            throw new ExcepcionDeDominio(
                "Los evaluadores externos reciben el segundo factor exclusivamente por correo electrónico.",
                "CANAL_EVALUADOR_EXTERNO_SOLO_CORREO");
        }

        CanalPreferido = nuevoCanal;
        IncrementarVersion();
    }

    public void AsignarRoles(IEnumerable<Guid> nuevosTipos)
    {
        ArgumentNullException.ThrowIfNull(nuevosTipos);

        var lista = nuevosTipos.Where(t => t != Guid.Empty).Distinct().ToList();
        if (lista.Count == 0)
        {
            throw new ExcepcionDeDominio(MensajeRolObligatorio, "USUARIO_ROL_OBLIGATORIO");
        }

        _tipoUsuarioIds.Clear();
        foreach (var tipoId in lista)
        {
            _tipoUsuarioIds.Add(tipoId);
        }

        // C14: cambiar roles exige nuevo inicio de sesión obligatorio
        SelloSeguridad = Guid.NewGuid();
        IncrementarVersion();
    }

    public void InvalidarSesionesActivas()
    {
        SelloSeguridad = Guid.NewGuid();
        IncrementarVersion();
    }

    public void VincularKeycloak(string keycloakUserId)
    {
        var limpio = (keycloakUserId ?? string.Empty).Trim();
        if (limpio.Length == 0)
        {
            throw new ExcepcionDeDominio("El identificador de Keycloak es obligatorio.", "KEYCLOAK_ID_INVALIDO");
        }

        KeycloakUserId = limpio;
        IncrementarVersion();
    }

    public bool EstaBloqueado2Fa(DateTimeOffset ahoraUtc) =>
        Bloqueado2FaHastaUtc.HasValue && ahoraUtc.ToUniversalTime() < Bloqueado2FaHastaUtc.Value;

    public void RegistrarBloqueo2Fa(DateTimeOffset hastaUtc)
    {
        Bloqueado2FaHastaUtc = hastaUtc.ToUniversalTime();
        IncrementarVersion();
    }

    public void LimpiarBloqueo2Fa()
    {
        if (Bloqueado2FaHastaUtc is not null)
        {
            Bloqueado2FaHastaUtc = null;
            IncrementarVersion();
        }
    }

    public void Desactivar()
    {
        if (Estado == EstadoUsuario.DESACTIVADO)
        {
            return;
        }

        Estado = EstadoUsuario.DESACTIVADO;
        SelloSeguridad = Guid.NewGuid();
        IncrementarVersion();
    }
}
