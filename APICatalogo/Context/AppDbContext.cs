using ApiCatalogo.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Context
{
    public class AppDbContext : IdentityDbContext //DbContext é responsável por relacionar as classes do projeto com o banco de dados
    {
        //Construtor
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { 
        }

        //Definir o mapeamento objeto relacional
        public DbSet<Categoria>? Categorias { get; set; }
        public DbSet<Produto>? Produtos { get; set; }
    }
}