using AutoMapper;
using Microsoft.Extensions.Logging;
using ONG.Donation.Application.DTOs;
using ONG.Donation.Application.Interfaces;
using ONG.Donation.Domain.Entities;
using ONG.Donation.Domain.Exceptions;

namespace ONG.Donation.Application.Services;

public class CampaignService : ICampaignService
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CampaignService> _logger;

    public CampaignService(ICampaignRepository campaignRepository, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CampaignService> logger)
    {
        _campaignRepository = campaignRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CampaignResponse>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all campaigns");
        var campaigns = await _campaignRepository.GetAllAsync();
        _logger.LogInformation("Retrieved {Count} campaigns", campaigns.Count());
        return _mapper.Map<IEnumerable<CampaignResponse>>(campaigns);
    }

    public async Task<IEnumerable<TransparencyCampaignResponse>> GetActiveAsync()
    {
        _logger.LogInformation("Fetching active campaigns");
        var campaigns = await _campaignRepository.GetActiveAsync();
        _logger.LogInformation("Retrieved {Count} active campaigns", campaigns.Count());
        return _mapper.Map<IEnumerable<TransparencyCampaignResponse>>(campaigns);
    }

    public async Task<CampaignResponse> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching campaign {CampaignId}", id);
        var campaign = await _campaignRepository.GetByIdAsync(id);
        if (campaign is null)
        {
            _logger.LogWarning("Campaign {CampaignId} not found", id);
            throw new DomainException("Campaign not found.");
        }
        return _mapper.Map<CampaignResponse>(campaign);
    }

    public async Task<CampaignResponse> CreateAsync(CreateCampaignRequest request)
    {
        _logger.LogInformation("Creating campaign: {Title}", request.Title);

        var campaign = new Campaign(
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.FinancialGoal);

        await _campaignRepository.AddAsync(campaign);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Campaign {CampaignId} created successfully: {Title}", campaign.Id, campaign.Title);
        return _mapper.Map<CampaignResponse>(campaign);
    }

    public async Task<CampaignResponse> UpdateAsync(int id, UpdateCampaignRequest request)
    {
        _logger.LogInformation("Updating campaign {CampaignId}", id);

        var campaign = await _campaignRepository.GetByIdAsync(id);
        if (campaign is null)
        {
            _logger.LogWarning("Update failed: campaign {CampaignId} not found", id);
            throw new DomainException("Campaign not found.");
        }

        campaign.Update(
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.FinancialGoal);

        _campaignRepository.Update(campaign);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Campaign {CampaignId} updated successfully", id);
        return _mapper.Map<CampaignResponse>(campaign);
    }

    public async Task<CampaignResponse> SetStatusAsync(int id, SetCampaignStatusRequest request)
    {
        _logger.LogInformation("Setting status of campaign {CampaignId} to {Status}", id, request.Status);

        var campaign = await _campaignRepository.GetByIdAsync(id);
        if (campaign is null)
        {
            _logger.LogWarning("SetStatus failed: campaign {CampaignId} not found", id);
            throw new DomainException("Campaign not found.");
        }

        switch (request.Status.ToLowerInvariant())
        {
            case "ativa":
                campaign.Activate();
                break;
            case "inativa":
                campaign.Inactivate();
                break;
            default:
                throw new DomainException($"Invalid status '{request.Status}'. Valid values: ativa, cancelada.");
        }

        _campaignRepository.Update(campaign);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Campaign {CampaignId} status changed to {Status}", id, campaign.Status);
        return _mapper.Map<CampaignResponse>(campaign);
    }
}
