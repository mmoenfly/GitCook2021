// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.MamlNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;

namespace System.Management.Automation
{
  internal class MamlNode
  {
    private XmlNode _xmlNode;
    private PSObject _mshObject;
    private Collection<ErrorRecord> _errors = new Collection<ErrorRecord>();
    [TraceSource("MamlNode", "MamlNode")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (MamlNode), nameof (MamlNode));

    internal MamlNode(XmlNode xmlNode)
    {
      using (MamlNode.tracer.TraceConstructor((object) this))
        this._xmlNode = xmlNode;
    }

    internal XmlNode XmlNode
    {
      get
      {
        using (MamlNode.tracer.TraceProperty())
          return this._xmlNode;
      }
    }

    internal PSObject PSObject
    {
      get
      {
        using (MamlNode.tracer.TraceProperty())
        {
          if (this._mshObject == null)
          {
            this.RemoveUnsupportedNodes(this._xmlNode);
            this._mshObject = this.GetPSObject(this._xmlNode);
          }
          return this._mshObject;
        }
      }
    }

    private PSObject GetPSObject(XmlNode xmlNode)
    {
      if (xmlNode == null)
        return new PSObject();
      PSObject psObject;
      if (MamlNode.IsAtomic(xmlNode))
        psObject = new PSObject((object) xmlNode.InnerText.Trim());
      else if (MamlNode.IncludeMamlFormatting(xmlNode))
      {
        psObject = new PSObject((object) this.GetMamlFormattingPSObjects(xmlNode));
      }
      else
      {
        psObject = new PSObject((object) this.GetInsidePSObject(xmlNode));
        psObject.TypeNames.Clear();
        psObject.TypeNames.Add("MamlCommandHelpInfo#" + xmlNode.LocalName);
      }
      if (xmlNode.Attributes != null)
      {
        foreach (XmlNode attribute in (XmlNamedNodeMap) xmlNode.Attributes)
          psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty(attribute.Name, (object) attribute.Value));
      }
      return psObject;
    }

