// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.SafeRegistryHandle
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace Microsoft.PowerShell.Commands.Internal
{
  internal sealed class SafeRegistryHandle : SafeHandleZeroOrMinusOneIsInvalid
  {
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal SafeRegistryHandle()
      : base(true)
    {
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal SafeRegistryHandle(IntPtr preexistingHandle, bool ownsHandle)
      : base(ownsHandle)
      => this.SetHandle(preexistingHandle);

    [SuppressUnmanagedCodeSecurity]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [DllImport("advapi32.dll")]
    internal static extern int RegCloseKey(IntPtr hKey);

    protected override bool ReleaseHandle() => SafeRegistryHandle.RegCloseKey(this.handle) == 0;
  }
}
