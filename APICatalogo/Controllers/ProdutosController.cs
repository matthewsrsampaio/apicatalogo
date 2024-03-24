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
            return NotFound("Produtos não encontrados");
        }
        return produtos;
    }
}
