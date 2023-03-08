using System;
using System.Collections.Generic;
using System.Linq;
using VFCurveEditor.Interfaces;
using VFCurveEditor.Models;

namespace VFCurveEditor.Services.Methods;

internal class SmoothSlopeMethod : IMethod
{
    public string Name => "Smooth slope method";

    public IEnumerable<CurvePoint> Apply(
        IEnumerable<CurvePoint> curvePoints,
        float targetVoltage,
        float targetFrequency,
        float offsetVoltage)
    {
        if (!curvePoints.Any()) return Array.Empty<CurvePoint>();

        CurvePoint[] points = curvePoints.ToArray();

        CurvePoint startSlopePoint = points.First(_ => _.Voltage >= targetVoltage - offsetVoltage);
        CurvePoint endSlopePoint = points.First(_ => _.Voltage >= targetVoltage);

        int startSlopeIndex = Array.IndexOf(points, startSlopePoint);
        int endSlopeIndex = Array.IndexOf(points, endSlopePoint);

        float inclineDelta = (targetFrequency - startSlopePoint.Frequency) / (endSlopeIndex - startSlopeIndex);

        for (int i = startSlopeIndex; i < endSlopeIndex; i++)
        {
            float frequency = points[i].Frequency;
            float offset = MathF.Ceiling(inclineDelta * (i - startSlopeIndex) / Settings.FREQUENCY_STEP) * Settings.FREQUENCY_STEP - (frequency - startSlopePoint.Frequency);
            points[i].Offset = offset;
        }

        // Form target frequency rack for the rest of the points
        for (int i = endSlopeIndex; i < points.Length; i++)
        {
            points[i].Offset = targetFrequency - points[i].Frequency;
        }

        return points;
    }
}
