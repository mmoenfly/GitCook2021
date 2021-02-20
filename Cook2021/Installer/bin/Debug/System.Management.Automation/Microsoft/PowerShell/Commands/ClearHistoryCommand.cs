// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.ClearHistoryCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Clear", "History", DefaultParameterSetName = "IDParameter", SupportsShouldProcess = true)]
  public class ClearHistoryCommand : PSCmdlet
  {
    private const string ErrorBase = "History";
    private int[] _id;
    private string[] _commandline;
    private int _count = 32;
    private bool _countParamterSpecified;
    private SwitchParameter _newest;
    private History history;
    private HistoryInfo[] entries;
    [TraceSource("History", "Clear History class")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("History", "Clear History class");

    [ValidateRange(1, 2147483647)]
    [Parameter(HelpMessage = "Specifies the ID of a command in the session history.Clear history clears only the specified command", ParameterSetName = "IDParameter", Position = 0)]
    public int[] Id
    {
      get
      {
        using (ClearHistoryCommand._trace.TraceProperty())
          return this._id;
      }
      set
      {
        using (ClearHistoryCommand._trace.TraceProperty())
          this._id = value;
      }
    }

    [Parameter(HelpMessage = "Specifies the name of a command in the session history", ParameterSetName = "CommandLineParameter")]
    [ValidateNotNullOrEmpty]
    public string[] CommandLine
    {
      get
      {
        using (ClearHistoryCommand._trace.TraceProperty())
          return this._commandline;
      }
      set
      {
        using (ClearHistoryCommand._trace.TraceProperty())
          this._commandline = value;
      }
    }

    [Parameter(HelpMessage = "Clears the specified number of history entries", Mandatory = false, Position = 1)]
    [ValidateRange(1, 2147483647)]
    public int Count
    {
      get
      {
        using (ClearHistoryCommand._trace.TraceProperty())
          return this._count;
      }
      set
      {
        using (ClearHistoryCommand._trace.TraceProperty())
        {
          this._countParamterSpecified = true;
          this._count = value;
        }
      }
    }

    [Parameter(HelpMessage = "Specifies whether new entries to be cleared or the default old ones.", Mandatory = false)]
    public SwitchParameter Newest
    {
      get
      {
        using (ClearHistoryCommand._trace.TraceProperty())
          return this._newest;
      }
      set
      {
        using (ClearHistoryCommand._trace.TraceProperty())
          this._newest = value;
      }
    }

    protected override void BeginProcessing() => this.history = ((LocalRunspace) this.Context.CurrentRunspace).History;

    protected override void ProcessRecord()
    {
      using (ClearHistoryCommand._trace.TraceMethod())
      {
        switch (this.ParameterSetName.ToString())
        {
          case "IDParameter":
            this.ClearHistoryByID();
            break;
          case "CommandLineParameter":
            this.ClearHistoryByCmdLine();
            break;
          default:
            this.ThrowTerminatingError(new ErrorRecord((Exception) new ArgumentException("Invalid ParameterSet Name"), "Unable to access the session history", ErrorCategory.InvalidOperation, (object) null));
            break;
        }
      }
    }

    private void ClearHistoryByID()
    {
      using (ClearHistoryCommand._trace.TraceMethod())
      {
        if (this._countParamterSpecified && this.Count < 0)
          this.ThrowTerminatingError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.FormatResourceString("History", "InvalidCountValue")), "ClearHistoryInvalidCountValue", ErrorCategory.InvalidArgument, (object) this._count));
        if (this._id != null)
        {
          if (!this._countParamterSpecified)
          {
            foreach (long id in this._id)
            {
              HistoryInfo entry = this.history.GetEntry(id);
              if (entry != null && entry.Id == id)
                this.history.ClearEntry(entry.Id);
              else
                this.WriteError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.FormatResourceString("History", "NoHistoryForId", (object) id)), "GetHistoryNoHistoryForId", ErrorCategory.ObjectNotFound, (object) id));
            }
          }
          else if (this._id.Length > 1)
            this.ThrowTerminatingError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.FormatResourceString("History", "NoCountWithMultipleIds")), "GetHistoryNoCountWithMultipleIds", ErrorCategory.InvalidArgument, (object) this._count));
          else
            this.ClearHistoryEntries((long) this._id[0], this._count, (string) null, this._newest);
        }
        else if (!this._countParamterSpecified)
        {
          if (!this.ShouldProcess(ResourceManagerCache.FormatResourceString("History", "ClearHistoryWarning", (object) "Warning")))
            return;
          this.ClearHistoryEntries(0L, -1, (string) null, this._newest);
        }
        else
          this.ClearHistoryEntries(0L, this._count, (string) null, this._newest);
      }
    }

    private void ClearHistoryByCmdLine()
    {
      using (ClearHistoryCommand._trace.TraceMethod())
      {
        if (this._countParamterSpecified && this.Count < 0)
          this.ThrowTerminatingError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.FormatResourceString("History", "InvalidCountValue")), "ClearHistoryInvalidCountValue", ErrorCategory.InvalidArgument, (object) this._count));
        if (this._commandline == null)
          return;
        if (!this._countParamterSpecified)
        {
          foreach (string cmdline in this._commandline)
            this.ClearHistoryEntries(0L, 1, cmdline, this._newest);
        }
        else if (this._commandline.Length > 1)
          this.ThrowTerminatingError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.FormatResourceString("History", "NoCountWithMultipleCmdLine")), "NoCountWithMultipleCmdLine ", ErrorCategory.InvalidArgument, (object) this._commandline));
        else
          this.ClearHistoryEntries(0L, this._count, this._commandline[0], this._newest);
      }
    }

    private void ClearHistoryEntries(long id, int count, string cmdline, SwitchParameter newest)
    {
      using (ClearHistoryCommand._trace.TraceMethod())
      {
        if (cmdline == null)
        {
          if (id > 0L)
          {
            HistoryInfo entry = this.history.GetEntry(id);
            if (entry == null || entry.Id != id)
              this.WriteError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.FormatResourceString("History", "NoHistoryForId", (object) id)), "GetHistoryNoHistoryForId", ErrorCategory.ObjectNotFound, (object) id));
            this.entries = this.history.GetEntries(id, (long) count, newest);
          }
          else
            this.entries = this.history.GetEntries(0L, (long) count, newest);
        }
        else
        {
          WildcardPattern wildcardpattern = new WildcardPattern(cmdline, WildcardOptions.IgnoreCase);
          if (!this._countParamterSpecified && WildcardPattern.ContainsWildcardCharacters(cmdline))
            count = 0;
          this.entries = this.history.GetEntries(wildcardpattern, (long) count, newest);
        }
        foreach (HistoryInfo entry in this.entries)
        {
          if (entry != null && !entry.Cleared)
            this.history.ClearEntry(entry.Id);
        }
      }
    }
  }
}
