using ApiCatalogo.Models;
using APICatalogo.Context;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {
        readonly AppDbContext _context;

        public ProdutoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Produto>> GetProdutos()
        {
            var produtos = await _context.Produtos.AsNoTracking().ToListAsync();
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

        public Produto Update(Produto produto)
        {
            if (produto is null)
                throw new ArgumentNullException(nameof(produto));

            _context.Entry(produto).State = EntityState.Modified;
            _context.SaveChanges();

            return produto;
        }

        public Produto Delete(int id)
        {
            var produto = _context.Produtos.Find(id);

            if (produto is null)
                throw new ArgumentNullException(nameof(produto));

            _context.Produtos.Remove(produto);
            _context.SaveChanges();

            return produto;
        }
    }
}