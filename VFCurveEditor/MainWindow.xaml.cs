using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using VFCurveEditor.Interfaces;
using VFCurveEditor.Models;
using VFCurveEditor.Services;

namespace VFCurveEditor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ICurveReader _curveReader;
    private readonly IProfileReader _profileReader;
    private readonly IEnumerable<IMethod> _methods;

    private readonly MainViewModel _mainViewModel;

    public MainWindow()
    {
        InitializeComponent();

        CultureInfo ci = new("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;

        _curveReader = new CurveReader();
        _profileReader = new ProfileReader();
        _methods = ServiceProviderHelper.GetInstances<IMethod>();

        _mainViewModel = new MainViewModel
        {
            Methods = _methods
        };

        DataContext = _mainViewModel;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        string gpuName = string.Empty;
        string deviceId = string.Empty;

        using var searcher = new ManagementObjectSearcher("select * from Win32_VideoController");
        foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
        {
            gpuName = (string)obj["Name"];
            deviceId = (string)obj["PNPDeviceID"];
        }

        deviceId = deviceId.Split("\\")[1];

        var directory = new DirectoryInfo(Settings.DEFAULT_PROFILE_FOLDER);
        FileInfo configFile = directory.GetFiles($"{deviceId}*.cfg", SearchOption.TopDirectoryOnly)
            .OrderByDescending(f => f.LastWriteTime)
            .FirstOrDefault();

        if (configFile != null)
        {
            OpenConfig(configFile.FullName);
        }
    }

    private void OpenConfig(string path)
    {
        _mainViewModel.Reset();
        _mainViewModel.Path = path;

        var profiles = _profileReader.Read(path);
        Profile selectedProfile = profiles.FirstOrDefault();

        _mainViewModel.Profiles = profiles;
        _mainViewModel.SelectedProfile = selectedProfile;
    }

    private void CopySourceCurve_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_mainViewModel.CurveString);
    }

    private void CopyTargetCurve_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_mainViewModel.TargetCurveString);
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        _mainViewModel.CurvePoints = _curveReader.Read(_mainViewModel.CurveString);
        _mainViewModel.TargetCurveString = _curveReader.Write(_mainViewModel.CurvePoints);
        _mainViewModel.SelectedMethod = null;
    }

    private void SelectConfig_Click(object sender, RoutedEventArgs e)
    {
        string initialDirectory = Directory.Exists(Settings.DEFAULT_PROFILE_FOLDER)
            ? Settings.DEFAULT_PROFILE_FOLDER
            : Settings.MYCOMPUTER_FOLDER;

        Microsoft.Win32.OpenFileDialog dlg = new()
        {
            InitialDirectory = initialDirectory,
            DefaultExt = ".cfg",
            Filter = "MSI Afterburner Config (*.cfg)|*.cfg"
        };
        bool? result = dlg.ShowDialog();

        if (result == true)
        {
            OpenConfig(dlg.FileName);
        }
    }

    private void OpenConfig_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_mainViewModel.Path))
        {
            Process.Start(Settings.EXPLORER, _mainViewModel.Path);
        }
    }

    private void Profile_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_mainViewModel.SelectedProfile == null) return;

        string curveString = _mainViewModel.SelectedProfile.VFCurve;
        IEnumerable<CurvePoint> points = _curveReader.Read(curveString);

        _mainViewModel.CurveString = curveString;
        _mainViewModel.CurvePoints = points;

        RefreshTargetCurveString();
    }

    private void Properties_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(_mainViewModel.SelectedMethod != null)
        {
            Apply(_mainViewModel.SelectedMethod);
        }
    }

    private void DataGridTextColumn_SourceUpdated(object sender, EventArgs e)
    {
        _mainViewModel.RefreshGraph();

        RefreshTargetCurveString();
    }

    private void RefreshTargetCurveString()
    {
        var points = _mainViewModel.CurvePoints;
        var outputCurve = _curveReader.Write(points);
        _mainViewModel.TargetCurveString = outputCurve;
    }

    private void Apply(IMethod method)
    {
        var sourcePoints = _curveReader.Read(_mainViewModel.CurveString);
        _mainViewModel.CurvePoints = method.Apply(
            sourcePoints,
            _mainViewModel.TargetVoltage,
            _mainViewModel.TargetFrequency,
            _mainViewModel.TargetOffset);

        _mainViewModel.TargetCurveString = _curveReader.Write(_mainViewModel.CurvePoints);
    }
}
