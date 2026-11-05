namespace SoundDefect.Domain.Laser
{
    /// <summary>
    /// Работа лазера за время записи: интервалы, когда он был включён.
    /// Внутри [RecordStart; RecordEnd] вне интервалов лазер выключен, за пределами записи состояние неизвестно.
    /// </summary>
    public sealed class LaserActivity
    {
        public double RecordStart { get; }
        public double RecordEnd { get; }

        /// <summary>Интервалы включения, упорядоченные по времени и не пересекающиеся.</summary>
        public IReadOnlyList<LaserInterval> OnIntervals { get; }

        /// <summary>Сколько кратких скачков было отброшено при распознавании.</summary>
        public int DiscardedSpikes { get; }

        public LaserActivity(double recordStart, double recordEnd, IReadOnlyList<LaserInterval> onIntervals, int discardedSpikes)
        {
            if (recordEnd < recordStart)
                throw new ArgumentException("Конец записи не может быть раньше начала.", nameof(recordEnd));
            ArgumentNullException.ThrowIfNull(onIntervals);
            if (discardedSpikes < 0)
                throw new ArgumentOutOfRangeException(nameof(discardedSpikes));

            for (int i = 0; i < onIntervals.Count; i++)
            {
                var interval = onIntervals[i];
                if (interval.Start < recordStart || interval.End > recordEnd)
                    throw new ArgumentException("Интервал включения выходит за пределы записи.", nameof(onIntervals));
                if (i > 0 && interval.Start <= onIntervals[i - 1].End)
                    throw new ArgumentException("Интервалы включения должны идти по порядку и не пересекаться.", nameof(onIntervals));
            }

            RecordStart = recordStart;
            RecordEnd = recordEnd;
            OnIntervals = onIntervals.ToArray();
            DiscardedSpikes = discardedSpikes;
        }

        public LaserState StateAt(double x)
        {
            if (x < RecordStart || x > RecordEnd)
                return LaserState.Unknown;

            foreach (var interval in OnIntervals)
            {
                if (interval.Contains(x)) return LaserState.On;
                if (interval.Start > x) break;
            }

            return LaserState.Off;
        }

        /// <summary>
        /// Диапазоны индексов точек, в которых лазер находился в заданном состоянии.
        /// Точки должны идти по возрастанию X.
        /// </summary>
        /// <returns>Пары (индекс первой точки, количество точек) в порядке следования.</returns>
        public IReadOnlyList<(int Start, int Count)> FindRanges(IReadOnlyList<double> xs, LaserState state)
        {
            var ranges = new List<(int Start, int Count)>();
            int rangeStart = -1;

            for (int i = 0; i < xs.Count; i++)
            {
                bool matches = StateAt(xs[i]) == state;

                if (matches && rangeStart < 0)
                {
                    rangeStart = i;
                }
                else if (!matches && rangeStart >= 0)
                {
                    ranges.Add((rangeStart, i - rangeStart));
                    rangeStart = -1;
                }
            }

            if (rangeStart >= 0)
                ranges.Add((rangeStart, xs.Count - rangeStart));

            return ranges;
        }
    }
}
