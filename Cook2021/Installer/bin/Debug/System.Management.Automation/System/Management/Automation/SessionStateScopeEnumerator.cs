// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SessionStateScopeEnumerator
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;

namespace System.Management.Automation
{
  internal sealed class SessionStateScopeEnumerator : 
    IEnumerator<SessionStateScope>,
    IDisposable,
    IEnumerator,
    IEnumerable<SessionStateScope>,
    IEnumerable
  {
    [TraceSource("SessionStateScope", "A scope of session state that holds virtual drives")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("SessionStateScope", "A scope of session state that holds virtual drives");
    private SessionStateInternal _sessionState;
    private SessionStateScope _initialScope;
    private SessionStateScope _currentEnumeratedScope;

    internal SessionStateScopeEnumerator(SessionStateInternal sessionState, SessionStateScope scope)
    {
      if (sessionState == null)
        throw SessionStateScopeEnumerator.tracer.NewArgumentNullException(nameof (sessionState));
      if (scope == null)
        throw SessionStateScopeEnumerator.tracer.NewArgumentNullException(nameof (scope));
      this._sessionState = sessionState;
      this._initialScope = scope;
    }

    public bool MoveNext()
    {
      this._currentEnumeratedScope = this._currentEnumeratedScope != null ? this._currentEnumeratedScope.Parent : this._initialScope;
      bool flag = true;
      if (this._currentEnumeratedScope == null)
        flag = false;
      return flag;
    }

    public void Reset() => this._currentEnumeratedScope = (SessionStateScope) null;

    SessionStateScope IEnumerator<SessionStateScope>.Current => this._currentEnumeratedScope != null ? this._currentEnumeratedScope : throw SessionStateScopeEnumerator.tracer.NewInvalidOperationException();

    object IEnumerator.Current => (object) ((IEnumerator<SessionStateScope>) this).Current;

    IEnumerator<SessionStateScope> IEnumerable<SessionStateScope>.GetEnumerator() => (IEnumerator<SessionStateScope>) this;

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this;

    public void Dispose() => this.Reset();
  }
}
