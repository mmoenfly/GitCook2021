// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.GetHistoryCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  [OutputType(new Type[] {typeof (HistoryInfo)})]
  [Cmdlet("Get", "History")]
  public class GetHistoryCommand : PSCmdlet
  {
    private const string ErrorBase = "History";
    private long[] _id;
    private bool _countParamterSpecified;
    private int _count = 32;
    [TraceSource("History", "Get History class")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("History", "Get History class");

    [ValidateRange(1, 9223372036854775807)]
    [Parameter(Position = 0, ValueFromPipeline = true)]
    public long[] Id
    {
      get
      {
        using (GetHistoryCommand._trace.TraceProperty())
          return this._id;
      }
      set
      {
        using (GetHistoryCommand._trace.TraceProperty())
          this._id = value;
      }
    }

    [ValidateRange(0, 32767)]
    [Parameter(Position = 1)]
    public int Count
    {
      get
      {
        using (GetHistoryCommand._trace.TraceProperty())
          return this._count;
      }
      set
      {
        using (GetHistoryCommand._trace.TraceProperty())
        {
          this._countParamterSpecified = true;
          this._count = value;
        }
      }
    }

    protected override void ProcessRecord()
    {
      using (GetHistoryCommand._trace.TraceMethod())
      {
        History history = ((LocalRunspace) this.Context.CurrentRunspace).History;
        if (this._id != null)
        {
          if (!this._countParamterSpecified)
          {
            foreach (long id in this._id)
            {
              HistoryInfo entry = history.GetEntry(id);
              if (entry != null && entry.Id == id)
                this.WriteObject((object) entry);
              else
                this.WriteError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.FormatResourceString("History", "NoHistoryForId", (object) id)), "GetHistoryNoHistoryForId", ErrorCategory.ObjectNotFound, (object) id));
            }
          }
          else if (this._id.Length > 1)
          {
            this.ThrowTerminatingError(new ErrorRecord((Exception) new ArgumentException(ResourceManagerCache.FormatResourceString("History", "NoCountWithMultipleIds")), "GetHistoryNoCountWithMultipleIds", ErrorCategory.InvalidArgument, (object) this._count));
          }
          else
          {
            long id = this._id[0];
            this.WriteObject((object) history.GetEntries(id, (long) this._count, (SwitchParameter) false), true);
          }
        }
        else
        {
          HistoryInfo[] entries = history.GetEntries(0L, (long) this._count, (SwitchParameter) true);
          for (long index = (long) (entries.Length - 1); index >= 0L; --index)
            this.WriteObject((object) entries[index]);
        }
      }
    }
  }
}
