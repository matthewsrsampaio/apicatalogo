using ApiCatalogo.Models;
using APICatalogo.Validations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

[Table("Produtos")]
public class Produto
{
    [Key]
    public int ProdutoId { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório")] //especifica que o valor do campo é NOTNULL
    [StringLength(20, MinimumLength = 3, ErrorMessage = "O nome deve ter entre {2} e {1} caracteres.")]
    [PrimeiraLetraMaiuscula] //Criamos essa anotação que verifica se a primeira letra do nome inserido é maiúscula
    public string? Nome { get; set; }

    [Required]//especifica que o valor do campo é NOTNULL
    [StringLength(80, ErrorMessage = "A descrição deve ter no máximo entre {1} caracteres.")]
    public string? Descricao { get; set; }

    [Required]//especifica que o valor do campo é NOTNULL
    [Column(TypeName = "decimal(10,2)")]
    [Range(1, 10000, ErrorMessage = "O preço deve estar entre {1} e {2}")]
    public decimal Preco { get; set; }

    [Required]//especifica que o valor do campo é NOTNULL
    [StringLength(300)]
    public string? ImagemUrl { get; set; }

    public float Estoque { get; set; }

    public DateTime DataCadastro {  get; set; }

    //CategoriaId será a chave a chave estrangeira
    public int CategoriaId { get; set; }

    //Aqui em Categoria eu estou informando que Produto está mapeado para uma Categoria
    [JsonIgnore] //vai ignorar a serialização dessa propriedade
    public Categoria? Categoria { get; set; }
}
