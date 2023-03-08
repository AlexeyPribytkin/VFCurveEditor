using System;

namespace VFCurveEditor;

internal static class Settings
{
    public readonly static string DEFAULT_PROFILE_FOLDER =
        Environment.GetFolderPath(
            Environment.SpecialFolder.ProgramFilesX86) +
        "\\MSI Afterburner\\Profiles\\";

    public readonly static string MYCOMPUTER_FOLDER = "shell:MyComputerFolder";
    public readonly static string EXPLORER = "explorer.exe";
    public readonly static float FREQUENCY_STEP = 15f;
    public readonly static float VOLTAGE_STEP = 6.25f;
    public readonly static float MIN_FREQUENCY = 0f;
    public readonly static float MAX_FREQUENCY = 4000.0f;
    public readonly static float MIN_VOLTAGE = 0f;
    public readonly static float MAX_VOLTAGE = 1500f;
    public readonly static float MIN_OFFSET = 0f;
    public readonly static float MAX_OFFSET = 4000.0f;
}
