using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VFCurveEditor.Interfaces;
using VFCurveEditor.Models;

namespace VFCurveEditor.Services;

internal class ProfileReader : IProfileReader
{
    private string _path;
    private readonly List<Profile> _profiles = new();

    public IEnumerable<Profile> Read(string path)
    {
        _path = path;
        _profiles.Clear();

        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
        {
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                var profile = new Profile()
                {
                    Name = line
                };
                _profiles.Add(profile);

                continue;
            }

            var currentProfile = _profiles.LastOrDefault();
            if (currentProfile == null) { continue; }

            var pair = ParseLine(line);
            if (pair != null)
            {
                currentProfile.Values.Add(pair.Item1, pair.Item2);
            }
        }

        return _profiles
            .Where(p => !string.IsNullOrEmpty(p.VFCurve))
            .OrderBy(p => p.Name);
    }

    public void Save(string profileName, string curve)
    {
        bool changesApplied = false;

        string[] lines = File.ReadAllLines(_path);

        int profileIndex = Array.IndexOf(lines, profileName);

        for(int i = profileIndex + 1; i < profileIndex + 12; i++)
        {
            if (lines[i].StartsWith('[') && lines[i].EndsWith(']'))
            {
                break;
            }

            if (lines[i].StartsWith("VFCurve="))
            {
                lines[i] = $"VFCurve={curve}";
                changesApplied = true;
                break;
            }
        }

        if (changesApplied)
        {
            File.WriteAllLines(_path, lines);
        }
    }

    private static Tuple<string, string> ParseLine(string line)
    {
        string[] values = line.Split('=', 2, StringSplitOptions.None);

        if(values.Length == 2 &&
            !string.IsNullOrEmpty(values[0]))
        {
            return new Tuple<string, string>(values[0], values[1]);
        }

        return null;
    }
}
