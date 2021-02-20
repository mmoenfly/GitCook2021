// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.Server.OutOfProcessServerTransportManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation.Remoting.Server
{
  internal class OutOfProcessServerTransportManager : AbstractServerTransportManager
  {
    private OutOfProcessTextWriter stdOutWriter;
    private Guid powershellInstanceId;
    private bool isDataAckSendPending;

    internal OutOfProcessServerTransportManager(
      OutOfProcessTextWriter stdOutWriter,
      Guid powershellInstanceId,
      TypeTable typeTableToUse,
      int fragmentSize,
      PSRemotingCryptoHelper cryptoHelper)
      : base(fragmentSize, cryptoHelper)
    {
      this.stdOutWriter = stdOutWriter;
      this.powershellInstanceId = powershellInstanceId;
      this.TypeTable = typeTableToUse;
    }

    internal override void ProcessRawData(byte[] data, string stream)
    {
      this.isDataAckSendPending = true;
      base.ProcessRawData(data, stream);
      if (!this.isDataAckSendPending)
        return;
      this.isDataAckSendPending = false;
      this.stdOutWriter.WriteLine(OutOfProcessUtils.CreateDataAckPacket(this.powershellInstanceId));
    }

    protected override void SendDataToClient(byte[] data, bool flush) => this.stdOutWriter.WriteLine(OutOfProcessUtils.CreateDataPacket(data, DataPriorityType.Default, this.powershellInstanceId));

    internal override void Prepare()
    {
      if (!this.isDataAckSendPending)
        return;
      this.isDataAckSendPending = false;
      base.Prepare();
      this.stdOutWriter.WriteLine(OutOfProcessUtils.CreateDataAckPacket(this.powershellInstanceId));
    }

    internal override void Close(Exception reasonForClose) => this.RaiseClosingEvent();
  }
}
