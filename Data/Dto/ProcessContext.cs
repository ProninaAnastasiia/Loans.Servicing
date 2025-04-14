namespace Loans.Servicing.Data.Dto;

public class ProcessContext
{
    public Guid ProcessId { get; set; }

    // Гибкий способ хранить промежуточные данные между шагами
    public Dictionary<string, object> Data { get; set; } = new();

    public T Get<T>(string key) => Data.TryGetValue(key, out var value)
        ? (T)value
        : throw new KeyNotFoundException($"Key '{key}' not found in context.");

    public void Set<T>(string key, T value) => Data[key] = value;
}