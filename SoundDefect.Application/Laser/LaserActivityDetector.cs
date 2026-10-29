using SoundDefect.Domain.Laser;
using SoundDefect.Domain.Signals;

namespace SoundDefect.Application.Laser
{
    /// <summary>
    /// Определяет по сигналу лазера, когда он был включён.
    /// 1. Каждая точка выше порога — «включён», иначе — «выключен».
    /// 2. Подряд идущие точки с одним состоянием объединяются в участки.
    /// 3. Участки короче минимальной длительности считаются скачками: их состояние меняется на противоположное,
    ///    и они сливаются с соседями. Начинаем с самого короткого, пока такие участки остаются.
    /// </summary>
    public class LaserActivityDetector
    {
        /// <summary>
        /// Порог по умолчанию — середина между минимальным и максимальным значением сигнала лазера.
        /// </summary>
        public double SuggestThreshold(Signal laser)
        {
            ReadOnlySpan<double> ys = laser.Ys.Span;

            double min = double.MaxValue;
            double max = double.MinValue;
            foreach (double y in ys)
            {
                if (y < min) min = y;
                if (y > max) max = y;
            }

            return (min + max) / 2;
        }

        public LaserActivity Detect(Signal laser, LaserDetectionSettings settings)
        {
            if (settings.MinDuration < 0)
                throw new ArgumentOutOfRangeException(nameof(settings), "Минимальная длительность не может быть отрицательной.");

            ReadOnlySpan<double> xs = laser.Xs.Span;
            ReadOnlySpan<double> ys = laser.Ys.Span;

            var runs = BuildRuns(ys, settings.Threshold);
            int discarded = DiscardShortRuns(runs, xs, settings.MinDuration);

            var onIntervals = new List<LaserInterval>();
            foreach (var run in runs)
            {
                if (run.IsOn)
                    onIntervals.Add(new LaserInterval(xs[run.Start], xs[run.End - 1]));
            }

            return new LaserActivity(xs[0], xs[^1], onIntervals, discarded);
        }

        /// <summary>
        /// Участок подряд идущих точек с одинаковым состоянием: индексы [Start; End).
        /// </summary>
        private sealed class Run
        {
            public int Start;
            public int End;
            public bool IsOn;
        }

        private static List<Run> BuildRuns(ReadOnlySpan<double> ys, double threshold)
        {
            var runs = new List<Run>();

            for (int i = 0; i < ys.Length; i++)
            {
                bool isOn = ys[i] >= threshold;

                if (runs.Count > 0 && runs[^1].IsOn == isOn)
                    runs[^1].End = i + 1;
                else
                    runs.Add(new Run { Start = i, End = i + 1, IsOn = isOn });
            }

            return runs;
        }

        /// <returns>Сколько участков было отброшено как скачки.</returns>
        private static int DiscardShortRuns(List<Run> runs, ReadOnlySpan<double> xs, double minDuration)
        {
            int discarded = 0;

            while (runs.Count > 1)
            {
                int shortest = -1;
                double shortestDuration = double.MaxValue;

                for (int i = 0; i < runs.Count; i++)
                {
                    double duration = Duration(runs, i, xs);
                    if (duration < minDuration && duration < shortestDuration)
                    {
                        shortest = i;
                        shortestDuration = duration;
                    }
                }

                if (shortest < 0) break;

                runs[shortest].IsOn = !runs[shortest].IsOn;
                discarded++;
                MergeNeighbours(runs, shortest);
            }

            return discarded;
        }

        /// <summary>
        /// Длительность участка: от его первой точки до первой точки следующего (у последнего — до его последней точки).
        /// </summary>
        private static double Duration(List<Run> runs, int index, ReadOnlySpan<double> xs)
        {
            var run = runs[index];
            double end = index + 1 < runs.Count ? xs[runs[index + 1].Start] : xs[run.End - 1];
            return end - xs[run.Start];
        }

        private static void MergeNeighbours(List<Run> runs, int index)
        {
            if (index + 1 < runs.Count && runs[index + 1].IsOn == runs[index].IsOn)
            {
                runs[index].End = runs[index + 1].End;
                runs.RemoveAt(index + 1);
            }

            if (index > 0 && runs[index - 1].IsOn == runs[index].IsOn)
            {
                runs[index - 1].End = runs[index].End;
                runs.RemoveAt(index);
            }
        }
    }
}
