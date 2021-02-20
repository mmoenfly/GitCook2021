// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ScopedItemLookupPath
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class ScopedItemLookupPath
  {
    protected string userPath = string.Empty;
    protected UniversalResourceName modifiedPath;
    protected bool isGlobalLookup;
    protected bool isLocalLookup;
    protected bool isScriptLookup;
    protected bool isUnqualified;
    protected bool isScopedItem;
    protected bool isPrivateLookup;

    internal ScopedItemLookupPath()
    {
    }

    internal ScopedItemLookupPath(string path)
    {
      this.userPath = path != null ? path : throw new ArgumentNullException(nameof (path));
      UniversalResourceName universalResourceName = new UniversalResourceName(path);
      if (string.Equals(universalResourceName.NamespaceID, "GLOBAL", StringComparison.OrdinalIgnoreCase))
      {
        this.isGlobalLookup = true;
        this.isScopedItem = true;
        this.modifiedPath = new UniversalResourceName(universalResourceName.NamespaceSpecificString);
      }
      else if (string.Equals(universalResourceName.NamespaceID, "LOCAL", StringComparison.OrdinalIgnoreCase))
      {
        this.isLocalLookup = true;
        this.isScopedItem = true;
        this.modifiedPath = new UniversalResourceName(universalResourceName.NamespaceSpecificString);
      }
      else if (string.Equals(universalResourceName.NamespaceID, "Variable", StringComparison.OrdinalIgnoreCase))
      {
        this.isScopedItem = true;
        this.modifiedPath = new UniversalResourceName(universalResourceName.NamespaceSpecificString);
      }
      else if (string.Equals(universalResourceName.NamespaceID, "PRIVATE", StringComparison.OrdinalIgnoreCase))
      {
        this.isPrivateLookup = true;
        this.isScopedItem = true;
        this.modifiedPath = new UniversalResourceName(universalResourceName.NamespaceSpecificString);
      }
      else if (string.Equals(universalResourceName.NamespaceID, "SCRIPT", StringComparison.OrdinalIgnoreCase))
      {
        this.isScopedItem = true;
        this.isScriptLookup = true;
        this.modifiedPath = new UniversalResourceName(universalResourceName.NamespaceSpecificString);
      }
      else
      {
        this.isUnqualified = true;
        if (string.IsNullOrEmpty(universalResourceName.NamespaceID))
          this.isScopedItem = true;
        this.modifiedPath = universalResourceName;
      }
    }

    internal ScopedItemLookupPath(ScopedItemLookupPath silp, bool isLocal)
    {
      if (isLocal)
      {
        this.isUnqualified = false;
        this.isLocalLookup = true;
      }
      else
        this.isUnqualified = silp.isUnqualified;
      this.isGlobalLookup = silp.IsGlobal;
      this.isPrivateLookup = silp.isPrivateLookup;
      this.isScopedItem = silp.isScopedItem;
      this.isScriptLookup = silp.isScriptLookup;
      this.modifiedPath = silp.modifiedPath;
      this.userPath = silp.userPath;
    }

    internal string UserPath => this.userPath;

    internal UniversalResourceName LookupPath => this.modifiedPath;

    internal bool IsGlobal => this.isGlobalLookup;

    internal bool IsLocal => this.isLocalLookup;

    internal bool IsPrivate => this.isPrivateLookup;

    internal bool IsScript => this.isScriptLookup;

    internal bool IsUnqualified => this.isUnqualified;

    internal bool IsScopedItem
    {
      get => this.isScopedItem;
      set => this.isScopedItem = value;
    }

    public override string ToString() => this.modifiedPath.ToString();
  }
}
