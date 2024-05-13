using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ManejoPresupuesto.Repositories
{
    public interface ITransaccionesRepository
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int transaccionId);
        Task Crear(Transaccion transaccion);
        Task<Transaccion> ObtenerPorId(int transaccionId, int usuarioId);
    }
    public class TransaccionesRepository: ITransaccionesRepository
    {
        private readonly string connectionString;
        public TransaccionesRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>
                                        ("Transacciones_Insertar",
                                         new { 
                                             transaccion.UsuarioId, 
                                             transaccion.FechaTransaccion,
                                             transaccion.Monto,
                                             transaccion.CategoriaId,
                                             transaccion.CuentaId,
                                             transaccion.Nota
                                         },
                                         commandType: CommandType.StoredProcedure);
            transaccion.Id = id;
        }

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync
                                ("Transacciones_Actualizar",
                                    new
                                    {
                                        transaccion.Id,
                                        transaccion.FechaTransaccion,
                                        transaccion.Monto,
                                        transaccion.CategoriaId,
                                        transaccion.CuentaId,
                                        transaccion.Nota,
                                        montoAnterior,
                                        cuentaAnteriorId
                                    }, commandType: CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int transaccionId, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>
                                        (@"SELECT 
                                            Transacciones.Id, 
                                            Transacciones.UsuarioId, 
                                            Transacciones.FechaTransaccion, 
                                            Transacciones.Monto,
                                            Transacciones.Nota,
                                            Transacciones.CuentaId,
                                            Transacciones.CategoriaId,
                                            Categorias.TipoOperacionId 
                                            FROM Transacciones
                                            INNER JOIN Categorias
                                            ON Categorias.Id = Transacciones.CategoriaId
                                            WHERE Transacciones.Id = @Id AND Transacciones.UsuarioId = @UsuarioId",
                                            new { id = transaccionId, usuarioId });
        }

        public async Task Borrar(int transaccionId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync
                                ("Transacciones_Borrar", 
                                 new { id = transaccionId },
                                 commandType: CommandType.StoredProcedure);
        }
    }
}
