// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSMemberInfoIntegratingCollection`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace System.Management.Automation
{
  internal class PSMemberInfoIntegratingCollection<T> : 
    PSMemberInfoCollection<T>,
    IEnumerable<T>,
    IEnumerable
    where T : PSMemberInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    private static MethodInfo typeNamesMethodInfo = typeof (MshTypeNamesClass).GetMethod("MshTypeNames");
    private Collection<CollectionEntry<T>> collections;
    private PSObject mshOwner;
    private PSMemberSet memberSetOwner;

    private void GenerateBaseMemberSet()
    {
      if (this.mshOwner.InstanceMembers["psbase"] != null)
        return;
      PSMemberSet psMemberSet = (PSMemberSet) new PSInternalMemberSet("psbase", this.mshOwner);
      psMemberSet.ShouldSerialize = false;
      psMemberSet.isHidden = true;
      psMemberSet.isReservedMember = true;
      this.mshOwner.InstanceMembers.Add((PSMemberInfo) psMemberSet);
    }

    private void GenerateAdaptedMemberSet()
    {
      if (this.mshOwner.InstanceMembers["psadapted"] != null)
        return;
      PSMemberSet psMemberSet = (PSMemberSet) new PSInternalMemberSet("psadapted", this.mshOwner);
      psMemberSet.ShouldSerialize = false;
      psMemberSet.isHidden = true;
      psMemberSet.isReservedMember = true;
      this.mshOwner.InstanceMembers.Add((PSMemberInfo) psMemberSet);
    }

    private void GeneratePSObjectMemberSet()
    {
      if (this.mshOwner.InstanceMembers["psobject"] != null)
        return;
      PSMemberSet psMemberSet = (PSMemberSet) new PSInternalMemberSet("psobject", this.mshOwner);
      psMemberSet.ShouldSerialize = false;
      psMemberSet.isHidden = true;
      psMemberSet.isReservedMember = true;
      this.mshOwner.InstanceMembers.Add((PSMemberInfo) psMemberSet);
    }

    private void GenerateExtendedMemberSet()
    {
      if (this.mshOwner.InstanceMembers["psextended"] != null)
        return;
      PSMemberSet psMemberSet = new PSMemberSet("psextended", this.mshOwner);
      psMemberSet.ReplicateInstance(this.mshOwner);
      psMemberSet.ShouldSerialize = false;
      psMemberSet.isHidden = true;
      psMemberSet.isReservedMember = true;
      this.mshOwner.InstanceMembers.Add((PSMemberInfo) psMemberSet);
    }

    private void GenerateMshTypeNames()
    {
      if (this.mshOwner.InstanceMembers["pstypenames"] != null)
        return;
      PSCodeProperty psCodeProperty = new PSCodeProperty("pstypenames", PSMemberInfoIntegratingCollection<T>.typeNamesMethodInfo);
      psCodeProperty.shouldSerialize = false;
      psCodeProperty.instance = this.mshOwner;
      psCodeProperty.isHidden = true;
      psCodeProperty.isReservedMember = true;
      this.mshOwner.InstanceMembers.Add((PSMemberInfo) psCodeProperty);
    }

    private void GenerateAllReservedMembers()
    {
      if (this.mshOwner.hasGeneratedReservedMembers)
        return;
      this.mshOwner.hasGeneratedReservedMembers = true;
      this.GenerateExtendedMemberSet();
      this.GenerateBaseMemberSet();
      this.GeneratePSObjectMemberSet();
      this.GenerateAdaptedMemberSet();
      this.GenerateMshTypeNames();
    }

    internal Collection<CollectionEntry<T>> Collections => this.collections;

    internal PSMemberInfoIntegratingCollection(
      object owner,
      Collection<CollectionEntry<T>> collections)
    {
      this.mshOwner = owner != null ? owner as PSObject : throw PSMemberInfoIntegratingCollection<T>.tracer.NewArgumentNullException(nameof (owner));
      this.memberSetOwner = owner as PSMemberSet;
      if (this.mshOwner == null && this.memberSetOwner == null)
        throw PSMemberInfoIntegratingCollection<T>.tracer.NewArgumentException(nameof (owner));
      this.collections = collections != null ? collections : throw PSMemberInfoIntegratingCollection<T>.tracer.NewArgumentNullException(nameof (collections));
    }

    public override void Add(T member) => this.Add(member, false);

    public override void Add(T member, bool preValidated)
    {
      if ((object) member == null)
        throw PSMemberInfoIntegratingCollection<T>.tracer.NewArgumentNullException(nameof (member));
      if (!preValidated)
      {
        if (member.MemberType == PSMemberTypes.Property || member.MemberType == PSMemberTypes.Method)
          throw new ExtendedTypeSystemException("CannotAddMethodOrProperty", (Exception) null, "ExtendedTypeSystem", "CannotAddPropertyOrMethod", new object[0]);
        if (this.memberSetOwner != null && this.memberSetOwner.IsReservedMember)
          throw new ExtendedTypeSystemException("CannotAddToReservedNameMemberset", (Exception) null, "ExtendedTypeSystem", "CannotChangeReservedMember", new object[1]
          {
            (object) this.memberSetOwner.Name
          });
      }
      this.AddToReservedMemberSet(member, preValidated);
    }

    internal void AddToReservedMemberSet(T member, bool preValidated)
    {
      if (!preValidated && this.memberSetOwner != null && !this.memberSetOwner.IsInstance)
        throw new ExtendedTypeSystemException("RemoveMemberFromStaticMemberSet", (Exception) null, "ExtendedTypeSystem", "ChangeStaticMember", new object[1]
        {
          (object) member.Name
        });
      this.AddToTypesXmlCache(member, preValidated);
    }

    internal void AddToTypesXmlCache(T member, bool preValidated)
    {
      if ((object) member == null)
        throw PSMemberInfoIntegratingCollection<T>.tracer.NewArgumentNullException(nameof (member));
      if (!preValidated && PSMemberInfoCollection<T>.IsReservedName(member.Name))
        throw new ExtendedTypeSystemException("PSObjectMembersMembersAddReservedName", (Exception) null, "ExtendedTypeSystem", "ReservedMemberName", new object[1]
        {
          (object) member.Name
        });
      PSMemberInfo member1 = member.Copy();
      if (this.mshOwner != null)
      {
        if (!preValidated)
        {
          TypeTable typeTable = this.mshOwner.GetTypeTable();
          if (typeTable != null && (object) typeTable.GetMembers<T>(this.mshOwner.InternalTypeNames)[member.Name] != null)
            throw new ExtendedTypeSystemException("AlreadyPresentInTypesXml", (Exception) null, "ExtendedTypeSystem", "MemberAlreadyPresentFromTypesXml", new object[1]
            {
              (object) member.Name
            });
        }
        member1.ReplicateInstance(this.mshOwner);
        this.mshOwner.InstanceMembers.Add(member1, preValidated);
      }
      else
        this.memberSetOwner.InternalMembers.Add(member1, preValidated);
    }

    public override void Remove(string name)
    {
      if (string.IsNullOrEmpty(name))
        throw PSMemberInfoIntegratingCollection<T>.tracer.NewArgumentException(nameof (name));
      if (this.mshOwner != null)
      {
        this.mshOwner.InstanceMembers.Remove(name);
      }
      else
      {
        if (!this.memberSetOwner.IsInstance)
          throw new ExtendedTypeSystemException("AddMemberToStaticMemberSet", (Exception) null, "ExtendedTypeSystem", "ChangeStaticMember", new object[1]
          {
            (object) name
          });
        if (PSMemberInfoCollection<T>.IsReservedName(this.memberSetOwner.Name))
          throw new ExtendedTypeSystemException("CannotRemoveFromReservedNameMemberset", (Exception) null, "ExtendedTypeSystem", "CannotChangeReservedMember", new object[1]
          {
            (object) this.memberSetOwner.Name
          });
        this.memberSetOwner.InternalMembers.Remove(name);
      }
    }

    private void EnsureReservedMemberIsLoaded(string name)
    {
      switch (name.ToLowerInvariant())
      {
        case "psbase":
          this.GenerateBaseMemberSet();
          break;
        case "psadapted":
          this.GenerateAdaptedMemberSet();
          break;
        case "psextended":
          this.GenerateExtendedMemberSet();
          break;
        case "psobject":
          this.GeneratePSObjectMemberSet();
          break;
        case "pstypenames":
          this.GenerateMshTypeNames();
          break;
      }
    }

    public override T this[string name]
    {
      get
      {
        using (PSObject.memberResolution.TraceScope("Lookup"))
        {
          if (string.IsNullOrEmpty(name))
            throw PSMemberInfoIntegratingCollection<T>.tracer.NewArgumentException(nameof (name));
          PSObject particularInstance;
          if (this.mshOwner != null)
          {
            this.EnsureReservedMemberIsLoaded(name);
            PSMemberInfo instanceMember = this.mshOwner.InstanceMembers[name];
            particularInstance = this.mshOwner;
            if (instanceMember is T obj)
            {
              PSObject.memberResolution.WriteLine("Found PSObject instance member: {0}.", (object) name);
              return obj;
            }
          }
          else
          {
            PSMemberInfo internalMember = this.memberSetOwner.InternalMembers[name];
            particularInstance = this.memberSetOwner.instance;
            if (internalMember is T obj)
            {
              PSObject.memberResolution.WriteLine("Found PSMemberSet member: {0}.", (object) name);
              internalMember.ReplicateInstance(particularInstance);
              return obj;
            }
          }
          foreach (CollectionEntry<T> collection in this.collections)
          {
            T obj = collection.GetMember(particularInstance, name);
            if ((object) obj != null)
            {
              if (collection.ShouldCloneWhenReturning)
                obj = (T) obj.Copy();
              if (collection.ShouldReplicateWhenReturning)
                obj.ReplicateInstance(particularInstance);
              return obj;
            }
          }
          return default (T);
        }
      }
    }

    private PSMemberInfoInternalCollection<T> GetIntegratedMembers(
      MshMemberMatchOptions matchOptions)
    {
      using (PSObject.memberResolution.TraceScope("Generating the total list of members"))
      {
        PSMemberInfoInternalCollection<T> internalCollection = new PSMemberInfoInternalCollection<T>();
        PSObject particularInstance;
        if (this.mshOwner != null)
        {
          particularInstance = this.mshOwner;
          foreach (PSMemberInfo instanceMember in (PSMemberInfoCollection<PSMemberInfo>) this.mshOwner.InstanceMembers)
          {
            if (instanceMember.MatchesOptions(matchOptions) && instanceMember is T member)
              internalCollection.Add(member);
          }
        }
        else
        {
          particularInstance = this.memberSetOwner.instance;
          foreach (PSMemberInfo internalMember in (PSMemberInfoCollection<PSMemberInfo>) this.memberSetOwner.InternalMembers)
          {
            if (internalMember.MatchesOptions(matchOptions) && internalMember is T member)
            {
              internalMember.ReplicateInstance(particularInstance);
              internalCollection.Add(member);
            }
          }
        }
        foreach (CollectionEntry<T> collection in this.collections)
        {
          foreach (T obj in (PSMemberInfoCollection<T>) collection.GetMembers(particularInstance))
          {
            PSMemberInfo psMemberInfo = (PSMemberInfo) internalCollection[obj.Name];
            if (psMemberInfo != null)
              PSObject.memberResolution.WriteLine("Member \"{0}\" of type \"{1}\" has been ignored because a member with the same name and type \"{2}\" is already present.", (object) obj.Name, (object) obj.MemberType, (object) psMemberInfo.MemberType);
            else if (!obj.MatchesOptions(matchOptions))
            {
              PSObject.memberResolution.WriteLine("Skipping hidden member \"{0}\".", (object) obj.Name);
            }
            else
            {
              T member = !collection.ShouldCloneWhenReturning ? obj : (T) obj.Copy();
              if (collection.ShouldReplicateWhenReturning)
                member.ReplicateInstance(particularInstance);
              internalCollection.Add(member);
            }
          }
        }
        return internalCollection;
      }
    }

    public override ReadOnlyPSMemberInfoCollection<T> Match(
      string name)
    {
      return !string.IsNullOrEmpty(name) ? this.Match(name, PSMemberTypes.All, MshMemberMatchOptions.None) : throw PSMemberInfoIntegratingCollection<T>.tracer.NewArgumentException(nameof (name));
    }

    public override ReadOnlyPSMemberInfoCollection<T> Match(
      string name,
      PSMemberTypes memberTypes)
    {
      return !string.IsNullOrEmpty(name) ? this.Match(name, memberTypes, MshMemberMatchOptions.None) : throw PSMemberInfoIntegratingCollection<T>.tracer.NewArgumentException(nameof (name));
    }

    internal override ReadOnlyPSMemberInfoCollection<T> Match(
      string name,
      PSMemberTypes memberTypes,
      MshMemberMatchOptions matchOptions)
    {
      using (PSObject.memberResolution.TraceScope("Matching \"{0}\"", (object) name))
      {
        if (string.IsNullOrEmpty(name))
          throw PSMemberInfoIntegratingCollection<T>.tracer.NewArgumentException(nameof (name));
        if (this.mshOwner != null)
          this.GenerateAllReservedMembers();
        WildcardPattern namePattern = MemberMatch.GetNamePattern(name);
        ReadOnlyPSMemberInfoCollection<T> memberInfoCollection = new ReadOnlyPSMemberInfoCollection<T>(MemberMatch.Match<T>(this.GetIntegratedMembers(matchOptions), name, namePattern, memberTypes));
        PSObject.memberResolution.WriteLine("{0} total matches.", (object) memberInfoCollection.Count);
        return memberInfoCollection;
      }
    }

    public override IEnumerator<T> GetEnumerator() => (IEnumerator<T>) new PSMemberInfoIntegratingCollection<T>.Enumerator<T>(this);

    internal struct Enumerator<S> : IEnumerator<S>, IDisposable, IEnumerator
      where S : PSMemberInfo
    {
      private S current;
      private int currentIndex;
      private PSMemberInfoInternalCollection<S> allMembers;

      internal Enumerator(
        PSMemberInfoIntegratingCollection<S> integratingCollection)
      {
        using (PSObject.memberResolution.TraceScope("Enumeration Start"))
        {
          this.currentIndex = -1;
          this.current = default (S);
          this.allMembers = integratingCollection.GetIntegratedMembers(MshMemberMatchOptions.None);
          if (integratingCollection.mshOwner != null)
          {
            integratingCollection.GenerateAllReservedMembers();
            PSObject.memberResolution.WriteLine("Enumerating PSObject with type \"{0}\".", (object) integratingCollection.mshOwner.ImmediateBaseObject.GetType().FullName);
            PSObject.memberResolution.WriteLine("PSObject instance members: {0}", (object) (this.allMembers.Count - this.allMembers.countHidden));
          }
          else
          {
            PSObject.memberResolution.WriteLine("Enumerating PSMemberSet \"{0}\".", (object) integratingCollection.memberSetOwner.Name);
            PSObject.memberResolution.WriteLine("MemberSet instance members: {0}", (object) (this.allMembers.Count - this.allMembers.countHidden));
          }
        }
      }

      public bool MoveNext()
      {
        ++this.currentIndex;
        PSMemberInfo psMemberInfo = (PSMemberInfo) null;
        for (; this.currentIndex < this.allMembers.Count; ++this.currentIndex)
        {
          psMemberInfo = (PSMemberInfo) this.allMembers.members[this.currentIndex];
          if (!psMemberInfo.IsHidden)
            break;
        }
        if (this.currentIndex < this.allMembers.Count)
        {
          this.current = psMemberInfo as S;
          return true;
        }
        this.current = default (S);
        return false;
      }

      S IEnumerator<S>.Current
      {
        get
        {
          if (this.currentIndex == -1)
            throw PSMemberInfoIntegratingCollection<T>.tracer.NewInvalidOperationException();
          return this.current;
        }
      }

      object IEnumerator.Current => (object) ((IEnumerator<S>) this).Current;

      void IEnumerator.Reset()
      {
        this.currentIndex = -1;
        this.current = default (S);
      }

      public void Dispose()
      {
      }
    }
  }
}
