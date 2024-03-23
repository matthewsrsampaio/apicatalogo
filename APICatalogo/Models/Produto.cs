using ApiCatalogo.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Produtos")]
public class Produto
{
    [Key]
    public int ProdutoId { get; set; }

    [Required]
    [StringLength(80)]
    public string? Nome { get; set; }

    [Required]
    [StringLength(300)]
    public string? Descricao { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Preco {  get; set; }

    [Required]
    [StringLength(300)]
    public string? ImagemUrl { get; set;}

    public float Estoque {  get; set; }

    public DateTime DataCadastro {  get; set; }

    //CategoriaId será a chave a chave estrangeira
    public int CategoriaId { get; set; }

    //Aqui em Categoria eu estou informando que Produto está mapeado para uma Categoria
    public Categoria? Categoria { get; set; }
}
