using System.Collections.Generic;

namespace VFCurveEditor.Models;

internal class Profile
{
    public string Name { get; set; }

    public string VFCurve
    {
        get
        {
            Values.TryGetValue("VFCurve", out var value);
            return value;
        }
    }

    public Dictionary<string, string> Values { get; private set; }

    public Profile()
    {
        Values = new Dictionary<string, string>();
    }

    public override string ToString()
    {
        return Name;
    }
}
