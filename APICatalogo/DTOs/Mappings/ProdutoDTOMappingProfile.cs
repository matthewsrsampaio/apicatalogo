using ApiCatalogo.Models;
using AutoMapper;

namespace APICatalogo.DTOs.Mappings
{
    public class ProdutoDTOMappingProfile : Profile
    {
        //Construtor
        public ProdutoDTOMappingProfile() 
        {
            //Realiza as transformações de ida e volta entre entidade e DTO
                     //Source - Destination
            CreateMap<Produto, ProdutoDTO>().ReverseMap();
            CreateMap<Categoria, CategoriaDTO>().ReverseMap();
            CreateMap<Produto, ProdutoDTOUpdateRequest>().ReverseMap();
            CreateMap<Produto, ProdutoDTOUpdateResponse>().ReverseMap();
        }
    }
}
