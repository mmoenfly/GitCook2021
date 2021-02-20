// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemoteSession
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Remoting;

namespace System.Management.Automation
{
  internal abstract class RemoteSession
  {
    private Guid _instanceId = new Guid();
    private BaseSessionDataStructureHandler _dsHandler;

    internal Guid InstanceId => this._instanceId;

    internal abstract RemotingDestination MySelf { get; }

    internal abstract void StartKeyExchange();

    internal abstract void CompleteKeyExchange();

    internal BaseSessionDataStructureHandler BaseSessionDataStructureHandler
    {
      get => this._dsHandler;
      set => this._dsHandler = value;
    }
  }
}
