using ApiCatalogo.Models;
using APICatalogo.Context;
using APICatalogo.Filters;
using APICatalogo.Repositories;
using APICatalogo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    
    //**CONSTRUTOR**
    public CategoriasController(ICategoriaRepository repository,
                                IConfiguration configuration,
                                ILogger<CategoriasController> logger)
    {
        _repository = repository;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("LerArquivoConfiguracao")]
    public string GetValores()
    {
        var valor1 = _configuration["chave1"];
        var valor2 = _configuration["chave2"];

        var secao1 = _configuration["secao1:chave2"];
        var connectionString = _configuration["ConnectionStrings:DefaultConnection"];

        return $"Chave1 = {valor1}\nChave2 = {valor2}\nSeção1 => Chave2 = {secao1}\nInfo Conexão =>> {connectionString}";
    }

    //O [FromServices] é usado quando se quer linkar a injeção somente a um método. Normalmente a injeção é feita la no construtor para a classe inteira.
    [HttpGet("saudacaoComFromService/{nome}")] 
    public ActionResult<string> GetSaudacaoComFromServices([FromServices] IMeuServico meuServico, string nome)
    {
        return meuServico.saudacao(nome);
    }

    //Este é o comportamento padrão, sem o [FromServices]
    [HttpGet("saudacaoSemFromService/{nome}")]
    public ActionResult<string> GetSaudacaoSemFromServices(IMeuServico meuServico, string nome)
    {
        return meuServico.saudacao(nome);
    }

    [HttpGet]
    public ActionResult<IEnumerable<Categoria>> GetCategoria()
    {
        var categorias = _repository.GetCategorias().ToList();
        return Ok(categorias);
    }


    [HttpGet("produtos")]
    [ServiceFilter(typeof(ApiLoggingFilter))]
    public ActionResult<IEnumerable<Categoria>> GetCategoriasProdutos()
    {

        //throw new ArgumentException("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%  TESTE TESTE TESTE  %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");

        _logger.LogInformation("=================== Log-Information  GET api/categorias/produtos =====================");

        //var listaProdutos = _context.Categorias.Include(p => p.Produtos).ToList();
        //Dessa forma aqui consigo retornar uma lista de categorias onde o ID é menor ou igual a 5. Massa né?
        //Aaahh o professor falou que nunca é bom retornar uma lista completa. É sempre bom colocar filtros.
        //var listaProdutos = _context.Categorias.Include(p => p.Produtos).Where(c => c.CategoriaId <= 5).ToList();

        /*if (listaProdutos is null)
            return BadRequest("Objeto não encontrado.");*/

        var listaProdutos = _repository.GetCategoriaProdutos();

        return Ok(listaProdutos);
    }

    /*// api/categorias?numero=digitaQualquerNumero&nome=digitaQualquerString            o & concatena os atributos
    [HttpGet]
    public ActionResult<IEnumerable<Categoria>> GetCategoriasTeste([BindRequired] int numero, [BindRequired] string nome) //Com o [BindRequired] eu estou obrigando o requerente a me enviar os parametros numero e nome
    {
        var _numero = numero;
        var _nome = nome;

        _logger.LogInformation("=================== Log-Information  GET api/categorias/produtos =====================");

        var categorias = _context.Categorias.AsNoTracking().ToList(); //Adicionei AsNoTracking() para a procura nao ser armazenada no cache(contexto). Pois é apenas uma consulta que não vai precisar do contexto(cache).

        if (categorias is null)
            return NotFound("Nenhuma categoria foi encontrada!");

        return categorias;
    }*/

    [HttpGet("{id:int}/{nome:alpha:minlength(3)=abc}", Name = "ObterCategoria")] //{nome:alpha:minlength(3)=abc} -> quer dizer que eu espero receber pelo menos 3 caracteres alphanumericos, mas se eu nao receber eles, por padrão, eu receberei "abc"
    public ActionResult<IEnumerable<Categoria>> Get(int id, string nome)
    {
        //O código da linha abaixo foi somente para testar o tratamento de uma exceção através de um middleware
        //throw new Exception("Exceção ao retornar a categoria pelo id.");

        var teste = nome; //existe só pra testar os parametros do roteamento

        var categoria = _repository.GetCategoria(id);

        if (categoria is null)
        {
            //LOG INFORMATION
            _logger.LogInformation($"=================== Log-Information  GET api/categorias/id={id}&nome={nome} =====================");
            return NotFound($"Essa categoria não foi encontrada. ID = {id}");
        }
            
        return Ok(categoria);
    }

    [HttpPost]
    public ActionResult Post(Categoria categoria)
    {
        if (categoria is null) 
        { 
            _logger.LogWarning("POST - Dados inválidos");
            return BadRequest();
        }

        var categoriaCriada = _repository.Create(categoria);

        return new CreatedAtRouteResult("ObterCategoria", new { id = categoriaCriada.CategoriaId }, categoriaCriada);
    }

    [HttpPut("{id:int}")]
    public ActionResult Put(int id, Categoria categoria)
    {
        if (id != categoria.CategoriaId)
        {
            _logger.LogWarning("PUT - Dados inválidos");
            return BadRequest("Categoria não encontrada.");
        }

        _repository.Update(categoria);

        return Ok(categoria); 
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var categoria = _repository.GetCategoria(id);

        if (categoria is null)
        {
            _logger.LogWarning($"Categoria {id} não foi encontrada.");
            return NotFound($"Categoria {id} não foi encontrada.");
        }

        var categoriaExcluida = _repository.Delete(id);

        return Ok(categoriaExcluida);
    }

}
