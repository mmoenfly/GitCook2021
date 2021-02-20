// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.Client.BaseClientCommandTransportManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Runspaces.Internal;
using System.Text;

namespace System.Management.Automation.Remoting.Client
{
  internal abstract class BaseClientCommandTransportManager : BaseClientTransportManager, IDisposable
  {
    protected StringBuilder cmdText;
    protected SerializedDataStream serializedPipeline;
    protected Guid powershellInstanceId;

    protected BaseClientCommandTransportManager(
      ClientRemotePowerShell shell,
      PSRemotingCryptoHelper cryptoHelper,
      BaseClientSessionTransportManager sessnTM)
      : base(sessnTM.RunspacePoolInstanceId, cryptoHelper)
    {
      this.Fragmentor.FragmentSize = sessnTM.Fragmentor.FragmentSize;
      this.Fragmentor.TypeTable = sessnTM.Fragmentor.TypeTable;
      this.dataToBeSent.Fragmentor = this.Fragmentor;
      this.powershellInstanceId = shell.PowerShell.InstanceId;
      this.cmdText = new StringBuilder();
      foreach (Command command in (Collection<Command>) shell.PowerShell.Commands.Commands)
      {
        this.cmdText.Append(command.CommandText);
        this.cmdText.Append(" | ");
      }
      this.cmdText.Remove(this.cmdText.Length - 3, 3);
      RemoteDataObject remoteDataObject = !shell.PowerShell.IsGetCommandMetadataSpecialPipeline ? RemotingEncoder.GenerateCreatePowerShell(shell) : RemotingEncoder.GenerateGetCommandMetadata(shell);
      this.serializedPipeline = new SerializedDataStream(this.Fragmentor.FragmentSize);
      this.Fragmentor.Fragment<object>((RemoteDataObject<object>) remoteDataObject, this.serializedPipeline);
    }

    internal event EventHandler<EventArgs> SignalCompleted;

    internal void RaiseSignalCompleted()
    {
      if (this.SignalCompleted == null)
        return;
      this.SignalCompleted((object) this, EventArgs.Empty);
    }

    internal override void Dispose(bool isDisposing)
    {
      base.Dispose(isDisposing);
      this.serializedPipeline.Dispose();
    }

    internal virtual void SendStopSignal() => throw new NotImplementedException();
  }
}
