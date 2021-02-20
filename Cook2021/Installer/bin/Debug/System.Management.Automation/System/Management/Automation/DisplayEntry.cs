// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.DisplayEntry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Xml;

namespace System.Management.Automation
{
  public sealed class DisplayEntry
  {
    private DisplayEntryValueType _type;
    private string _value;
    private static string _tagPropertyName = "PropertyName";
    private static string _tagScriptBlock = "ScriptBlock";
    private static string _safeScriptBlock = ";";

    public DisplayEntryValueType ValueType
    {
      get => this._type;
      internal set => this._type = value;
    }

    public string Value
    {
      get => this._value;
      internal set => this._value = value;
    }

    internal DisplayEntry()
    {
    }

    internal DisplayEntry(string value, DisplayEntryValueType type)
    {
      this._value = value;
      this._type = type;
    }

    internal void WriteToXML(XmlWriter _writer, bool exportScriptBlock)
    {
      if (this._type == DisplayEntryValueType.Property)
      {
        _writer.WriteElementString(DisplayEntry._tagPropertyName, this._value);
      }
      else
      {
        if (this._type != DisplayEntryValueType.ScriptBlock)
          return;
        _writer.WriteStartElement(DisplayEntry._tagScriptBlock);
        if (exportScriptBlock)
          _writer.WriteValue(this._value);
        else
          _writer.WriteValue(DisplayEntry._safeScriptBlock);
        _writer.WriteEndElement();
      }
    }

    public override string ToString() => this._value;
  }
}
