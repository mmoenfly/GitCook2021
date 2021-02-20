// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CmdletProviderInvocationException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class CmdletProviderInvocationException : CmdletInvocationException
  {
    [NonSerialized]
    private ProviderInvocationException _providerInvocationException;

    internal CmdletProviderInvocationException(
      ProviderInvocationException innerException,
      InvocationInfo myInvocation)
      : base(CmdletProviderInvocationException.GetInnerException((Exception) innerException), myInvocation)
    {
      this._providerInvocationException = innerException != null ? innerException : throw new ArgumentNullException(nameof (innerException));
    }

    public CmdletProviderInvocationException()
    {
    }

    protected CmdletProviderInvocationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
      => this._providerInvocationException = this.InnerException as ProviderInvocationException;

    public CmdletProviderInvocationException(string message)
      : base(message)
    {
    }

    public CmdletProviderInvocationException(string message, Exception innerException)
      : base(message, innerException)
      => this._providerInvocationException = innerException as ProviderInvocationException;

    public ProviderInvocationException ProviderInvocationException => this._providerInvocationException;

    public ProviderInfo ProviderInfo => this._providerInvocationException != null ? this._providerInvocationException.ProviderInfo : (ProviderInfo) null;

    private static Exception GetInnerException(Exception e) => e?.InnerException;
  }
}
