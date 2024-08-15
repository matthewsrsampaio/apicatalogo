using APICatalogo.Context;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Extensions;
using APICatalogo.Filters;
using APICatalogo.Logging;
using APICatalogo.Models;
using APICatalogo.RateLimitOptions;
using APICatalogo.Repositories;
using APICatalogo.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

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

//DFINIR POLITICA CORS USANDO UM NOME ESPECIFICO
var OrigensComAcessoPermitido = "_origensComAcessoPermitido";
var PoliticaCORS1 = "_politicaCORS1";
var PoliticaCORS2 = "_politicaCORS2";

builder.Services.AddCors(options =>
{
    options.AddPolicy(OrigensComAcessoPermitido,
        policy =>
        {
            policy.WithOrigins("https://localhost:7292")
            .WithMethods("GET", "POST")
            .AllowAnyHeader();
        });
    options.AddPolicy(PoliticaCORS1,
        policy =>
        {
            policy.WithOrigins("https://apirequest.io",
                               "https://globo.com")
                               .WithMethods("GET");
        });
    options.AddPolicy(PoliticaCORS2,
        policy =>
        {
            policy.WithOrigins("https://apirequest.io")
                               .WithMethods("GET", "DELETE");
        });
});
    

// DEFINIR POLITICA CORS PADRÃO
/*builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://apirequest.io")
        .WithMethods("GET", "POST")
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
*/

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//Fazendo com que o Swagger exiga o token
builder.Services.AddSwaggerGen(c =>
{
    /*c.SwaggerDoc("v1", new OpenApiInfo { Title = "apicatalogo", Version = "v1" });
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "apicatalogo", Version = "v2" });*/
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "APICatalogo",
        Description = "Catálogo de Produtos e Categorias",
        TermsOfService = new Uri("https://matthewsrsampaio.github.io/swapi"),
        Contact = new OpenApiContact
        {
            Name = "matthews",
            Email = "matthewsrsampaio@outlook.com",
            Url = new Uri("https://matthewsrsampaio.github.io/swapi")
        },
        License = new OpenApiLicense
        {
            Name = "Usar sobre LICX",
            Url = new Uri("https://matthewsrsampaio.github.io/swapi")
        }
    });

    //Add comentários XML ao Swagger
    var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer JWT",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

//Outra forma de se obter informações sobrbe as configurações
var valor1 = builder.Configuration["chave1"];
var valor2 = builder.Configuration["secao1:chave2"];

//Definição da string de conexão
string mySqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");

//Registro do contexto do EFCore para setar a conexão como um serviço da aplicação
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(mySqlConnection,
        ServerVersion.AutoDetect(mySqlConnection)));

//Registro do serviço de aautorização
builder.Services.AddAuthorization();

//Registro de registro de autenticação JWT
//builder.Services.AddAuthentication("Bearer").AddJwtBearer();

//Registro de configuração do Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>() //Atenção para inserir aqui a class AppDbContext
.AddDefaultTokenProviders();

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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("SuperAdminOnly", policy => policy
                                                    .RequireRole("Admin")
                                                    .RequireClaim("id", "matthews")
    );
    options.AddPolicy("User", policy => policy.RequireRole("User"));
    options.AddPolicy("ExclusivePolicyOnly", policy =>
                                                policy.RequireAssertion(context =>
                                                    context.User.HasClaim(claim =>
                                                        claim.Type == "id" && claim.Value == "matthews")
                                                        ||  context.User.IsInRole("SuperAdmin")   
    ));
});

var myOptions = new MyRateLimitOptions();

builder.Configuration.GetSection(MyRateLimitOptions.MyRateLimit).Bind(myOptions);

//Política de limitação de taxa
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter(policyName: "fixedwindow", options =>
    {//Posso fazer uma requisição a cada 5 segundos
        options.PermitLimit = myOptions.PermitLimit;//1;
        options.Window = TimeSpan.FromSeconds(myOptions.Window);//TimeSpan.FromSeconds(10);
        options.QueueLimit = myOptions.QueueLimit;//2;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ??
                          httpContext.Request.Headers.Host.ToString(),
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 2,
                    QueueLimit = 0,
                    Window = TimeSpan.FromSeconds(10)
                }));
});

//Adiciona os serviços de Versionamento da API
builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
    //Por padrão o leitor de versao da api é por Query
    o.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader(),
        new UrlSegmentApiVersionReader());//Agora nós suportamos a leitura da versão pela URL
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

//Registro do serviço do filtro
builder.Services.AddScoped<ApiLoggingFilter>(); // =>AddScoped é o tempo de vida do Scopo do request. Isso garante que para cada request haverá uma nova instancia.

//Registro do repositório de categoria; Injeta as dependências
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();  // =>AddScoped é o tempo de vida do Scopo do request. Isso garante que para cada request haverá uma nova instancia.
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // -> Add o repositório genérico e sua interface
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); //Add Unit of Work
builder.Services.AddScoped<ITokenService, TokenService>(); //Add TokenServices


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
    app.UseSwagger(); //Habilita o middleware para servir o SWAGGER e é gerado como um endpoint JSON
    //app.UseSwaggerUI(); //Habilita o middleware de arquivos estáticos
    
    //Habilitar o SwaggerUI de forma mais específica (nao precisaria no meu caso)
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json",
            "APICatalogo");
    });
    app.ConfigureExceptionHandler();
    //app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection(); //Define o middleware para redirecionar as requisições HTTP para HTTPS
app.UseStaticFiles(); //habilita o middleware de arquivos estáticos
app.UseRouting(); // habilita o middleware de roteamento
//app.UseCors(OrigensComAcessoPermitido);
app.UseRateLimiter();//Aplica as limitações de taxa definidas acima
app.UseCors();

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
