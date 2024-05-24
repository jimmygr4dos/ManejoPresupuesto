using System.Security.Claims;

namespace ManejoPresupuesto.Services
{
    public interface IUsersService
    {
        int ObtenerUsuarioId();
    }
    public class UsersService: IUsersService
    {
        private readonly HttpContext _httpContext;

        public UsersService(IHttpContextAccessor httpContextAccesor)
        {
            _httpContext = httpContextAccesor.HttpContext;
        }
        public int ObtenerUsuarioId()
        {
            if(_httpContext.User.Identity.IsAuthenticated)
            {
                var idClaim = _httpContext.User.Claims.Where
                                (x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                var id = int.Parse (idClaim.Value);
                
                return id;
            }
            else
            {
                throw new ApplicationException("El usuario no está autenticado.");
            }
        }
    }
}
