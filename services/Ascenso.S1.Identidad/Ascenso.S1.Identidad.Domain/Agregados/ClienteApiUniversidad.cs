using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S1.Identidad.Domain.Agregados;

/// <summary>
/// Cliente OAuth2 client_credentials autorizado para una universidad aliada (DDD v3.3 §13, C5).
/// </summary>
public sealed class ClienteApiUniversidad : RaizDeAgregado<Guid>
{
    public Guid UniversidadId { get; private set; }
    public string ClientIdKeycloak { get; private set; } = string.Empty;
    public bool Activo { get; private set; }

    private ClienteApiUniversidad()
    {
    }

    private ClienteApiUniversidad(Guid id, Guid universidadId, string clientIdKeycloak) : base(id)
    {
        UniversidadId = universidadId;
        ClientIdKeycloak = clientIdKeycloak;
        Activo = true;
    }

    public static ClienteApiUniversidad Crear(Guid universidadId, string clientIdKeycloak, Guid? id = null)
    {
        if (universidadId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("La universidad asociada es obligatoria.", "UNIVERSIDAD_OBLIGATORIA");
        }

        var limpio = (clientIdKeycloak ?? string.Empty).Trim();
        if (limpio.Length == 0)
        {
            throw new ExcepcionDeDominio("El ClientId de Keycloak es obligatorio.", "CLIENT_ID_OBLIGATORIO");
        }

        return new ClienteApiUniversidad(id ?? Guid.NewGuid(), universidadId, limpio);
    }

    public bool PuedeEnviarPor(Guid universidadId) => Activo && UniversidadId == universidadId;

    public void Revocar()
    {
        if (!Activo)
        {
            return;
        }

        Activo = false;
        IncrementarVersion();
    }
}
