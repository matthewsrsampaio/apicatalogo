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
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ITokenService tokenService,
                              UserManager<ApplicationUser> userManager,
                              RoleManager<IdentityRole> roleManager,
                              IConfiguration configuration,
                              ILogger<AuthController> logger)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        [Route("Auth/login")]                        //Receberá no body do request as credenciais                   
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {   //Busca pelo o usuário e verifica se ele existe ou não
            var user = await _userManager.FindByNameAsync(model.Username!);//A exclamação da a certeza de que esse valor não será nulo.

            //Verifica se o usuário é nulo e se a senha é verdadeira
            if (user is not null && await _userManager.CheckPasswordAsync(user, model.Password!))
            {
                //Obtém os perfis do usuário
                var userRoles = await _userManager.GetRolesAsync(user);

                //lista das claims que são informações do usuário que serão incluídas no token
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim("id", user.UserName!),
                    //Essa claim dará um identificador exclusico para o token
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                //Vai iterar sobre cada perfil de usuário
                foreach (var userRole in userRoles)
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
                return Ok(new //Retorna token, refreshToken e data de expiração no cabeçalho
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo
                });
            }
            return Unauthorized("Dados inválidos");
        }

        [HttpPost]
        [Route("Auth/register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            //Verifica se o usuário existe
            var userExists = await _userManager.FindByNameAsync(model.Username!);

            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  new Response { 
                                      Status = "Error", 
                                      Message = "User already exists" 
                                  });
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
                                  new Response 
                                  { 
                                      Status = "Error", 
                                      Message = "User creation failed" 
                                  });
            }

            return Ok(
                new Response
                { 
                    Status = "Success", 
                    Message = "User created successfully" 
                });
        }

        [HttpPost]
        [Route("Auth/refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModel tokenModel)
        {
            if (tokenModel is null)
            {
                return BadRequest("Invalid client request");
            }
            //A partir do tokenModel eu extraio o AccessToken atual
            string? accessToken = tokenModel.AccessToken ?? throw new ArgumentNullException(nameof(tokenModel));
            //A partir do tokenModel eu extraio o RefreshToken atual
            string? refreshToken = tokenModel.RefreshToken ?? throw new ArgumentNullException(nameof(tokenModel));
            //Extrai a identidade do usuário do token expirado
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken!, _configuration);

            if (principal == null)
            {
                return BadRequest("Principal is null. Invalid access token / refresh token");
            }

            //Extrai o nome do usuário da base de dados
            var username = principal.Identity.Name;
            //COm o nome do usuário vai ser possível buscar ele no banco de dados
            var user = await _userManager.FindByNameAsync(username!);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime >= DateTime.Now)
            {
                return BadRequest(
                    $"\nUser is null {user.UserName} or " +
                    $"\nResfreshToken is different {user.RefreshToken} or " +
                    $"\nRefresh Token Expiry time is over {user.RefreshTokenExpiryTime}. " +
                    $"\nHora atual: {DateTime.Now}" +
                    $"\nInvalid access token / refresh token");
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

        [HttpPost]
        [Authorize(Policy = "ExclusiveOnly")]
        [Route("Auth/revoke/{username}")]
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

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        [Route("CreateRole")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            var roleExist = await _roleManager.RoleExistsAsync(roleName);
            //Verifica se essa role ja existe
            if (!roleExist)
            {
                //Se a role não existir eu crio uma nova
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
                //Se a criação tiver sido feita com sucesso
                if (roleResult.Succeeded)
                {
                    _logger.LogInformation(1, "Roles Added");
                    return StatusCode(StatusCodes.Status200OK,
                        new Response { 
                            Status = "Success", 
                            Message = $"Role {roleName} added successfuly" 
                        });
                }
                //Se a criação da roole tiver falhado
                else
                {
                    _logger.LogInformation(2, "Error");
                    return StatusCode(StatusCodes.Status400BadRequest,
                        new Response { 
                            Status = "Error", 
                            Message = $"Issue adding the new {roleName} role" 
                        });
                }
            }
            //se a role já existir
            return StatusCode(StatusCodes.Status400BadRequest,
                new Response { Status = "Error", Message = "Role already exist" });
        }

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        [Route("AddUserRole")]
        public async Task<IActionResult> AddUserToRole(string email, string name, string roleName)
        {
            var user_email = await _userManager.FindByEmailAsync(email);
            var user_name = await _userManager.FindByNameAsync(name);
            
            if (user_email != null && user_name != null && user_name.Id == user_email.Id)
            {
                var result = await _userManager.AddToRoleAsync(user_email, roleName);

                if (result.Succeeded)
                {
                    _logger.LogInformation(1, $"User {user_name.UserName} - {user_email.Email} added to the {roleName} role");
                    return StatusCode(
                        StatusCodes.Status200OK,
                        new Response
                        {
                            Status = "Success",
                            Message = $"User {user_name.UserName} - {user_email.Email} added to the {roleName} role"
                        });
                }
                else
                {
                    _logger.LogInformation(1, $"Error: Unable to add user {user_name.UserName} - {user_email.Email} to the {roleName} role");
                    return StatusCode(
                        StatusCodes.Status400BadRequest,
                        new Response
                        {
                            Status = "Error",
                            Message = $"Error: Unable to add user {user_name.UserName} - {user_email.Email} to the {roleName} role"
                        });
                }
            }

            return BadRequest(new { error = "Unable to find user" });
        }
    }
}
