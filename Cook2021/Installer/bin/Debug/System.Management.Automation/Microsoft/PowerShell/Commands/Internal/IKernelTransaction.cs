// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.IKernelTransaction
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands.Internal
{
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("79427A2B-F895-40e0-BE79-B57DC82ED231")]
  internal interface IKernelTransaction
  {
    int GetHandle(out IntPtr pHandle);
  }
}
