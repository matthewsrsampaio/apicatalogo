using ApiCatalogo.Models;
using APICatalogo.DTOs;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using APICatalogo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;

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
    public CategoriasController(IUnitOfWork uof, 
           IConfiguration configuration, 
           ILogger<CategoriasController> logger)
    {
        _uof = uof;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("Pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get([FromQuery] CategoriasParameters categoriasParameters)
    {
        var categorias = await _uof.CategoriaRepository.GetCategoriasAsync(categoriasParameters);
        return ObterCategorias(categorias);
    }

    [HttpGet("filter/nome/pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetCategoriasFiltradas([FromQuery] CategoriasFiltroNome categoriasFiltro)
    {
        var categoriasFiltradas = await _uof.CategoriaRepository.GetCategoriasFiltroNomeAsync(categoriasFiltro);
        return ObterCategorias(categoriasFiltradas);
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

    //[Authorize(AuthenticationSchemes = "Bearer")] //Usei essa abordagem pq a autenticação não estava funcionando
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetCategorias()
    {
        var categoriasList = await _uof.CategoriaRepository.GetAllAsync();

        if (categoriasList is null)
            return NotFound("Não existem categorias.");
        
        var categoriaListDto = categoriasList.ToCategoriaDTOList();

        return Ok(categoriaListDto);
    }

    [HttpGet("{id:int}/{nome:alpha:minlength(3)=abc}", Name = "ObterCategoria")] //{nome:alpha:minlength(3)=abc} -> quer dizer que eu espero receber pelo menos 3 caracteres alphanumericos, mas se eu nao receber eles, por padrão, eu receberei "abc"
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get(int id, string nome)
    {
        //O código da linha abaixo foi somente para testar o tratamento de uma exceção através de um middleware
        //throw new Exception("Exceção ao retornar a categoria pelo id.");

        var teste = nome; //existe só pra testar os parametros do roteamento

        var categoria = await _uof.CategoriaRepository.GetAsync(c => c.CategoriaId == id);

        if (categoria is null)
        {
            //LOG INFORMATION
            _logger.LogInformation($"=================== Log-Information  GET api/categorias/id={id}&nome={nome} =====================");
            return NotFound($"Essa categoria não foi encontrada. ID = {id}");
        }

        //Converte CategoriaDTO para Categoria - Agora eu posso salvar no banco - RESPONSE
        var categoriaDTO = categoria.ToCategoriaDTO();

        return Ok(categoriaDTO);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDTO>> Post(CategoriaDTO categoriaDto)
    {
        if (categoriaDto is null)
        {
            _logger.LogWarning("POST - Dados inválidos");
            return BadRequest();
        }

        //Converte CategoriaDTO para Categoria - Agora eu posso salvar no banco - REQUEST
        var categoria = categoriaDto.ToCategoria();

        var categoriaCriada = _uof.CategoriaRepository.Create(categoria);
        await _uof.CommitAsync();//Aqui eu estou persistindo as informações

        //Converte Categoria para CategoriaDTO - Agora eu posso passar meu response filtrado - RESPONSE
        var novaCategoriaDto = categoria.ToCategoriaDTO();

        return new CreatedAtRouteResult("ObterCategoria", 
               new { id = novaCategoriaDto.CategoriaId },
               novaCategoriaDto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoriaDTO>> Put(int id, CategoriaDTO categoriaDto)
    {
        if (id != categoriaDto.CategoriaId)
        {
            _logger.LogWarning("PUT - Dados inválidos");
            return BadRequest("Categoria não encontrada.");
        }

        //Converte CategoriaDTO para Categoria - Agora eu posso salvar no banco - REQUEST
        var categoria = categoriaDto.ToCategoria();

        var categoriaAtualizada = _uof.CategoriaRepository.Update(categoria);
        await _uof.CommitAsync();//Aqui eu estou persistindo as informações

        //Converte Categoria(Já atualizada) para CategoriaDTO - Agora eu posso passar meu response filtrado - RESPONSE
        var categoriaAtualizadaDto = categoria.ToCategoriaDTO();

        return Ok(categoriaAtualizadaDto);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<CategoriaDTO>> Delete(int id)
    {
        var categoria = await _uof.CategoriaRepository.GetAsync(c => c.CategoriaId == id);

        if (categoria is null)
        {
            _logger.LogWarning($"Categoria {id} não foi encontrada.");
            return NotFound($"Categoria {id} não foi encontrada.");
        }

        var categoriaExcluida = _uof.CategoriaRepository.Delete(categoria);
        await _uof.CommitAsync();//Aqui eu estou persistindo as informações

        //Converte Categoria(Já excluída) para CategoriaDTO - Agora eu posso passar meu response filtrado - RESPONSE
        var categoriaExcluidaDto = categoria.ToCategoriaDTO();

        return Ok(categoriaExcluidaDto);
    }

    private ActionResult<IEnumerable<CategoriaDTO>> ObterCategorias(IPagedList<Categoria> categorias)
    {
        var metadata = new
        {
            categorias.Count, //TotalCount,
            categorias.PageSize,
            categorias.PageCount, //CurrentPage,
            categorias.TotalItemCount, //TotalPages,
            categorias.HasNextPage,  //HasNext
            categorias.HasPreviousPage   //HasPreviousPage
        };

        //Vai exibir na cabeçalho da Response informações pertinentes a paginação
        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        var categoriasDto = categorias.ToCategoriaDTOList();

        return Ok(categoriasDto);
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
