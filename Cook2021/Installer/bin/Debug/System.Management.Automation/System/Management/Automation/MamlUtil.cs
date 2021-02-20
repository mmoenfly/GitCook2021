// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.MamlUtil
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  internal class MamlUtil
  {
    internal static void OverrideName(PSObject maml1, PSObject maml2)
    {
      MamlUtil.PrependPropertyValue(maml1, maml2, new string[1]
      {
        "Name"
      }, true);
      MamlUtil.PrependPropertyValue(maml1, maml2, new string[2]
      {
        "Details",
        "Name"
      }, true);
    }

    internal static void PrependSyntax(PSObject maml1, PSObject maml2) => MamlUtil.PrependPropertyValue(maml1, maml2, new string[2]
    {
      "Syntax",
      "SyntaxItem"
    }, false);

    internal static void PrependDetailedDescription(PSObject maml1, PSObject maml2) => MamlUtil.PrependPropertyValue(maml1, maml2, new string[1]
    {
      "Description"
    }, false);

    internal static void OverrideParameters(PSObject maml1, PSObject maml2)
    {
      string[] path = new string[2]
      {
        "Parameters",
        "Parameter"
      };
      List<PSObject> psObjectList1 = new List<PSObject>();
      PSPropertyInfo properyInfo1 = MamlUtil.GetProperyInfo(maml2, path);
      if (properyInfo1.Value is PSObject[])
        psObjectList1.AddRange((IEnumerable<PSObject>) (PSObject[]) properyInfo1.Value);
      else
        psObjectList1.Add(PSObject.AsPSObject(properyInfo1.Value));
      MamlUtil.EnsurePropertyInfoPathExists(maml1, path);
      PSPropertyInfo properyInfo2 = MamlUtil.GetProperyInfo(maml1, path);
      List<PSObject> psObjectList2 = new List<PSObject>();
      if (properyInfo2.Value is PSObject[])
        psObjectList2.AddRange((IEnumerable<PSObject>) (PSObject[]) properyInfo2.Value);
      else
        psObjectList2.Add(PSObject.AsPSObject(properyInfo2.Value));
      for (int index = 0; index < psObjectList1.Count; ++index)
      {
        string result1;
        if (LanguagePrimitives.TryConvertTo<string>(psObjectList1[index].Properties["Name"].Value, out result1))
        {
          bool flag = false;
          foreach (PSObject psObject in psObjectList2)
          {
            string result2;
            if (LanguagePrimitives.TryConvertTo<string>(psObject.Properties["Name"].Value, out result2) && result2.Equals(result1, StringComparison.OrdinalIgnoreCase))
              flag = true;
          }
          if (!flag)
            psObjectList2.Add(psObjectList1[index]);
        }
      }
      if (psObjectList2.Count == 1)
      {
        properyInfo2.Value = (object) psObjectList2[0];
      }
      else
      {
        if (psObjectList2.Count < 2)
          return;
        properyInfo2.Value = (object) psObjectList2.ToArray();
      }
    }

    internal static void PrependNotes(PSObject maml1, PSObject maml2) => MamlUtil.PrependPropertyValue(maml1, maml2, new string[2]
    {
      "AlertSet",
      "Alert"
    }, false);

    internal static PSPropertyInfo GetProperyInfo(PSObject psObject, string[] path)
    {
      if (path.Length <= 0)
        return (PSPropertyInfo) null;
      for (int index = 0; index < path.Length; ++index)
      {
        string name = path[index];
        PSPropertyInfo property = psObject.Properties[name];
        if (index == path.Length - 1)
          return property;
        if (property == null || !(property.Value is PSObject))
          return (PSPropertyInfo) null;
        psObject = (PSObject) property.Value;
      }
      return (PSPropertyInfo) null;
    }

    internal static void PrependPropertyValue(
      PSObject maml1,
      PSObject maml2,
      string[] path,
      bool shouldOverride)
    {
      List<object> objectList = new List<object>();
      PSPropertyInfo properyInfo1 = MamlUtil.GetProperyInfo(maml2, path);
      if (properyInfo1.Value is PSObject[])
        objectList.AddRange((IEnumerable<object>) (PSObject[]) properyInfo1.Value);
      else
        objectList.Add(properyInfo1.Value);
      MamlUtil.EnsurePropertyInfoPathExists(maml1, path);
      PSPropertyInfo properyInfo2 = MamlUtil.GetProperyInfo(maml1, path);
      if (!shouldOverride)
      {
        if (properyInfo2.Value is PSObject[])
          objectList.AddRange((IEnumerable<object>) (PSObject[]) properyInfo2.Value);
        else
          objectList.Add(properyInfo2.Value);
      }
      if (objectList.Count == 1)
      {
        properyInfo2.Value = objectList[0];
      }
      else
      {
        if (objectList.Count < 2)
          return;
        properyInfo2.Value = (object) objectList.ToArray();
      }
    }

    internal static void EnsurePropertyInfoPathExists(PSObject psObject, string[] path)
    {
      if (path.Length <= 0)
        return;
      for (int index = 0; index < path.Length; ++index)
      {
        string name = path[index];
        PSPropertyInfo member = psObject.Properties[name];
        if (member == null)
        {
          object obj = index < path.Length - 1 ? (object) new PSObject() : (object) (PSObject) null;
          member = (PSPropertyInfo) new PSNoteProperty(name, obj);
          psObject.Properties.Add(member);
        }
        if (index == path.Length - 1)
          break;
        if (member.Value == null || !(member.Value is PSObject))
          member.Value = (object) new PSObject();
        psObject = (PSObject) member.Value;
      }
    }
  }
}
