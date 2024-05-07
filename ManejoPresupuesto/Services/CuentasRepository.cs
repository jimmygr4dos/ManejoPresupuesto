using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface ICuentasRepository
    {
        Task<IEnumerable<Cuenta>> Obtener(int usuarioId);
        Task Crear(Cuenta cuenta);
    }
    public class CuentasRepository: ICuentasRepository
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
    }
}
