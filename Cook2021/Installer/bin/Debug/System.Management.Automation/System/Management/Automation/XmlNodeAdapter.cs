// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.XmlNodeAdapter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;

namespace System.Management.Automation
{
  internal class XmlNodeAdapter : PropertyOnlyAdapter
  {
    protected override Collection<string> GetTypeNameHierarchy(object obj)
    {
      XmlNode xmlNode = (XmlNode) obj;
      if (string.IsNullOrEmpty(xmlNode.NamespaceURI))
        return base.GetTypeNameHierarchy(obj);
      Collection<string> typeNameHierarchy = base.GetTypeNameHierarchy(obj);
      Collection<string> collection = new Collection<string>();
      StringBuilder stringBuilder = new StringBuilder(typeNameHierarchy[0]);
      stringBuilder.Append("#");
      stringBuilder.Append(xmlNode.NamespaceURI);
      stringBuilder.Append("#");
      stringBuilder.Append(xmlNode.LocalName);
      collection.Add(stringBuilder.ToString());
      foreach (string str in typeNameHierarchy)
        collection.Add(str);
      return collection;
    }

    protected override void DoAddAllProperties<T>(
      object obj,
      PSMemberInfoInternalCollection<T> members)
    {
      XmlNode xmlNode = (XmlNode) obj;
      Dictionary<string, List<XmlNode>> dictionary = new Dictionary<string, List<XmlNode>>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      if (xmlNode.Attributes != null)
      {
        foreach (XmlNode attribute in (XmlNamedNodeMap) xmlNode.Attributes)
        {
          List<XmlNode> xmlNodeList;
          if (!dictionary.TryGetValue(attribute.LocalName, out xmlNodeList))
          {
            xmlNodeList = new List<XmlNode>();
            dictionary[attribute.LocalName] = xmlNodeList;
          }
          xmlNodeList.Add(attribute);
        }
      }
      if (xmlNode.ChildNodes != null)
      {
        foreach (XmlNode childNode in xmlNode.ChildNodes)
        {
          List<XmlNode> xmlNodeList;
          if (!dictionary.TryGetValue(childNode.LocalName, out xmlNodeList))
          {
            xmlNodeList = new List<XmlNode>();
            dictionary[childNode.LocalName] = xmlNodeList;
          }
          xmlNodeList.Add(childNode);
        }
      }
      foreach (KeyValuePair<string, List<XmlNode>> keyValuePair in dictionary)
        members.Add(new PSProperty(keyValuePair.Key, (Adapter) this, obj, (object) keyValuePair.Value.ToArray()) as T);
    }

    protected override PSProperty DoGetProperty(object obj, string propertyName)
    {
      XmlNode[] nodes = XmlNodeAdapter.FindNodes(obj, propertyName, StringComparison.OrdinalIgnoreCase);
      return nodes.Length == 0 ? (PSProperty) null : new PSProperty(nodes[0].LocalName, (Adapter) this, obj, (object) nodes);
    }

    protected override bool PropertyIsSettable(PSProperty property)
    {
      XmlNode[] adapterData = (XmlNode[]) property.adapterData;
      if (adapterData.Length != 1)
        return false;
      XmlNode xmlNode = adapterData[0];
      switch (xmlNode)
      {
        case XmlText _:
          return true;
        case XmlAttribute _:
          return true;
        default:
          XmlAttributeCollection attributes = xmlNode.Attributes;
          if (attributes != null && attributes.Count != 0)
            return false;
          XmlNodeList childNodes = xmlNode.ChildNodes;
          return childNodes == null || childNodes.Count == 0 || childNodes.Count == 1 && childNodes[0].NodeType == XmlNodeType.Text;
      }
    }

    protected override bool PropertyIsGettable(PSProperty property) => true;

