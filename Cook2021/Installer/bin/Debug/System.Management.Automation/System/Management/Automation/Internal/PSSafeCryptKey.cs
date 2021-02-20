// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSSafeCryptKey
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;

namespace System.Management.Automation.Internal
{
  internal class PSSafeCryptKey : SafeHandleZeroOrMinusOneIsInvalid
  {
    private static PSSafeCryptKey _zero = new PSSafeCryptKey();

    internal PSSafeCryptKey()
      : base(true)
    {
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    protected override bool ReleaseHandle() => PSCryptoNativeUtils.CryptDestroyKey(this.handle);

    internal static PSSafeCryptKey Zero => PSSafeCryptKey._zero;
  }
}
