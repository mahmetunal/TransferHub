using Serilog.Core;
using Serilog.Events;
using Shared.Common.Logging;

namespace Shared.Infrastructure.Logging;

public sealed class HashedLoggingEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var property in logEvent.Properties.ToList())
        {
            var hashedValue = HashIfNeeded(property.Value);
            if (hashedValue != null)
            {
                logEvent.AddOrUpdateProperty(
                    new LogEventProperty(property.Key, hashedValue));
            }
        }
    }

    private static LogEventPropertyValue? HashIfNeeded(LogEventPropertyValue value)
    {
        return value switch
        {
            ScalarValue scalar => HashScalar(scalar),
            StructureValue structure => HashStructure(structure),
            SequenceValue sequence => HashSequence(sequence),
            _ => null
        };
    }

    private static LogEventPropertyValue? HashScalar(ScalarValue scalar)
    {
        if (scalar.Value is string s && IsSensitive(scalar))
        {
            return new ScalarValue(Hash(s));
        }

        return null;
    }

    private static LogEventPropertyValue HashStructure(StructureValue structure)
    {
        var props = new List<LogEventProperty>();

        foreach (var prop in structure.Properties)
        {
            var hashed = HashIfNeeded(prop.Value);
            props.Add(new LogEventProperty(
                prop.Name,
                hashed ?? prop.Value));
        }

        return new StructureValue(props, structure.TypeTag);
    }

    private static LogEventPropertyValue HashSequence(SequenceValue sequence)
    {
        var values = sequence.Elements
            .Select(HashIfNeeded)
            .Select(v => v ?? v)
            .ToList();

        return new SequenceValue(values!);
    }

    private static bool IsSensitive(ScalarValue scalar)
    {
        return scalar.Value is string { Length: > 6 };
    }

    private static string Hash(string input)
    {
        return input.MaskPii();
    }
}