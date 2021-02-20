// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DriveScopeItemSearcher
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class DriveScopeItemSearcher : ScopedItemSearcher<PSDriveInfo>
  {
    public DriveScopeItemSearcher(
      SessionStateInternal sessionState,
      ScopedItemLookupPath lookupPath)
      : base(sessionState, lookupPath)
    {
    }

    protected override bool GetScopeItem(
      SessionStateScope scope,
      ScopedItemLookupPath name,
      out PSDriveInfo drive)
    {
      bool flag = true;
      drive = scope.GetDrive(name.LookupPath.NamespaceID);
      if (drive == (PSDriveInfo) null)
        flag = false;
      return flag;
    }
  }
}
