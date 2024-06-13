using APICatalogo.Context;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Extensions;
using APICatalogo.Filters;
using APICatalogo.Logging;
using APICatalogo.Models;
using APICatalogo.Repositories;
using APICatalogo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//Add Tratamento de Exceção e Tratamento para ignorar os ciclos na Serialização
builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(ApiExceptionFilter));
})
    .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
    .AddNewtonsoftJson(); // -> AddNewtonsoftJson(); adiciona o tratamento JSON PATCH 

//Registrar o servico para add Atributo [FromServices]
//O tempo de vida transient implica que sempre que o serviço for invocado ele ganhará uma nova instância
builder.Services.AddTransient<IMeuServico, MeuServico>(); //Sempre que eu invocar a Interface ele vai me responder com a implementação dessa interface definida na classe concreta MeuServico


//Desabilitar o mecanismo de inferência de injeção de dependência nos controladores
/*builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.DisableImplicitFromServicesParameters = true;
});*/


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Outra forma de se obter informações sobrbe as configurações
var valor1 = builder.Configuration["chave1"];
var valor2 = builder.Configuration["secao1:chave2"];

//Definição da string de conexão
string mySqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");

//Registro do contexto do EFCore para setar a conexão como um serviço da aplicação
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(mySqlConnection,
        ServerVersion.AutoDetect(mySqlConnection)));

//JWT BEARER
var secretKey = builder.Configuration["JWT:SecretKey"] ?? throw new ArgumentException("Invalid secret key!!");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; //Se o usuário tentar acessar algo protegido sem permissão minha app lancará o desafio que consiste em pedir o login e senha
}).AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false; //Esta opção deve ser marcada como true para PROD.
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero, //permite ajustar o tempo para tratar diferenças de tempo entre servidor de aplicação e o servidor de autenticação.
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

//Registro do serviço de aautorização
builder.Services.AddAuthorization();

//Registro de registro de autenticação JWT
builder.Services.AddAuthentication("Bearer").AddJwtBearer();

//Registro de configuração do Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>() //Atenção para inserir aqui a class AppDbContext
    .AddDefaultTokenProviders();

//Registro do serviço do filtro
builder.Services.AddScoped<ApiLoggingFilter>(); // =>AddScoped é o tempo de vida do Scopo do request. Isso garante que para cada request haverá uma nova instancia.

//Registro do repositório de categoria
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();  // =>AddScoped é o tempo de vida do Scopo do request. Isso garante que para cada request haverá uma nova instancia.
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // -> Add o repositório genérico e sua interface
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); //Add Unit of Work


//ADD Provedor do LOG personalizado ao sistema de log do ASP.NET Core definindo o nível mínimo de LOG
builder.Logging.AddProvider(new CustomLoggerProvider(new CustomLoggerProviderConfiguration
{
    LogLevel = LogLevel.Information //Este foi o level escolhido
}));

//Adiciona o serviço de AutoMapper
//Desinstalei o pacote de injeção de dependência do AutoMapper pq, aparentemente, ele ja está incluso na sua versão 13. Que é a qual estou usando. A partir daí, o erro de ambiguidade sumiu.
builder.Services.AddAutoMapper(typeof(ProdutoDTOMappingProfile));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) //Verifico se meu ambiente é o de desenvolvimento
{
    app.UseSwagger(); //Define o middleware do SWAGGER
    app.UseSwaggerUI(); //Define o middlewware SWAGGER UserInterface
    app.ConfigureExceptionHandler();
    //app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection(); //Define o middleware para redirecionar as requisições HTTP para HTTPS

//app.UseAuthentication(); //Define a autenticação do usuário

//Não estou precisando desse cara abaixo pq ja tenho um autorizador la em cima, mas vou deixar ele ai.
app.UseAuthorization(); //Define o middleware para verificar as verificações de acesso

app.MapControllers(); //Define o mapeamento do controladores da aplicação

/*app.Use(async (context, next) =>
{
    //adicionar o código antes do request
    await next(context);
    //adicionar o código depois do request
});*/


/*app.Run(async (context) =>
{
    await context.Response.WriteAsync("Middleware final");
});*/

app.Run(); //Inicio a aplicação e encerramento do pipeline dos middlewares.
