// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.Client.BaseClientSessionTransportManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Runspaces.Internal;

namespace System.Management.Automation.Remoting.Client
{
  internal abstract class BaseClientSessionTransportManager : BaseClientTransportManager, IDisposable
  {
    protected BaseClientSessionTransportManager(
      Guid runspaceId,
      PSRemotingCryptoHelper cryptoHelper)
      : base(runspaceId, cryptoHelper)
    {
    }

    internal virtual BaseClientCommandTransportManager CreateClientCommandTransportManager(
      RunspaceConnectionInfo connectionInfo,
      ClientRemotePowerShell cmd,
      bool noInput)
    {
      throw new NotImplementedException();
    }

    internal virtual void RemoveCommandTransportManager(Guid powerShellCmdId)
    {
    }

    internal virtual void Redirect(Uri newUri, RunspaceConnectionInfo connectionInfo) => throw new NotImplementedException();

    internal virtual void PrepareForRedirection() => throw new NotImplementedException();
  }
}
