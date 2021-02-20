// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.FunctionScopeItemSearcher
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class FunctionScopeItemSearcher : ScopedItemSearcher<FunctionInfo>
  {
    private CommandOrigin _origin;
    private string name = string.Empty;

    public FunctionScopeItemSearcher(
      SessionStateInternal sessionState,
      ScopedItemLookupPath lookupPath,
      CommandOrigin origin)
      : base(sessionState, lookupPath)
    {
      this._origin = origin;
    }

    protected override bool GetScopeItem(
      SessionStateScope scope,
      ScopedItemLookupPath path,
      out FunctionInfo script)
    {
      bool flag = true;
      this.name = path.ToString();
      if (path.IsScopedItem)
        this.name = path.LookupPath.NamespaceSpecificString;
      script = scope.GetFunction(this.name);
      if (script != null)
      {
        if ((!(script is FilterInfo filterInfo) ? (script.Options & ScopedItemOptions.Private) != ScopedItemOptions.None : (filterInfo.Options & ScopedItemOptions.Private) != ScopedItemOptions.None) && scope != this.sessionState.CurrentScope)
          flag = false;
        else
          SessionState.ThrowIfNotVisible(this._origin, (object) script);
      }
      else
        flag = false;
      return flag;
    }

    internal string Name => this.name;
  }
}
