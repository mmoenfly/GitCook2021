// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.BaseRegistryKeys
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;

namespace Microsoft.PowerShell.Commands.Internal
{
  internal sealed class BaseRegistryKeys
  {
    internal static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr(int.MinValue);
    internal static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(-2147483647);
    internal static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(-2147483646);
    internal static readonly IntPtr HKEY_USERS = new IntPtr(-2147483645);
    internal static readonly IntPtr HKEY_CURRENT_CONFIG = new IntPtr(-2147483643);
  }
}
