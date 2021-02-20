// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.PSRemotingDataStructureException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation.Remoting
{
  [Serializable]
  public class PSRemotingDataStructureException : RuntimeException
  {
    public PSRemotingDataStructureException()
      : base(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.DefaultRemotingExceptionMessage, (object) typeof (PSRemotingDataStructureException).FullName))
      => this.SetDefaultErrorRecord();

    public PSRemotingDataStructureException(string message)
      : base(message)
      => this.SetDefaultErrorRecord();

    public PSRemotingDataStructureException(string message, Exception innerException)
      : base(message, innerException)
      => this.SetDefaultErrorRecord();

    internal PSRemotingDataStructureException(PSRemotingErrorId errorId, params object[] args)
      : base(PSRemotingErrorInvariants.FormatResourceString(errorId, args))
      => this.SetDefaultErrorRecord();

    internal PSRemotingDataStructureException(
      Exception innerException,
      PSRemotingErrorId errorId,
      params object[] args)
      : base(PSRemotingErrorInvariants.FormatResourceString(errorId, args), innerException)
    {
      this.SetDefaultErrorRecord();
    }

    protected PSRemotingDataStructureException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    private void SetDefaultErrorRecord()
    {
      this.SetErrorCategory(ErrorCategory.ResourceUnavailable);
      this.SetErrorId(typeof (PSRemotingDataStructureException).FullName);
    }
  }
}
