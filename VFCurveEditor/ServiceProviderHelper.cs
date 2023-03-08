using System;
using System.Collections.Generic;
using System.Linq;

namespace VFCurveEditor;

public static class ServiceProviderHelper
{
    public static IEnumerable<T> GetInstances<T>()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(T).IsAssignableFrom(p))
            .Where(t => !(t.IsAbstract || t.IsInterface))
            .Select(t => (T)Activator.CreateInstance(t));
    }
}
