using SoundDefect.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoundDefect.Service
{
    public interface IInternalGraphicParser
    {
        Task<IEnumerable<Graphic>> ParseAsync(string filePath);
    }
}
