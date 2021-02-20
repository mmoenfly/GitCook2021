// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SerializationUtilities
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal static class SerializationUtilities
  {
    internal static void AddProperty(PSObject psObject, string propertyName, object propertyValue)
    {
      if (propertyValue == null)
        return;
      psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty(propertyName, propertyValue));
    }

    internal static object GetPropertyValue(PSObject psObject, string propertyName) => psObject.Properties[propertyName]?.Value;

    internal static object GetPsObjectPropertyBaseObject(PSObject psObject, string propertyName) => ((PSObject) SerializationUtilities.GetPropertyValue(psObject, propertyName))?.BaseObject;
  }
}
