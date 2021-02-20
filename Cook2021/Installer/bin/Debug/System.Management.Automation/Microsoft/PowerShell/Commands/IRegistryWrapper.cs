// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.IRegistryWrapper
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32;
using System.Security.AccessControl;

namespace Microsoft.PowerShell.Commands
{
  internal interface IRegistryWrapper
  {
    void SetValue(string name, object value);

    void SetValue(string name, object value, RegistryValueKind valueKind);

    string[] GetValueNames();

    void DeleteValue(string name);

    string[] GetSubKeyNames();

    IRegistryWrapper CreateSubKey(string subkey);

    IRegistryWrapper OpenSubKey(string name, bool writable);

    void DeleteSubKeyTree(string subkey);

    object GetValue(string name);

    object GetValue(string name, object defaultValue, RegistryValueOptions options);

    RegistryValueKind GetValueKind(string name);

    object RegistryKey { get; }

    void SetAccessControl(ObjectSecurity securityDescriptor);

    ObjectSecurity GetAccessControl(AccessControlSections includeSections);

    void Close();

    string Name { get; }

    int SubKeyCount { get; }
  }
}
