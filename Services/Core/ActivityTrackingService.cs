using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using DigitalTwin.Data;
using DigitalTwin.Models;

namespace DigitalTwin.Services.Core;

public class ActivityTrackingService
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }

    private readonly DigitalTwinDbContext _context;
    private readonly System.Timers.Timer _trackingTimer;
    private string _lastWindowTitle = string.Empty;
    private string _lastProcessName = string.Empty;
    private DateTime _lastActivityTime = DateTime.Now;
    private int? _currentFocusSessionId;

    public bool IsTracking { get; private set; }
    public int IdleThresholdSeconds { get; set; } = 300; // 5 minutes

    public ActivityTrackingService()
    {
        _context = new DigitalTwinDbContext();
        _trackingTimer = new System.Timers.Timer(5000); // Check every 5 seconds
        _trackingTimer.Elapsed += OnTrackingTick;
    }

    public void StartTracking()
    {
        IsTracking = true;
        _trackingTimer.Start();
    }

    public void StopTracking()
    {
        IsTracking = false;
        _trackingTimer.Stop();
        SaveCurrentActivity();
    }

    public void StartFocusSession(string? goal = null)
    {
        var session = new FocusSession
        {
            StartTime = DateTime.Now,
            Goal = goal,
            InterruptionCount = 0,
            TotalFocusSeconds = 0,
            ProductivityScore = 0
        };

        _context.FocusSessions.Add(session);
        _context.SaveChanges();
        _currentFocusSessionId = session.Id;
    }

    public void EndFocusSession()
    {
        if (_currentFocusSessionId == null) return;

        var session = _context.FocusSessions.Find(_currentFocusSessionId);
        if (session != null)
        {
            session.EndTime = DateTime.Now;
            session.TotalFocusSeconds = (int)(session.EndTime.Value - session.StartTime).TotalSeconds;
            _context.SaveChanges();
        }

        _currentFocusSessionId = null;
    }

    private void OnTrackingTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            var (windowTitle, processName) = GetActiveWindowInfo();
            var isIdle = IsUserIdle();

            if (windowTitle != _lastWindowTitle || processName != _lastProcessName)
            {
                SaveCurrentActivity();
                _lastWindowTitle = windowTitle;
                _lastProcessName = processName;
                _lastActivityTime = DateTime.Now;
            }
        }
        catch (Exception ex)
        {
            // Log error silently
            Debug.WriteLine($"Tracking error: {ex.Message}");
        }
    }

    private (string windowTitle, string processName) GetActiveWindowInfo()
    {
        var handle = GetForegroundWindow();
        var sb = new StringBuilder(256);
        GetWindowText(handle, sb, 256);
        
        GetWindowThreadProcessId(handle, out uint processId);
        var process = Process.GetProcessById((int)processId);
        
        return (sb.ToString(), process.ProcessName);
    }

    private bool IsUserIdle()
    {
        var lastInput = new LASTINPUTINFO { cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO)) };
        GetLastInputInfo(ref lastInput);
        
        var idleTime = (Environment.TickCount - lastInput.dwTime) / 1000;
        return idleTime > IdleThresholdSeconds;
    }

    private void SaveCurrentActivity()
    {
        if (string.IsNullOrEmpty(_lastWindowTitle)) return;

        var duration = (int)(DateTime.Now - _lastActivityTime).TotalSeconds;
        if (duration < 1) return;

        var log = new ActivityLog
        {
            Timestamp = _lastActivityTime,
            WindowTitle = _lastWindowTitle,
            ProcessName = _lastProcessName,
            IsIdle = false,
            DurationSeconds = duration,
            FocusSessionId = _currentFocusSessionId
        };

        _context.ActivityLogs.Add(log);
        _context.SaveChanges();
    }
}
