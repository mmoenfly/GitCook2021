// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.Server.OutOfProcessServerSessionTransportManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace System.Management.Automation.Remoting.Server
{
  internal class OutOfProcessServerSessionTransportManager : AbstractServerSessionTransportManager
  {
    private OutOfProcessTextWriter stdOutWriter;
    private Dictionary<Guid, OutOfProcessServerTransportManager> cmdTransportManagers;
    private object syncObject = new object();

    internal OutOfProcessServerSessionTransportManager(OutOfProcessTextWriter outWriter)
      : base(32768, (PSRemotingCryptoHelper) new PSRemotingCryptoHelperServer())
    {
      this.stdOutWriter = outWriter;
      this.cmdTransportManagers = new Dictionary<Guid, OutOfProcessServerTransportManager>();
    }

    internal override void ProcessRawData(byte[] data, string stream)
    {
      base.ProcessRawData(data, stream);
      this.stdOutWriter.WriteLine(OutOfProcessUtils.CreateDataAckPacket(Guid.Empty));
    }

    internal override void Prepare() => throw new NotSupportedException();

    protected override void SendDataToClient(byte[] data, bool flush) => this.stdOutWriter.WriteLine(OutOfProcessUtils.CreateDataPacket(data, DataPriorityType.Default, Guid.Empty));

    internal void CreateCommandTransportManager(Guid powerShellCmdId)
    {
      OutOfProcessServerTransportManager transportManager = new OutOfProcessServerTransportManager(this.stdOutWriter, powerShellCmdId, this.TypeTable, this.Fragmentor.FragmentSize, this.CryptoHelper);
      transportManager.MigrateDataReadyEventHandlers((BaseTransportManager) this);
      lock (this.syncObject)
        this.cmdTransportManagers.Add(powerShellCmdId, transportManager);
      this.stdOutWriter.WriteLine(OutOfProcessUtils.CreateCommandAckPacket(powerShellCmdId));
    }

    internal override AbstractServerTransportManager GetCommandTransportManager(
      Guid powerShellCmdId)
    {
      lock (this.syncObject)
      {
        OutOfProcessServerTransportManager transportManager = (OutOfProcessServerTransportManager) null;
        this.cmdTransportManagers.TryGetValue(powerShellCmdId, out transportManager);
        return (AbstractServerTransportManager) transportManager;
      }
    }

    internal override void RemoveCommandTransportManager(Guid powerShellCmdId)
    {
      lock (this.syncObject)
      {
        if (!this.cmdTransportManagers.ContainsKey(powerShellCmdId))
          return;
        this.cmdTransportManagers.Remove(powerShellCmdId);
      }
    }

    internal override void Close(Exception reasonForClose) => this.RaiseClosingEvent();
  }
}
