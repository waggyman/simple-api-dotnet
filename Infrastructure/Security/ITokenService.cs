using simple_api.Domain.Entities;

namespace simple_api.Infrastructure.Security;

public interface ITokenService
{
    string CreateToken(User user);
}
