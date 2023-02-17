using System.Collections.Generic;
using VFCurveEditor.Models;

namespace VFCurveEditor.Interfaces;

internal interface IProfileReader
{
    IEnumerable<Profile> Read(string path);

    void Save(string profileName, string curve);
}
