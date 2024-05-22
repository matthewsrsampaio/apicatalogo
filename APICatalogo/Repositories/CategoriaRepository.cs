using ApiCatalogo.Models;
using APICatalogo.Context;
using APICatalogo.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace APICatalogo.Repositories
{
    public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
    {
        //Construtor está recebendo a instância do contexto lá de Repository.cs
        public CategoriaRepository(AppDbContext context) : base(context)
        {
        }

        public PagedList<Categoria> GetCategorias(CategoriasParameters categoriasParameters)
        {
            //Obtenho uma lista de categorias ordenadas pelo id, transformo-as em Queryable e armazeno na variável categorias.
            var categorias = GetAll().OrderBy(categorias => categorias.CategoriaId).AsQueryable();
            //Chamo o método estático ToPagedList(objeto, pageNumber, pageSize) e armazeno as listas em categoriasOrdenadas.
            var categoriasOrdenadas = PagedList<Categoria>.ToPagedList(categorias, categoriasParameters.pageNumber, categoriasParameters.pageSize);
            return categoriasOrdenadas;
        }

        /*public IEnumerable<Categoria> GetCategorias()
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
        }*/
    }
}
