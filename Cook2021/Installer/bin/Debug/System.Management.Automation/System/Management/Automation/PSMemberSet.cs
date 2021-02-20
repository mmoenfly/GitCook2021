// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSMemberSet
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace System.Management.Automation
{
  public class PSMemberSet : PSMemberInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    private PSMemberInfoIntegratingCollection<PSMemberInfo> members;
    private PSMemberInfoIntegratingCollection<PSPropertyInfo> properties;
    private PSMemberInfoIntegratingCollection<PSMethodInfo> methods;
    internal PSMemberInfoInternalCollection<PSMemberInfo> internalMembers;
    private PSObject constructorPSObject;
    private static Collection<CollectionEntry<PSMemberInfo>> emptyMemberCollection = new Collection<CollectionEntry<PSMemberInfo>>();
    private static Collection<CollectionEntry<PSMethodInfo>> emptyMethodCollection = new Collection<CollectionEntry<PSMethodInfo>>();
    private static Collection<CollectionEntry<PSPropertyInfo>> emptyPropertyCollection = new Collection<CollectionEntry<PSPropertyInfo>>();
    private static Collection<CollectionEntry<PSMemberInfo>> typeMemberCollection = PSMemberSet.GetTypeMemberCollection();
    private static Collection<CollectionEntry<PSMethodInfo>> typeMethodCollection = PSMemberSet.GetTypeMethodCollection();
    private static Collection<CollectionEntry<PSPropertyInfo>> typePropertyCollection = PSMemberSet.GetTypePropertyCollection();
    internal bool inheritMembers = true;

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(" {");
      foreach (PSMemberInfo member in this.Members)
      {
        stringBuilder.Append(member.Name);
        stringBuilder.Append(", ");
      }
      if (stringBuilder.Length > 2)
        stringBuilder.Remove(stringBuilder.Length - 2, 2);
      stringBuilder.Insert(0, this.Name);
      stringBuilder.Append("}");
      return stringBuilder.ToString();
    }

    public PSMemberSet(string name)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw PSMemberSet.tracer.NewArgumentException(nameof (name));
      this.internalMembers = new PSMemberInfoInternalCollection<PSMemberInfo>();
      this.members = new PSMemberInfoIntegratingCollection<PSMemberInfo>((object) this, PSMemberSet.emptyMemberCollection);
      this.properties = new PSMemberInfoIntegratingCollection<PSPropertyInfo>((object) this, PSMemberSet.emptyPropertyCollection);
      this.methods = new PSMemberInfoIntegratingCollection<PSMethodInfo>((object) this, PSMemberSet.emptyMethodCollection);
    }

    public PSMemberSet(string name, IEnumerable<PSMemberInfo> members)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw PSMemberSet.tracer.NewArgumentException(nameof (name));
      if (members == null)
        throw PSMemberSet.tracer.NewArgumentNullException(nameof (members));
      this.internalMembers = new PSMemberInfoInternalCollection<PSMemberInfo>();
      foreach (PSMemberInfo member in members)
      {
        if (member == null)
          throw PSMemberSet.tracer.NewArgumentNullException(nameof (members));
        this.internalMembers.Add(member.Copy());
      }
      this.members = new PSMemberInfoIntegratingCollection<PSMemberInfo>((object) this, PSMemberSet.emptyMemberCollection);
      this.properties = new PSMemberInfoIntegratingCollection<PSPropertyInfo>((object) this, PSMemberSet.emptyPropertyCollection);
      this.methods = new PSMemberInfoIntegratingCollection<PSMethodInfo>((object) this, PSMemberSet.emptyMethodCollection);
    }

    private static Collection<CollectionEntry<PSMemberInfo>> GetTypeMemberCollection() => new Collection<CollectionEntry<PSMemberInfo>>()
    {
      new CollectionEntry<PSMemberInfo>(new CollectionEntry<PSMemberInfo>.GetMembersDelegate(PSObject.TypeTableGetMembersDelegate<PSMemberInfo>), new CollectionEntry<PSMemberInfo>.GetMemberDelegate(PSObject.TypeTableGetMemberDelegate<PSMemberInfo>), true, true, "type table members")
    };

    private static Collection<CollectionEntry<PSMethodInfo>> GetTypeMethodCollection() => new Collection<CollectionEntry<PSMethodInfo>>()
    {
      new CollectionEntry<PSMethodInfo>(new CollectionEntry<PSMethodInfo>.GetMembersDelegate(PSObject.TypeTableGetMembersDelegate<PSMethodInfo>), new CollectionEntry<PSMethodInfo>.GetMemberDelegate(PSObject.TypeTableGetMemberDelegate<PSMethodInfo>), true, true, "type table members")
    };

    private static Collection<CollectionEntry<PSPropertyInfo>> GetTypePropertyCollection() => new Collection<CollectionEntry<PSPropertyInfo>>()
    {
      new CollectionEntry<PSPropertyInfo>(new CollectionEntry<PSPropertyInfo>.GetMembersDelegate(PSObject.TypeTableGetMembersDelegate<PSPropertyInfo>), new CollectionEntry<PSPropertyInfo>.GetMemberDelegate(PSObject.TypeTableGetMemberDelegate<PSPropertyInfo>), true, true, "type table members")
    };

    internal PSMemberSet(string name, PSObject mshObject)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw PSMemberSet.tracer.NewArgumentException(nameof (name));
      this.constructorPSObject = mshObject != null ? mshObject : throw PSMemberSet.tracer.NewArgumentNullException(nameof (mshObject));
      this.internalMembers = mshObject.InstanceMembers;
      this.members = new PSMemberInfoIntegratingCollection<PSMemberInfo>((object) this, PSMemberSet.typeMemberCollection);
      this.properties = new PSMemberInfoIntegratingCollection<PSPropertyInfo>((object) this, PSMemberSet.typePropertyCollection);
      this.methods = new PSMemberInfoIntegratingCollection<PSMethodInfo>((object) this, PSMemberSet.typeMethodCollection);
    }

    public bool InheritMembers => this.inheritMembers;

    internal virtual PSMemberInfoInternalCollection<PSMemberInfo> InternalMembers => this.internalMembers;

    public PSMemberInfoCollection<PSMemberInfo> Members => (PSMemberInfoCollection<PSMemberInfo>) this.members;

    public PSMemberInfoCollection<PSPropertyInfo> Properties => (PSMemberInfoCollection<PSPropertyInfo>) this.properties;

    public PSMemberInfoCollection<PSMethodInfo> Methods => (PSMemberInfoCollection<PSMethodInfo>) this.methods;

    public override PSMemberInfo Copy()
    {
      if (this.constructorPSObject != null)
        return (PSMemberInfo) new PSMemberSet(this.name, this.constructorPSObject);
      PSMemberSet psMemberSet = new PSMemberSet(this.name);
      foreach (PSMemberInfo member in this.Members)
        psMemberSet.Members.Add(member);
      this.CloneBaseProperties((PSMemberInfo) psMemberSet);
      return (PSMemberInfo) psMemberSet;
    }

    public override PSMemberTypes MemberType => PSMemberTypes.MemberSet;

    public override object Value
    {
      get => (object) this;
      set => throw new ExtendedTypeSystemException("CannotChangePSMemberSetValue", (Exception) null, "ExtendedTypeSystem", "CannotSetValueForMemberType", new object[1]
      {
        (object) this.GetType().FullName
      });
    }

    public override string TypeNameOfValue => typeof (PSMemberSet).FullName;
  }
}
