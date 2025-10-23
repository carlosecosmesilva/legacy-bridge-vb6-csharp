using AutoMapper;
using LegacyBridge.Application.DTOs;
using LegacyBridge.Domain.Entities;

namespace LegacyBridge.Application.Mappings;

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
