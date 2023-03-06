using System;
using System.Collections;
using System.Windows.Data;

namespace VFCurveEditor.Converters;

internal class HasItemsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is IEnumerable collection)
        {
            return collection.GetEnumerator().MoveNext();
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}