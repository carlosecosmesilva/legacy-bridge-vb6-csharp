using Api.DTOs;
using Api.Models;
using AutoMapper;

namespace Api.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>().ReverseMap();
    }
}
