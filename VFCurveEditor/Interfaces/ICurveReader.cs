using System.Collections.Generic;
using VFCurveEditor.Models;

namespace VFCurveEditor.Interfaces;

internal interface ICurveReader
{
    IEnumerable<CurvePoint> Read(string curveString);

    string Write(IEnumerable<CurvePoint> points);
}
