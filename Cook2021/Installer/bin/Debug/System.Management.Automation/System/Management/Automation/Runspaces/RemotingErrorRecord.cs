// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.RemotingErrorRecord
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Remoting;

namespace System.Management.Automation.Runspaces
{
  public class RemotingErrorRecord : ErrorRecord
  {
    private OriginInfo originInfo;

    public OriginInfo OriginInfo => this.originInfo;

    public RemotingErrorRecord(ErrorRecord errorRecord, OriginInfo originInfo)
      : base(errorRecord, (Exception) null)
    {
      if (errorRecord != null)
        this.SetInvocationInfo(errorRecord.InvocationInfo);
      this.originInfo = originInfo;
    }
  }
}
