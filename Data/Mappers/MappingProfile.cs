using AutoMapper;
using Loans.Servicing.Data.Dto;
using Loans.Servicing.Kafka.InternalEvents;

namespace Loans.Servicing.Data.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<LoanApplicationRequest, LoanApplicationSubmittedEvent>();
    }
}