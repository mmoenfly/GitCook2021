// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.WideControl
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands.Internal.Format;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace System.Management.Automation
{
  public sealed class WideControl : PSControl
  {
    private List<WideControlEntryItem> _entries = new List<WideControlEntryItem>();
    private uint _columns;
    private Alignment _aligment;
    private static string _tagWideControl = nameof (WideControl);
    private static string _tagWideEntries = "WideEntries";
    private static string _tagWideEntry = "WideEntry";
    private static string _tagWideItem = "WideItem";
    private static string _tagAlignment = nameof (Alignment);
    private static string _tagColumnNumber = "ColumnNumber";
    private static string _tagSelectedBy = "EntrySelectedBy";
    private static string _tagTypeName = "TypeName";

    public List<WideControlEntryItem> Entries
    {
      get => this._entries;
      internal set => this._entries = value;
    }

    public Alignment Alignment
    {
      get => this._aligment;
      internal set => this._aligment = value;
    }

    public uint Columns
    {
      get => this._columns;
      internal set => this._columns = value;
    }

    internal override void WriteToXML(XmlWriter _writer, bool exportScriptBlock)
    {
      _writer.WriteStartElement(WideControl._tagWideControl);
      if (this._columns > 0U)
        _writer.WriteElementString(WideControl._tagColumnNumber, this._columns.ToString((IFormatProvider) CultureInfo.InvariantCulture));
      if (this._aligment != Alignment.Undefined)
        _writer.WriteElementString(WideControl._tagAlignment, this._aligment.ToString());
      _writer.WriteStartElement(WideControl._tagWideEntries);
      foreach (WideControlEntryItem entry in this._entries)
      {
        _writer.WriteStartElement(WideControl._tagWideEntry);
        if (entry.SelectedBy.Count > 0)
        {
          _writer.WriteStartElement(WideControl._tagSelectedBy);
          foreach (string str in entry.SelectedBy)
            _writer.WriteElementString(WideControl._tagTypeName, str);
          _writer.WriteEndElement();
        }
        _writer.WriteStartElement(WideControl._tagWideItem);
        entry.DisplayEntry.WriteToXML(_writer, exportScriptBlock);
        _writer.WriteEndElement();
        _writer.WriteEndElement();
      }
      _writer.WriteEndElement();
      _writer.WriteEndElement();
    }

    public override string ToString() => string.Format((IFormatProvider) CultureInfo.InvariantCulture, nameof (WideControl));

    internal override bool SafeForExport()
    {
      foreach (WideControlEntryItem entry in this._entries)
      {
        if (entry.DisplayEntry.ValueType == DisplayEntryValueType.ScriptBlock)
          return false;
      }
      return true;
    }

    internal WideControl()
    {
    }

    internal WideControl(WideControlBody widecontrolbody)
    {
      this._columns = (uint) widecontrolbody.columns;
      this._aligment = (Alignment) widecontrolbody.alignment;
      this._entries.Add(new WideControlEntryItem(widecontrolbody.defaultEntryDefinition));
      foreach (WideControlEntryDefinition optionalEntry in widecontrolbody.optionalEntryList)
        this._entries.Add(new WideControlEntryItem(optionalEntry));
    }
  }
}
