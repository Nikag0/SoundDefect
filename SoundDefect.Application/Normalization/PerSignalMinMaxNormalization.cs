using SoundDefect.Domain.Signals;

namespace SoundDefect.Application.Normalization
{
    /// <summary>
    /// Приводит каждый сигнал к диапазону [0; 1] по его собственному минимуму и максимуму.
    /// Форма сигналов становится сравнимой, но соотношение амплитуд между ними теряется.
    /// </summary>
    public class PerSignalMinMaxNormalization : INormalizationMethod
    {
        public string Name => "Min-Max (по каждому сигналу)";

        public string Description => "Каждый сигнал приводится к диапазону [0; 1] по своему минимуму и максимуму. Удобно сравнивать форму сигналов с разными единицами и масштабами, но соотношение амплитуд между сигналами теряется.";

        public IReadOnlyDictionary<SignalId, double[]> Normalize(IReadOnlyCollection<Signal> signals)
        {
            var result = new Dictionary<SignalId, double[]>(signals.Count);

            foreach (var signal in signals)
            {
                ReadOnlySpan<double> source = signal.Ys.Span;

                double min = double.MaxValue;
                double max = double.MinValue;

                foreach (double y in source)
                {
                    if (y < min) min = y;
                    if (y > max) max = y;
                }

                double range = max - min;
                double[] normalized = new double[source.Length];

                for (int i = 0; i < source.Length; i++)
                {
                    normalized[i] = range != 0
                        ? (source[i] - min) / range
                        : 0.0;
                }

                result[signal.Id] = normalized;
            }

            return result;
        }
    }
}
