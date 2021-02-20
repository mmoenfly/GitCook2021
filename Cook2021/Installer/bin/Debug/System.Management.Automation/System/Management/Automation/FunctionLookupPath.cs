// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.FunctionLookupPath
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class FunctionLookupPath : ScopedItemLookupPath
  {
    internal FunctionLookupPath(string path)
    {
      this.userPath = path;
      UniversalResourceName universalResourceName = new UniversalResourceName(path);
      if (string.Equals(universalResourceName.NamespaceID, "GLOBAL", StringComparison.OrdinalIgnoreCase))
      {
        this.isGlobalLookup = true;
        this.isScopedItem = true;
      }
      else if (string.Equals(universalResourceName.NamespaceID, "LOCAL", StringComparison.OrdinalIgnoreCase))
      {
        this.isLocalLookup = true;
        this.isScopedItem = true;
      }
      else if (string.Equals(universalResourceName.NamespaceID, "Variable", StringComparison.OrdinalIgnoreCase))
        this.isScopedItem = true;
      else if (string.Equals(universalResourceName.NamespaceID, "PRIVATE", StringComparison.OrdinalIgnoreCase))
      {
        this.isPrivateLookup = true;
        this.isScopedItem = true;
      }
      else if (string.Equals(universalResourceName.NamespaceID, "SCRIPT", StringComparison.OrdinalIgnoreCase))
      {
        this.isScopedItem = true;
        this.isScriptLookup = true;
      }
      else
      {
        this.isUnqualified = true;
        this.isScopedItem = true;
        universalResourceName = new UniversalResourceName();
        universalResourceName.NamespaceSpecificString = path;
      }
      this.modifiedPath = universalResourceName;
    }
  }
}
