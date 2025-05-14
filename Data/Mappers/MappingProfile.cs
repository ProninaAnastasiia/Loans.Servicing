using AutoMapper;
using Loans.Servicing.Data.Dto;
using Loans.Servicing.Kafka.Events.CalculateAllContractValues;
using Loans.Servicing.Kafka.Events.CalculateContractValues;
using Loans.Servicing.Kafka.Events.CalculateFullLoanValue;
using Loans.Servicing.Kafka.Events.CalculateRepaymentSchedule;
using Loans.Servicing.Kafka.Events.CreateDraftContract;
using Loans.Servicing.Kafka.Events.GetContractApproved;
using Loans.Servicing.Kafka.Events.InnerEvents;

namespace Loans.Servicing.Data.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<LoanApplicationRequest, CreateContractRequestedEvent>()
            .ForCtorParam("OperationId", opt => opt.MapFrom(ResolveOperationId));
        CreateMap<LoanApplicationRequest, CreateContractRequestedEvent>()
            .ForCtorParam("OperationId", opt => opt.MapFrom(ResolveOperationId));
        CreateMap<LoanApplicationRequest, LoanApplicationRecieved>()
            .ForCtorParam("OperationId", opt => opt.MapFrom(ResolveOperationId));
        CreateMap<CalculateScheduleRequest, CalculateScheduleRequested>()
            .ForCtorParam("OperationId", opt => opt.MapFrom(ResolveScheduleOperationId));
        CreateMap<CalculateScheduleRequest, CalculateRepaymentScheduleEvent>()
            .ForCtorParam("OperationId", opt => opt.MapFrom(ResolveScheduleOperationId));
        CreateMap<CalculateFullLoanValueRequest, CalculateFullLoanValueRequested>()
            .ForCtorParam("OperationId", opt => opt.MapFrom(ResolveFullLoanValueOperationId));
        CreateMap<CalculateFullLoanValueRequest, CalculateFullLoanValueEvent>()
            .ForCtorParam("OperationId", opt => opt.MapFrom(ResolveFullLoanValueOperationId));
        CreateMap<CalculateAllContractValuesRequest, CalculateAllValuesRequested>()
            .ForCtorParam("OperationId", opt => opt.MapFrom(ResolveAllValuesOperationId));
        CreateMap<CalculateAllContractValuesRequest, CalculateContractValuesEvent>()
            .ForCtorParam("OperationId", opt => opt.MapFrom(ResolveAllValuesOperationId));

        CreateMap<CalculateScheduleRequested, CalculateScheduleRequest>(); 
        CreateMap<CalculateAllValuesRequested, CalculateAllContractValuesRequest>(); 
        CreateMap<CalculateFullLoanValueRequested, CalculateFullLoanValueRequest>(); 
        
        CreateMap<LoanApplicationRecieved, LoanApplicationRequest>()
            .ForMember(dest => dest.ApplicationId, opt => opt.MapFrom(src => src.ApplicationId.ToString()))
            .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.ClientId.ToString()))
            .ForMember(dest => dest.DecisionId, opt => opt.MapFrom(src => src.DecisionId.ToString()))
            .ForMember(dest => dest.CreditProductId, opt => opt.MapFrom(src => src.CreditProductId.ToString()));

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
    
    private Guid ResolveScheduleOperationId(CalculateScheduleRequest src, ResolutionContext context)
    {
        return (Guid)context.Items["OperationId"];
    }
    
    private Guid ResolveFullLoanValueOperationId(CalculateFullLoanValueRequest src, ResolutionContext context)
    {
        return (Guid)context.Items["OperationId"];
    }
    
    private Guid ResolveAllValuesOperationId(CalculateAllContractValuesRequest src, ResolutionContext context)
    {
        return (Guid)context.Items["OperationId"];
    }
}