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
    private const string DefaultMSIAfterburnerProfileFolder = "C:\\Program Files (x86)\\MSI Afterburner\\Profiles\\";

    private readonly ICurveReader _curveReader;
    private readonly ICurveEditor _curveEditor;
    private readonly IProfileReader _profileReader;

    private readonly MainViewModel _mainViewModel;

    public MainWindow()
    {
        InitializeComponent();

        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;

        _curveReader = new CurveReader();
        _curveEditor = new CurveEditor();
        _profileReader = new ProfileReader();

        _mainViewModel = new MainViewModel();

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

        var directory = new DirectoryInfo(DefaultMSIAfterburnerProfileFolder);
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

    private void ApplyMethod1_Click(object sender, RoutedEventArgs e)
    {
        Apply(0);
    }

    private void ApplyMethod2_Click(object sender, RoutedEventArgs e)
    {
        Apply(1);
    }

    private void Apply(int method)
    {
        var sourcePoints = _curveReader.Read(_mainViewModel.CurveString);
        _mainViewModel.CurvePoints = _curveEditor.Generate(
            sourcePoints,
            _mainViewModel.TargetVoltage,
            _mainViewModel.TargetFrequency,
            _mainViewModel.TargetOffset,
            method);

        _mainViewModel.TargetCurveString = _curveReader.Write(_mainViewModel.CurvePoints);
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        _mainViewModel.CurvePoints = _curveReader.Read(_mainViewModel.CurveString);
        _mainViewModel.TargetCurveString = _curveReader.Write(_mainViewModel.CurvePoints);
    }

    private void SelectConfig_Click(object sender, RoutedEventArgs e)
    {
        string initialDirectory = Directory.Exists(DefaultMSIAfterburnerProfileFolder)
            ? DefaultMSIAfterburnerProfileFolder
            : "shell:MyComputerFolder";

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
            Process.Start("explorer.exe", _mainViewModel.Path);
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
}
