// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.GettingValueExceptionEventArgs
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public class GettingValueExceptionEventArgs : EventArgs
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    private bool _shouldThrow;
    private Exception _exception;
    private object _valueReplacement;

    public bool ShouldThrow
    {
      get => this._shouldThrow;
      set => this._shouldThrow = value;
    }

    public Exception Exception => this._exception;

    internal GettingValueExceptionEventArgs(Exception exception)
    {
      this._exception = exception;
      this._valueReplacement = (object) null;
      this._shouldThrow = true;
    }

    public object ValueReplacement
    {
      get => this._valueReplacement;
      set => this._valueReplacement = value;
    }
  }
}
