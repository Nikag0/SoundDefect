using SoundDefect.Domain;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Text;

public class GraphicNormalizationService
{
    public void NormalizeAll(IEnumerable<Graphic> graphics)
    {
        double globalMin = double.MaxValue;
        double globalMax = double.MinValue;
        bool hasData = false;

        foreach (var graphic in graphics)
        {
            foreach (double y in graphic.YValuesOriginal)
            {
                if (y < globalMin) globalMin = y;
                if (y > globalMax) globalMax = y;
                hasData = true;
            }
        }

        if (!hasData) return;

        double globalRange = globalMax - globalMin;
        
        foreach (var graphic in graphics)
        {
            double[] source = graphic.YValuesOriginal;
            double[] normalized = new double[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                normalized[i] = globalRange != 0
                    ? (source[i] - globalMin) / globalRange
                    : 0.0;
            }

            graphic.UpdateNormalizedData(normalized);
        }
    }
}
