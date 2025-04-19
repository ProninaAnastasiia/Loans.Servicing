namespace Loans.Servicing.Saga.Commands;

public interface  ConfirmDraftContractCommand
{
    Guid ContractId { get; }
}