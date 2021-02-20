// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.InvokeHistoryCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Invoke", "History", SupportsShouldProcess = true)]
  public class InvokeHistoryCommand : PSCmdlet
  {
    private const string ErrorBase = "History";
    private bool _multipleIdProvided;
    private string _id;
    private long _historyId = -1;
    private string _commandLine;
    [TraceSource("History", "InvokeHistory")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("History", "InvokeHistory");

    [Parameter(Position = 0, ValueFromPipelineByPropertyName = true)]
    public string Id
    {
      get
      {
        using (InvokeHistoryCommand._trace.TraceProperty())
          return this._id;
      }
      set
      {
        using (InvokeHistoryCommand._trace.TraceProperty())
        {
          if (this._id != null)
            this._multipleIdProvided = true;
          this._id = value;
        }
      }
    }

    protected override void EndProcessing()
    {
      using (InvokeHistoryCommand._trace.TraceMethod())
      {
        if (this._multipleIdProvided)
          this.ThrowTerminatingError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.FormatResourceString("History", "InvokeHistoryMultipleCommandsError")), "InvokeHistoryMultipleCommandsError", ErrorCategory.InvalidArgument, (object) null));
        HistoryInfo historyEntryToInvoke = this.GetHistoryEntryToInvoke(((LocalRunspace) this.Context.CurrentRunspace).History);
        LocalPipeline currentlyRunningPipeline = (LocalPipeline) this.Context.CurrentRunspace.GetCurrentlyRunningPipeline();
        if (!currentlyRunningPipeline.PresentInInvokeHistoryEntryList(historyEntryToInvoke))
          currentlyRunningPipeline.AddToInvokeHistoryEntryList(historyEntryToInvoke);
        else
          this.ThrowTerminatingError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("History", "InvokeHistoryLoopDetected")), "InvokeHistoryLoopDetected", ErrorCategory.InvalidOperation, (object) null));
        this.ReplaceHistoryString(historyEntryToInvoke);
        string commandLine = historyEntryToInvoke.CommandLine;
        if (!this.ShouldProcess(commandLine))
          return;
        try
        {
          this.Host.UI.WriteLine(commandLine);
        }
        catch (HostException ex)
        {
        }
        Collection<PSObject> collection = this.InvokeCommand.InvokeScript(commandLine, false, PipelineResultTypes.Output | PipelineResultTypes.Error, (IList) null, (object[]) null);
        if (collection.Count > 0)
          this.WriteObject((object) collection, true);
        currentlyRunningPipeline.RemoveFromInvokeHistoryEntryList(historyEntryToInvoke);
      }
    }

    private HistoryInfo GetHistoryEntryToInvoke(History history)
    {
      using (InvokeHistoryCommand._trace.TraceMethod())
      {
        HistoryInfo historyInfo = (HistoryInfo) null;
        if (this._id == null)
        {
          HistoryInfo[] entries = history.GetEntries(0L, 1L, (SwitchParameter) true);
          if (entries.Length == 1)
            historyInfo = entries[0];
          else
            this.ThrowTerminatingError(new ErrorRecord((Exception) new InvalidOperationException(ResourceManagerCache.FormatResourceString("History", "NoLastHistoryEntryFound")), "InvokeHistoryNoLastHistoryEntryFound", ErrorCategory.InvalidOperation, (object) null));
        }
        else
        {
          this.PopulateIdAndCommandLine();
          if (this._commandLine != null)
          {
            HistoryInfo[] entries = history.GetEntries(0L, -1L, (SwitchParameter) false);
            for (int index = entries.Length - 1; index >= 0; --index)
            {
              if (entries[index].CommandLine.StartsWith(this._commandLine, StringComparison.CurrentCulture))
              {
                historyInfo = entries[index];
                break;
              }
            }
            if (historyInfo == null)
              this.ThrowTerminatingError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.FormatResourceString("History", "NoHistoryForCommandline", (object) this._commandLine)), "InvokeHistoryNoHistoryForCommandline", ErrorCategory.ObjectNotFound, (object) this._commandLine));
          }
          else if (this._historyId <= 0L)
          {
            this.ThrowTerminatingError(new ErrorRecord((Exception) new ArgumentOutOfRangeException("Id", ResourceManagerCache.FormatResourceString("History", "InvalidIdGetHistory", (object) this._historyId)), "InvokeHistoryInvalidIdGetHistory", ErrorCategory.InvalidArgument, (object) this._historyId));
          }
          else
          {
            historyInfo = history.GetEntry(this._historyId);
            if (historyInfo == null || historyInfo.Id != this._historyId)
              this.ThrowTerminatingError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.FormatResourceString("History", "NoHistoryForId", (object) this._historyId)), "InvokeHistoryNoHistoryForId", ErrorCategory.ObjectNotFound, (object) this._historyId));
          }
        }
        return historyInfo;
      }
    }

    private void PopulateIdAndCommandLine()
    {
      using (InvokeHistoryCommand._trace.TraceMethod())
      {
        if (this._id == null)
          return;
        try
        {
          this._historyId = (long) LanguagePrimitives.ConvertTo((object) this._id, typeof (long), (IFormatProvider) CultureInfo.InvariantCulture);
        }
        catch (PSInvalidCastException ex)
        {
          this._commandLine = this._id;
        }
      }
    }

    private void ReplaceHistoryString(HistoryInfo entry)
    {
      LocalPipeline currentlyRunningPipeline = (LocalPipeline) this.Context.CurrentRunspace.GetCurrentlyRunningPipeline();
      if (!currentlyRunningPipeline.AddToHistory)
        return;
      currentlyRunningPipeline.HistoryString = entry.CommandLine;
    }
  }
}
