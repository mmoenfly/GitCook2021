// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ScopedItemSearcher`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;

namespace System.Management.Automation
{
  internal abstract class ScopedItemSearcher<T> : 
    IEnumerator<T>,
    IDisposable,
    IEnumerator,
    IEnumerable<T>,
    IEnumerable
  {
    [TraceSource("SessionStateScope", "A scope of session state that holds virtual drives")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("SessionStateScope", "A scope of session state that holds virtual drives");
    private SessionStateScope currentScope;
    private SessionStateScope initialScope;
    private T current = default (T);
    protected SessionStateInternal sessionState;
    private ScopedItemLookupPath lookupPath;
    private SessionStateScopeEnumerator scopeEnumerable;
    private bool isSingleScopeLookup;
    private bool isInitialized;

    internal ScopedItemSearcher(SessionStateInternal sessionState, ScopedItemLookupPath lookupPath)
    {
      using (ScopedItemSearcher<T>.tracer.TraceConstructor((object) this))
      {
        if (sessionState == null)
          throw ScopedItemSearcher<T>.tracer.NewArgumentNullException(nameof (sessionState));
        if (lookupPath == null)
          throw ScopedItemSearcher<T>.tracer.NewArgumentNullException(nameof (lookupPath));
        this.sessionState = sessionState;
        this.lookupPath = lookupPath;
        this.InitializeScopeEnumerator();
      }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => (IEnumerator<T>) this;

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this;

    public bool MoveNext()
    {
      bool flag = true;
      if (!this.isInitialized)
        this.InitializeScopeEnumerator();
      while (this.scopeEnumerable.MoveNext())
      {
        T newCurrentItem = default (T);
        if (this.TryGetNewScopeItem(((IEnumerator<SessionStateScope>) this.scopeEnumerable).Current, out newCurrentItem))
        {
          this.currentScope = ((IEnumerator<SessionStateScope>) this.scopeEnumerable).Current;
          this.current = newCurrentItem;
          flag = true;
          break;
        }
        flag = false;
        if (this.isSingleScopeLookup)
        {
          flag = false;
          break;
        }
      }
      return flag;
    }

    T IEnumerator<T>.Current => this.current;

    public object Current => (object) this.current;

    public void Reset() => this.InitializeScopeEnumerator();

    public void Dispose()
    {
      this.current = default (T);
      this.scopeEnumerable.Dispose();
      this.scopeEnumerable = (SessionStateScopeEnumerator) null;
      this.isInitialized = false;
      GC.SuppressFinalize((object) this);
    }

    protected abstract bool GetScopeItem(
      SessionStateScope scope,
      ScopedItemLookupPath name,
      out T newCurrentItem);

    internal SessionStateScope CurrentLookupScope => this.currentScope;

    internal SessionStateScope InitialScope => this.initialScope;

    private bool TryGetNewScopeItem(SessionStateScope lookupScope, out T newCurrentItem) => this.GetScopeItem(lookupScope, this.lookupPath, out newCurrentItem);

    private void InitializeScopeEnumerator()
    {
      this.initialScope = this.sessionState.CurrentScope;
      if (this.lookupPath.IsGlobal)
      {
        this.initialScope = this.sessionState.GlobalScope;
        this.isSingleScopeLookup = true;
      }
      else if (this.lookupPath.IsLocal || this.lookupPath.IsPrivate)
      {
        this.initialScope = this.sessionState.CurrentScope;
        this.isSingleScopeLookup = true;
      }
      else if (this.lookupPath.IsScript)
      {
        this.initialScope = this.sessionState.ScriptScope;
        this.isSingleScopeLookup = true;
      }
      this.scopeEnumerable = new SessionStateScopeEnumerator(this.sessionState, this.initialScope);
      this.isInitialized = true;
    }
  }
}
