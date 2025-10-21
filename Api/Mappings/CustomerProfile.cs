using Api.DTOs;
using Api.Models;
using AutoMapper;

namespace Api.Mappings;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Active))
            .ReverseMap()
            .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Status));
    }
}
