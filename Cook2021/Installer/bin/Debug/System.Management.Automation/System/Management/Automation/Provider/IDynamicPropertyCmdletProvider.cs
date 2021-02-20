// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Provider.IDynamicPropertyCmdletProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Provider
{
  public interface IDynamicPropertyCmdletProvider : IPropertyCmdletProvider
  {
    void NewProperty(string path, string propertyName, string propertyTypeName, object value);

    object NewPropertyDynamicParameters(
      string path,
      string propertyName,
      string propertyTypeName,
      object value);

    void RemoveProperty(string path, string propertyName);

    object RemovePropertyDynamicParameters(string path, string propertyName);

    void RenameProperty(string path, string sourceProperty, string destinationProperty);

    object RenamePropertyDynamicParameters(
      string path,
      string sourceProperty,
      string destinationProperty);

    void CopyProperty(
      string sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty);

    object CopyPropertyDynamicParameters(
      string sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty);

    void MoveProperty(
      string sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty);

    object MovePropertyDynamicParameters(
      string sourcePath,
      string sourceProperty,
      string destinationPath,
      string destinationProperty);
  }
}
