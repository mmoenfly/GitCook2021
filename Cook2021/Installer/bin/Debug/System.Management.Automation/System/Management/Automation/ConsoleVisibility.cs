// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ConsoleVisibility
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.InteropServices;

namespace System.Management.Automation
{
  internal static class ConsoleVisibility
  {
    internal const int SW_HIDE = 0;
    internal const int SW_SHOWNORMAL = 1;
    internal const int SW_NORMAL = 1;
    internal const int SW_SHOWMINIMIZED = 2;
    internal const int SW_SHOWMAXIMIZED = 3;
    internal const int SW_MAXIMIZE = 3;
    internal const int SW_SHOWNOACTIVATE = 4;
    internal const int SW_SHOW = 5;
    internal const int SW_MINIMIZE = 6;
    internal const int SW_SHOWMINNOACTIVE = 7;
    internal const int SW_SHOWNA = 8;
    internal const int SW_RESTORE = 9;
    internal const int SW_SHOWDEFAULT = 10;
    internal const int SW_FORCEMINIMIZE = 11;
    internal const int SW_MAX = 11;
    [TraceSource("NativeCP", "NativeCP")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("NativeCP", "NativeCP");
    private static bool _alwaysCaptureApplicationIO;

    public static bool AlwaysCaptureApplicationIO
    {
      get => ConsoleVisibility._alwaysCaptureApplicationIO;
      set => ConsoleVisibility._alwaysCaptureApplicationIO = value;
    }

    [DllImport("kernel32.dll")]
    internal static extern int GetConsoleProcessList([In, Out] int[] lpdwProcessList, int dwProcessCount);

    [DllImport("Kernel32.dll")]
    internal static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool AllocConsole();

    internal static bool AllocateHiddenConsole()
    {
      if (ConsoleVisibility.GetConsoleWindow() == IntPtr.Zero)
      {
        ConsoleVisibility.AllocConsole();
        IntPtr consoleWindow = ConsoleVisibility.GetConsoleWindow();
        if (consoleWindow != IntPtr.Zero)
        {
          ConsoleVisibility.ShowWindow(consoleWindow, 0);
          ConsoleVisibility.AlwaysCaptureApplicationIO = true;
          return true;
        }
      }
      return false;
    }

    public static void Show()
    {
      IntPtr consoleWindow = ConsoleVisibility.GetConsoleWindow();
      if (!(consoleWindow != IntPtr.Zero))
        throw ConsoleVisibility.tracer.NewInvalidOperationException();
      ConsoleVisibility.ShowWindow(consoleWindow, 5);
      ConsoleVisibility.AlwaysCaptureApplicationIO = false;
    }

    public static void Hide()
    {
      IntPtr consoleWindow = ConsoleVisibility.GetConsoleWindow();
      if (!(consoleWindow != IntPtr.Zero))
        throw ConsoleVisibility.tracer.NewInvalidOperationException();
      ConsoleVisibility.ShowWindow(consoleWindow, 0);
      ConsoleVisibility.AlwaysCaptureApplicationIO = true;
    }
  }
}
