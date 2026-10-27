namespace SoundDefect.Domain.Signals
{
    /// <summary>
    /// Сигнал, загруженный из файла: набор точек (X, Y).
    /// Неизменяем после создания: всё производное (нормализация, фильтры, анализ)
    /// вычисляется отдельно и не хранится в сущности.
    /// </summary>
    public sealed class Signal
    {
        private readonly double[] _xs;
        private readonly double[] _ys;

        public SignalId Id { get; }
        public string Name { get; }

        public ReadOnlyMemory<double> Xs => _xs;
        public ReadOnlyMemory<double> Ys => _ys;

        public int Length => _xs.Length;

        private Signal(SignalId id, string name, double[] xs, double[] ys)
        {
            Id = id;
            Name = name;
            _xs = xs;
            _ys = ys;
        }

        public static Signal Create(string name, IReadOnlyList<double> xs, IReadOnlyList<double> ys)
            => Create(SignalId.New(), name, xs, ys);

        public static Signal Create(SignalId id, string name, IReadOnlyList<double> xs, IReadOnlyList<double> ys)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя сигнала не может быть пустым или состоять из пробелов.", nameof(name));

            ArgumentNullException.ThrowIfNull(xs);
            ArgumentNullException.ThrowIfNull(ys);

            if (xs.Count == 0)
                throw new ArgumentException("Сигнал должен содержать хотя бы одну точку.", nameof(xs));

            if (xs.Count != ys.Count)
                throw new ArgumentException("Количество координат X должно совпадать с количеством координат Y.", nameof(ys));

            double[] xsCopy = xs.ToArray();
            double[] ysCopy = ys.ToArray();

            EnsureFinite(xsCopy, nameof(xs));
            EnsureFinite(ysCopy, nameof(ys));
            EnsureAscending(xsCopy, nameof(xs));

            return new Signal(id, name, xsCopy, ysCopy);
        }

        private static void EnsureAscending(double[] xs, string paramName)
        {
            for (int i = 1; i < xs.Length; i++)
            {
                if (xs[i] < xs[i - 1])
                    throw new ArgumentException($"Координаты X должны возрастать: точка {i} ({xs[i]}) меньше предыдущей ({xs[i - 1]}).", paramName);
            }
        }

        private static void EnsureFinite(double[] values, string paramName)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (!double.IsFinite(values[i]))
                    throw new ArgumentException($"Точка {i} содержит недопустимое значение: {values[i]}.", paramName);
            }
        }
    }
}
