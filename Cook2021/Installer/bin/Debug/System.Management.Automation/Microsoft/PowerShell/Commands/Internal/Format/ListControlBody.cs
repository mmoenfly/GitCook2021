// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.ListControlBody
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal sealed class ListControlBody : ControlBody
  {
    internal ListControlEntryDefinition defaultEntryDefinition;
    internal List<ListControlEntryDefinition> optionalEntryList = new List<ListControlEntryDefinition>();

    internal override ControlBase Copy()
    {
      ListControlBody listControlBody = new ListControlBody();
      listControlBody.autosize = this.autosize;
      if (this.defaultEntryDefinition != null)
        listControlBody.defaultEntryDefinition = this.defaultEntryDefinition.Copy();
      foreach (ListControlEntryDefinition optionalEntry in this.optionalEntryList)
        listControlBody.optionalEntryList.Add(optionalEntry);
      return (ControlBase) listControlBody;
    }
  }
}
