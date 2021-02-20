// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSInternalMemberSet
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class PSInternalMemberSet : PSMemberSet
  {
    private object syncObject = new object();
    private PSObject psObject;

    internal PSInternalMemberSet(string propertyName, PSObject psObject)
      : base(propertyName)
    {
      this.internalMembers = (PSMemberInfoInternalCollection<PSMemberInfo>) null;
      this.psObject = psObject;
    }

    internal override PSMemberInfoInternalCollection<PSMemberInfo> InternalMembers
    {
      get
      {
        if (this.name.Equals("psadapted", StringComparison.OrdinalIgnoreCase))
          return this.GetInternalMembersFromAdapted();
        if (this.internalMembers == null)
        {
          lock (this.syncObject)
          {
            if (this.internalMembers == null)
            {
              this.internalMembers = new PSMemberInfoInternalCollection<PSMemberInfo>();
              switch (this.name.ToLowerInvariant())
              {
                case "psbase":
                  this.GenerateInternalMembersFromBase();
                  break;
                case "psobject":
                  this.GenerateInternalMembersFromPSObject();
                  break;
              }
            }
          }
        }
        return this.internalMembers;
      }
    }

    private void GenerateInternalMembersFromBase()
    {
      if (this.psObject.isDeserialized)
      {
        if (this.psObject.clrMembers == null)
          return;
        foreach (PSMemberInfo clrMember in (PSMemberInfoCollection<PSPropertyInfo>) this.psObject.clrMembers)
          this.internalMembers.Add(clrMember.Copy());
      }
      else
      {
        foreach (PSMemberInfo member in (PSMemberInfoCollection<PSMemberInfo>) PSObject.dotNetInstanceAdapter.BaseGetMembers<PSMemberInfo>(this.psObject.ImmediateBaseObject))
          this.internalMembers.Add(member.Copy());
      }
    }

    private PSMemberInfoInternalCollection<PSMemberInfo> GetInternalMembersFromAdapted()
    {
      PSMemberInfoInternalCollection<PSMemberInfo> internalCollection = new PSMemberInfoInternalCollection<PSMemberInfo>();
      if (this.psObject.isDeserialized)
      {
        if (this.psObject.adaptedMembers != null)
        {
          foreach (PSMemberInfo adaptedMember in (PSMemberInfoCollection<PSPropertyInfo>) this.psObject.adaptedMembers)
            internalCollection.Add(adaptedMember.Copy());
        }
      }
      else
      {
        foreach (PSMemberInfo member in (PSMemberInfoCollection<PSMemberInfo>) this.psObject.InternalAdapter.BaseGetMembers<PSMemberInfo>(this.psObject.ImmediateBaseObject))
          internalCollection.Add(member.Copy());
      }
      return internalCollection;
    }

    private void GenerateInternalMembersFromPSObject()
    {
      foreach (PSMemberInfo member in (PSMemberInfoCollection<PSMemberInfo>) PSObject.dotNetInstanceAdapter.BaseGetMembers<PSMemberInfo>((object) this.psObject))
        this.internalMembers.Add(member.Copy());
    }
  }
}
