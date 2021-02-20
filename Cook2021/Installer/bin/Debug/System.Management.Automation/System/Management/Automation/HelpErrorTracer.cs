// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.HelpErrorTracer
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Management.Automation
{
  internal class HelpErrorTracer
  {
    private HelpSystem _helpSystem;
    private ArrayList _traceFrames = new ArrayList();
    [TraceSource("HelpSystem", "HelpSystem")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (HelpSystem), nameof (HelpSystem));

    internal HelpSystem HelpSystem => this._helpSystem;

    internal HelpErrorTracer(HelpSystem helpSystem) => this._helpSystem = helpSystem != null ? helpSystem : throw HelpErrorTracer.tracer.NewArgumentNullException(nameof (HelpSystem));

    internal IDisposable Trace(string helpFile)
    {
      HelpErrorTracer.TraceFrame traceFrame = new HelpErrorTracer.TraceFrame(this, helpFile);
      this._traceFrames.Add((object) traceFrame);
      return (IDisposable) traceFrame;
    }

    internal void TraceError(ErrorRecord errorRecord)
    {
      if (this._traceFrames.Count <= 0)
        return;
      ((HelpErrorTracer.TraceFrame) this._traceFrames[this._traceFrames.Count - 1]).TraceError(errorRecord);
    }

    internal void TraceErrors(Collection<ErrorRecord> errorRecords)
    {
      if (this._traceFrames.Count <= 0)
        return;
      ((HelpErrorTracer.TraceFrame) this._traceFrames[this._traceFrames.Count - 1]).TraceErrors(errorRecords);
    }

    internal void PopFrame(HelpErrorTracer.TraceFrame traceFrame)
    {
      if (this._traceFrames.Count <= 0 || (HelpErrorTracer.TraceFrame) this._traceFrames[this._traceFrames.Count - 1] != traceFrame)
        return;
      this._traceFrames.RemoveAt(this._traceFrames.Count - 1);
    }

    internal bool IsOn => this._traceFrames.Count > 0 && this.HelpSystem.VerboseHelpErrors;

    internal sealed class TraceFrame : IDisposable
    {
      private string _helpFile = "";
      private Collection<ErrorRecord> _errors = new Collection<ErrorRecord>();
      private HelpErrorTracer _helpTracer;

      internal TraceFrame(HelpErrorTracer helpTracer, string helpFile)
      {
        this._helpTracer = helpTracer;
        this._helpFile = helpFile;
      }

      internal void TraceError(ErrorRecord errorRecord)
      {
        if (!this._helpTracer.HelpSystem.VerboseHelpErrors)
          return;
        this._errors.Add(errorRecord);
      }

      internal void TraceErrors(Collection<ErrorRecord> errorRecords)
      {
        if (!this._helpTracer.HelpSystem.VerboseHelpErrors)
          return;
        foreach (ErrorRecord errorRecord in errorRecords)
          this._errors.Add(errorRecord);
      }

      public void Dispose()
      {
        if (this._helpTracer.HelpSystem.VerboseHelpErrors && this._errors.Count > 0)
        {
          this._helpTracer.HelpSystem.LastErrors.Add(new ErrorRecord((Exception) new ParentContainsErrorRecordException("Help Load Error"), "HelpLoadError", ErrorCategory.SyntaxError, (object) null)
          {
            ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "HelpLoadError", new object[2]
            {
              (object) this._helpFile,
              (object) this._errors.Count
            })
          });
          foreach (ErrorRecord error in this._errors)
            this._helpTracer.HelpSystem.LastErrors.Add(error);
        }
        this._helpTracer.PopFrame(this);
      }
    }
  }
}
