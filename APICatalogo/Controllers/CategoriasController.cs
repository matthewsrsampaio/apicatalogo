using ApiCatalogo.Models;
using APICatalogo.Context;
using APICatalogo.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
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


        [HttpGet("produtos")]
        public ActionResult<IEnumerable<Categoria>> GetCategoriasProdutos()
        {
            try
            {
                var listaProdutos = _context.Categorias.Include(p => p.Produtos).AsNoTracking().ToList();  //Adicionei AsNoTracking() para a procura nao ser armazenada no cache(contexto). Pois é apenas uma consulta que não vai precisar do contexto(cache).

                //Dessa forma aqui consigo retornar uma lista de categorias onde o ID é menor ou igual a 5. Massa né?
                //Aaahh o professor falou que nunca é bom retornar uma lista completa. É sempre bom colocar filtros.
                //var listaProdutos = _context.Categorias.Include(p => p.Produtos).Where(c => c.CategoriaId <= 5).ToList();

                if (listaProdutos is null)
                    return BadRequest("Objeto não encontrado.");

                return listaProdutos;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Oops, algo deu errado.");
            }
        }

        // api/categorias?numero=digitaQualquerNumero&nome=digitaQualquerString            o & concatena os atributos
        [HttpGet]
        public ActionResult<IEnumerable<Categoria>> GetCategoriasTeste([BindRequired] int numero, [BindRequired] string nome) //Com o [BindRequired] eu estou obrigando o requerente a me enviar os parametros numero e nome
        {
            var _numero = numero;
            var _nome = nome;

            try
            {
                var categorias = _context.Categorias.AsNoTracking().ToList(); //Adicionei AsNoTracking() para a procura nao ser armazenada no cache(contexto). Pois é apenas uma consulta que não vai precisar do contexto(cache).

                if (categorias is null)
                    return NotFound("Nenhuma categoria foi encontrada!");

                return categorias;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Oops, algo deu errado.");
            }
        }

        [HttpGet("{id:int}/{nome:alpha:minlength(3)=abc}", Name = "ObterCategoria")] //{nome:alpha:minlength(3)=abc} -> quer dizer que eu espero receber pelo menos 3 caracteres alphanumericos, mas se eu nao receber eles, por padrão, eu receberei "abc"
        public ActionResult<IEnumerable<Categoria>> Get(int id, string nome)
        {
            try
            {
                var teste = nome; //existe só pra testar os parametros do roteamento

                var categoria = _context.Categorias.FirstOrDefault(c => c.CategoriaId == id);

                if (categoria is null)
                    return NotFound($"Essa categoria não foi encontrada. ID = {id}");

                return Ok(categoria);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Oops, algo deu errado.");
            }
        }

        [HttpPost]
        public ActionResult Post(Categoria categoria)
        {
            try
            {
                if (categoria is null)
                    return BadRequest();

                var categoria_nome = _context.Categorias.FirstOrDefault(c => c.Nome == categoria.Nome);

                _context.Categorias.Add(categoria);
                _context.SaveChanges(); //Nesse momento, acabamos de salvar no banco.

                return new CreatedAtRouteResult("ObterCategoria", new { id = categoria.CategoriaId }, categoria);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Oops, algo deu errado.");
            }
        }

        [HttpPut("{id:int}")]
        public ActionResult Put(int id, Categoria categoria)
        {
            try
            {
                if (id != categoria.CategoriaId)
                    return BadRequest("Categoria não encontrada.");

                _context.Entry(categoria).State = EntityState.Modified; //Basicamente vai adicionar o estado modificado ao estado que está no banco.
                _context.SaveChanges();

                return Ok(categoria);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Oops, algo deu errado.");
            }

            
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            try
            {
                var categoria = _context.Categorias.FirstOrDefault(c => c.CategoriaId == id);

                if (categoria is null)
                    return NotFound("Categoria não encontrada.");

                _context.Categorias.Remove(categoria);
                _context.SaveChanges();

                return Ok(categoria);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Oops, algo deu errado.");
            }
        }

    }
}
