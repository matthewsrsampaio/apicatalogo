using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace APICatalogo.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(ITokenService tokenService, 
                              UserManager<ApplicationUser> userManager, 
                              RoleManager<IdentityRole> roleManager, 
                              IConfiguration configuration)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("login")]                        //Receberá no body do request as credenciais                   
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {   //Busca pelo o usuário e verifica se ele existe ou não
            var user = await _userManager.FindByNameAsync(model.Username!);//A exclamação da a certeza de que esse valor não será nulo.

            //Verifica se o usuário é nulo e se a senha é verdadeira
            if(user is not null && await _userManager.CheckPasswordAsync(user, model.Password!))
            {   
                //Obtém os perfis do usuário
                var userRoles = await _userManager.GetRolesAsync(user);

                if(userRoles is EmptyResult)
                {
                    return Unauthorized("Nenhum papel foi encontrado para este usuário.");
                }
                
                //lista das claims que são informações do usuário que serão incluídas no token
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.Email, user.Email!),
                    //Essa claim dará um identificador exclusico para o token
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
            
                //Vai iterar sobre cada perfil de usuário
                foreach(var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }
                
                //Gera o token
                var token = _tokenService.GenerateAccessToken(authClaims, _configuration);
                
                //Gera token de atualização
                var refreshToken = _tokenService.GenerateRefreshToken();
                
                //Faz uso do descarte " _ " apenas para converter a string recebida em inteiro. Graças ao "out" o valor ja sai direto em refreshTokenValidityInMinutes.
                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out int refreshTokenValidityInMinutes);
                
                user.RefreshToken = refreshToken;
                
                //Add valor a Expiração de tempo de vida do token
                user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(refreshTokenValidityInMinutes);
                
                //Persiste as novas informações no Banco de Dados
                await _userManager.UpdateAsync(user);
                return Ok(new //Retorna token, refreshToken e data de expiração
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo
                });
            }
            return Unauthorized("Dados inválidos");
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register ([FromBody] RegisterModel model){
            //Verifica se o usuário existe
            var userExists = await _userManager.FindByNameAsync(model.Username!);
            
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  new Response { Status = "Error", Message = "User already exists" });
            }

            //Cria instancia de Application User e atribui valores
            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            //Cria usuário e senha assíncrona
            var result = await _userManager.CreateAsync(user, model.Password!);

            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  new Response { Status = "Error", Message = "User creation failed" });
            }

            return Ok(new Response { Status = "Success", Message = "User created successfully" });
        }

        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenModel tokenModel)
        {
            if(tokenModel is null)
            {
                return BadRequest("Invalid client request");
            }
            //A partir do tokenModel eu extraio o AccessToken atual
            string? accessToken = tokenModel.AccessToken ?? throw new ArgumentNullException(nameof(tokenModel));
            //A partir do tokenModel eu extraio o RefreshToken atual
            string? refreshToken = tokenModel.RefreshToken ?? throw new ArgumentNullException(nameof(tokenModel));
            //Extrai a identidade do usuário do token expirado
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken!, _configuration);

            if(principal == null)
            {
                return BadRequest("Invalid access token / refresh token");
            }
            //Extrai o nome do usuário da base de dados
            var username = principal.Identity.Name;
            //COm o nome do usuário vai ser possível buscar ele no banco de dados
            var user = await _userManager.FindByNameAsync(username!);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid access token / refresh token");
            }
            //Gera um novo token
            var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims.ToList(), _configuration);
            //Gera um novo RefreshToken
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            //Atribui o novo token ao usuário para atualizar o BD
            user.RefreshToken = newRefreshToken;
            //De forma assíncrona, atualiza o BD
            await _userManager.UpdateAsync(user);
            //Retorna o novo accessToken e o novo refreshToken para o usuário
            return new ObjectResult(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken
            });
        }

        [Authorize]
        [HttpPost]
        [Route("revoke/{username}")]
        public async Task<IActionResult> Revoke(string username)
        {
            //Procuro  pelo usuário no banco
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return BadRequest("Invalid user name");
            //Se o usuário for encontrado eu atribuo nulo ao seu RefreshToken
            user.RefreshToken = null;
            //Salvo as informações que foram feitas
            await _userManager.UpdateAsync(user);
            //Retorno sem conteúdo
            return NoContent();
        }

    }
}
