// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Provider.IPropertyCmdletProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation.Provider
{
  public interface IPropertyCmdletProvider
  {
    void GetProperty(string path, Collection<string> providerSpecificPickList);

    object GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList);

    void SetProperty(string path, PSObject propertyValue);

    object SetPropertyDynamicParameters(string path, PSObject propertyValue);

    void ClearProperty(string path, Collection<string> propertyToClear);

    object ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear);
  }
}
