// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ListControlEntryItem
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands.Internal.Format;

namespace System.Management.Automation
{
  public sealed class ListControlEntryItem
  {
    private string _label;
    private DisplayEntry _entry;

    public string Label
    {
      get => this._label;
      internal set => this._label = value;
    }

    public DisplayEntry DisplayEntry
    {
      get => this._entry;
      internal set => this._entry = value;
    }

    internal ListControlEntryItem()
    {
    }

    internal ListControlEntryItem(ListControlItemDefinition definition)
    {
      if (definition.label != null)
        this._label = definition.label.text;
      if (!(definition.formatTokenList[0] is FieldPropertyToken formatToken))
        return;
      if (formatToken.expression.isScriptBlock)
        this._entry = new DisplayEntry(formatToken.expression.expressionValue, DisplayEntryValueType.ScriptBlock);
      else
        this._entry = new DisplayEntry(formatToken.expression.expressionValue, DisplayEntryValueType.Property);
    }
  }
}
