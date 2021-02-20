// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.RegistryWrapper
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32;
using System.Security.AccessControl;

namespace Microsoft.PowerShell.Commands
{
  internal class RegistryWrapper : IRegistryWrapper
  {
    private Microsoft.Win32.RegistryKey regKey;

    internal RegistryWrapper(Microsoft.Win32.RegistryKey regKey) => this.regKey = regKey;

    public void SetValue(string name, object value) => this.regKey.SetValue(name, value);

    public void SetValue(string name, object value, RegistryValueKind valueKind) => this.regKey.SetValue(name, value, valueKind);

    public string[] GetValueNames() => this.regKey.GetValueNames();

    public void DeleteValue(string name) => this.regKey.DeleteValue(name);

    public string[] GetSubKeyNames() => this.regKey.GetSubKeyNames();

    public IRegistryWrapper CreateSubKey(string subkey)
    {
      Microsoft.Win32.RegistryKey subKey = this.regKey.CreateSubKey(subkey);
      return subKey == null ? (IRegistryWrapper) null : (IRegistryWrapper) new RegistryWrapper(subKey);
    }

    public IRegistryWrapper OpenSubKey(string name, bool writable)
    {
      Microsoft.Win32.RegistryKey regKey = this.regKey.OpenSubKey(name, writable);
      return regKey == null ? (IRegistryWrapper) null : (IRegistryWrapper) new RegistryWrapper(regKey);
    }

    public void DeleteSubKeyTree(string subkey) => this.regKey.DeleteSubKeyTree(subkey);

    public object GetValue(string name) => this.regKey.GetValue(name);

    public object GetValue(string name, object defaultValue, RegistryValueOptions options) => this.regKey.GetValue(name, defaultValue, options);

    public RegistryValueKind GetValueKind(string name) => this.regKey.GetValueKind(name);

    public void Close() => this.regKey.Close();

    public string Name => this.regKey.Name;

    public int SubKeyCount => this.regKey.SubKeyCount;

    public object RegistryKey => (object) this.regKey;

    public void SetAccessControl(ObjectSecurity securityDescriptor) => this.regKey.SetAccessControl((RegistrySecurity) securityDescriptor);

    public ObjectSecurity GetAccessControl(AccessControlSections includeSections) => (ObjectSecurity) this.regKey.GetAccessControl(includeSections);
  }
}
