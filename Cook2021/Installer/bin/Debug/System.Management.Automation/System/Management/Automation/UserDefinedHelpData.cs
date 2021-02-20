// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.UserDefinedHelpData
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Xml;

namespace System.Management.Automation
{
  internal class UserDefinedHelpData
  {
    private Dictionary<string, string> _properties = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private string _name;

    private UserDefinedHelpData()
    {
    }

    internal Dictionary<string, string> Properties => this._properties;

    internal string Name => this._name;

    internal static UserDefinedHelpData Load(XmlNode dataNode)
    {
      if (dataNode == null)
        return (UserDefinedHelpData) null;
      UserDefinedHelpData userDefinedHelpData = new UserDefinedHelpData();
      for (int i = 0; i < dataNode.ChildNodes.Count; ++i)
      {
        XmlNode childNode = dataNode.ChildNodes[i];
        if (childNode.NodeType == XmlNodeType.Element)
          userDefinedHelpData.Properties[childNode.Name] = childNode.InnerText.Trim();
      }
      if (!userDefinedHelpData.Properties.ContainsKey("name"))
        return (UserDefinedHelpData) null;
      userDefinedHelpData._name = userDefinedHelpData.Properties["name"];
      return string.IsNullOrEmpty(userDefinedHelpData.Name) ? (UserDefinedHelpData) null : userDefinedHelpData;
    }
  }
}
