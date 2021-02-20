// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ReadOnlyPSMemberInfoCollection`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;

namespace System.Management.Automation
{
  public class ReadOnlyPSMemberInfoCollection<T> : IEnumerable<T>, IEnumerable where T : PSMemberInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    private PSMemberInfoInternalCollection<T> members;

    internal ReadOnlyPSMemberInfoCollection(PSMemberInfoInternalCollection<T> members) => this.members = members != null ? members : throw ReadOnlyPSMemberInfoCollection<T>.tracer.NewArgumentNullException(nameof (members));

    public T this[string name] => !string.IsNullOrEmpty(name) ? this.members[name] : throw ReadOnlyPSMemberInfoCollection<T>.tracer.NewArgumentException(nameof (name));

    public ReadOnlyPSMemberInfoCollection<T> Match(string name) => !string.IsNullOrEmpty(name) ? this.members.Match(name) : throw ReadOnlyPSMemberInfoCollection<T>.tracer.NewArgumentException(nameof (name));

    public ReadOnlyPSMemberInfoCollection<T> Match(
      string name,
      PSMemberTypes memberTypes)
    {
      if (string.IsNullOrEmpty(name))
        throw ReadOnlyPSMemberInfoCollection<T>.tracer.NewArgumentException(nameof (name));
      return this.members.Match(name, memberTypes);
    }

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();

    public virtual IEnumerator<T> GetEnumerator() => this.members.GetEnumerator();

    public int Count => this.members.members.Count;

    public T this[int index]
    {
      get
      {
        if (index < 0 || index >= this.members.members.Count)
          throw new ArgumentOutOfRangeException(nameof (index));
        return this.members.members[index];
      }
    }
  }
}
