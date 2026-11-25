using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.S1.Identidad.Domain.Repositorios;
using Ascenso.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Ascenso.S1.Identidad.Infrastructure.Persistencia;

public sealed class UsuarioRepositoryEfCore(IdentidadDbContext db) : IUsuarioRepository
{
    public Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Usuarios.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Usuario?> ObtenerPorCedulaAsync(Cedula cedula, CancellationToken cancellationToken = default) =>
        db.Usuarios.SingleOrDefaultAsync(x => x.Cedula == cedula, cancellationToken);

    public Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default)
    {
        var normalizado = (correo ?? string.Empty).Trim().ToLowerInvariant();
        return db.Usuarios.SingleOrDefaultAsync(x => x.Contacto.Correo == normalizado, cancellationToken);
    }

    public async Task<IReadOnlyList<Usuario>> ObtenerPorTipoAsync(Guid tipoUsuarioId, CancellationToken cancellationToken = default)
    {
        var todos = await db.Usuarios.ToListAsync(cancellationToken);
        return todos.Where(u => u.TipoUsuarioIds.Contains(tipoUsuarioId)).ToList();
    }

    public async Task<IReadOnlyList<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
        await db.Usuarios.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<bool> ExistenUsuariosConTipoAsync(Guid tipoUsuarioId, CancellationToken cancellationToken = default)
    {
        var todos = await db.Usuarios.AsNoTracking().ToListAsync(cancellationToken);
        return todos.Any(u => u.TipoUsuarioIds.Contains(tipoUsuarioId));
    }

    public Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        db.Usuarios.Add(usuario);
        return Task.CompletedTask;
    }
}

public sealed class TipoUsuarioRepositoryEfCore(IdentidadDbContext db) : ITipoUsuarioRepository
{
    public Task<TipoUsuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.TiposUsuario.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<TipoUsuario?> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default)
    {
        var normalizado = TipoUsuario.NormalizarNombre(nombre);
        return db.TiposUsuario.SingleOrDefaultAsync(x => x.NombreNormalizado == normalizado, cancellationToken);
    }

    public async Task<IReadOnlyList<TipoUsuario>> ObtenerPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var lista = ids.Distinct().ToList();
        if (lista.Count == 0)
        {
            return Array.Empty<TipoUsuario>();
        }

        return await db.TiposUsuario.Where(x => lista.Contains(x.Id)).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TipoUsuario>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
        await db.TiposUsuario.AsNoTracking().ToListAsync(cancellationToken);

    public Task AgregarAsync(TipoUsuario tipoUsuario, CancellationToken cancellationToken = default)
    {
        db.TiposUsuario.Add(tipoUsuario);
        return Task.CompletedTask;
    }

    public void Eliminar(TipoUsuario tipoUsuario) => db.TiposUsuario.Remove(tipoUsuario);
}

public sealed class ClienteApiUniversidadRepositoryEfCore(IdentidadDbContext db) : IClienteApiUniversidadRepository
{
    public Task<ClienteApiUniversidad?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.ClientesApiUniversidad.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<ClienteApiUniversidad?> ObtenerPorClientIdAsync(string clientIdKeycloak, CancellationToken cancellationToken = default) =>
        db.ClientesApiUniversidad.SingleOrDefaultAsync(x => x.ClientIdKeycloak == clientIdKeycloak, cancellationToken);

    public Task AgregarAsync(ClienteApiUniversidad cliente, CancellationToken cancellationToken = default)
    {
        db.ClientesApiUniversidad.Add(cliente);
        return Task.CompletedTask;
    }
}

public sealed class DesafioSegundoFactorRepositoryEfCore(IdentidadDbContext db) : IDesafioSegundoFactorRepository
{
    public Task<DesafioSegundoFactor?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.DesafiosSegundoFactor.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<DesafioSegundoFactor>> ObtenerActivosPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        await db.DesafiosSegundoFactor
            .Where(x => x.UsuarioId == usuarioId && x.ConsumidoEnUtc == null && x.InvalidadoEnUtc == null)
            .ToListAsync(cancellationToken);

    public Task AgregarAsync(DesafioSegundoFactor desafio, CancellationToken cancellationToken = default)
    {
        db.DesafiosSegundoFactor.Add(desafio);
        return Task.CompletedTask;
    }
}

public sealed class UnidadTrabajoIdentidadEfCore(IdentidadDbContext db) : IUnidadTrabajoIdentidad
{
    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
