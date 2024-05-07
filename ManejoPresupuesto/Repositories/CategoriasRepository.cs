using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Repositories
{
    public interface ICategoriasRepository
    {
        Task Actualizar(Categoria categoria);
        Task Borrar(int categoriaId);
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId);
        Task<Categoria> ObtenerPorId(int categoriaId, int usuarioId);
    }
    public class CategoriasRepository: ICategoriasRepository
    {
        private readonly string connectionString;
        public CategoriasRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear (Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>
                                         (@"INSERT INTO Categorias
                                            (Nombre, TipoOperacionId, UsuarioId)
                                            VALUES (@Nombre, @TipoOperacionId, @UsuarioId)
                                            SELECT SCOPE_IDENTITY();", categoria);
            categoria.Id = id;
        }

        public async Task<IEnumerable<Categoria>> Obtener (int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>
                                         (@"SELECT Id, Nombre, TipoOperacionId, UsuarioId
                                            FROM Categorias
                                            WHERE UsuarioId = @UsuarioId",
                                            new { usuarioId });
        }

        public async Task<Categoria> ObtenerPorId (int categoriaId, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>
                                         (@"SELECT Id, Nombre, TipoOperacionId, UsuarioId
                                            FROM Categorias
                                            WHERE Id = @Id AND UsuarioId = @UsuarioId",
                                            new { id = categoriaId, usuarioId });
        }

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Categorias
                                            SET Nombre = @Nombre,
                                            TipoOperacionId = @TipoOperacionId
                                            WHERE Id = @Id", 
                                            categoria);
        }

        public async Task Borrar (int categoriaId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE Categorias
                                            WHERE Id = @Id", new { id = categoriaId });
        }
    }
}
