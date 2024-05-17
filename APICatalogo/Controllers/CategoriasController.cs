using ApiCatalogo.Models;
using APICatalogo.Context;
using APICatalogo.DTOs;
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
    //private readonly ICategoriaRepository _repository;
    private readonly IUnitOfWork _uof;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    
    //**CONSTRUTOR**
    public CategoriasController(IUnitOfWork uof, IConfiguration configuration, ILogger<CategoriasController> logger)
    {
        _uof = uof;
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
    public ActionResult<IEnumerable<CategoriaDTO>> GetCategorias()
    {
        var categorias = _uof.CategoriaRepository.GetAll();

        if (categorias is null)
            return NotFound("Não existem categorias.");

        var categoriaListDto = new List<CategoriaDTO>();

        foreach(var cat in categorias)
        {
            var categoriaDto = new CategoriaDTO
            {
                CategoriaId = cat.CategoriaId,
                Nome = cat.Nome,
                ImagemUrl = cat.ImagemUrl

            };
            categoriaListDto.Add(categoriaDto);
        }

        return Ok(categoriaListDto);
    }

    [HttpGet("{id:int}/{nome:alpha:minlength(3)=abc}", Name = "ObterCategoria")] //{nome:alpha:minlength(3)=abc} -> quer dizer que eu espero receber pelo menos 3 caracteres alphanumericos, mas se eu nao receber eles, por padrão, eu receberei "abc"
    public ActionResult<IEnumerable<CategoriaDTO>> Get(int id, string nome)
    {
        //O código da linha abaixo foi somente para testar o tratamento de uma exceção através de um middleware
        //throw new Exception("Exceção ao retornar a categoria pelo id.");

        var teste = nome; //existe só pra testar os parametros do roteamento

        var categoria = _uof.CategoriaRepository.Get(c => c.CategoriaId == id);

        if (categoria is null)
        {
            //LOG INFORMATION
            _logger.LogInformation($"=================== Log-Information  GET api/categorias/id={id}&nome={nome} =====================");
            return NotFound($"Essa categoria não foi encontrada. ID = {id}");
        }

        var categoriaDto = new CategoriaDTO()
        {
            CategoriaId = categoria.CategoriaId,
            Nome = categoria.Nome,
            ImagemUrl = categoria.ImagemUrl
        };

        return Ok(categoria);
    }

    [HttpPost]
    public ActionResult<CategoriaDTO> Post(CategoriaDTO categoriaDto)
    {
        if (categoriaDto is null)
        {
            _logger.LogWarning("POST - Dados inválidos");
            return BadRequest();
        }

        //Converte CategoriaDTO para Categoria - Agora eu posso salvar no banco - REQUEST
        var categoria = new Categoria()
        {
            CategoriaId = categoriaDto.CategoriaId,
            Nome = categoriaDto.Nome,
            ImagemUrl = categoriaDto.ImagemUrl
        };

        var categoriaCriada = _uof.CategoriaRepository.Create(categoria);
        _uof.Commit();//Aqui eu estou persistindo as informações

        //Converte Categoria para CategoriaDTO - Agora eu posso passar meu response filtrado - RESPONSE
        var novaCategoriaDto = new CategoriaDTO()
        {
            CategoriaId = categoriaCriada.CategoriaId,
            Nome = categoriaCriada.Nome,
            ImagemUrl = categoriaCriada.ImagemUrl
        };

        return new CreatedAtRouteResult("ObterCategoria", new { id = novaCategoriaDto.CategoriaId }, novaCategoriaDto);
    }

    [HttpPut("{id:int}")]
    public ActionResult<CategoriaDTO> Put(int id, CategoriaDTO categoriaDto)
    {
        if (id != categoriaDto.CategoriaId)
        {
            _logger.LogWarning("PUT - Dados inválidos");
            return BadRequest("Categoria não encontrada.");
        }

        //Converte CategoriaDTO para Categoria - Agora eu posso salvar no banco - REQUEST
        var categoria = new Categoria()
        {
            CategoriaId = categoriaDto.CategoriaId,
            Nome = categoriaDto.Nome,
            ImagemUrl = categoriaDto.ImagemUrl
        };

        var categoriaAtualizada = _uof.CategoriaRepository.Update(categoria);
        _uof.Commit();//Aqui eu estou persistindo as informações

        //Converte Categoria(Já atualizada) para CategoriaDTO - Agora eu posso passar meu response filtrado - RESPONSE
        var categoriaAtualizadaDto = new CategoriaDTO()
        {
            CategoriaId = categoriaAtualizada.CategoriaId,
            Nome = categoriaAtualizada.Nome,
            ImagemUrl = categoriaAtualizada.ImagemUrl
        };

        return Ok(categoriaAtualizadaDto);
    }

    [HttpDelete("{id:int}")]
    public ActionResult<CategoriaDTO> Delete(int id)
    {
        var categoria = _uof.CategoriaRepository.Get(c => c.CategoriaId == id);

        if (categoria is null)
        {
            _logger.LogWarning($"Categoria {id} não foi encontrada.");
            return NotFound($"Categoria {id} não foi encontrada.");
        }

        var categoriaExcluida = _uof.CategoriaRepository.Delete(categoria);
        _uof.Commit();//Aqui eu estou persistindo as informações

        //Converte Categoria(Já excluída) para CategoriaDTO - Agora eu posso passar meu response filtrado - RESPONSE
        var categoriaExcluidaDto = new CategoriaDTO()
        {
            CategoriaId = categoriaExcluida.CategoriaId,
            Nome = categoriaExcluida.Nome,
            ImagemUrl = categoriaExcluida.ImagemUrl
        };

        return Ok(categoriaExcluidaDto);
    }


    /*   [HttpGet("produtos")]
       [ServiceFilter(typeof(ApiLoggingFilter))]
       public ActionResult<IEnumerable<Categoria>> GetCategoriasProdutos()
       {

           //throw new ArgumentException("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%  TESTE TESTE TESTE  %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");

           _logger.LogInformation("=================== Log-Information  GET api/categorias/produtos =====================");

           //var listaProdutos = _context.Categorias.Include(p => p.Produtos).ToList();
           //Dessa forma aqui consigo retornar uma lista de categorias onde o ID é menor ou igual a 5. Massa né?
           //Aaahh o professor falou que nunca é bom retornar uma lista completa. É sempre bom colocar filtros.
           //var listaProdutos = _context.Categorias.Include(p => p.Produtos).Where(c => c.CategoriaId <= 5).ToList();

           *//*if (listaProdutos is null)
               return BadRequest("Objeto não encontrado.");*//*

          // var listaProdutos = _repository.GetCategoriaProdutos();

           return Ok(listaProdutos);
       }*/

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

}
