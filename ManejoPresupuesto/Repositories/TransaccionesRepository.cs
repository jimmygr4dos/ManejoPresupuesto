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
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int transaccionId, int usuarioId);
        Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
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

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>
                                 (@"SELECT t.Id, t.Monto, t.FechaTransaccion, 
                                    c.Nombre AS Categoria, cu.Nombre AS Cuenta, 
                                    c.TipoOperacionId
                                    FROM Transacciones t
                                    INNER JOIN Categorias c
                                    ON c.Id = t.CategoriaId
                                    INNER JOIN Cuentas cu
                                    ON cu.Id = t.CuentaId
                                    WHERE t.CuentaId = @CuentaId
                                    AND t.UsuarioId = @UsuarioId
                                    AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin",
                                    modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId
            (ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>
                                 (@"SELECT t.Id, t.Monto, t.FechaTransaccion, 
                                    c.Nombre AS Categoria, cu.Nombre AS Cuenta, 
                                    c.TipoOperacionId,
                                    Nota
                                    FROM Transacciones t
                                    INNER JOIN Categorias c
                                    ON c.Id = t.CategoriaId
                                    INNER JOIN Cuentas cu
                                    ON cu.Id = t.CuentaId
                                    WHERE t.UsuarioId = @UsuarioId
                                    AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                                    ORDER BY t.FechaTransaccion DESC",
                                    modelo);
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana
            (ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorSemana>
                                 (@"SELECT 
                                    DATEDIFF(d, @fechaInicio, FechaTransaccion) / 7 + 1 AS Semana,
                                    SUM(Monto) AS Monto, 
                                    cat.TipoOperacionId
                                    FROM Transacciones
                                    INNER JOIN Categorias cat
                                    ON cat.Id = Transacciones.CategoriaId
                                    WHERE Transacciones.UsuarioId = @usuarioId
                                    AND FechaTransaccion BETWEEN @fechaInicio AND @fechaFin
                                    GROUP BY 
                                    DATEDIFF(d, @fechaInicio, FechaTransaccion) / 7, 
                                    cat.TipoOperacionId",
                                    modelo);
        }

        public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes
            (int usuarioId, int año)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorMes>
                                 (@"SELECT 
                                    MONTH(FechaTransaccion) AS Mes,
                                    SUM(Monto) AS Monto,
                                    cat.TipoOperacionId
                                    FROM Transacciones
                                    INNER JOIN Categorias cat
                                    ON cat.Id = Transacciones.CategoriaId
                                    WHERE Transacciones.UsuarioId = @usuarioId
                                    AND YEAR(FechaTransaccion) = @Año
                                    GROUP BY 
                                    MONTH(FechaTransaccion),
                                    cat.TipoOperacionId",
                                    new { usuarioId, año });
        }

    }
}
