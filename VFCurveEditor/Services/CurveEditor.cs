using System;
using System.Collections.Generic;
using System.Linq;
using VFCurveEditor.Interfaces;
using VFCurveEditor.Models;

namespace VFCurveEditor.Services;

internal class CurveEditor : ICurveEditor
{
    private const float FREQUENCY_MULTIPLIER = 15;

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
    public IEnumerable<CurvePoint> Generate(IEnumerable<CurvePoint> curvePoints, float targetVoltage, float targetFrequency, float offsetVoltage, int method = 0)
    {
        if (!curvePoints.Any()) return Array.Empty<CurvePoint>();

        CurvePoint[] points = curvePoints.ToArray();

        CurvePoint startSlopePoint = points.First(_ => _.Voltage >= targetVoltage - offsetVoltage);
        CurvePoint endSlopePoint = points.First(_ => _.Voltage >= targetVoltage);

        int startSlopeIndex = Array.IndexOf(points, startSlopePoint);
        int endSlopeIndex = Array.IndexOf(points, endSlopePoint);

        float inclineDelta = (targetFrequency - startSlopePoint.Frequency) / (endSlopeIndex - startSlopeIndex);

        switch (method)
        {
            case 0:
                // Slope calculation to a target point
                for (int i = startSlopeIndex; i < endSlopeIndex; i++)
                {
                    float frequency = points[i].Frequency;
                    float offset = MathF.Ceiling(inclineDelta * (i - startSlopeIndex) / FREQUENCY_MULTIPLIER) * FREQUENCY_MULTIPLIER - (frequency - startSlopePoint.Frequency);
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
                break;
            // Offset based method
            case 1:
                // Slope calculation to a target point
                for (int i = startSlopeIndex; i < endSlopeIndex; i++)
                {
                    float frequency = points[i].Frequency;
                    float offset = MathF.Ceiling(inclineDelta * (i - startSlopeIndex) / FREQUENCY_MULTIPLIER) * FREQUENCY_MULTIPLIER - (frequency - startSlopePoint.Frequency);
                    points[i].Offset = offset;
                }

                // Form target frequency rack for the rest of the points
                for (int i = endSlopeIndex; i < points.Length; i++)
                {
                    points[i].Offset = targetFrequency - points[i].Frequency;
                }
                break;
            default: break;
        }
        return points;
    }
}
