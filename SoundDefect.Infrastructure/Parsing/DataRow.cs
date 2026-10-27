namespace SoundDefect.Infrastructure.Parsing
{
    /// <summary>
    /// Строка файла с данными, уже разобранная на числа.
    /// </summary>
    public readonly record struct DataRow(int LineNumber, double[] Values);
}
