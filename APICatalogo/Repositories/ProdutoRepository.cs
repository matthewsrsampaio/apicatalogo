using ApiCatalogo.Models;
using APICatalogo.Context;
using APICatalogo.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace APICatalogo.Repositories
{
    public class ProdutoRepository : Repository<Produto>, IProdutoRepository
    {
        readonly AppDbContext _context;

        //Construtor está recebendo a instância do contexto lá de Repository.cs
        public ProdutoRepository(AppDbContext context) : base(context)
        {
        }

        public PagedList<Produto> GetProdutos(ProdutosParameters produtosParameters)
        {
            var produtos = GetAll().OrderBy(p => p.ProdutoId).AsQueryable(); // AsQueryable vai tranformar o resultado de IEnumerable para IQueryable
            var produtosOrdenados = PagedList<Produto>.ToPagedList(produtos, produtosParameters.pageNumber, produtosParameters.pageSize);
            return produtosOrdenados;
        }

        //Obter um produto filtrado
        public PagedList<Produto> GetProdutosFiltroPreco(ProdutosFiltroPreco produtosFiltroParams)
        {
            // Aqui iremos obter uma lista de todos os produtos e transforma-los em IQueryable;
            var produtos = GetAll().AsQueryable();

            //Se o objeto passado tiver valor e não for vazio ou nulo nós iremos aplicar nossos filtros
            if (produtosFiltroParams.Preco.HasValue && !string.IsNullOrEmpty(produtosFiltroParams.PrecoCriterio))
            {
                //Ordenaremos pelo produtos de maior preço
                if (produtosFiltroParams.PrecoCriterio.Equals("maior", StringComparison.OrdinalIgnoreCase))
                {
                    produtos = produtos.Where(p => p.Preco > produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco);
                }
                //Ordenaremos pelo produtos de menor preço
                else if (produtosFiltroParams.PrecoCriterio.Equals("menor", StringComparison.OrdinalIgnoreCase))
                {
                    produtos = produtos.Where(p => p.Preco < produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco);
                }
                //Ordenaremos pelo produtos do mesmo preço
                else if (produtosFiltroParams.PrecoCriterio.Equals("igual", StringComparison.OrdinalIgnoreCase))
                {
                    produtos = produtos.Where(p => p.Preco == produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco);
                }
            }
            //Com o objeto desejado em "mãos" aplicaremos nossa paginação a ele
            var produtosFiltrados = PagedList<Produto>.ToPagedList(produtos, produtosFiltroParams.pageNumber, produtosFiltroParams.pageSize);
            //Retornamos nosso objeto filtrado e paginado
            return produtosFiltrados;
        }

        public IEnumerable<Produto> GetProdutosPorCategoria(int id)
        {
            var produtoCategoria = GetAll().Where(c => c.CategoriaId == id);
            return produtoCategoria;
        }

    }
}