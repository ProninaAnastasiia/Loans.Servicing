using AutoMapper;
using Loans.Servicing.Data.Dto;
using Loans.Servicing.Kafka.Events;

namespace Loans.Servicing.Data.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<LoanApplicationRequest, CreateContractRequestedEvent>()
            .ForCtorParam("OperationId", opt => opt.MapFrom(ResolveOperationId));
        CreateMap<LoanApplicationRequest, CreateContractRequestedEvent>()
            .ForCtorParam("OperationId", opt => opt.MapFrom(ResolveOperationId));
        
    }

    private Guid ResolveOperationId(LoanApplicationRequest src, ResolutionContext context)
    {
        return (Guid)context.Items["OperationId"];
    }
}