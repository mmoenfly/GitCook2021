// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.MemberMatch
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class MemberMatch
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");

    internal static WildcardPattern GetNamePattern(string name) => name != null && WildcardPattern.ContainsWildcardCharacters(name) ? new WildcardPattern(name, WildcardOptions.IgnoreCase) : (WildcardPattern) null;

    internal static PSMemberInfoInternalCollection<T> Match<T>(
      PSMemberInfoInternalCollection<T> memberList,
      string name,
      WildcardPattern nameMatch,
      PSMemberTypes memberTypes)
      where T : PSMemberInfo
    {
      PSMemberInfoInternalCollection<T> internalCollection = new PSMemberInfoInternalCollection<T>();
      if (memberList == null)
        throw MemberMatch.tracer.NewArgumentNullException(nameof (memberList));
      if (string.IsNullOrEmpty(name))
        throw MemberMatch.tracer.NewArgumentException(nameof (name));
      if (nameMatch == null)
      {
        object hashedMember = memberList.hashedMembers[(object) name];
        if (hashedMember != null)
        {
          PSMemberInfo member = (PSMemberInfo) memberList.members[((int?) hashedMember).Value];
          if ((member.MemberType & memberTypes) != (PSMemberTypes) 0)
            internalCollection.Add(member as T);
        }
        return internalCollection;
      }
      foreach (T member in (PSMemberInfoCollection<T>) memberList)
      {
        PSMemberInfo psMemberInfo = (PSMemberInfo) member;
        if (nameMatch.IsMatch(psMemberInfo.Name) && (psMemberInfo.MemberType & memberTypes) != (PSMemberTypes) 0)
          internalCollection.Add(psMemberInfo as T);
      }
      return internalCollection;
    }
  }
}
