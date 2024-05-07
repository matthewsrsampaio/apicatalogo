using ApiCatalogo.Models;
using APICatalogo.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        readonly AppDbContext _context;

        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Categoria> GetCategorias()
        {
            var categorias = _context.Categorias.AsNoTracking().ToList();
            return categorias;
        }

        public IEnumerable<Categoria> GetCategoriaProdutos()
        {
            var categoriasProdutos = _context.Categorias.AsNoTracking().Include(p => p.Produtos).ToList();
            return categoriasProdutos;  
        }

        public Categoria GetCategoria(int id)
        {
            var categorias = _context.Categorias.AsNoTracking().FirstOrDefault(c => c.CategoriaId == id);
            return categorias;
        }

        public Categoria Create(Categoria categoria)
        {
            //Essa exceção está explicitata pq estamos recebendo dados 
            if (categoria is null)
                throw new ArgumentNullException(nameof(categoria));

            _context.Categorias.Add(categoria);
            _context.SaveChanges();

            return categoria;
        }       

        public Categoria Update(Categoria categoria)
        {
            //Essa exceção está explicitata pq estamos recebendo dados 
            if (categoria is null)
                throw new ArgumentNullException(nameof(categoria));

            _context.Entry(categoria).State = EntityState.Modified;
            _context.SaveChanges();

            return categoria;

        }

        public Categoria Delete(int id)
        {
            var categoria = _context.Categorias.Find(id);

            if (categoria is null)
                throw new ArgumentNullException(nameof(categoria));

            _context.Categorias.Remove(categoria);
            _context.SaveChanges();

            return categoria;
        }
 
    }
}
