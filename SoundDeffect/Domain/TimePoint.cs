using System;
using System.Collections.Generic;
using System.Text;

namespace SoundDeffect.Domain
{
    public readonly record struct TimePoint(
        double X,
        double Y,
        int ChannelId = 0
    );
}
