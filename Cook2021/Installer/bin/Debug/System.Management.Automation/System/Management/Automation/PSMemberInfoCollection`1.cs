// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSMemberInfoCollection`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;

namespace System.Management.Automation
{
  public abstract class PSMemberInfoCollection<T> : IEnumerable<T>, IEnumerable
    where T : PSMemberInfo
  {
    public abstract void Add(T member);

    public abstract void Add(T member, bool preValidated);

    public abstract void Remove(string name);

    public abstract T this[string name] { get; }

    public abstract ReadOnlyPSMemberInfoCollection<T> Match(
      string name);

    public abstract ReadOnlyPSMemberInfoCollection<T> Match(
      string name,
      PSMemberTypes memberTypes);

    internal abstract ReadOnlyPSMemberInfoCollection<T> Match(
      string name,
      PSMemberTypes memberTypes,
      MshMemberMatchOptions matchOptions);

    internal static bool IsReservedName(string name) => string.Equals(name, "psbase", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "psadapted", StringComparison.OrdinalIgnoreCase) || (string.Equals(name, "psextended", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "psobject", StringComparison.OrdinalIgnoreCase)) || string.Equals(name, "pstypenames", StringComparison.OrdinalIgnoreCase);

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();

    public abstract IEnumerator<T> GetEnumerator();
  }
}
