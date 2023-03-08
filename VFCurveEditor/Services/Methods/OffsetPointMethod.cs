using System.Collections.Generic;
using System.Linq;
using System;
using VFCurveEditor.Interfaces;
using VFCurveEditor.Models;

namespace VFCurveEditor.Services.Methods;

internal class OffsetPointMethod : IMethod
{
    public string Name => "Offset point method";

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

        // Slope calculation to a target point
        for (int i = startSlopeIndex; i < endSlopeIndex; i++)
        {
            float frequency = points[i].Frequency;
            float offset = MathF.Ceiling(inclineDelta * (i - startSlopeIndex) / Settings.FREQUENCY_STEP) * Settings.FREQUENCY_STEP - (frequency - startSlopePoint.Frequency);
            points[i].Frequency += offset;
        }

        // Offset for a target point
        points[endSlopeIndex].Offset = targetFrequency - points[endSlopeIndex].Frequency;

        // Form target frequency rack for the rest of the points
        for (int i = endSlopeIndex + 1; i < points.Length; i++)
        {
            if (points[i].Frequency < targetFrequency)
            {
                points[i].Frequency = targetFrequency;
            }
            else
            {
                points[i].Offset = targetFrequency - points[i].Frequency;
            }
        }

        return points;
    }
}
