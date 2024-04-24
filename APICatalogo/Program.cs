using APICatalogo.Context;
using APICatalogo.Services;
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


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Definição da string de conexão
string mySqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");

//Registro do contexto do EFCore para setar a conexão como um serviço da aplicação
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(mySqlConnection,
        ServerVersion.AutoDetect(mySqlConnection)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
