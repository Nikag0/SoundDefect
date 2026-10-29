namespace SoundDefect.Domain.Laser
{
    /// <summary>
    /// Интервал по оси X, в течение которого лазер был включён. Границы включаются в интервал.
    /// </summary>
    public readonly record struct LaserInterval
    {
        public double Start { get; }
        public double End { get; }

        public double Duration => End - Start;

        public LaserInterval(double start, double end)
        {
            if (end < start)
                throw new ArgumentException($"Конец интервала ({end}) не может быть раньше начала ({start}).", nameof(end));

            Start = start;
            End = end;
        }

        public bool Contains(double x) => x >= Start && x <= End;
    }
}
