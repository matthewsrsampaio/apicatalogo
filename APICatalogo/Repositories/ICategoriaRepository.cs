using ApiCatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories;

public interface ICategoriaRepository : IRepository<Categoria>
{
    PagedList<Categoria> GetCategorias(CategoriasParameters categoriasParameters);

    /*//Get
    IEnumerable<Categoria> GetCategorias();

    //Get produtos de cada categoria
    IEnumerable<Categoria> GetCategoriaProdutos();

    //Get(id)
    Categoria GetCategoria(int id);

    //Post
    Categoria Create(Categoria categoria);

    //Put
    Categoria Update(Categoria categoria);

    //Delete
    Categoria Delete(int id);*/
}
