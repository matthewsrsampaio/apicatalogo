namespace APICatalogo.Repositories
{
    public interface IProdutoRepository
    {
        //Get
        IEnumerable<Produto> GetProdutos();

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
