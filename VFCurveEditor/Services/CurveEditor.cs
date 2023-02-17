using System;
using System.Collections.Generic;
using System.Linq;
using VFCurveEditor.Interfaces;
using VFCurveEditor.Models;

namespace VFCurveEditor.Services;

internal class CurveEditor : ICurveEditor
{
    // Options
    public float FrequencyMultiplicator = 15;
    public bool IsOffset = true;

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
    public IEnumerable<CurvePoint> Generate(IEnumerable<CurvePoint> curvePoints, float targetVoltage, float targetFrequency, float offsetVoltage)
    {
        if (!curvePoints.Any()) return curvePoints;

        CurvePoint[] points = curvePoints
            .Select(p => new CurvePoint
            {
                Offset = 0f,
                Frequency = p.Frequency,
                Voltage = p.Voltage
            })
            .ToArray();

        var minVoltage = points.Select(_ => _.Voltage).Min();
        var maxVoltage = points.Select(_ => _.Voltage).Max();

        int pointsNumBetween = 0;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].Voltage > targetVoltage - offsetVoltage &&
                points[i].Voltage <= targetVoltage)
            {
                pointsNumBetween++;
            }
        }

        float startFreq = points.First(_ => _.Voltage >= targetVoltage - offsetVoltage).Frequency;

        int pointsDone = 0;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].Frequency > startFreq &&
                pointsDone < pointsNumBetween)
            {
                pointsDone++;
                // Offset must be a multiple of 15
                if (points[i].Voltage != targetVoltage)
                {
                    points[i].Frequency += MathF.Ceiling((targetFrequency - startFreq) / pointsNumBetween * pointsDone / FrequencyMultiplicator) * FrequencyMultiplicator - (points[i].Frequency - startFreq);
                }
            }

            if (points[i].Voltage == targetVoltage)
            {
                points[i].Offset = targetFrequency - points[i].Frequency;
            }

            if (points[i].Frequency > targetFrequency)
            {
                points[i].Offset = targetFrequency - points[i].Frequency;
            }

            if (points[i].Frequency < targetFrequency &&
                points[i].Voltage > targetVoltage)
            {
                points[i].Frequency = targetFrequency;
            }
        }

        return points;
    }
}
