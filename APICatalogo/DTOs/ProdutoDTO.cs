using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace APICatalogo.DTOs
{
    public class ProdutoDTO
    {
        public int ProdutoId { get; set; }
        [Required]
        public string? Nome { get; set; }                
        [Required]
        public string? Descricao { get; set; }
        [Required]
        public decimal Preco { get; set; }
        [Required]
        [StringLength(300)]
        public string? ImagemUrl { get; set; }
        public int CategoriaId { get; set; }
    }
}
