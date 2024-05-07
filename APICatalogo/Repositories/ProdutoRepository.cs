
using APICatalogo.Context;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {
        readonly  AppDbContext _context;

        public ProdutoRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Produto> GetProdutos()
        {
            var produto = _context.Produtos.AsNoTracking().ToList();
            return produto;
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

            _context.Produtos.Add(produto);
            _context.SaveChanges();

            return produto;
        }

        public Produto Update(Produto produto)
        {
            if (produto is null)
                throw new ArgumentException(nameof(produto));

            _context.Entry(produto).State = EntityState.Modified;
            _context.SaveChanges();

            return produto;
        }

        public Produto Delete(int id)
        {
            var produto = _context.Produtos.Find(id);

            if (produto is null)
                throw new ArgumentException(nameof(produto));

            _context.Remove(produto);
            _context.SaveChanges();

            return produto;
        }
        
    }
}
