using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace APICatalogo.Services
{
    public interface ITokenService
    {
        //Gera o token JWT
        JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration _config);

        //Gera o refresh token
        string GenerateRefreshToken();

        //Vai extrair as claims para gerar o novo token de acesso
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration _config);
    }
}
