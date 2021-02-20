// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ListControl
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands.Internal.Format;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace System.Management.Automation
{
  public sealed class ListControl : PSControl
  {
    private List<ListControlEntry> _entries = new List<ListControlEntry>();
    private static string _tagListControl = nameof (ListControl);
    private static string _tagListEntries = "ListEntries";
    private static string _tagListEntry = "ListEntry";
    private static string _tagListItems = "ListItems";
    private static string _tagListItem = "ListItem";
    private static string _tagLabel = "Label";
    private static string _tagEntrySelectedBy = "EntrySelectedBy";
    private static string _tagTypeName = "TypeName";

    public List<ListControlEntry> Entries
    {
      get => this._entries;
      internal set => this._entries = value;
    }

    internal override void WriteToXML(XmlWriter _writer, bool exportScriptBlock)
    {
      _writer.WriteStartElement(ListControl._tagListControl);
      _writer.WriteStartElement(ListControl._tagListEntries);
      foreach (ListControlEntry entry in this._entries)
      {
        _writer.WriteStartElement(ListControl._tagListEntry);
        if (entry.SelectedBy.Count > 0)
        {
          _writer.WriteStartElement(ListControl._tagEntrySelectedBy);
          foreach (string str in entry.SelectedBy)
            _writer.WriteElementString(ListControl._tagTypeName, str);
          _writer.WriteEndElement();
        }
        _writer.WriteStartElement(ListControl._tagListItems);
        foreach (ListControlEntryItem controlEntryItem in entry.Items)
        {
          _writer.WriteStartElement(ListControl._tagListItem);
          if (!string.IsNullOrEmpty(controlEntryItem.Label))
            _writer.WriteElementString(ListControl._tagLabel, controlEntryItem.Label);
          controlEntryItem.DisplayEntry.WriteToXML(_writer, exportScriptBlock);
          _writer.WriteEndElement();
        }
        _writer.WriteEndElement();
        _writer.WriteEndElement();
      }
      _writer.WriteEndElement();
      _writer.WriteEndElement();
    }

    public override string ToString() => string.Format((IFormatProvider) CultureInfo.InvariantCulture, nameof (ListControl));

    internal override bool SafeForExport()
    {
      foreach (ListControlEntry entry in this._entries)
      {
        foreach (ListControlEntryItem controlEntryItem in entry.Items)
        {
          if (controlEntryItem.DisplayEntry.ValueType == DisplayEntryValueType.ScriptBlock)
            return false;
        }
      }
      return true;
    }

    internal ListControl()
    {
    }

    internal ListControl(ListControlBody listcontrolbody)
    {
      this._entries.Add(new ListControlEntry(listcontrolbody.defaultEntryDefinition));
      foreach (ListControlEntryDefinition optionalEntry in listcontrolbody.optionalEntryList)
        this._entries.Add(new ListControlEntry(optionalEntry));
    }
  }
}
