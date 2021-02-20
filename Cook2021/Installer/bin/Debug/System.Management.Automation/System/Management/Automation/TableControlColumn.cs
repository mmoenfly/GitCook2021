// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.TableControlColumn
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public sealed class TableControlColumn
  {
    private Alignment _alignment;
    private DisplayEntry _entry;

    public Alignment Alignment
    {
      get => this._alignment;
      internal set => this._alignment = value;
    }

    public DisplayEntry DisplayEntry
    {
      get => this._entry;
      internal set => this._entry = value;
    }

    public override string ToString() => this._entry.Value;

    internal TableControlColumn()
    {
    }

    internal TableControlColumn(string text, int alignment, bool isscriptblock)
    {
      this._alignment = (Alignment) alignment;
      if (isscriptblock)
        this._entry = new DisplayEntry(text, DisplayEntryValueType.ScriptBlock);
      else
        this._entry = new DisplayEntry(text, DisplayEntryValueType.Property);
    }
  }
}
