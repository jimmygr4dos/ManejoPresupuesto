using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Repositories
{
    public interface ICuentasRepository
    {
        Task<IEnumerable<Cuenta>> Obtener(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int cuentaId, int usuarioId);
        Task Actualizar(CuentaCreacionViewModel cuenta);
    }
    public class CuentasRepository : ICuentasRepository
    {
        private readonly string connectionString;

        public CuentasRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>
                                     (@"INSERT INTO Cuentas (Nombre, TipoCuentaId, Descripcion, Balance)
                                        VALUES (@Nombre, @TipoCuentaId, @Descripcion, @Balance);
                                        SELECT SCOPE_IDENTITY();", cuenta);
            cuenta.Id = id;
        }

        public async Task<IEnumerable<Cuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Cuenta>
                                     (@"SELECT a.Id, a.Nombre, a.Balance, b.Nombre AS TipoCuenta
                                        FROM Cuentas AS a
                                        INNER JOIN TiposCuentas AS b
                                        ON a.TipoCuentaId = b.Id
                                        WHERE b.UsuarioId = @UsuarioId
                                        ORDER BY b.Orden", new { usuarioId });
        }

        public async Task<Cuenta> ObtenerPorId (int cuentaId, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>
                                    (@"SELECT a.Id, a.Nombre, a.Balance, a.Descripcion, b.Id AS TipoCuentaId
                                        FROM Cuentas AS a
                                        INNER JOIN TiposCuentas AS b
                                        ON a.TipoCuentaId = b.Id
                                        WHERE b.UsuarioId = @UsuarioId
                                        AND a.Id = @Id", 
                                        new { id = cuentaId, usuarioId });
        }

        public async Task Actualizar (CuentaCreacionViewModel cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync
                                     (@"UPDATE Cuentas 
                                        Set Nombre = @Nombre, Balance = @Balance, 
                                        Descripcion = @Descripcion, TipoCuentaId = @TipoCuentaId
                                        WHERE Id = @Id", cuenta);
        }
    }
}
