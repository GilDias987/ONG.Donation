using AutoMapper;
using Microsoft.Extensions.Logging;
using ONG.Donation.Application.DTOs;
using ONG.Donation.Application.Interfaces;
using ONG.Donation.Domain.Enums;
using ONG.Donation.Domain.Events;
using ONG.Donation.Domain.Exceptions;
using DomainDonation = ONG.Donation.Domain.Entities.Donation;

namespace ONG.Donation.Application.Services;

public class DonationService : IDonationService
{
    private readonly IDonationRepository _donationRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IDonorRepository _donorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<DonationService> _logger;

    public DonationService(
        IDonationRepository donationRepository,
        ICampaignRepository campaignRepository,
        IDonorRepository donorRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEventPublisher eventPublisher,
        ILogger<DonationService> logger)
    {
        _donationRepository = donationRepository;
        _campaignRepository = campaignRepository;
        _donorRepository = donorRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<DonationResponse> CreateAsync(int donorId, int userId, CreateDonationRequest request)
    {
        _logger.LogInformation("Creating donation for campaign {CampaignId} by donor {DonorId}, amount {Amount}",
            request.CampaignId, donorId, request.Amount);

        var campaign = await _campaignRepository.GetByIdAsync(request.CampaignId);
        if (campaign is null)
        {
            _logger.LogWarning("Donation creation failed: campaign {CampaignId} not found", request.CampaignId);
            throw new DomainException("Campaign not found.");
        }

        if (campaign.Status != CampaignStatus.Ativa)
        {
            _logger.LogWarning("Donation creation failed: campaign {CampaignId} is not active (status: {Status})",
                request.CampaignId, campaign.Status);
            throw new DomainException("Donations are only allowed for active campaigns.");
        }

        if (!campaign.IsWithinPeriod())
        {
            _logger.LogWarning("Donation creation failed: campaign {CampaignId} is outside its valid period ({Start} to {End})",
                request.CampaignId, campaign.StartDate, campaign.EndDate);
            throw new DomainException("Donations are only allowed during the campaign period.");
        }

        var donation = new DomainDonation(request.CampaignId, donorId, request.Amount);
        await _donationRepository.AddAsync(donation);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Donation {DonationId} created for campaign {CampaignId}, publishing event",
            donation.Id, request.CampaignId);

        await _eventPublisher.PublishAsync(new DonationCreatedEvent(
            donation.Id,
            campaign.Id,
            donorId,
            userId,
            request.Amount,
            DateTime.UtcNow));

        _logger.LogInformation("Donation {DonationId} created and event published successfully", donation.Id);
        return _mapper.Map<DonationResponse>(donation);
    }

    public async Task<IEnumerable<DonationResponse>> GetByCampaignIdAsync(int campaignId)
    {
        _logger.LogInformation("Fetching donations for campaign {CampaignId}", campaignId);
        var donations = await _donationRepository.GetByCampaignIdAsync(campaignId);
        _logger.LogInformation("Retrieved {Count} donations for campaign {CampaignId}", donations.Count(), campaignId);
        return _mapper.Map<IEnumerable<DonationResponse>>(donations);
    }
}
