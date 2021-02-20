// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.SafeTransactionHandle
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Management.Automation;
using System.Security;
using System.Transactions;

namespace Microsoft.PowerShell.Commands.Internal
{
  [SuppressUnmanagedCodeSecurity]
  internal sealed class SafeTransactionHandle : SafeHandleZeroOrMinusOneIsInvalid
  {
    private const string resBaseName = "RegistryProviderStrings";

    private SafeTransactionHandle(IntPtr handle)
      : base(true)
      => this.handle = handle;

    internal static SafeTransactionHandle Create() => SafeTransactionHandle.Create(Transaction.Current);

    internal static SafeTransactionHandle Create(Transaction managedTransaction)
    {
      if (managedTransaction == (Transaction) null)
        throw new InvalidOperationException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "InvalidOperation_NeedTransaction"));
      if (!(TransactionInterop.GetDtcTransaction(managedTransaction) is IKernelTransaction dtcTransaction))
        throw new NotSupportedException(ResourceManagerCache.GetResourceString("RegistryProviderStrings", "NotSupported_KernelTransactions"));
      IntPtr pHandle;
      SafeTransactionHandle.HandleError(dtcTransaction.GetHandle(out pHandle));
      return new SafeTransactionHandle(pHandle);
    }

    protected override bool ReleaseHandle() => Win32Native.CloseHandle(this.handle);

    private static void HandleError(int error)
    {
      if (error != 0)
        throw new Win32Exception(error);
    }
  }
}
