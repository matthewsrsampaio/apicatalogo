using APICatalogo.Context;
using APICatalogo.Extensions;
using APICatalogo.Filters;
using APICatalogo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//Tratamento para ignorar os ciclos na Serialização
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions
            .ReferenceHandler = ReferenceHandler.IgnoreCycles);

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

//Registro do serviço do filtro
builder.Services.AddScoped<ApiLoggingFilter>();

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
