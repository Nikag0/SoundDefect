namespace SoundDefect.Domain.Signals
{
    /// <summary>
    /// Уникальный идентификатор сигнала в рамках работы приложения.
    /// </summary>
    public readonly record struct SignalId(Guid Value)
    {
        public static SignalId New() => new(Guid.NewGuid());

        public override string ToString() => Value.ToString();
    }
}
