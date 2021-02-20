// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.ConsolidatedString
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace System.Management.Automation.Runspaces
{
  internal class ConsolidatedString : Collection<string>
  {
    private const string separator = "@@@";
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    private string key;

    protected override void SetItem(int index, string item)
    {
      if (string.IsNullOrEmpty(item))
        throw ConsolidatedString.tracer.NewArgumentException(nameof (item));
      base.SetItem(index, item);
      this.UpdateKey();
    }

    protected override void ClearItems()
    {
      base.ClearItems();
      this.UpdateKey();
    }

    protected override void InsertItem(int index, string item)
    {
      if (string.IsNullOrEmpty(item))
        throw ConsolidatedString.tracer.NewArgumentException(nameof (item));
      base.InsertItem(index, item);
      this.UpdateKey();
    }

    protected override void RemoveItem(int index)
    {
      base.RemoveItem(index);
      this.UpdateKey();
    }

    internal string Key => this.key;

    private void UpdateKey()
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string str in (Collection<string>) this)
      {
        stringBuilder.Append(str);
        stringBuilder.Append("@@@");
      }
      this.key = stringBuilder.ToString();
    }

    public ConsolidatedString(ConsolidatedString other)
      : base((IList<string>) new List<string>((IEnumerable<string>) other))
      => this.key = other.key;

    public ConsolidatedString(Collection<string> strings)
      : base((IList<string>) strings)
    {
      if (strings == null)
        throw ConsolidatedString.tracer.NewArgumentException(nameof (strings));
      foreach (string str in (Collection<string>) this)
      {
        if (string.IsNullOrEmpty(str))
          throw ConsolidatedString.tracer.NewArgumentException(nameof (strings));
      }
      this.UpdateKey();
    }
  }
}
