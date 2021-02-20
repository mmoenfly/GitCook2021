// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.TypeAccelerators
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;

namespace System.Management.Automation
{
  internal static class TypeAccelerators
  {
    internal static Dictionary<string, Type> builtinTypeAccelerators = new Dictionary<string, Type>(64, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    internal static Dictionary<string, Type> userTypeAccelerators = new Dictionary<string, Type>(64, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private static Dictionary<string, Type> allTypeAccelerators = (Dictionary<string, Type>) null;

    static TypeAccelerators()
    {
      TypeAccelerators.builtinTypeAccelerators.Add("int", typeof (int));
      TypeAccelerators.builtinTypeAccelerators.Add("long", typeof (long));
      TypeAccelerators.builtinTypeAccelerators.Add("string", typeof (string));
      TypeAccelerators.builtinTypeAccelerators.Add("char", typeof (char));
      TypeAccelerators.builtinTypeAccelerators.Add("bool", typeof (bool));
      TypeAccelerators.builtinTypeAccelerators.Add("byte", typeof (byte));
      TypeAccelerators.builtinTypeAccelerators.Add("double", typeof (double));
      TypeAccelerators.builtinTypeAccelerators.Add("decimal", typeof (Decimal));
      TypeAccelerators.builtinTypeAccelerators.Add("float", typeof (float));
      TypeAccelerators.builtinTypeAccelerators.Add("single", typeof (float));
      TypeAccelerators.builtinTypeAccelerators.Add("regex", typeof (Regex));
      TypeAccelerators.builtinTypeAccelerators.Add("array", typeof (Array));
      TypeAccelerators.builtinTypeAccelerators.Add("xml", typeof (XmlDocument));
      TypeAccelerators.builtinTypeAccelerators.Add("scriptblock", typeof (ScriptBlock));
      TypeAccelerators.builtinTypeAccelerators.Add("switch", typeof (SwitchParameter));
      TypeAccelerators.builtinTypeAccelerators.Add("hashtable", typeof (Hashtable));
      TypeAccelerators.builtinTypeAccelerators.Add("type", typeof (Type));
      TypeAccelerators.builtinTypeAccelerators.Add("ref", typeof (PSReference));
      TypeAccelerators.builtinTypeAccelerators.Add("psobject", typeof (PSObject));
      TypeAccelerators.builtinTypeAccelerators.Add("pscustomobject", typeof (PSObject));
      TypeAccelerators.builtinTypeAccelerators.Add("psmoduleinfo", typeof (PSModuleInfo));
      TypeAccelerators.builtinTypeAccelerators.Add("powershell", typeof (PowerShell));
      TypeAccelerators.builtinTypeAccelerators.Add("runspacefactory", typeof (RunspaceFactory));
      TypeAccelerators.builtinTypeAccelerators.Add("runspace", typeof (Runspace));
      TypeAccelerators.builtinTypeAccelerators.Add("ipaddress", typeof (IPAddress));
      TypeAccelerators.builtinTypeAccelerators.Add("wmi", typeof (ManagementObject));
      TypeAccelerators.builtinTypeAccelerators.Add("wmisearcher", typeof (ManagementObjectSearcher));
      TypeAccelerators.builtinTypeAccelerators.Add("wmiclass", typeof (ManagementClass));
      TypeAccelerators.builtinTypeAccelerators.Add("adsi", typeof (DirectoryEntry));
      TypeAccelerators.builtinTypeAccelerators.Add("adsisearcher", typeof (DirectorySearcher));
      TypeAccelerators.builtinTypeAccelerators.Add("psprimitivedictionary", typeof (PSPrimitiveDictionary));
    }

    internal static string FindBuiltinAccelerator(Type type)
    {
      foreach (KeyValuePair<string, Type> builtinTypeAccelerator in TypeAccelerators.builtinTypeAccelerators)
      {
        if (builtinTypeAccelerator.Value.Equals(type))
          return builtinTypeAccelerator.Key;
      }
      return (string) null;
    }

    public static void Add(string typeName, Type type)
    {
      lock (LanguagePrimitives.stringToTypeCache)
      {
        TypeAccelerators.userTypeAccelerators.Add(typeName, type);
        if (TypeAccelerators.allTypeAccelerators != null)
          TypeAccelerators.allTypeAccelerators.Add(typeName, type);
        LanguagePrimitives.stringToTypeCache.Add(typeName, type);
      }
    }

    public static bool Remove(string typeName)
    {
      lock (LanguagePrimitives.stringToTypeCache)
      {
        TypeAccelerators.userTypeAccelerators.Remove(typeName);
        if (TypeAccelerators.allTypeAccelerators != null)
          TypeAccelerators.allTypeAccelerators.Remove(typeName);
        return LanguagePrimitives.stringToTypeCache.Remove(typeName);
      }
    }

    public static Dictionary<string, Type> Get
    {
      get
      {
        if (TypeAccelerators.allTypeAccelerators == null)
        {
          TypeAccelerators.allTypeAccelerators = new Dictionary<string, Type>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
          TypeAccelerators.FillCache(TypeAccelerators.allTypeAccelerators);
        }
        return TypeAccelerators.allTypeAccelerators;
      }
    }

    internal static void FillCache(Dictionary<string, Type> cache)
    {
      foreach (KeyValuePair<string, Type> builtinTypeAccelerator in TypeAccelerators.builtinTypeAccelerators)
        cache.Add(builtinTypeAccelerator.Key, builtinTypeAccelerator.Value);
      foreach (KeyValuePair<string, Type> userTypeAccelerator in TypeAccelerators.userTypeAccelerators)
        cache.Add(userTypeAccelerator.Key, userTypeAccelerator.Value);
    }
  }
}
