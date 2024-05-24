using ApiCatalogo.Models;
using APICatalogo.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Repositories
{
    public interface IProdutoRepository : IRepository<Produto>
    {
        PagedList<Produto> GetProdutos(ProdutosParameters produtosParams);
        PagedList<Produto> GetProdutosFiltroPreco(ProdutosFiltroPreco produtosFiltroPrecoParams);
        IEnumerable<Produto> GetProdutosPorCategoria(int id);
    }
}
