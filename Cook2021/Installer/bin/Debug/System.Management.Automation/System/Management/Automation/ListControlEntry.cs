// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ListControlEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands.Internal.Format;
using System.Collections.Generic;

namespace System.Management.Automation
{
  public sealed class ListControlEntry
  {
    private List<ListControlEntryItem> _items = new List<ListControlEntryItem>();
    private List<string> _entrySelectedBy = new List<string>();

    public List<ListControlEntryItem> Items
    {
      get => this._items;
      internal set => this._items = value;
    }

    public List<string> SelectedBy
    {
      get => this._entrySelectedBy;
      internal set => this._entrySelectedBy = value;
    }

    internal ListControlEntry()
    {
    }

    internal ListControlEntry(ListControlEntryDefinition entrydefn)
    {
      if (entrydefn.appliesTo != null)
      {
        foreach (TypeOrGroupReference reference in entrydefn.appliesTo.referenceList)
          this._entrySelectedBy.Add(reference.name);
      }
      foreach (ListControlItemDefinition itemDefinition in entrydefn.itemDefinitionList)
        this._items.Add(new ListControlEntryItem(itemDefinition));
    }
  }
}
