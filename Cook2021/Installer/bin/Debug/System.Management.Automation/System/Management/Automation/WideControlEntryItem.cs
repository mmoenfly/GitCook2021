// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.WideControlEntryItem
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands.Internal.Format;
using System.Collections.Generic;

namespace System.Management.Automation
{
  public sealed class WideControlEntryItem
  {
    private DisplayEntry _entry;
    private List<string> _selectedBy = new List<string>();

    public DisplayEntry DisplayEntry
    {
      get => this._entry;
      internal set => this._entry = value;
    }

    public List<string> SelectedBy
    {
      get => this._selectedBy;
      internal set => this._selectedBy = value;
    }

    internal WideControlEntryItem()
    {
    }

    internal WideControlEntryItem(WideControlEntryDefinition definition)
    {
      if (definition.formatTokenList[0] is FieldPropertyToken formatToken)
        this._entry = !formatToken.expression.isScriptBlock ? new DisplayEntry(formatToken.expression.expressionValue, DisplayEntryValueType.Property) : new DisplayEntry(formatToken.expression.expressionValue, DisplayEntryValueType.ScriptBlock);
      if (definition.appliesTo == null)
        return;
      foreach (TypeOrGroupReference reference in definition.appliesTo.referenceList)
        this._selectedBy.Add(reference.name);
    }
  }
}
