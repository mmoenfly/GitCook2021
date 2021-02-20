// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.TransactedRegistry
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands.Internal
{
  [ComVisible(true)]
  internal static class TransactedRegistry
  {
    private const string resBaseName = "RegistryProviderStrings";
    internal static readonly TransactedRegistryKey CurrentUser = TransactedRegistryKey.GetBaseKey(BaseRegistryKeys.HKEY_CURRENT_USER);
    internal static readonly TransactedRegistryKey LocalMachine = TransactedRegistryKey.GetBaseKey(BaseRegistryKeys.HKEY_LOCAL_MACHINE);
    internal static readonly TransactedRegistryKey ClassesRoot = TransactedRegistryKey.GetBaseKey(BaseRegistryKeys.HKEY_CLASSES_ROOT);
    internal static readonly TransactedRegistryKey Users = TransactedRegistryKey.GetBaseKey(BaseRegistryKeys.HKEY_USERS);
    internal static readonly TransactedRegistryKey CurrentConfig = TransactedRegistryKey.GetBaseKey(BaseRegistryKeys.HKEY_CURRENT_CONFIG);
  }
}
