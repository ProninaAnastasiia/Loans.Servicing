using AutoMapper;
using Loans.Servicing.Data.Dto;
using Loans.Servicing.Kafka.Events.CalculateContractValues;
using Loans.Servicing.Kafka.Events.CreateDraftContract;
using Loans.Servicing.Kafka.Events.GetContractApproved;

namespace Loans.Servicing.Data.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<LoanApplicationRequest, CreateContractRequestedEvent>()
            .ForCtorParam("OperationId", opt => opt.MapFrom(ResolveOperationId));
        CreateMap<LoanApplicationRequest, CreateContractRequestedEvent>()
            .ForCtorParam("OperationId", opt => opt.MapFrom(ResolveOperationId));

        CreateMap<DraftContractCreatedEvent, CalculateContractValuesEvent>()
            .ForCtorParam("ContractId", opt => opt.MapFrom(src => src.ContractId))
            .ForCtorParam("LoanAmount", opt => opt.MapFrom(src => src.LoanAmount))
            .ForCtorParam("LoanTermMonths", opt => opt.MapFrom(src => src.LoanTermMonths))
            .ForCtorParam("InterestRate", opt => opt.MapFrom(src => src.InterestRate))
            .ForCtorParam("PaymentType", opt => opt.MapFrom(src => src.PaymentType))
            .ForCtorParam("OperationId", opt => opt.MapFrom(src => src.OperationId));
        
        CreateMap<ContractDetailsResponseEvent, ContractSentToClientEvent>();
        
    }

    private Guid ResolveOperationId(LoanApplicationRequest src, ResolutionContext context)
    {
        return (Guid)context.Items["OperationId"];
    }
}