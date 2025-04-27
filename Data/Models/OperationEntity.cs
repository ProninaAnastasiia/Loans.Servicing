using Loans.Servicing.Data.Enums;

namespace Loans.Servicing.Data.Models;

public class OperationEntity
{
    public Guid OperationId { get; set; }
    
    public string Description { get; set; } = null!;
    
    public OperationStatus Status { get; set; }
    
    public string ContextJson { get; set; } = null!;
    
    public DateTime StartedAt { get; set; }
    
    public DateTime? FinishedAt { get; set; }
}