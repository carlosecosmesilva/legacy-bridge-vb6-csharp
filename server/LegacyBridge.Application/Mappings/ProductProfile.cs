using AutoMapper;
using LegacyBridge.Application.DTOs;
using LegacyBridge.Domain.Entities;

namespace LegacyBridge.Application.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>().ReverseMap();
    }
}
