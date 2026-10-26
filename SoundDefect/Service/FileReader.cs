using System.Globalization;
using System.IO;

namespace SoundDeffect.Service
{
    public class FileReader
    {
        //public async Task<Dataset> ParseFileAsync(string filePath)
        //{
        //    if (!File.Exists(filePath))
        //        throw new FileNotFoundException($"Файл не найден: {filePath}");


        //    using var reader = new StreamReader(filePath);

        //    while (await reader.ReadLineAsync() is string line)
        //    {
        //        if (string.IsNullOrWhiteSpace(line)) continue;

        //        var parts = line.Split(new[] { ' ', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);

        //        if (parts.Length == 17)
        //        {
        //            if (double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double x) &&
        //                double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
        //            {
        //                dataset.AddPoint(channelId: 0, x, y);
        //            }
        //        }
        //        else if (parts.Length == 3)
        //        {
        //            if (int.TryParse(parts[0], out int channelId) &&
        //                double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double x) &&
        //                double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
        //            {
        //                dataset.AddPoint(channelId, x, y);
        //            }
        //        }
        //    }

        //    return dataset;
        //}
    }
}
