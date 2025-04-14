using Loans.Servicing.Data.Enums;

namespace Loans.Servicing.Data.Models;

public abstract class Operation
{
    public Guid OperationId  { get; set; }
    public OperationStatus Status { get; set; }
    public DateTime? ExecutionStartTime { get; set; }
    public DateTime? ExecutionEndTime { get; set; }
    public string Result { get; set; }
    public string ErrorMessage { get; set; }

    public abstract OperationType Type { get; }
}