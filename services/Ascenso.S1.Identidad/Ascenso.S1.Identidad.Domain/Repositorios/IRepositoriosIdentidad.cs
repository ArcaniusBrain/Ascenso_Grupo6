using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S1.Identidad.Domain.Repositorios;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerPorCedulaAsync(Cedula cedula, CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Usuario>> ObtenerPorTipoAsync(Guid tipoUsuarioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistenUsuariosConTipoAsync(Guid tipoUsuarioId, CancellationToken cancellationToken = default);
    Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}

public interface ITipoUsuarioRepository
{
    Task<TipoUsuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TipoUsuario?> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TipoUsuario>> ObtenerPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TipoUsuario>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task AgregarAsync(TipoUsuario tipoUsuario, CancellationToken cancellationToken = default);
    void Eliminar(TipoUsuario tipoUsuario);
}

public interface IClienteApiUniversidadRepository
{
    Task<ClienteApiUniversidad?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClienteApiUniversidad?> ObtenerPorClientIdAsync(string clientIdKeycloak, CancellationToken cancellationToken = default);
    Task AgregarAsync(ClienteApiUniversidad cliente, CancellationToken cancellationToken = default);
}

public interface IDesafioSegundoFactorRepository
{
    Task<DesafioSegundoFactor?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DesafioSegundoFactor>> ObtenerActivosPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task AgregarAsync(DesafioSegundoFactor desafio, CancellationToken cancellationToken = default);
}

public interface IUnidadTrabajoIdentidad
{
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
