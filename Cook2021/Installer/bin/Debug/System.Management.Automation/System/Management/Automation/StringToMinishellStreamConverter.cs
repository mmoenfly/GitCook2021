// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.StringToMinishellStreamConverter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal static class StringToMinishellStreamConverter
  {
    internal const string OutputStream = "output";
    internal const string ErrorStream = "error";
    internal const string DebugStream = "debug";
    internal const string VerboseStream = "verbose";
    internal const string WarningStream = "warning";
    internal const string ProgressStream = "progress";

    internal static MinishellStream ToMinishellStream(string stream)
    {
      MinishellStream minishellStream = MinishellStream.Unknown;
      if ("output".Equals(stream, StringComparison.OrdinalIgnoreCase))
        minishellStream = MinishellStream.Output;
      else if ("error".Equals(stream, StringComparison.OrdinalIgnoreCase))
        minishellStream = MinishellStream.Error;
      else if ("debug".Equals(stream, StringComparison.OrdinalIgnoreCase))
        minishellStream = MinishellStream.Debug;
      else if ("verbose".Equals(stream, StringComparison.OrdinalIgnoreCase))
        minishellStream = MinishellStream.Verbose;
      else if ("warning".Equals(stream, StringComparison.OrdinalIgnoreCase))
        minishellStream = MinishellStream.Warning;
      else if ("progress".Equals(stream, StringComparison.OrdinalIgnoreCase))
        minishellStream = MinishellStream.Progress;
      return minishellStream;
    }
  }
}
