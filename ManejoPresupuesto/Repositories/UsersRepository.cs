using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ManejoPresupuesto.Repositories
{
    public interface IUsersRepository
    {
        Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
        Task<int> CrearUsuario(Usuario usuario);
    }

    public class UsersRepository: IUsersRepository
    {
        private readonly string connectionString;
        public UsersRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CrearUsuario(Usuario usuario)
        {
            using var connection = new SqlConnection(connectionString);

            var usuarioId = await connection.QuerySingleAsync<int>
                         (@"INSERT INTO Usuarios (Email, EmailNormalizado, PasswordHash)
                            VALUES(@Email, @EmailNormalizado, @PasswordHash);
                            SELECT SCOPE_IDENTITY();",
                            usuario);

            await connection.ExecuteAsync("CrearDatosUsuarioNuevo", new { usuarioId },
                    commandType: CommandType.StoredProcedure);

            return usuarioId;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail (string emailNormalizado)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QuerySingleOrDefaultAsync<Usuario>
                         (@"SELECT Id, Email, EmailNormalizado, PasswordHash
                            FROM Usuarios
                            WHERE EmailNormalizado = @EmailNormalizado",
                            new { emailNormalizado });
        }

    }
}
