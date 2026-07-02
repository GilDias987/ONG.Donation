using AutoMapper;
using ONG.Donation.Application.DTOs;
using DomainCampaign = ONG.Donation.Domain.Entities.Campaign;
using DomainDonation = ONG.Donation.Domain.Entities.Donation;

namespace ONG.Donation.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DomainCampaign, CampaignResponse>()
            .ForMember(dest => dest.TotalRaised, opt => opt.MapFrom(src => src.GetTotalRaised()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<DomainCampaign, TransparencyCampaignResponse>()
            .ForMember(dest => dest.TotalRaised, opt => opt.MapFrom(src => src.GetTotalRaised()));

        CreateMap<DomainDonation, DonationResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
