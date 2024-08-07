using ApiCatalogo.Models;
using APICatalogo.DTOs;
using APICatalogo.Filters;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;

namespace APICatalogo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ProdutosController : ControllerBase
    {
        //private readonly IProdutoRepository _produtoRepository;
        //private readonly IRepository<Produto> _repository;
        private readonly IUnitOfWork _uof;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ProdutosController(IUnitOfWork uof, ILogger<ProdutosController> logger, IMapper mapper)
        {
            _uof = uof;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("pagination")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Get([FromQuery] ProdutosParameters produtosParameters)
        {
            var produtos = await _uof.ProdutoRepository.GetProdutosAsync(produtosParameters);
            return ObterProdutos(produtos);
        }

        [HttpGet("filter/preco/pagination")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosFilterPreco([FromQuery] ProdutosFiltroPreco produtosFiltroPrecoParameters)
        {
            var produtos = await _uof.ProdutoRepository.GetProdutosFiltroPrecoAsync(produtosFiltroPrecoParameters);
            return ObterProdutos(produtos);
        }

        //api/produtos
        //[ServiceFilter(typeof(ApiLoggingFilter))]
        //[Authorize(AuthenticationSchemes = "Bearer")] //Usei essa abordagem pq a autenticação não estava funcionando
        [HttpGet]
        //[Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IQueryable<ProdutoDTO>>> GetProdutos()
        {
            _logger.LogInformation($"=================== Log-Information  GET api/produtos =====================");

            var produtos = await _uof.ProdutoRepository.GetAllAsync();

            if (produtos is null)
                return NotFound();

            //var destino = _mapper.Map<Destino>(origem);   -> Aqui estamos usando Map do pct AutoMapper.
            var produtoDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

            return Ok(produtoDto);
        }

        //api/produtosPorCategoria/id
        [HttpGet("produtosPorCategoria/{id}")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosCategoria(int id)
        {
            var produto = await _uof.ProdutoRepository.GetProdutosPorCategoriaAsync(id);

            if (produto is null)
            {
                return NotFound();
            }

            _logger.LogInformation($"==============     PUT     ==============");

            //var destino = _mapper.Map<Destino>(origem);   -> Aqui estamos usando Map do pct AutoMapper.
            var produtoDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produto);

            return Ok(produtoDto);
        }

        [HttpGet("{id:int}", Name = "ObterProdutos")]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public async Task<ActionResult<ProdutoDTO>> GetProduto(int id)
        {
            var produto = await _uof.ProdutoRepository.GetAsync(p => p.ProdutoId == id);

            if (produto is null)
                NotFound($"Produto de id = {id} não foi encontrado.");

            _logger.LogInformation($"=================== Log - Information  GET api/produtos/{id} ===================== ");

            //var destino = _mapper.Map<Destino>(origem);   -> Aqui estamos usando Map do pct AutoMapper.
            var produtoDto = _mapper.Map<ProdutoDTO>(produto);  

            return Ok(produtoDto);
        }

        [HttpPost]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public async Task<ActionResult<ProdutoDTO>> Post(ProdutoDTO produtoDto)
        {
            if (produtoDto is null)
            {
                _logger.LogWarning("POST - Dados inválidos.");
                return BadRequest();
            }

            //var destino = _mapper.Map<Destino>(origem);   -> Aqui estamos usando Map do pct AutoMapper.
            var produto = _mapper.Map<Produto>(produtoDto);

            var produtoCriado = _uof.ProdutoRepository.Create(produto);
            await _uof.CommitAsync();//Aqui eu estou persistindo as informações

            //var destino = _mapper.Map<Destino>(origem);   -> Aqui estamos usando Map do pct AutoMapper.
            var produtoCriadoDto = _mapper.Map<ProdutoDTO>(produtoCriado);

            _logger.LogInformation($"=================== Log - Information  POST api/produtos ===================== ");
            _logger.LogInformation($"=================== POST id = {produtoCriadoDto.ProdutoId}, produto = {produtoCriadoDto.Nome} ===================== ");

            return Ok(new CreatedAtRouteResult("ObterProdutos", new { id = produtoCriadoDto.ProdutoId }, produtoCriadoDto));
        }

        [HttpPut("{id:int}")]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public async Task<ActionResult<ProdutoDTO>> Put(int id, ProdutoDTO produtoDto)
        {
            //var destino = _mapper.Map<Destino>(origem);   -> Aqui estamos usando Map do pct AutoMapper.
            var produto = _mapper.Map<Produto>(produtoDto);

            if (id != produto.ProdutoId)
            {
               _logger.LogWarning("PUT - Dados inválidos.");
                return BadRequest();
            }

            var produtoAtualizado = _uof.ProdutoRepository.Update(produto);
            await _uof.CommitAsync();//Aqui eu estou persistindo as informações

            //var destino = _mapper.Map<Destino>(origem);   -> Aqui estamos usando Map do pct AutoMapper.
            var produtoAtualizadoDto = _mapper.Map<ProdutoDTO>(produtoAtualizado);

            _logger.LogInformation($"==============     PUT     ==============");
            _logger.LogInformation($"Produto de id={id} foi atualizado.");

            return Ok(produtoAtualizadoDto);
        }

        //Este método realiza atualizações parciais.
        [HttpPatch("{id}/UpdatePartial")]
        public async Task<ActionResult<ProdutoDTOUpdateResponse>> Patch(int id,
            JsonPatchDocument<ProdutoDTOUpdateRequest> patchProdutoDTO)
        {
            //Verifica se o id recebido pela requisição é válido e o se o corpo é nulo
            if (id <= 0 || patchProdutoDTO is null)
                return BadRequest("Id inválido ou produto nulo.");

            //Crio uma variável para receber o produto com o id da requisição
            var produto = await _uof.ProdutoRepository.GetAsync(p => p.ProdutoId == id);

            //Verifico se o produto recebido é nulo
            if (produto is null)
                return NotFound("produto não existe");

            //Converto o Produto em ProdutoDTOUpdateRequest(Este DTO pode realizar as atualizações PATCH)
            var produtoUpdateRequest = _mapper.Map<ProdutoDTOUpdateRequest>(produto);

            //Aplico o patchProdutoDTO no produtoUpdateRequest e coloco em modo de transição ModelState
            patchProdutoDTO.ApplyTo(produtoUpdateRequest,ModelState);

            //Verifico se o ModelState é válido
            if(!ModelState.IsValid || TryValidateModel(produtoUpdateRequest))
                return BadRequest(ModelState);

            //Mapeia produtoUpdateRequest(DTO) para produto(Produto)
            _mapper.Map(produtoUpdateRequest, produto);

           //Persiste no no banco.
            _uof.ProdutoRepository.Update(produto);
            await _uof.CommitAsync();

            //Transforma o Produto em ProdutoDTOUpdateResponse
                                             //SOURCE - DESTINATION
            var produtoUpdated = _mapper.Map<ProdutoDTOUpdateResponse>(produto);

            return Ok(produtoUpdated);
        }

        [HttpDelete("{id:int}")]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public async Task<ActionResult<ProdutoDTO>> Delete(int id)
        {   
            var produto = await _uof.ProdutoRepository.GetAsync(d => d.ProdutoId == id);

            if (produto is null)
                return NotFound("Produto não encontrado");

            var produtoExcluido = _uof.ProdutoRepository.Delete(produto);
            await _uof.CommitAsync();//Aqui eu estou persistindo as informações

            //var destino = _mapper.Map<Destino>(origem);   -> Aqui estamos usando Map do pct AutoMapper.
            var produtoExcluidoDto = _mapper.Map<ProdutoDTO>(produtoExcluido);

            _logger.LogInformation($"==============     DELETE     ==============");
            _logger.LogWarning($"Produto de id={id} foi deletado.");

            return Ok(produtoExcluidoDto);

            /*if (produto is null)
            {
                _logger.LogWarning($"Produto {id} não foi encontrada.");
                return Ok($"Produto de id={id} foi excluído.");
            }
            else
            {
                return StatusCode(500, $"Falha ao excluir objeto de id={id}");
            }*/
        }

        private ActionResult<IEnumerable<ProdutoDTO>> ObterProdutos(IPagedList<Produto> produtos)
        {
            var metadata = new
            {
                produtos.Count, //TotalCount,
                produtos.PageSize,
                produtos.PageCount,    //CurrentPage,
                produtos.TotalItemCount,     //TotalPages,
                produtos.HasNextPage,   //HasNext
                produtos.HasPreviousPage   //HasPrevious
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
            return Ok(produtosDto);
        }

        /*//Apenas um teste do uso de IActionResult -> Note: Usado para MVC
        [HttpGet("teste")]
        public IActionResult GetTeste()
        {
            //var produto = _context.Produtos.FirstOrDefault(p => p.Nome.Contains("Mat"));

           *//* if (produto == null)
                return NotFound();

            return Ok(produto);*//*
        }*/

        /*// api/produtos/primeiro
        [HttpGet("primeiro")]
        // /primeiro
        [HttpGet("/primeiro")]
        public async Task<ActionResult<Produto>> GetPrimeiro() //Esse método foi implementado só para exercitar o roteamento
        {
            // throw new ArgumentException("Ocorreu um erro no tratamento do request");

            var produto = await _context.Produtos.AsNoTracking().FirstOrDefaultAsync();

            if (produto is null)
                return NotFound("Produto não encontrado");

           return Ok(produto);
        }*/

        // /api/produtos
        /*[HttpGet]
        //Aplicando o filtro criado
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public async Task<ActionResult<IEnumerable<Produto>>> Get2() //Testando métodos assincronos
         //Para usar NotFound é necessário envelopar a classe no método ActionResult
        {
            var produtos = await _context.Produtos.AsNoTracking().ToListAsync();

            if (produtos is null)
                return NotFound("Produtos não encontrados!");

            return Ok(produtos);
        }*/

        // /api/produtos/id                                                   // {nome=nomePadrao} significa que eu estou esperando um id/umaString, mas se a string não for passada ela receberá por padrão a string "nomePadrao"
        /*[HttpGet("{id:int:min(1)}/{nome=nomePadrao}", Name = "ObterProduto")] // "{id:int}") quer dizer que estou esperando um parâmetro do tipo inteiro chamado id
                                                                              // {id:int:min(1)} Estou criando uma restrição para que valores menores que 1 não sejam válidos. Pq? Pq dessa forma eu evito uma consulta desnecessária ao banco de dados.
        public async Task<ActionResult<Produto>> Get(int id, string nome)
        {
            var nomeQualquer = nome;

            var produto = await _context.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.ProdutoId == id); //FirstOrDefault vai atrás do primeiro id na tabela que se
                                                                                    //assemelha ao id  que está sendo passado no parâmtro.
                                                                                    //Caso não encontre nada esse 'método me retornará um null.
            if (produto is null)
                return NotFound("Produto não encontrado!");

            return Ok(produto);
        }

        // /api/produtos
        [HttpPost]
        public ActionResult Post(Produto produto)
        {
            //Não estou precisando dessa validação aqui pq consigo fazer ela com o [ApiController] e com os Data Annotations
            *//*if (!ModelState.IsValid)
                return BadRequest();*//*

            //Ajusta a data UTC automaticamente
            produto.DataCadastro = DateTime.UtcNow;

            _context.Produtos.Add(produto); //Adicionando a instancia de produto
            _context.SaveChanges(); //Persiste as informações


            //O método CreatedAtRouteResult lança o código 201 e acessa pelo nome ObterProduto os dados do produto inserido
            return Ok(new CreatedAtRouteResult("ObterProduto", //Criamos essa rota nomeavel para buscar o produto pelo ID.
                new { id = produto.ProdutoId }, produto));

        }

        // /api/produtos
        //Atualizamos toda a entidade, mas se quiséssemos atualizar somente alguns atributos poderiamos usar o método [HttpPatch]
        [HttpPut("{id:int}")]
        public ActionResult Put(int id, Produto produto)
        {
            //já faz uma verificação pra saber se os ids batem. Se o id passado não existir no banco o sistema lança um BadRequest();
            if (id != produto.ProdutoId)
                return BadRequest();

            //Ajusta a data UTC automaticamente
            produto.DataCadastro = DateTime.UtcNow;

            //Aqui precisamos informar que a entidade produto encontra-se em estado modificado.
            //Com isso o EntityFrameworkCore vai entender que essa entidade precisa ser persistida.
            _context.Entry(produto).State = EntityState.Modified;  //=> aqui marcamos
            _context.SaveChanges(); //=> aqui persistimos

            return Ok(produto);

        }

        // /api/produtos
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

        }*/

    }
}


