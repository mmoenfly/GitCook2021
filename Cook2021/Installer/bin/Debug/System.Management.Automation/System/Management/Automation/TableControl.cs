// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.TableControl
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands.Internal.Format;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace System.Management.Automation
{
  public sealed class TableControl : PSControl
  {
    private List<TableControlColumnHeader> _headers = new List<TableControlColumnHeader>();
    private List<TableControlRow> _rows = new List<TableControlRow>();
    private static string _tagTableControl = nameof (TableControl);
    private static string _tagTableHeaders = "TableHeaders";
    private static string _tagTableColumnHeader = "TableColumnHeader";
    private static string _tagLabel = "Label";
    private static string _tagWidth = "Width";
    private static string _tagAlignment = "Alignment";
    private static string _tagTableRowEntries = "TableRowEntries";
    private static string _tagTableRowEntry = "TableRowEntry";
    private static string _tagTableColumnItems = "TableColumnItems";
    private static string _tagTableColumnItem = "TableColumnItem";

    public List<TableControlColumnHeader> Headers
    {
      get => this._headers;
      internal set => this._headers = value;
    }

    public List<TableControlRow> Rows
    {
      get => this._rows;
      internal set => this._rows = value;
    }

    internal TableControl()
    {
    }

    public override string ToString() => string.Format((IFormatProvider) CultureInfo.InvariantCulture, nameof (TableControl));

    internal override void WriteToXML(XmlWriter _writer, bool exportScriptBlock)
    {
      _writer.WriteStartElement(TableControl._tagTableControl);
      _writer.WriteStartElement(TableControl._tagTableHeaders);
      foreach (TableControlColumnHeader header in this._headers)
      {
        _writer.WriteStartElement(TableControl._tagTableColumnHeader);
        if (!string.IsNullOrEmpty(header.Label))
          _writer.WriteElementString(TableControl._tagLabel, header.Label);
        if (header.Width > 0)
          _writer.WriteElementString(TableControl._tagWidth, header.Width.ToString((IFormatProvider) CultureInfo.InvariantCulture));
        if (header.Alignment != Alignment.Undefined)
          _writer.WriteElementString(TableControl._tagAlignment, header.Alignment.ToString());
        _writer.WriteEndElement();
      }
      _writer.WriteEndElement();
      _writer.WriteStartElement(TableControl._tagTableRowEntries);
      foreach (TableControlRow row in this._rows)
      {
        _writer.WriteStartElement(TableControl._tagTableRowEntry);
        _writer.WriteStartElement(TableControl._tagTableColumnItems);
        foreach (TableControlColumn column in row.Columns)
        {
          _writer.WriteStartElement(TableControl._tagTableColumnItem);
          if (column.Alignment != Alignment.Undefined)
            _writer.WriteElementString(TableControl._tagAlignment, column.Alignment.ToString());
          column.DisplayEntry.WriteToXML(_writer, exportScriptBlock);
          _writer.WriteEndElement();
        }
        _writer.WriteEndElement();
        _writer.WriteEndElement();
      }
      _writer.WriteEndElement();
      _writer.WriteEndElement();
    }

    internal override bool SafeForExport()
    {
      foreach (TableControlRow row in this._rows)
      {
        foreach (TableControlColumn column in row.Columns)
        {
          if (column.DisplayEntry.ValueType == DisplayEntryValueType.ScriptBlock)
            return false;
        }
      }
      return true;
    }

    internal TableControl(TableControlBody tcb)
    {
      this._rows.Add(new TableControlRow(tcb.defaultDefinition));
      foreach (TableRowDefinition optionalDefinition in tcb.optionalDefinitionList)
        this._rows.Add(new TableControlRow(optionalDefinition));
      foreach (TableColumnHeaderDefinition headerDefinition in tcb.header.columnHeaderDefinitionList)
        this._headers.Add(new TableControlColumnHeader(headerDefinition));
    }
  }
}
