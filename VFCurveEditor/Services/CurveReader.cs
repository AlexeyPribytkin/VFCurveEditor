using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VFCurveEditor.Interfaces;
using VFCurveEditor.Models;

namespace VFCurveEditor.Services;

internal class CurveReader : ICurveReader
{
    private const int Int32Length = 8;

    private int _position;
    private string _curveString;
    private string _header;
    private string _ending;

    public string Header => _header;

    public string Ending => _ending;
    //{ get; } = "030000000020DF440000A6420020DF440000A6420020DF440000AC420020DF440000AC420000B4440000B4420000B4440000B44200000000000000000000000000000000";

    public CurveReader()
    {
        Reset();
    }

    public IEnumerable<CurvePoint> Read(string curveString)
    {
        Reset();

        _curveString = curveString ?? throw new ArgumentNullException(nameof(curveString));

        List<CurvePoint> points = new();

        _header = ReadHeader();

        CurvePoint point;
        while ((point = ReadPoint()) != null)
        {
            if (point.Frequency == 0 && point.Voltage == 0 && point.Offset == 0)
            {
                _ending = _curveString.Substring(_position - 3 * Int32Length);
                _position = _curveString.Length;
                break;
            }

            points.Add(point);
        }

        return points;
    }

    public string Write(IEnumerable<CurvePoint> points)
    {
        StringBuilder builder = new(_header);

        foreach (var p in points)
        {
            builder.Append(WritePoint(p));
        }

        builder.Append(_ending);

        return builder.ToString();
    }

    private void Reset()
    {
        _header = string.Empty;
        _curveString = string.Empty;
        _position = 0;
    }

    private static string ToHexString(float f)
    {
        var bytes = BitConverter.GetBytes(f).Reverse().ToArray();
        var i = BitConverter.ToInt32(bytes, 0);
        return i.ToString("X8");
    }

    private static float FromHexString(string s)
    {
        var i = Convert.ToInt32(s, 16);
        var bytes = BitConverter.GetBytes(i).Reverse().ToArray();
        return BitConverter.ToSingle(bytes, 0);
    }

    private float? Read()
    {
        if (_position >= _curveString.Length) return null;

        string stringValue = _curveString.Substring(_position, Int32Length);
        _position += Int32Length;

        return FromHexString(stringValue);
    }

    private CurvePoint ReadPoint()
    {
        float? voltage = Read();
        float? frequency = Read();
        float? offset = Read();

        if (voltage.HasValue &&
            frequency.HasValue &&
            offset.HasValue)
        {
            return new CurvePoint
            {
                Voltage = voltage.Value,
                Frequency = frequency.Value,
                Offset = offset.Value
            };
        }

        return null;
    }

    private string ReadHeader()
    {
        if (_position == 0)
        {
            _position += Int32Length * 3;
            return _curveString[..(Int32Length * 3)];
        }

        return string.Empty;
    }

    private static string WritePoint(CurvePoint point)
    {
        return $"{ToHexString(point.Voltage)}{ToHexString(point.Frequency)}{ToHexString(point.Offset)}";
    }
}
