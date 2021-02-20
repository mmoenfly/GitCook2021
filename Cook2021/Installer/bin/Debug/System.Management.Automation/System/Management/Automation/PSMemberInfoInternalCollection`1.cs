// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSMemberInfoInternalCollection`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace System.Management.Automation
{
  internal class PSMemberInfoInternalCollection<T> : 
    PSMemberInfoCollection<T>,
    IEnumerable<T>,
    IEnumerable
    where T : PSMemberInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    internal Collection<T> members;
    internal HybridDictionary hashedMembers;
    internal int countHidden;

    internal PSMemberInfoInternalCollection()
    {
      this.members = new Collection<T>();
      this.hashedMembers = new HybridDictionary(true);
    }

    internal void Replace(T newMember) => this.members[((int?) this.hashedMembers[(object) newMember.Name]).Value] = newMember;

    public override void Add(T member) => this.Add(member, false);

    public override void Add(T member, bool preValidated)
    {
      if ((object) member == null)
        throw PSMemberInfoInternalCollection<T>.tracer.NewArgumentNullException(nameof (member));
      int? nullable = new int?(this.members.Count);
      if (preValidated)
      {
        this.hashedMembers[(object) member.Name] = (object) nullable;
      }
      else
      {
        try
        {
          this.hashedMembers.Add((object) member.Name, (object) nullable);
        }
        catch (ArgumentException ex)
        {
          throw new ExtendedTypeSystemException("AlreadyPresentPSMemberInfoInternalCollectionAdd", (Exception) ex, "ExtendedTypeSystem", "MemberAlreadyPresent", new object[1]
          {
            (object) member.Name
          });
        }
      }
      if (member.IsHidden)
        ++this.countHidden;
      this.members.Add(member);
    }

    public override void Remove(string name)
    {
      object obj = !string.IsNullOrEmpty(name) ? this.hashedMembers[(object) name] : throw PSMemberInfoInternalCollection<T>.tracer.NewArgumentException(nameof (name));
      if (obj == null)
        return;
      if (PSMemberInfoCollection<T>.IsReservedName(name))
        throw new ExtendedTypeSystemException("PSMemberInfoInternalCollectionRemoveReservedName", (Exception) null, "ExtendedTypeSystem", "ReservedMemberName", new object[1]
        {
          (object) name
        });
      int index1 = ((int?) obj).Value;
      if (this.members[index1].IsHidden)
        --this.countHidden;
      this.members.RemoveAt(index1);
      for (int index2 = index1; index2 < this.members.Count; ++index2)
      {
        int? hashedMember = (int?) this.hashedMembers[(object) this.members[index2].Name];
        this.hashedMembers[(object) this.members[index2].Name] = (object) new int?(hashedMember.Value - 1);
      }
      this.hashedMembers.Remove((object) name);
    }

    public override T this[string name]
    {
      get
      {
        object obj = !string.IsNullOrEmpty(name) ? this.hashedMembers[(object) name] : throw PSMemberInfoInternalCollection<T>.tracer.NewArgumentException(nameof (name));
        return obj == null ? default (T) : this.members[((int?) obj).Value];
      }
    }

    public override ReadOnlyPSMemberInfoCollection<T> Match(
      string name)
    {
      return !string.IsNullOrEmpty(name) ? this.Match(name, PSMemberTypes.All, MshMemberMatchOptions.None) : throw PSMemberInfoInternalCollection<T>.tracer.NewArgumentException(nameof (name));
    }

    public override ReadOnlyPSMemberInfoCollection<T> Match(
      string name,
      PSMemberTypes memberTypes)
    {
      return !string.IsNullOrEmpty(name) ? this.Match(name, memberTypes, MshMemberMatchOptions.None) : throw PSMemberInfoInternalCollection<T>.tracer.NewArgumentException(nameof (name));
    }

    internal override ReadOnlyPSMemberInfoCollection<T> Match(
      string name,
      PSMemberTypes memberTypes,
      MshMemberMatchOptions matchOptions)
    {
      if (string.IsNullOrEmpty(name))
        throw PSMemberInfoInternalCollection<T>.tracer.NewArgumentException(nameof (name));
      return new ReadOnlyPSMemberInfoCollection<T>(MemberMatch.Match<T>(this.GetInternalMembers(matchOptions), name, MemberMatch.GetNamePattern(name), memberTypes));
    }

    private PSMemberInfoInternalCollection<T> GetInternalMembers(
      MshMemberMatchOptions matchOptions)
    {
      PSMemberInfoInternalCollection<T> internalCollection = new PSMemberInfoInternalCollection<T>();
      foreach (T obj in (PSMemberInfoCollection<T>) this)
      {
        PSMemberInfo psMemberInfo = (PSMemberInfo) obj;
        if (psMemberInfo.MatchesOptions(matchOptions) && psMemberInfo is T member)
          internalCollection.Add(member);
      }
      return internalCollection;
    }

    internal int Count => this.members.Count;

    internal int VisibleCount => this.members.Count - this.countHidden;

    internal T this[int index]
    {
      get
      {
        if (index < 0 || index >= this.members.Count)
          throw new ArgumentOutOfRangeException(nameof (index));
        return this.members[index];
      }
      set
      {
        if (index < 0 || index >= this.members.Count)
          throw new ArgumentOutOfRangeException(nameof (index));
        if ((object) value == null)
          throw PSMemberInfoInternalCollection<T>.tracer.NewArgumentNullException("member");
        if (string.Equals(value.Name, this.members[index].Name, StringComparison.OrdinalIgnoreCase))
        {
          this.members[index] = (T) value.Copy();
        }
        else
        {
          if (this.hashedMembers.Contains((object) value.Name))
            throw new ExtendedTypeSystemException("AlreadyPresentIndexerSet", (Exception) null, "ExtendedTypeSystem", "MemberAlreadyPresent", new object[1]
            {
              (object) value.Name
            });
          this.members[index] = (T) value.Copy();
          this.hashedMembers[(object) value.Name] = (object) new int?(index);
        }
      }
    }

    public override IEnumerator<T> GetEnumerator() => this.members.GetEnumerator();
  }
}
