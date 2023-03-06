using System.Collections.Generic;
using VFCurveEditor.Models;

namespace VFCurveEditor.Interfaces;

internal interface ICurveEditor
{
    /// <summary>
    /// Generates a new curve by given parameters.
    /// </summary>
    /// <param name="targetVoltage">Target voltage.</param>
    /// <param name="targetFrequency">Target frequency.</param>
    /// <param name="offsetVoltage">Voltage offset.</param>
    /// <example>
    /// targetVoltage = 900;
    /// targetFrequency = 1935;
    /// offsetVoltage = 50;
    /// </example>
    IEnumerable<CurvePoint> Generate(IEnumerable<CurvePoint> points, float targetVoltage, float targetFrequency, float offsetVoltage, int method = 0);
}
