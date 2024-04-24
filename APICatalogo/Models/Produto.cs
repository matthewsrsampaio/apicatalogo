using ApiCatalogo.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

[Table("Produtos")]
public class Produto
{
    [Key]
    public int ProdutoId { get; set; }

    [Required] //especifica que o valor do campo é NOTNULL
    [StringLength(80)]
    public string? Nome { get; set; }

    [Required]//especifica que o valor do campo é NOTNULL
    [StringLength(300)]
    public string? Descricao { get; set; }
    
    [Required]//especifica que o valor do campo é NOTNULL
    [Column(TypeName = "decimal(10,2)")]
    public decimal Preco {  get; set; }

    [Required]//especifica que o valor do campo é NOTNULL
    [StringLength(300)]
    public string? ImagemUrl { get; set;}

    public float Estoque {  get; set; }

    public DateTime DataCadastro {  get; set; }

    //CategoriaId será a chave a chave estrangeira
    public int CategoriaId { get; set; }

    //Aqui em Categoria eu estou informando que Produto está mapeado para uma Categoria
    [JsonIgnore] //vai ignorar a serialização dessa propriedade
    public Categoria? Categoria { get; set; }
}
