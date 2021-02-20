// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandLineParameterBinderNativeMethods
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.InteropServices;

namespace System.Management.Automation
{
  internal static class CommandLineParameterBinderNativeMethods
  {
    public static string[] PreParseCommandLine(string commandLine)
    {
      int pNumArgs = 0;
      IntPtr argvW = CommandLineParameterBinderNativeMethods.CommandLineToArgvW(commandLine, out pNumArgs);
      if (argvW == IntPtr.Zero)
        return (string[]) null;
      try
      {
        string[] strArray = new string[pNumArgs - 1];
        for (int index = 1; index < pNumArgs; ++index)
          strArray[index - 1] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(argvW, index * IntPtr.Size));
        return strArray;
      }
      finally
      {
        CommandLineParameterBinderNativeMethods.LocalFree(argvW);
      }
    }

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

    [DllImport("kernel32.dll")]
    private static extern IntPtr LocalFree(IntPtr hMem);
  }
}
