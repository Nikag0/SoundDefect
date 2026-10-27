using SoundDefect.Domain.Signals;

namespace SoundDefect.Application.Normalization
{
    /// <summary>
    /// Способ нормализации сигналов. Получает весь набор сигналов сразу,
    /// так как некоторым способам (например, глобальному min-max) нужны общие характеристики набора.
    /// </summary>
    public interface INormalizationMethod
    {
        string Name { get; }
        string Description { get; }

        /// <returns>Нормализованные значения Y для каждого сигнала набора.</returns>
        IReadOnlyDictionary<SignalId, double[]> Normalize(IReadOnlyCollection<Signal> signals);
    }
}
