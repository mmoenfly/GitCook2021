// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.TableControlColumnHeader
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands.Internal.Format;

namespace System.Management.Automation
{
  public sealed class TableControlColumnHeader
  {
    private string _label;
    private Alignment _alignment;
    private int _width;

    public string Label
    {
      get => this._label;
      internal set => this._label = value;
    }

    public Alignment Alignment
    {
      get => this._alignment;
      internal set => this._alignment = value;
    }

    public int Width
    {
      get => this._width;
      internal set => this._width = value;
    }

    internal TableControlColumnHeader(TableColumnHeaderDefinition colheaderdefinition)
    {
      if (colheaderdefinition.label != null)
        this._label = colheaderdefinition.label.text;
      this._alignment = (Alignment) colheaderdefinition.alignment;
      this._width = colheaderdefinition.width;
    }

    internal TableControlColumnHeader()
    {
    }
  }
}
