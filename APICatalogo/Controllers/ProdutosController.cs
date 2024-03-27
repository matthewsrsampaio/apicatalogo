using APICatalogo.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Controllers;

[Route("[controller]")]
[ApiController]
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
            return NotFound("Produtos não encontrados!");

        return produtos;
    }

    [HttpGet("{id:int}", Name = "ObterProduto")] // "{id:int}") quer dizer que estou esperando um parâmetro do tipo inteiro chamado id
    public ActionResult<Produto> Get(int id)
    {
        var produto = _context.Produtos.FirstOrDefault(p => p.ProdutoId == id); //FirstOrDefault vai atrás do primeiro id na tabela que se
                                                                                //assemelha ao id  que está sendo passado no parâmtro.
                                                                                //Caso não encontre nada esse 'método me retornará um null.
        if(produto is null)
            return NotFound("Produto não encontrado!");
        
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

    //Atualizamos toda a entidade, mas se quiséssemos atualizar somente alguns atributos poderiamos usar o método [HttpPatch]
    [HttpPut("{id:int}")]
    public ActionResult Put(int id, Produto produto)
    {
        //já faz uma verificação pra saber se os ids batem. Se o id passado não existir no banco o sistema lança um BadRequest();
        if(id != produto.ProdutoId)
            return BadRequest();

        //Aqui precisamos informar que a entidade produto encontra-se em estado modificado.
        //Com isso o EntityFrameworkCore vai entender que essa entidade precisa ser persistida.
        _context.Entry(produto).State = EntityState.Modified;  //=> aqui marcamos
        _context.SaveChanges(); //=> aqui persistimos

        return Ok(produto);
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var produto = _context.Produtos.FirstOrDefault(p => p.ProdutoId == id);
        //var produto = _context.Produtos.Find(id); //Poderia usar o Find(). O Find() vai buscar primeiro na memória,
                                                  //mas para funcionar o id teria que ser uma chave primária.
        if (produto is null)
            return NotFound("Produto não localizado!");

        _context.Produtos.Remove(produto);
        _context.SaveChanges();

        return Ok(produto);
    }

}
