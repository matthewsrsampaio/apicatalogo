using System.ComponentModel.DataAnnotations;

namespace APICatalogo.DTOs
{
    public class ProdutoDTOUpdateRequest : IValidatableObject
    {
        [Range(1,9999, ErrorMessage = "O estoque deve estar entre 1 e 9999.")]
        public float Estoque { get; set; }
        public DateTime DataCadastro { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Se a data inserida não for mais recente que a data atual o sistema lançará essa mensagem de erro
            if(DataCadastro.Date <= DateTime.Now.Date)
            {
                yield return new ValidationResult("A data deve ser anterior a data atual.",
                             new[] { nameof(this.DataCadastro) }
                             );
            }
        }
    }
}
