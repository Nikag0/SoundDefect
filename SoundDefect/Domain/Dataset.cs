using System;
using System.Collections.Generic;
using System.Text;

namespace SoundDeffect.Domain
{
    public class Dataset
    {
        public Dictionary<int, (List<double> XValues, List<double> YValues)> Channels { get; }
            = new Dictionary<int, (List<double>, List<double>)>();

        public void AddPoint(int channelId, double x, double y)
        {
            if (!Channels.ContainsKey(channelId))
            {
                Channels[channelId] = (new List<double>(), new List<double>());
            }

            Channels[channelId].XValues.Add(x);
            Channels[channelId].YValues.Add(y);
        }
    }
}
