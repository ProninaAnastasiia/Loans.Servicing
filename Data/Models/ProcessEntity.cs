using Loans.Servicing.Data.Enums;

namespace Loans.Servicing.Data.Models;

public class ProcessEntity
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public int CurrentStepIndex { get; set; }
    public ProcessStatus Status { get; set; }
    public string ContextJson { get; set; } = null!;
}