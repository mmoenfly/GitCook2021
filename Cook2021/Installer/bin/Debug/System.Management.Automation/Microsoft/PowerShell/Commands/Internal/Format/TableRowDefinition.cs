// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.TableRowDefinition
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal sealed class TableRowDefinition
  {
    internal AppliesTo appliesTo;
    internal bool multiLine;
    internal List<TableRowItemDefinition> rowItemDefinitionList = new List<TableRowItemDefinition>();

    internal TableRowDefinition Copy()
    {
      TableRowDefinition tableRowDefinition = new TableRowDefinition();
      tableRowDefinition.appliesTo = this.appliesTo;
      tableRowDefinition.multiLine = this.multiLine;
      foreach (TableRowItemDefinition rowItemDefinition in this.rowItemDefinitionList)
        tableRowDefinition.rowItemDefinitionList.Add(rowItemDefinition);
      return tableRowDefinition;
    }
  }
}
