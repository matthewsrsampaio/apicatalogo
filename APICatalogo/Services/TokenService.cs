using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace APICatalogo.Services
{
    public class TokenService : ITokenService
    {
        public JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration _config)
        {
            //Cria chave a partir dos dados inseridos em appsettings.json
            var key = _config.GetSection("JWT").GetValue<string>("SecreKey") ?? throw new InvalidOperationException("Invalid secret key");
            //Cria chave privada em um array de bytes
            var privateKey = Encoding.UTF8.GetBytes(key);
            //Cria as credenciais que serão usadas para assinar o token
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(privateKey), SecurityAlgorithms.HmacSha256Signature);
            //Cria todos os requisitos necessários para criar um TOKEN JWT
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_config.GetSection("JWT").GetValue<double>("TokenValidityInMinutes")),
                Audience = _config.GetSection("JWT").GetValue<string>("ValidAudience"),
                Issuer = _config.GetSection("JWT").GetValue<string>("ValidIssuer"),
                SigningCredentials = signingCredentials

            };
            //Cria o Hanlder para a criação do token
            var tokenHandler = new JwtSecurityTokenHandler();
            //Cria o token com as descrições acima que vieram de appsettings.json
            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            return token;
        }

        public string GenerateRefreshToken()
        {
            //Cria uma array de bytes com o tamanho de 128bytes que serão armazenados de forma segura
            var secureRandomBytes = new byte[128];
            //Gerador de números aleatórios
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            //Preenche secureRandomBytes com bytes aleatórios
            randomNumberGenerator.GetBytes(secureRandomBytes);
            //Converte o valor de secureRandomBytes em base64
            var refreshToken = Convert.ToBase64String(secureRandomBytes);
            return refreshToken;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration _config)
        {
            //Busca pelo valor da chave secreta; Se ela for inválida uma exceção é lançada
            var secretKey = _config["JWT:SecretKey"] ?? throw new InvalidOperationException("Invalid key");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                //Configura a assinatura do emissor com a chave secreta
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = false
            };
            //Essa variável é criada para fazer a minupulação do Token
            var tokenHandler = new JwtSecurityTokenHandler();
            //Valida o token                                                             //Esse é chamado como parâmetro de saída; Significa que essa variável vai ser preenchida com as informações obtidas do token 
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            //Verifica se securityToken é um instancia de JwtSecurityToken e se o algoritmo utilizado é o HmachSha256
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                                                    StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            return principal;
        }
    }
}
