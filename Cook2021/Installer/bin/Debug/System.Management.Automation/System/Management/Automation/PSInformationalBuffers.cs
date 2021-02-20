// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSInformationalBuffers
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal sealed class PSInformationalBuffers
  {
    private Guid psInstanceId;
    internal PSDataCollection<ProgressRecord> progress;
    internal PSDataCollection<VerboseRecord> verbose;
    internal PSDataCollection<DebugRecord> debug;
    private PSDataCollection<WarningRecord> warning;

    internal PSInformationalBuffers(Guid psInstanceId)
    {
      this.psInstanceId = psInstanceId;
      this.progress = new PSDataCollection<ProgressRecord>();
      this.verbose = new PSDataCollection<VerboseRecord>();
      this.debug = new PSDataCollection<DebugRecord>();
      this.warning = new PSDataCollection<WarningRecord>();
    }

    internal PSDataCollection<ProgressRecord> Progress
    {
      get => this.progress;
      set => this.progress = value;
    }

    internal PSDataCollection<VerboseRecord> Verbose
    {
      get => this.verbose;
      set => this.verbose = value;
    }

    internal PSDataCollection<DebugRecord> Debug
    {
      get => this.debug;
      set => this.debug = value;
    }

    internal PSDataCollection<WarningRecord> Warning
    {
      get => this.warning;
      set => this.warning = value;
    }

    internal void AddProgress(ProgressRecord item)
    {
      if (this.progress == null)
        return;
      this.progress.InternalAdd(this.psInstanceId, item);
    }

    internal void AddVerbose(VerboseRecord item)
    {
      if (this.verbose == null)
        return;
      this.verbose.InternalAdd(this.psInstanceId, item);
    }

    internal void AddDebug(DebugRecord item)
    {
      if (this.debug == null)
        return;
      this.debug.InternalAdd(this.psInstanceId, item);
    }

    internal void AddWarning(WarningRecord item)
    {
      if (this.warning == null)
        return;
      this.warning.InternalAdd(this.psInstanceId, item);
    }
  }
}
