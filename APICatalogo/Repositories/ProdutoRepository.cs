using ApiCatalogo.Models;
using APICatalogo.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace APICatalogo.Repositories
{
    public class ProdutoRepository : Repository<Produto>,  IProdutoRepository
    {
        readonly AppDbContext _context;

        //Construtor está recebendo a instância do contexto lá de Repository.cs
        public ProdutoRepository(AppDbContext context) : base(context)
        {
        }

        public IEnumerable<Produto> GetProdutosPorCategoria(int id)
        {
            var produtoCategoria = GetAll().Where(c => c.CategoriaId == id);
            return produtoCategoria;
        }

        /*public IQueryable<Produto> GetProdutos()
        {
            var produtos = _context.Produtos;
            return produtos;
        }

        public Produto GetProduto(int id)
        {
            var produto = _context.Produtos.AsNoTracking().FirstOrDefault(p => p.ProdutoId == id);
            return produto;
        }

        public Produto Create(Produto produto)
        {
            if (produto is null)
                throw new ArgumentException(nameof(produto));
            
            produto.DataCadastro = DateTime.UtcNow;
            _context.Produtos.Add(produto);
            _context.SaveChanges();

            return produto;
        }

        public bool Update(Produto produto)
        {
            if (produto is null)
                throw new InvalidOperationException("Produto é inválido.");

            if(_context.Produtos.Any(p => p.ProdutoId == produto.ProdutoId))
            {
                *//*_context.Entry(produto).State = EntityState.Modified;*//* //Faz-se dessa forma quando a entidade não está sendo rastreada pelo contexto
                _context.Produtos.Update(produto); //Pode fazer assim se a entidade estiver sendo rastreada pelo contexto
                _context.SaveChanges();
                return true;
            }

            return false;
        }

        public bool Delete(int id)
        {
            var produto = _context.Produtos.Find(id);

            if (produto is null)
                throw new ArgumentNullException(nameof(produto));

            if (produto is not null)
            {
                _context.Produtos.Remove(produto);
                _context.SaveChanges();
                return true;
            }

            return false;
        }*/
    }
}