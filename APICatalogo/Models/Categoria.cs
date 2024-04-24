using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ApiCatalogo.Models
{
    [Table("Categorias")]
    public class Categoria
    {
        public Categoria()
        {
            //Para efeitos de boas práticas vamos iniciar nossa coleção nesse construtor. A classe Categoria fica responsável por inicializar essa coleção. 
            //Eu acho que também poderia ser feito assim, vi em algum lugar que dá. :)
            //Produtos = []
            Produtos = new Collection<Produto>();
        }

        [Key]
        public int CategoriaId { get; set; }

        [Required] //especifica que o valor do campo é NOTNULL
        [StringLength(80)]
        public string? Nome { get; set; }  //A interrogação aqui significa que nosso atributo pode ser nulo

        [BindNever] //Ñão poderei usar como binding
        [Required]//especifica que o valor do campo é NOTNULL
        [StringLength(300)]
        public string? ImagemUrl { get; set; }
        //Aqui estamos informando que uma categoria pode ter uma coleção de produto

        [JsonIgnore]
        public ICollection<Produto>? Produtos { get; set; }

    }
}  