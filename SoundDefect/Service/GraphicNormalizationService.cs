using SoundDefect.Domain.Signals;

namespace SoundDefect.Service
{
    public class GraphicNormalizationService
    {
        public IReadOnlyDictionary<SignalId, double[]> NormalizeAll(IReadOnlyCollection<Signal> signals)
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
