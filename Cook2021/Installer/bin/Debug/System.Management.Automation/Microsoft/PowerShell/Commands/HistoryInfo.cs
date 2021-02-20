// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.HistoryInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  public class HistoryInfo
  {
    private long _pipelineId;
    private long _id;
    private string _cmdline;
    private PipelineState _status;
    private DateTime _startTime;
    private DateTime _endTime;
    private bool _cleared;
    [TraceSource("History", "HistoryInfo class")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("History", "HistoryInfo class");

    internal HistoryInfo(
      long pipelineId,
      string cmdline,
      PipelineState status,
      DateTime startTime,
      DateTime endTime)
    {
      using (HistoryInfo._trace.TraceConstructor((object) this))
      {
        this._pipelineId = pipelineId;
        this._cmdline = cmdline;
        this._status = status;
        this._startTime = startTime;
        this._endTime = endTime;
        this._cleared = false;
      }
    }

    private HistoryInfo(HistoryInfo history)
    {
      using (HistoryInfo._trace.TraceConstructor((object) this))
      {
        this._id = history._id;
        this._pipelineId = history._pipelineId;
        this._cmdline = history._cmdline;
        this._status = history._status;
        this._startTime = history._startTime;
        this._endTime = history._endTime;
        this._cleared = history._cleared;
      }
    }

    public long Id
    {
      get
      {
        using (HistoryInfo._trace.TraceProperty())
          return this._id;
      }
    }

    public string CommandLine
    {
      get
      {
        using (HistoryInfo._trace.TraceProperty())
          return this._cmdline;
      }
    }

    public PipelineState ExecutionStatus
    {
      get
      {
        using (HistoryInfo._trace.TraceProperty())
          return this._status;
      }
    }

    public DateTime StartExecutionTime
    {
      get
      {
        using (HistoryInfo._trace.TraceProperty())
          return this._startTime;
      }
    }

    public DateTime EndExecutionTime
    {
      get
      {
        using (HistoryInfo._trace.TraceProperty())
          return this._endTime;
      }
    }

    public override string ToString()
    {
      using (HistoryInfo._trace.TraceMethod())
        return string.IsNullOrEmpty(this._cmdline) ? base.ToString() : this._cmdline;
    }

    internal bool Cleared
    {
      get
      {
        using (HistoryInfo._trace.TraceProperty())
          return this._cleared;
      }
      set
      {
        using (HistoryInfo._trace.TraceProperty())
          this._cleared = value;
      }
    }

    internal void SetId(long id)
    {
      using (HistoryInfo._trace.TraceMethod())
        this._id = id;
    }

    internal void SetStatus(PipelineState status)
    {
      using (HistoryInfo._trace.TraceMethod())
        this._status = status;
    }

    internal void SetEndTime(DateTime endTime)
    {
      using (HistoryInfo._trace.TraceMethod())
        this._endTime = endTime;
    }

    internal void SetCommand(string command)
    {
      using (HistoryInfo._trace.TraceMethod())
        this._cmdline = command;
    }

    public HistoryInfo Clone()
    {
      using (HistoryInfo._trace.TraceMethod())
        return new HistoryInfo(this);
    }
  }
}
