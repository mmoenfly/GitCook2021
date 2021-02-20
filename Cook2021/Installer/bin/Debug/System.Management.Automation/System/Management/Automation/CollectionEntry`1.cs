// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CollectionEntry`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class CollectionEntry<T> where T : PSMemberInfo
  {
    private CollectionEntry<T>.GetMembersDelegate getMembers;
    private CollectionEntry<T>.GetMemberDelegate getMember;
    private bool shouldReplicateWhenReturning;
    private bool shouldCloneWhenReturning;
    private string collectionNameForTracing;

    internal CollectionEntry(
      CollectionEntry<T>.GetMembersDelegate getMembers,
      CollectionEntry<T>.GetMemberDelegate getMember,
      bool shouldReplicateWhenReturning,
      bool shouldCloneWhenReturning,
      string collectionNameForTracing)
    {
      this.getMembers = getMembers;
      this.getMember = getMember;
      this.shouldReplicateWhenReturning = shouldReplicateWhenReturning;
      this.shouldCloneWhenReturning = shouldCloneWhenReturning;
      this.collectionNameForTracing = collectionNameForTracing;
    }

    internal CollectionEntry<T>.GetMembersDelegate GetMembers => this.getMembers;

    internal CollectionEntry<T>.GetMemberDelegate GetMember => this.getMember;

    internal bool ShouldReplicateWhenReturning => this.shouldReplicateWhenReturning;

    internal bool ShouldCloneWhenReturning => this.shouldCloneWhenReturning;

    internal string CollectionNameForTracing => this.collectionNameForTracing;

    internal delegate PSMemberInfoInternalCollection<T> GetMembersDelegate(
      PSObject obj)
      where T : PSMemberInfo;

    internal delegate T GetMemberDelegate(PSObject obj, string name) where T : PSMemberInfo;
  }
}
