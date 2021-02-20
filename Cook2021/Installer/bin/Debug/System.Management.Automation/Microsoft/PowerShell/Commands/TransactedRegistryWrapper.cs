// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.TransactedRegistryWrapper
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands.Internal;
using Microsoft.Win32;
using System.Management.Automation.Provider;
using System.Security.AccessControl;

namespace Microsoft.PowerShell.Commands
{
  internal class TransactedRegistryWrapper : IRegistryWrapper
  {
    private TransactedRegistryKey txRegKey;
    private CmdletProvider provider;

    internal TransactedRegistryWrapper(TransactedRegistryKey txRegKey, CmdletProvider provider)
    {
      this.txRegKey = txRegKey;
      this.provider = provider;
    }

    public void SetValue(string name, object value)
    {
      using (this.provider.CurrentPSTransaction)
        this.txRegKey.SetValue(name, value);
    }

    public void SetValue(string name, object value, RegistryValueKind valueKind)
    {
      using (this.provider.CurrentPSTransaction)
        this.txRegKey.SetValue(name, value, valueKind);
    }

    public string[] GetValueNames()
    {
      using (this.provider.CurrentPSTransaction)
        return this.txRegKey.GetValueNames();
    }

    public void DeleteValue(string name)
    {
      using (this.provider.CurrentPSTransaction)
        this.txRegKey.DeleteValue(name);
    }

    public string[] GetSubKeyNames()
    {
      using (this.provider.CurrentPSTransaction)
        return this.txRegKey.GetSubKeyNames();
    }

    public IRegistryWrapper CreateSubKey(string subkey)
    {
      using (this.provider.CurrentPSTransaction)
      {
        TransactedRegistryKey subKey = this.txRegKey.CreateSubKey(subkey);
        return subKey == null ? (IRegistryWrapper) null : (IRegistryWrapper) new TransactedRegistryWrapper(subKey, this.provider);
      }
    }

    public IRegistryWrapper OpenSubKey(string name, bool writable)
    {
      using (this.provider.CurrentPSTransaction)
      {
        TransactedRegistryKey txRegKey = this.txRegKey.OpenSubKey(name, writable);
        return txRegKey == null ? (IRegistryWrapper) null : (IRegistryWrapper) new TransactedRegistryWrapper(txRegKey, this.provider);
      }
    }

    public void DeleteSubKeyTree(string subkey)
    {
      using (this.provider.CurrentPSTransaction)
        this.txRegKey.DeleteSubKeyTree(subkey);
    }

    public object GetValue(string name)
    {
      using (this.provider.CurrentPSTransaction)
        return this.txRegKey.GetValue(name);
    }

    public object GetValue(string name, object defaultValue, RegistryValueOptions options)
    {
      using (this.provider.CurrentPSTransaction)
        return this.txRegKey.GetValue(name, defaultValue, options);
    }

    public RegistryValueKind GetValueKind(string name)
    {
      using (this.provider.CurrentPSTransaction)
        return this.txRegKey.GetValueKind(name);
    }

    public void Close()
    {
      using (this.provider.CurrentPSTransaction)
        this.txRegKey.Close();
    }

    public string Name
    {
      get
      {
        using (this.provider.CurrentPSTransaction)
          return this.txRegKey.Name;
      }
    }

    public int SubKeyCount
    {
      get
      {
        using (this.provider.CurrentPSTransaction)
          return this.txRegKey.SubKeyCount;
      }
    }

    public object RegistryKey => (object) this.txRegKey;

    public void SetAccessControl(ObjectSecurity securityDescriptor)
    {
      using (this.provider.CurrentPSTransaction)
        this.txRegKey.SetAccessControl((TransactedRegistrySecurity) securityDescriptor);
    }

    public ObjectSecurity GetAccessControl(AccessControlSections includeSections)
    {
      using (this.provider.CurrentPSTransaction)
        return (ObjectSecurity) this.txRegKey.GetAccessControl(includeSections);
    }
  }
}
