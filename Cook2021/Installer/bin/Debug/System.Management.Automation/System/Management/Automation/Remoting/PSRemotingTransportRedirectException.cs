// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.PSRemotingTransportRedirectException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation.Remoting
{
  [Serializable]
  public class PSRemotingTransportRedirectException : PSRemotingTransportException
  {
    private string redirectLocation;

    public PSRemotingTransportRedirectException()
      : base(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.DefaultRemotingExceptionMessage, (object) typeof (PSRemotingTransportRedirectException).FullName))
      => this.SetDefaultErrorRecord();

    public PSRemotingTransportRedirectException(string message)
      : base(message)
    {
    }

    public PSRemotingTransportRedirectException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    internal PSRemotingTransportRedirectException(
      Exception innerException,
      PSRemotingErrorId errorId,
      params object[] args)
      : base(innerException, errorId, args)
    {
    }

    protected PSRemotingTransportRedirectException(SerializationInfo info, StreamingContext context)
      : base(info, context)
      => this.redirectLocation = info != null ? info.GetString(nameof (RedirectLocation)) : throw new PSArgumentNullException(nameof (info));

    internal PSRemotingTransportRedirectException(
      string redirectLocation,
      PSRemotingErrorId errorId,
      params object[] args)
      : base(errorId, args)
    {
      this.redirectLocation = redirectLocation;
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new PSArgumentNullException(nameof (info));
      base.GetObjectData(info, context);
      info.AddValue("RedirectLocation", (object) this.redirectLocation);
    }

    public string RedirectLocation => this.redirectLocation;
  }
}
