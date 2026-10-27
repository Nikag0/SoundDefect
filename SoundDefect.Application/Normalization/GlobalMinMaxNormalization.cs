using SoundDefect.Domain.Signals;

namespace SoundDefect.Application.Normalization
{
    /// <summary>
    /// Приводит все сигналы к диапазону [0; 1] по общему минимуму и максимуму набора.
    /// Соотношение амплитуд между сигналами сохраняется.
    /// </summary>
    public class GlobalMinMaxNormalization : INormalizationMethod
    {
        public string Name => "Min-Max (общий)";

        public string Description => "Все сигналы приводятся к диапазону [0; 1] по общему минимуму и максимуму. Соотношение амплитуд между сигналами сохраняется.";

        public IReadOnlyDictionary<SignalId, double[]> Normalize(IReadOnlyCollection<Signal> signals)
        {
            double globalMin = double.MaxValue;
            double globalMax = double.MinValue;

            foreach (var signal in signals)
            {
                foreach (double y in signal.Ys.Span)
                {
                    if (y < globalMin) globalMin = y;
                    if (y > globalMax) globalMax = y;
                }
            }

            double globalRange = globalMax - globalMin;
            var result = new Dictionary<SignalId, double[]>(signals.Count);

            foreach (var signal in signals)
            {
                ReadOnlySpan<double> source = signal.Ys.Span;
                double[] normalized = new double[source.Length];

                for (int i = 0; i < source.Length; i++)
                {
                    normalized[i] = globalRange != 0
                        ? (source[i] - globalMin) / globalRange
                        : 0.0;
                }

                result[signal.Id] = normalized;
            }

            return result;
        }
    }
}
