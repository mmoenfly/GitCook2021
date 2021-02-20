// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.TableControlRow
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands.Internal.Format;
using System.Collections.Generic;

namespace System.Management.Automation
{
  public sealed class TableControlRow
  {
    private List<TableControlColumn> _columns = new List<TableControlColumn>();

    public List<TableControlColumn> Columns
    {
      get => this._columns;
      internal set => this._columns = value;
    }

    internal TableControlRow()
    {
    }

    internal TableControlRow(TableRowDefinition rowdefinition)
    {
      foreach (TableRowItemDefinition rowItemDefinition in rowdefinition.rowItemDefinitionList)
        this._columns.Add(!(rowItemDefinition.formatTokenList[0] is FieldPropertyToken formatToken) ? new TableControlColumn() : new TableControlColumn(formatToken.expression.expressionValue, rowItemDefinition.alignment, formatToken.expression.isScriptBlock));
    }
  }
}
