using APICatalogo.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers;

[ApiController]
[Route("[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly AppDbContext _context;
    public ProdutosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Produto>> Get()
     //Para usar NotFound é necessário envelopar a classe no método ActionResult
    {
        var produtos = _context.Produtos.ToList();
        if(produtos is null)
        {
            return NotFound("Produtos não encontrados!");
        }
        return produtos;
    }

    [HttpGet("{id:int}", Name = "ObterProduto")] // "{id:int}") quer dizer que estou esperando um parâmetro do tipo inteiro chamado id
    public ActionResult<Produto> Get(int id)
    {
        var produto = _context.Produtos.FirstOrDefault(p => p.ProdutoId == id);
        
        if(produto is null)
        {
            return NotFound("Produto não encontrado!");
        }
        return produto;
    }

    [HttpPost]
    public ActionResult Post(Produto produto)
    {
        if (produto is null)
            return BadRequest();

        _context.Produtos.Add(produto); //Adicionando a instancia de produto
        _context.SaveChanges(); //Persiste as informações


        //O método CreatedAtRouteResult lança o código 201 e acessa pelo nome ObterProduto os dados do produto inserido
        return new CreatedAtRouteResult("ObterProduto", //Criamos essa rota nomeavel para buscar o produto pelo ID.
            new { id = produto.ProdutoId }, produto);
    }

}
