using System;
using System.Collections.Generic;
using System.Windows.Data;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using VFCurveEditor.Models;

namespace VFCurveEditor.Converters;

internal class PlotModelConverter : IValueConverter
{
    private static PlotModel _plotModel;

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is IEnumerable<CurvePoint> curvePoints)
        {
            return GetPlot(curvePoints);
        }

        return new PlotModel();
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static PlotModel GetPlot(IEnumerable<CurvePoint> curvePoints)
    {
        if (_plotModel == null)
        {
            // Create the plot model
            var plotModel = new PlotModel();

            // Create two line series (markers are hidden by default)
            var source = new LineSeries { Title = "Source", MarkerType = MarkerType.Star, Color = OxyColor.FromArgb(64, 0, 255, 0) };
            var target = new LineSeries { Title = "Target", MarkerType = MarkerType.Square, Color = OxyColor.FromArgb(255, 0, 255, 0) };

            // Add the series to the plot model
            plotModel.Series.Add(source);
            plotModel.Series.Add(target);

            // Axes are created automatically if they are not defined
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Voltage, mV" });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Frequency, MHz" });

            plotModel.PlotMargins = new OxyThickness(48, 0, -8, 36);

            _plotModel = plotModel;
        }

        var sourceCurve = _plotModel.Series[0] as LineSeries;
        var targetCurve = _plotModel.Series[1] as LineSeries;

        sourceCurve.Points.Clear();
        targetCurve.Points.Clear();

        foreach (var curvePoint in curvePoints)
        {
            sourceCurve.Points.Add(new DataPoint(curvePoint.Voltage, curvePoint.Frequency));
            targetCurve.Points.Add(new DataPoint(curvePoint.Voltage, curvePoint.Frequency + curvePoint.Offset));
        }

        _plotModel.InvalidatePlot(true);

        return _plotModel;
    }
}