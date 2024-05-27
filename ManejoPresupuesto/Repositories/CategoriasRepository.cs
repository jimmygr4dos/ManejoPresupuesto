using Dapper;
using ManejoPresupuesto.Enums;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Repositories
{
    public interface ICategoriasRepository
    {
        Task Actualizar(Categoria categoria);
        Task Borrar(int categoriaId);
        Task<int> Contar(int usuarioId);
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, PaginacionViewModel paginacion);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId);
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

        public async Task<IEnumerable<Categoria>> Obtener (int usuarioId, PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>
                                         (@$"SELECT Id, Nombre, TipoOperacionId, UsuarioId
                                             FROM Categorias
                                             WHERE UsuarioId = @UsuarioId
                                             ORDER BY Nombre
                                             OFFSET { paginacion.RecordsASaltar }
                                             ROWS FETCH NEXT { paginacion.RecordsPorPagina } 
                                             ROWS ONLY",
                                             new { usuarioId });
        }

        public async Task<int> Contar (int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>
                                        (@"SELECT COUNT(*) 
                                           FROM CATEGORIAS
                                           WHERE UsuarioId = @UsuarioId",
                                           new { usuarioId });
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>
                                         (@"SELECT Id, Nombre, TipoOperacionId, UsuarioId
                                            FROM Categorias
                                            WHERE UsuarioId = @UsuarioId
                                            AND TipoOperacionId = @TipoOperacionId",
                                            new { usuarioId, tipoOperacionId });
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
