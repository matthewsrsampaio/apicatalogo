using ApiCatalogo.Models;

namespace APICatalogo.Repositories
{
    public interface IProdutoRepository
    {
        //Get
        Task<IEnumerable<Produto>> GetProdutos();

        //Get(id)
        Produto GetProduto(int id);

        //Post
        Produto Create(Produto produto);

        //Put
        Produto Update(Produto produto);

        //Delete
        Produto Delete(int id);
    }
}
