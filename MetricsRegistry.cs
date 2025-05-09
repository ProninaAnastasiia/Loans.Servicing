using System.Collections.Concurrent;
using Prometheus;

namespace Loans.Servicing;

public static class MetricsRegistry
{
    public static readonly Histogram LoanProcessingDuration = Metrics
        .CreateHistogram("loan_processing_duration_seconds", "Время обработки заявки (от API до финального события)", 
            new HistogramConfiguration
            {
                Buckets = new double[] { 20, 25, 30, 35, 40, 45, 50, 60, 75, 90, 105, 120, 150, 180, 200, 220, 300, 400, 500}
            });

    private static readonly ConcurrentDictionary<Guid, IDisposable> Timers = new();

    public static void StartTimer(Guid operationId)
    {
        var timer = LoanProcessingDuration.NewTimer();
        Timers.TryAdd(operationId, timer);
    }

    public static void StopTimer(Guid operationId)
    {
        if (Timers.TryRemove(operationId, out var timer))
        {
            timer.Dispose();
        }
    }
    
    public static readonly Histogram ScheduleCalculationLatency = Metrics
        .CreateHistogram("schedule_calculation_latency_seconds", "Latency: Loans.Servicing → Loans.Schedules → Loans.Servicing",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(start: 0.1, factor: 2, count: 15) // от 100 мс до ~160 сек
            });

    private static readonly ConcurrentDictionary<Guid, IDisposable> ScheduleLatencyTimers = new();

    public static void StartScheduleLatencyTimer(Guid correlationId)
    {
        var timer = ScheduleCalculationLatency.NewTimer();
        ScheduleLatencyTimers.TryAdd(correlationId, timer);
    }

    public static void StopScheduleLatencyTimer(Guid correlationId)
    {
        if (ScheduleLatencyTimers.TryRemove(correlationId, out var timer))
        {
            timer.Dispose();
        }
    }
}
