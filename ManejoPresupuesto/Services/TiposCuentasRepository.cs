using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface ITiposCuentasRepository
    {
        Task Actualizar(TipoCuenta tipoCuenta);
        Task Borrar(int tipoCuentaId);
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
        Task<TipoCuenta> ObtenerPorId(int tipoCuentaId, int usuarioId);
        Task Ordenar(IEnumerable<TipoCuenta> tiposCuentasOrdenados);
    }
    public class TiposCuentasRepository: ITiposCuentasRepository
    {
        private readonly string connectionString;
        public TiposCuentasRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            //var id = await connection.QuerySingleAsync<int>
            //                             (@"INSERT INTO TiposCuentas (Nombre, UsuarioId, Orden)
            //                                        VALUES (@Nombre, @UsuarioId, 0);
            //                                        SELECT SCOPE_IDENTITY();", tipoCuenta);
            var id = await connection.QuerySingleAsync<int>
                                            ("TiposCuentas_Insertar",
                                             new
                                             {
                                                nombre = tipoCuenta.Nombre,
                                                usuarioId = tipoCuenta.UsuarioId
                                             }, commandType: System.Data.CommandType.StoredProcedure);
            tipoCuenta.Id = id;
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>
                                         (@"SELECT 1 
                                            FROM TiposCuentas 
                                            WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId", 
                                            new {nombre, usuarioId});
            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<TipoCuenta>
                                         (@"SELECT Id, Nombre, Orden
                                            FROM TiposCuentas
                                            WHERE UsuarioId = @UsuarioId
                                            ORDER BY Orden", 
                                            new { usuarioId });
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync
                                         (@"UPDATE TiposCuentas 
                                            SET Nombre = @Nombre 
                                            WHERE Id=@Id", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int tipoCuentaId, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>
                                         (@"SELECT Id, Nombre, Orden
                                            FROM TiposCuentas
                                            WHERE Id = @Id AND UsuarioId = @UsuarioId",
                                            new { id = tipoCuentaId, usuarioId });
        }

        public async Task Borrar(int tipoCuentaId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE TiposCuentas WHERE Id = @Id", new { id = tipoCuentaId });
        }

        public async Task Ordenar (IEnumerable<TipoCuenta> tiposCuentasOrdenados)
        {
            var query = "UPDATE TiposCuentas SET Orden = @Orden Where Id = @Id";
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(query, tiposCuentasOrdenados);
        }
    }
}
