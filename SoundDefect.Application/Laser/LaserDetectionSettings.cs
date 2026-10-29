namespace SoundDefect.Application.Laser
{
    /// <summary>
    /// Настройки распознавания работы лазера.
    /// </summary>
    /// <param name="Threshold">Значение сигнала, начиная с которого лазер считается включённым.</param>
    /// <param name="MinDuration">
    /// Минимальная длительность (в единицах X) включения или выключения.
    /// Более короткие участки считаются скачками напряжения и отбрасываются.
    /// </param>
    public sealed record LaserDetectionSettings(double Threshold, double MinDuration)
    {
        public const double DefaultMinDuration = 2;
    }
}