    private PSObject GetInsidePSObject(XmlNode xmlNode)
    {
      Hashtable insideProperties = this.GetInsideProperties(xmlNode);
      PSObject psObject = new PSObject();
      IDictionaryEnumerator enumerator = insideProperties.GetEnumerator();
      while (enumerator.MoveNext())
        psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty((string) enumerator.Key, enumerator.Value));
      return psObject;
    }

    private Hashtable GetInsideProperties(XmlNode xmlNode)
    {
      Hashtable properties = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);
      if (xmlNode == null)
        return properties;
      if (xmlNode.ChildNodes != null)
      {
        foreach (XmlNode childNode in xmlNode.ChildNodes)
          MamlNode.AddProperty(properties, childNode.LocalName, this.GetPSObject(childNode));
      }
      return MamlNode.SimplifyProperties(properties);
    }

    private void RemoveUnsupportedNodes(XmlNode xmlNode)
    {
      XmlNode xmlNode1 = xmlNode.FirstChild;
      while (xmlNode1 != null)
      {
        if (xmlNode1.NodeType == XmlNodeType.Comment)
        {
          XmlNode oldChild = xmlNode1;
          xmlNode1 = xmlNode1.NextSibling;
          xmlNode.RemoveChild(oldChild);
        }
        else
        {
          this.RemoveUnsupportedNodes(xmlNode1);
          xmlNode1 = xmlNode1.NextSibling;
        }
      }
    }

    private static void AddProperty(Hashtable properties, string name, PSObject mshObject)
    {
      ArrayList arrayList = (ArrayList) properties[(object) name];
      if (arrayList == null)
      {
        arrayList = new ArrayList();
        properties[(object) name] = (object) arrayList;
      }
      if (mshObject == null)
        return;
      if (mshObject.BaseObject is PSCustomObject || !mshObject.BaseObject.GetType().Equals(typeof (PSObject[])))
      {
        arrayList.Add((object) mshObject);
      }
      else
      {
        foreach (object obj in (PSObject[]) mshObject.BaseObject)
          arrayList.Add(obj);
      }
    }

    private static Hashtable SimplifyProperties(Hashtable properties)
    {
      if (properties == null)
        return (Hashtable) null;
      Hashtable hashtable = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);
      IDictionaryEnumerator enumerator = properties.GetEnumerator();
      while (enumerator.MoveNext())
      {
        ArrayList arrayList = (ArrayList) enumerator.Value;
        if (arrayList != null && arrayList.Count != 0)
        {
          if (arrayList.Count == 1 && !MamlNode.IsMamlFormattingPSObject((PSObject) arrayList[0]))
          {
            PSObject psObject = (PSObject) arrayList[0];
            hashtable[enumerator.Key] = (object) psObject;
          }
          else
            hashtable[enumerator.Key] = (object) arrayList.ToArray(typeof (PSObject));
        }
      }
      return hashtable;
    }

    private static bool IsAtomic(XmlNode xmlNode) => xmlNode != null && (xmlNode.ChildNodes == null || xmlNode.ChildNodes.Count <= 1 && (xmlNode.ChildNodes.Count == 0 || xmlNode.ChildNodes[0].GetType().Equals(typeof (XmlText))));

    private static bool IncludeMamlFormatting(XmlNode xmlNode)
    {
      if (xmlNode == null || xmlNode.ChildNodes == null || xmlNode.ChildNodes.Count == 0)
        return false;
      foreach (XmlNode childNode in xmlNode.ChildNodes)
      {
        if (MamlNode.IsMamlFormattingNode(childNode))
          return true;
      }
      return false;
    }

    private static bool IsMamlFormattingNode(XmlNode xmlNode) => xmlNode.LocalName.Equals("para", StringComparison.OrdinalIgnoreCase) || xmlNode.LocalName.Equals("list", StringComparison.OrdinalIgnoreCase) || xmlNode.LocalName.Equals("definitionList", StringComparison.OrdinalIgnoreCase);

    private static bool IsMamlFormattingPSObject(PSObject mshObject)
    {
      Collection<string> typeNames = mshObject.TypeNames;
      return typeNames != null && typeNames.Count != 0 && typeNames[typeNames.Count - 1].Equals("MamlTextItem", StringComparison.OrdinalIgnoreCase);
    }

    private PSObject[] GetMamlFormattingPSObjects(XmlNode xmlNode)
    {
      ArrayList arrayList = new ArrayList();
      foreach (XmlNode childNode in xmlNode.ChildNodes)
      {
        if (childNode.LocalName.Equals("para", StringComparison.OrdinalIgnoreCase))
        {
          PSObject paraPsObject = MamlNode.GetParaPSObject(childNode);
          if (paraPsObject != null)
            arrayList.Add((object) paraPsObject);
        }
        else if (childNode.LocalName.Equals("list", StringComparison.OrdinalIgnoreCase))
        {
          ArrayList listPsObjects = this.GetListPSObjects(childNode);
          for (int index = 0; index < listPsObjects.Count; ++index)
            arrayList.Add(listPsObjects[index]);
        }
        else if (childNode.LocalName.Equals("definitionList", StringComparison.OrdinalIgnoreCase))
        {
          ArrayList definitionListPsObjects = this.GetDefinitionListPSObjects(childNode);
          for (int index = 0; index < definitionListPsObjects.Count; ++index)
            arrayList.Add(definitionListPsObjects[index]);
        }
        else
          this.WriteMamlInvalidChildNodeError(xmlNode, childNode);
      }
      return (PSObject[]) arrayList.ToArray(typeof (PSObject));
    }

    private void WriteMamlInvalidChildNodeError(XmlNode node, XmlNode childNode) => this.Errors.Add(new ErrorRecord((Exception) new ParentContainsErrorRecordException("MamlInvalidChildNodeError"), "MamlInvalidChildNodeError", ErrorCategory.SyntaxError, (object) null)
    {
      ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "MamlInvalidChildNodeError", new object[3]
      {
        (object) node.LocalName,
        (object) childNode.LocalName,
        (object) MamlNode.GetNodePath(node)
      })
    });

    private void WriteMamlInvalidChildNodeCountError(XmlNode node, string childNodeName, int count) => this.Errors.Add(new ErrorRecord((Exception) new ParentContainsErrorRecordException("MamlInvalidChildNodeCountError"), "MamlInvalidChildNodeCountError", ErrorCategory.SyntaxError, (object) null)
    {
      ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "MamlInvalidChildNodeCountError", new object[4]
      {
        (object) node.LocalName,
        (object) childNodeName,
        (object) count,
        (object) MamlNode.GetNodePath(node)
      })
    });

    private static string GetNodePath(XmlNode xmlNode)
    {
      if (xmlNode == null)
        return "";
      return xmlNode.ParentNode == null ? "\\" + xmlNode.LocalName : MamlNode.GetNodePath(xmlNode.ParentNode) + "\\" + xmlNode.LocalName + MamlNode.GetNodeIndex(xmlNode);
    }

    private static string GetNodeIndex(XmlNode xmlNode)
    {
      if (xmlNode == null || xmlNode.ParentNode == null)
        return "";
      int num1 = 0;
      int num2 = 0;
      foreach (XmlNode childNode in xmlNode.ParentNode.ChildNodes)
      {
        if (childNode == xmlNode)
          num1 = num2++;
        else if (childNode.LocalName.Equals(xmlNode.LocalName, StringComparison.OrdinalIgnoreCase))
          ++num2;
      }
      return num2 > 1 ? "[" + num1.ToString("d", (IFormatProvider) CultureInfo.CurrentCulture) + "]" : "";
    }

    private static PSObject GetParaPSObject(XmlNode xmlNode)
    {
      if (xmlNode == null)
        return (PSObject) null;
      if (!xmlNode.LocalName.Equals("para", StringComparison.OrdinalIgnoreCase))
        return (PSObject) null;
      PSObject psObject = new PSObject();
      psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("Text", (object) xmlNode.InnerText.Trim()));
      psObject.TypeNames.Clear();
      psObject.TypeNames.Add("MamlParaTextItem");
      psObject.TypeNames.Add("MamlTextItem");
      return psObject;
    }

    private ArrayList GetListPSObjects(XmlNode xmlNode)
    {
      ArrayList arrayList = new ArrayList();
      if (xmlNode == null || !xmlNode.LocalName.Equals("list", StringComparison.OrdinalIgnoreCase) || (xmlNode.ChildNodes == null || xmlNode.ChildNodes.Count == 0))
        return arrayList;
      bool ordered = MamlNode.IsOrderedList(xmlNode);
      int index = 1;
      foreach (XmlNode childNode in xmlNode.ChildNodes)
      {
        if (childNode.LocalName.Equals("listItem", StringComparison.OrdinalIgnoreCase))
        {
          PSObject listItemPsObject = this.GetListItemPSObject(childNode, ordered, ref index);
          if (listItemPsObject != null)
            arrayList.Add((object) listItemPsObject);
        }
        else
          this.WriteMamlInvalidChildNodeError(xmlNode, childNode);
      }
      return arrayList;
    }

    private static bool IsOrderedList(XmlNode xmlNode)
    {
      if (xmlNode == null || xmlNode.Attributes == null || xmlNode.Attributes.Count == 0)
        return false;
      foreach (XmlNode attribute in (XmlNamedNodeMap) xmlNode.Attributes)
      {
        if (attribute.Name.Equals("class", StringComparison.OrdinalIgnoreCase) && attribute.Value.Equals("ordered", StringComparison.OrdinalIgnoreCase))
          return true;
      }
      return false;
    }

    private PSObject GetListItemPSObject(XmlNode xmlNode, bool ordered, ref int index)
    {
      if (xmlNode == null)
        return (PSObject) null;
      if (!xmlNode.LocalName.Equals("listItem", StringComparison.OrdinalIgnoreCase))
        return (PSObject) null;
      string str1 = "";
      if (xmlNode.ChildNodes.Count > 1)
        this.WriteMamlInvalidChildNodeCountError(xmlNode, "para", 1);
      foreach (XmlNode childNode in xmlNode.ChildNodes)
      {
        if (childNode.LocalName.Equals("para", StringComparison.OrdinalIgnoreCase))
          str1 = childNode.InnerText.Trim();
        else
          this.WriteMamlInvalidChildNodeError(xmlNode, childNode);
      }
      string str2;
      if (ordered)
      {
        str2 = index.ToString("d2", (IFormatProvider) CultureInfo.CurrentCulture) + ". ";
        ++index;
      }
      else
        str2 = "* ";
      PSObject psObject = new PSObject();
      psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("Text", (object) str1));
      psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("Tag", (object) str2));
      psObject.TypeNames.Clear();
      if (ordered)
        psObject.TypeNames.Add("MamlOrderedListTextItem");
      else
        psObject.TypeNames.Add("MamlUnorderedListTextItem");
      psObject.TypeNames.Add("MamlTextItem");
      return psObject;
    }

    private ArrayList GetDefinitionListPSObjects(XmlNode xmlNode)
    {
      ArrayList arrayList = new ArrayList();
      if (xmlNode == null || !xmlNode.LocalName.Equals("definitionList", StringComparison.OrdinalIgnoreCase) || (xmlNode.ChildNodes == null || xmlNode.ChildNodes.Count == 0))
        return arrayList;
      foreach (XmlNode childNode in xmlNode.ChildNodes)
      {
        if (childNode.LocalName.Equals("definitionListItem", StringComparison.OrdinalIgnoreCase))
        {
          PSObject listItemPsObject = this.GetDefinitionListItemPSObject(childNode);
          if (listItemPsObject != null)
            arrayList.Add((object) listItemPsObject);
        }
        else
          this.WriteMamlInvalidChildNodeError(xmlNode, childNode);
      }
      return arrayList;
    }

    private PSObject GetDefinitionListItemPSObject(XmlNode xmlNode)
    {
      if (xmlNode == null)
        return (PSObject) null;
      if (!xmlNode.LocalName.Equals("definitionListItem", StringComparison.OrdinalIgnoreCase))
        return (PSObject) null;
      string str1 = (string) null;
      string str2 = (string) null;
      foreach (XmlNode childNode in xmlNode.ChildNodes)
      {
        if (childNode.LocalName.Equals("term", StringComparison.OrdinalIgnoreCase))
          str1 = childNode.InnerText.Trim();
        else if (childNode.LocalName.Equals("definition", StringComparison.OrdinalIgnoreCase))
          str2 = this.GetDefinitionText(childNode);
        else
          this.WriteMamlInvalidChildNodeError(xmlNode, childNode);
      }
      if (string.IsNullOrEmpty(str1))
        return (PSObject) null;
      PSObject psObject = new PSObject();
      psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("Term", (object) str1));
      psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("Definition", (object) str2));
      psObject.TypeNames.Clear();
      psObject.TypeNames.Add("MamlDefinitionTextItem");
      psObject.TypeNames.Add("MamlTextItem");
      return psObject;
    }

    private string GetDefinitionText(XmlNode xmlNode)
    {
      if (xmlNode == null)
        return (string) null;
      if (!xmlNode.LocalName.Equals("definition", StringComparison.OrdinalIgnoreCase))
        return (string) null;
      if (xmlNode.ChildNodes == null || xmlNode.ChildNodes.Count == 0)
        return "";
      if (xmlNode.ChildNodes.Count > 1)
        this.WriteMamlInvalidChildNodeCountError(xmlNode, "para", 1);
      string str = "";
      foreach (XmlNode childNode in xmlNode.ChildNodes)
      {
        if (childNode.LocalName.Equals("para", StringComparison.OrdinalIgnoreCase))
          str = childNode.InnerText.Trim();
        else
          this.WriteMamlInvalidChildNodeError(xmlNode, childNode);
      }
      return str;
    }

    private static string GetPreformattedText(string text)
    {
      string[] lines = MamlNode.TrimLines(text.Replace("\t", "    ").Split('\n'));
      if (lines == null || lines.Length == 0)
        return "";
      int minIndentation = MamlNode.GetMinIndentation(lines);
      string[] strArray = new string[lines.Length];
      for (int index = 0; index < lines.Length; ++index)
        strArray[index] = !MamlNode.IsEmptyLine(lines[index]) ? lines[index].Remove(0, minIndentation) : lines[index];
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < strArray.Length; ++index)
        stringBuilder.AppendLine(strArray[index]);
      return stringBuilder.ToString();
    }

    private static string[] TrimLines(string[] lines)
    {
      if (lines == null || lines.Length == 0)
        return (string[]) null;
      int index1 = 0;
      while (index1 < lines.Length && MamlNode.IsEmptyLine(lines[index1]))
        ++index1;
      int num1 = index1;
      if (num1 == lines.Length)
        return (string[]) null;
      int index2 = lines.Length - 1;
      while (index2 >= num1 && MamlNode.IsEmptyLine(lines[index2]))
        --index2;
      int num2 = index2;
      string[] strArray = new string[num2 - num1 + 1];
      for (int index3 = num1; index3 <= num2; ++index3)
        strArray[index3 - num1] = lines[index3];
      return strArray;
    }

    private static int GetMinIndentation(string[] lines)
    {
      int num = -1;
      for (int index = 0; index < lines.Length; ++index)
      {
        if (!MamlNode.IsEmptyLine(lines[index]))
        {
          int indentation = MamlNode.GetIndentation(lines[index]);
          if (num < 0 || indentation < num)
            num = indentation;
        }
      }
      return num;
    }

    private static int GetIndentation(string line)
    {
      if (MamlNode.IsEmptyLine(line))
        return 0;
      string str = line.TrimStart(' ');
      return line.Length - str.Length;
    }

    private static bool IsEmptyLine(string line) => string.IsNullOrEmpty(line) || string.IsNullOrEmpty(line.Trim());

    internal Collection<ErrorRecord> Errors
    {
      get
      {
        using (MamlNode.tracer.TraceProperty())
          return this._errors;
      }
    }
  }
}
