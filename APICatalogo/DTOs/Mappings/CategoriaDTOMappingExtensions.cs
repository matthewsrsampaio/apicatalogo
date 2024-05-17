using ApiCatalogo.Models;

namespace APICatalogo.DTOs.Mappings
{
    public static class CategoriaDTOMappingExtensions
    {
        //Método estático para transformar Categoria em CategoriaDTO
        public static CategoriaDTO? ToCategoriaDTO(this Categoria categoria)
        {
            if (categoria is null)
                return null;

            return new CategoriaDTO
            {
                CategoriaId = categoria.CategoriaId,
                Nome = categoria.Nome,
                ImagemUrl = categoria.ImagemUrl,
            };
        }


        public static Categoria? ToCategoria(this CategoriaDTO categoriaDTO)
        {
            if (categoriaDTO is null)
                return null;

            return new Categoria
            {
                CategoriaId = categoriaDTO.CategoriaId,
                Nome = categoriaDTO.Nome,
                ImagemUrl = categoriaDTO.ImagemUrl,
            };
        }
                                                                   //Recebo a lista de objetos do tipo Categoria
        public static IEnumerable<CategoriaDTO> ToCategoriaDTOList(this IEnumerable<Categoria> categorias)
        {
            //Verifico se a lista é nula ou vazia
            if(categorias is null || !categorias.Any())
            {
                return new List<CategoriaDTO>();
            }

            //Retorno a lista transformada em tipo CategoriaDTO
            return categorias.Select(categorias => new CategoriaDTO
            {
                CategoriaId = categorias.CategoriaId,
                Nome = categorias.Nome,
                ImagemUrl = categorias.ImagemUrl,
            }).ToList();
        }

    }
}
