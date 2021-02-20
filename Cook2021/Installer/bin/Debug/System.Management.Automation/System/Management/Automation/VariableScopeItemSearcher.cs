// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.VariableScopeItemSearcher
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class VariableScopeItemSearcher : ScopedItemSearcher<PSVariable>
  {
    private CommandOrigin _origin;

    public VariableScopeItemSearcher(
      SessionStateInternal sessionState,
      ScopedItemLookupPath lookupPath,
      CommandOrigin origin)
      : base(sessionState, lookupPath)
    {
      this._origin = origin;
    }

    protected override bool GetScopeItem(
      SessionStateScope scope,
      ScopedItemLookupPath name,
      out PSVariable variable)
    {
      bool flag = true;
      variable = scope.GetVariable(name.LookupPath.ToString(), this._origin);
      if (variable == null || variable.IsPrivate && scope != this.sessionState.CurrentScope)
        flag = false;
      return flag;
    }
  }
}
