// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.HostInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Host;
using System.Management.Automation.Internal.Host;

namespace System.Management.Automation.Remoting
{
  internal class HostInfo
  {
    private HostDefaultData _hostDefaultData;
    private bool _isHostNull;
    private bool _isHostUINull;
    private bool _isHostRawUINull;
    private bool _useRunspaceHost;

    internal HostDefaultData HostDefaultData => this._hostDefaultData;

    internal bool IsHostNull => this._isHostNull;

    internal bool IsHostUINull => this._isHostUINull;

    internal bool IsHostRawUINull => this._isHostRawUINull;

    internal bool UseRunspaceHost
    {
      get => this._useRunspaceHost;
      set => this._useRunspaceHost = value;
    }

    internal HostInfo(PSHost host)
    {
      HostInfo.CheckHostChain(host, ref this._isHostNull, ref this._isHostUINull, ref this._isHostRawUINull);
      if (this._isHostUINull || this._isHostRawUINull)
        return;
      this._hostDefaultData = HostDefaultData.Create(host.UI.RawUI);
    }

    private static void CheckHostChain(
      PSHost host,
      ref bool isHostNull,
      ref bool isHostUINull,
      ref bool isHostRawUINull)
    {
      isHostNull = true;
      isHostUINull = true;
      isHostRawUINull = true;
      if (host == null)
        return;
      if (host is InternalHost)
        host = ((InternalHost) host).ExternalHost;
      isHostNull = false;
      if (host.UI == null)
        return;
      isHostUINull = false;
      if (host.UI.RawUI == null)
        return;
      isHostRawUINull = false;
    }
  }
}
