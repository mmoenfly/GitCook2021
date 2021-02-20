// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.ToStringCodeMethods
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Management.Automation;
using System.Text;

namespace Microsoft.PowerShell
{
  public static class ToStringCodeMethods
  {
    public static string PropertyValueCollection(PSObject instance)
    {
      if (instance == null)
        return string.Empty;
      System.DirectoryServices.PropertyValueCollection baseObject = (System.DirectoryServices.PropertyValueCollection) instance.BaseObject;
      if (baseObject == null)
        return string.Empty;
      if (baseObject.Count != 1)
        return PSObject.ToStringEnumerable(instance.Context, (IEnumerable) baseObject, (string) null, (string) null, (IFormatProvider) null);
      return baseObject[0] == null ? string.Empty : PSObject.AsPSObject(baseObject[0]).ToString();
    }

    public static string XmlNode(PSObject instance)
    {
      if (instance == null)
        return string.Empty;
      System.Xml.XmlNode baseObject = (System.Xml.XmlNode) instance.BaseObject;
      return baseObject == null ? string.Empty : baseObject.LocalName;
    }

    public static string XmlNodeList(PSObject instance)
    {
      if (instance == null)
        return string.Empty;
      System.Xml.XmlNodeList baseObject = (System.Xml.XmlNodeList) instance.BaseObject;
      if (baseObject == null)
        return string.Empty;
      if (baseObject.Count != 1)
        return PSObject.ToStringEnumerable(instance.Context, (IEnumerable) baseObject, (string) null, (string) null, (IFormatProvider) null);
      return baseObject[0] == null ? string.Empty : PSObject.AsPSObject((object) baseObject[0]).ToString();
    }

    internal static string Type(System.Type type)
    {
      if (type == null)
        return string.Empty;
      string typeName;
      if (type.IsGenericType && !type.IsGenericTypeDefinition)
      {
        string str = ToStringCodeMethods.Type(type.GetGenericTypeDefinition());
        int num = str.LastIndexOf('`');
        int length = str.Length - (str.Length - num);
        StringBuilder stringBuilder = new StringBuilder(str, 0, length, 512);
        stringBuilder.Append('[');
        bool flag = true;
        foreach (System.Type genericArgument in type.GetGenericArguments())
        {
          if (!flag)
            stringBuilder.Append(',');
          flag = false;
          stringBuilder.Append(ToStringCodeMethods.Type(genericArgument));
        }
        stringBuilder.Append(']');
        typeName = stringBuilder.ToString();
      }
      else if (type.IsArray)
      {
        string str = ToStringCodeMethods.Type(type.GetElementType());
        StringBuilder stringBuilder = new StringBuilder(str, str.Length + 10);
        stringBuilder.Append("[");
        for (int index = 0; index < type.GetArrayRank() - 1; ++index)
          stringBuilder.Append(",");
        stringBuilder.Append("]");
        typeName = stringBuilder.ToString();
      }
      else
        typeName = TypeAccelerators.FindBuiltinAccelerator(type) ?? type.ToString();
      if (!type.IsGenericParameter && !type.ContainsGenericParameters && LanguagePrimitives.ConvertStringToType(typeName, out Exception _) != type)
        typeName = type.AssemblyQualifiedName;
      return typeName;
    }

    public static string Type(PSObject instance) => instance == null ? string.Empty : ToStringCodeMethods.Type((System.Type) instance.BaseObject);
  }
}
