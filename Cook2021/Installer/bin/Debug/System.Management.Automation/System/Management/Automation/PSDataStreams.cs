// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSDataStreams
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public sealed class PSDataStreams
  {
    private PowerShell powershell;

    internal PSDataStreams(PowerShell powershell) => this.powershell = powershell;

    public PSDataCollection<ErrorRecord> Error
    {
      get => this.powershell.ErrorBuffer;
      set => this.powershell.ErrorBuffer = value;
    }

    public PSDataCollection<ProgressRecord> Progress
    {
      get => this.powershell.ProgressBuffer;
      set => this.powershell.ProgressBuffer = value;
    }

    public PSDataCollection<VerboseRecord> Verbose
    {
      get => this.powershell.VerboseBuffer;
      set => this.powershell.VerboseBuffer = value;
    }

    public PSDataCollection<DebugRecord> Debug
    {
      get => this.powershell.DebugBuffer;
      set => this.powershell.DebugBuffer = value;
    }

    public PSDataCollection<WarningRecord> Warning
    {
      get => this.powershell.WarningBuffer;
      set => this.powershell.WarningBuffer = value;
    }

    public void ClearStreams()
    {
      this.Error.Clear();
      this.Progress.Clear();
      this.Verbose.Clear();
      this.Debug.Clear();
      this.Warning.Clear();
    }
  }
}