    private static object GetNodeObject(XmlNode node)
    {
      if (node is XmlText xmlText)
        return (object) xmlText.InnerText;
      XmlAttributeCollection attributes = node.Attributes;
      if (attributes != null && attributes.Count != 0)
        return (object) node;
      if (!node.HasChildNodes)
        return (object) node.InnerText;
      XmlNodeList childNodes = node.ChildNodes;
      if (childNodes.Count == 1 && childNodes[0].NodeType == XmlNodeType.Text)
        return (object) node.InnerText;
      return node is XmlAttribute xmlAttribute ? (object) xmlAttribute.Value : (object) node;
    }

    protected override object PropertyGet(PSProperty property)
    {
      XmlNode[] adapterData = (XmlNode[]) property.adapterData;
      if (adapterData.Length == 1)
        return XmlNodeAdapter.GetNodeObject(adapterData[0]);
      object[] objArray = new object[adapterData.Length];
      for (int index = 0; index < adapterData.Length; ++index)
        objArray[index] = XmlNodeAdapter.GetNodeObject(adapterData[index]);
      return (object) objArray;
    }

    protected override void PropertySet(
      PSProperty property,
      object setValue,
      bool convertIfPossible)
    {
      if (!(setValue is string str))
        throw new SetValueException("XmlNodeSetShouldBeAString", (Exception) null, "ExtendedTypeSystem", "XmlNodeSetShouldBeAString", new object[1]
        {
          (object) property.Name
        });
      XmlNode[] adapterData = (XmlNode[]) property.adapterData;
      XmlNode xmlNode = adapterData.Length <= 1 ? adapterData[0] : throw new SetValueException("XmlNodeSetRestrictionsMoreThanOneNode", (Exception) null, "ExtendedTypeSystem", "XmlNodeSetShouldBeAString", new object[1]
      {
        (object) property.Name
      });
      if (xmlNode is XmlText xmlText)
      {
        xmlText.InnerText = str;
      }
      else
      {
        XmlAttributeCollection attributes = xmlNode.Attributes;
        if (attributes != null && attributes.Count != 0)
          throw new SetValueException("XmlNodeSetRestrictionsNodeWithAttributes", (Exception) null, "ExtendedTypeSystem", "XmlNodeSetShouldBeAString", new object[1]
          {
            (object) property.Name
          });
        XmlNodeList childNodes = xmlNode.ChildNodes;
        if (childNodes == null || childNodes.Count == 0)
          xmlNode.InnerText = str;
        else if (childNodes.Count == 1 && childNodes[0].NodeType == XmlNodeType.Text)
          xmlNode.InnerText = str;
        else if (xmlNode is XmlAttribute xmlAttribute)
          xmlAttribute.Value = str;
        else
          throw new SetValueException("XmlNodeSetRestrictionsUnknownNodeType", (Exception) null, "ExtendedTypeSystem", "XmlNodeSetShouldBeAString", new object[1]
          {
            (object) property.Name
          });
      }
    }

    protected override string PropertyType(PSProperty property)
    {
      object obj = (object) null;
      try
      {
        obj = this.BasePropertyGet(property);
      }
      catch (GetValueException ex)
      {
        Adapter.tracer.TraceException((Exception) ex);
      }
      if (obj == null)
        return typeof (object).FullName;
      return obj.GetType().FullName;
    }

    private static XmlNode[] FindNodes(
      object obj,
      string propertyName,
      StringComparison comparisonType)
    {
      List<XmlNode> xmlNodeList = new List<XmlNode>();
      XmlNode xmlNode = (XmlNode) obj;
      if (xmlNode.Attributes != null)
      {
        foreach (XmlNode attribute in (XmlNamedNodeMap) xmlNode.Attributes)
        {
          if (attribute.LocalName.Equals(propertyName, comparisonType))
            xmlNodeList.Add(attribute);
        }
      }
      if (xmlNode.ChildNodes != null)
      {
        foreach (XmlNode childNode in xmlNode.ChildNodes)
        {
          if (childNode.LocalName.Equals(propertyName, comparisonType))
            xmlNodeList.Add(childNode);
        }
      }
      return xmlNodeList.ToArray();
    }
  }
}
