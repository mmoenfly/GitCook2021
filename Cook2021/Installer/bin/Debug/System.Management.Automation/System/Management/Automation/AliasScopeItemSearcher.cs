// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.AliasScopeItemSearcher
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class AliasScopeItemSearcher : ScopedItemSearcher<AliasInfo>
  {
    public AliasScopeItemSearcher(
      SessionStateInternal sessionState,
      ScopedItemLookupPath lookupPath)
      : base(sessionState, lookupPath)
    {
    }

    protected override bool GetScopeItem(
      SessionStateScope scope,
      ScopedItemLookupPath name,
      out AliasInfo alias)
    {
      bool flag = true;
      alias = scope.GetAlias(name.ToString());
      if (alias == null || (alias.Options & ScopedItemOptions.Private) != ScopedItemOptions.None && scope != this.sessionState.CurrentScope)
        flag = false;
      return flag;
    }
  }
}
