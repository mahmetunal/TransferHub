using Prometheus;

namespace MoneyTransfer.Application.Metrics;

public static class TransferMetrics
{
    private static readonly Counter TransferCreatedCounter = Prometheus.Metrics
        .CreateCounter(
            "transfers_created_total",
            "Total number of transfers created",
            new CounterConfiguration
            {
                LabelNames = ["status", "currency"]
            });

    private static readonly Histogram TransferDuration = Prometheus.Metrics
        .CreateHistogram(
            "transfer_duration_seconds",
            "Duration of transfer processing in seconds",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.01, 2, 10)
            });

    private static readonly Gauge ActiveSagas = Prometheus.Metrics
        .CreateGauge(
            "active_sagas",
            "Number of active transfer sagas");

    private static readonly Counter TransferFailures = Prometheus.Metrics
        .CreateCounter(
            "transfers_failed_total",
            "Total number of failed transfers",
            new CounterConfiguration
            {
                LabelNames = new[] { "reason" }
            });

    public static void RecordTransferCreated(string status, string currency)
    {
        TransferCreatedCounter.WithLabels(status, currency).Inc();
    }

    public static IDisposable MeasureTransferDuration()
    {
        return TransferDuration.NewTimer();
    }

    public static void SetActiveSagas(int count)
    {
        ActiveSagas.Set(count);
    }

    public static void RecordTransferFailure(string reason)
    {
        TransferFailures.WithLabels(reason).Inc();
    }
}