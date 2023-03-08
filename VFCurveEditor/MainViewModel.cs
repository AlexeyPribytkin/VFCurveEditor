using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using VFCurveEditor.Interfaces;
using VFCurveEditor.Models;

namespace VFCurveEditor;

/// <summary>
/// Represents the view-model for the main window.
/// </summary>
internal class MainViewModel : INotifyPropertyChanged
{
    private string _path;
    public string Path
    {
        get => _path;
        set => SetField(ref _path, value);
    }

    private string _curveString;
    public string CurveString
    {
        get => _curveString;
        set => SetField(ref _curveString, value);
    }

    private string _targetCurveString;
    public string TargetCurveString
    {
        get => _targetCurveString;
        set => SetField(ref _targetCurveString, value);
    }

    private float _targetFrequency;
    public float TargetFrequency
    {
        get => _targetFrequency;
        set => SetField(ref _targetFrequency, value);
    }

    private float _targetVoltage;
    public float TargetVoltage
    {
        get => _targetVoltage;
        set => SetField(ref _targetVoltage, value);
    }

    private float _targetOffset;
    public float TargetOffset
    {
        get => _targetOffset;
        set => SetField(ref _targetOffset, value);
    }

    private IEnumerable<CurvePoint> _curvePoints;
    public IEnumerable<CurvePoint> CurvePoints
    {
        get => _curvePoints;
        set => SetField(ref _curvePoints, value);
    }

    private IEnumerable<Profile> _profiles;
    public IEnumerable<Profile> Profiles
    {
        get => _profiles;
        set => SetField(ref _profiles, value);
    }

    private Profile _selectedProfile;
    public Profile SelectedProfile
    {
        get => _selectedProfile;
        set => SetField(ref _selectedProfile, value);
    }

    private IEnumerable<IMethod> _methods;
    public IEnumerable<IMethod> Methods
    {
        get => _methods;
        set => SetField(ref _methods, value);
    }

    private IMethod _selectedMethod;
    public IMethod SelectedMethod
    {
        get => _selectedMethod;
        set => SetField(ref _selectedMethod, value);
    }

    public IEnumerable<float> Frequencies { get; }
    public IEnumerable<float> Voltages { get; }
    public IEnumerable<float> Offsets { get; }

    public MainViewModel()
    {
        /// Default parameters
        TargetVoltage = 900f;
        TargetFrequency = 1800f;
        TargetOffset = 50f;

        Frequencies = Range(Settings.MIN_FREQUENCY, Settings.MAX_FREQUENCY, Settings.FREQUENCY_STEP);
        Voltages = Range(Settings.MIN_VOLTAGE, Settings.MAX_VOLTAGE, Settings.VOLTAGE_STEP);
        Offsets = Range(Settings.MIN_VOLTAGE, Settings.MAX_VOLTAGE, Settings.VOLTAGE_STEP);
    }

    public void Reset()
    {
        Path = string.Empty;
        CurveString = string.Empty;
        TargetCurveString = string.Empty;
        CurvePoints = System.Array.Empty<CurvePoint>();
        Profiles = System.Array.Empty<Profile>();
        SelectedProfile = null;
    }

    public void RefreshGraph()
    {
        OnPropertyChanged(nameof(CurvePoints));
    }

    #region Helpers
    private static IEnumerable<float> Range(float min, float max, float step)
    {
        float value = min;

        for (int i = 0; value < max; i++)
        {
            value = min + step * i;
            yield return value;
        }
    }
    #endregion

    #region NotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    #endregion
}
