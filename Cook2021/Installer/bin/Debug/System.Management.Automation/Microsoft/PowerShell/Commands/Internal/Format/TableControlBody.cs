// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.TableControlBody
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal sealed class TableControlBody : ControlBody
  {
    internal TableHeaderDefinition header = new TableHeaderDefinition();
    internal TableRowDefinition defaultDefinition;
    internal List<TableRowDefinition> optionalDefinitionList = new List<TableRowDefinition>();

    internal override ControlBase Copy()
    {
      TableControlBody tableControlBody = new TableControlBody();
      tableControlBody.autosize = this.autosize;
      tableControlBody.header = this.header.Copy();
      if (this.defaultDefinition != null)
        tableControlBody.defaultDefinition = this.defaultDefinition.Copy();
      foreach (TableRowDefinition optionalDefinition in this.optionalDefinitionList)
        tableControlBody.optionalDefinitionList.Add(optionalDefinition);
      return (ControlBase) tableControlBody;
    }
  }
}
