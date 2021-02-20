// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ScopeTracer
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Text;
using System.Threading;

namespace System.Management.Automation
{
  internal class ScopeTracer : IDisposable
  {
    private PSTraceSource _tracer;
    private PSTraceSourceOptions _flag;
    private string _scopeName;
    private string _leavingScopeFormatter;

    internal ScopeTracer(
      PSTraceSource tracer,
      PSTraceSourceOptions flag,
      string scopeOutputFormatter,
      string leavingScopeFormatter,
      string scopeName)
    {
      this._tracer = tracer;
      this.ScopeTracerHelper(flag, scopeOutputFormatter, leavingScopeFormatter, scopeName, "");
    }

    internal ScopeTracer(
      PSTraceSource tracer,
      PSTraceSourceOptions flag,
      string scopeOutputFormatter,
      string leavingScopeFormatter,
      string scopeName,
      string format,
      params object[] args)
    {
      this._tracer = tracer;
      if (format != null)
        this.ScopeTracerHelper(flag, scopeOutputFormatter, leavingScopeFormatter, scopeName, format, args);
      else
        this.ScopeTracerHelper(flag, scopeOutputFormatter, leavingScopeFormatter, scopeName, "");
    }

    internal void ScopeTracerHelper(
      PSTraceSourceOptions flag,
      string scopeOutputFormatter,
      string leavingScopeFormatter,
      string scopeName,
      string format,
      params object[] args)
    {
      this._flag = flag;
      this._scopeName = scopeName;
      this._leavingScopeFormatter = leavingScopeFormatter;
      StringBuilder stringBuilder = new StringBuilder();
      if (!string.IsNullOrEmpty(scopeOutputFormatter))
        stringBuilder.AppendFormat((IFormatProvider) Thread.CurrentThread.CurrentCulture, scopeOutputFormatter, (object) this._scopeName);
      if (!string.IsNullOrEmpty(format))
        stringBuilder.AppendFormat((IFormatProvider) Thread.CurrentThread.CurrentCulture, format, args);
      this._tracer.OutputLine(this._flag, stringBuilder.ToString());
      ++PSTraceSource.ThreadIndentLevel;
    }

    public void Dispose()
    {
      --PSTraceSource.ThreadIndentLevel;
      if (!string.IsNullOrEmpty(this._leavingScopeFormatter))
        this._tracer.OutputLine(this._flag, this._leavingScopeFormatter, (object) this._scopeName);
      GC.SuppressFinalize((object) this);
    }
  }
}
