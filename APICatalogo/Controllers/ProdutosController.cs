using ApiCatalogo.Models;
using APICatalogo.Context;
using APICatalogo.Filters;
using APICatalogo.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoRepository _repository;
        private readonly ILogger _logger;

        public ProdutosController(IProdutoRepository repository, ILogger<ProdutosController> logger, AppDbContext context)
        {
            _repository = repository;
            _logger = logger;
        }

        //api/produtos
        [HttpGet]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public ActionResult<IQueryable<Produto>> GetProdutox()
        {
            _logger.LogInformation($"=================== Log-Information  GET api/produtos =====================");

            var produtos = _repository.GetProdutos();

            if (produtos is null)
                return NotFound();

            return Ok(produtos);
        }

        [HttpGet("{id:int}", Name = "ObterProdutos")]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public ActionResult<IEnumerable<Produto>> GetProduto(int id)
        {
            var produto = _repository.GetProduto(id);

            if (produto is null)
                NotFound($"Produto de id = {id} não foi encontrado.");

            _logger.LogInformation($"=================== Log - Information  GET api/produtos/{id} ===================== ");

            return Ok(produto);
        }

        [HttpPost]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public ActionResult Post(Produto produto)
        {
            if (produto is null)
            {
                _logger.LogWarning("POST - Dados inválidos.");
                return BadRequest();
            }

            var produtoCriado = _repository.Create(produto);

            _logger.LogInformation($"=================== Log - Information  POST api/produtos ===================== ");
            _logger.LogInformation($"=================== POST id = {produtoCriado.ProdutoId}, produto = {produtoCriado.Nome} ===================== ");

            return Ok(new CreatedAtRouteResult("ObterProdutos", new { id = produtoCriado.ProdutoId }, produtoCriado));
        }

        [HttpPut("{id:int}")]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public ActionResult Put(int id, Produto produto)
        {
            if (produto.ProdutoId != id)
            {
                _logger.LogWarning("PUT - Dados inválidos.");
                return BadRequest();
            }

            _repository.Update(produto);

            return Ok(produto);
        }

        [HttpDelete("{id:int}")]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public ActionResult Delete(int id)
        {
            var produto = _repository.GetProduto(id);

            if (produto is null)
            {
                _logger.LogWarning($"Produto {id} não foi encontrada.");
                return NotFound($"Produto {id} não foi encontrada.");
            }

            var produtoExcluido = _repository.Delete(id);

            return Ok(produtoExcluido);
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


