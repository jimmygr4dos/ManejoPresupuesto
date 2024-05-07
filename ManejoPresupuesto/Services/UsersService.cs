namespace ManejoPresupuesto.Services
{
    public interface IUsersService
    {
        int ObtenerUsuarioId();
    }
    public class UsersService: IUsersService
    {
        public int ObtenerUsuarioId()
        {
            return 1;
        }
    }
}
