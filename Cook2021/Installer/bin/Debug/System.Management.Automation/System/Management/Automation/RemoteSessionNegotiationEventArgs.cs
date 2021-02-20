// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemoteSessionNegotiationEventArgs
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Remoting;

namespace System.Management.Automation
{
  internal sealed class RemoteSessionNegotiationEventArgs : EventArgs
  {
    [TraceSource("RSNEA", "RemoteSessionNegotiationEventArgs")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("RSNEA", nameof (RemoteSessionNegotiationEventArgs));
    private RemoteSessionCapability _remoteSessionCapability;
    private RemoteDataObject<PSObject> _remoteObject;

    internal RemoteSessionNegotiationEventArgs(RemoteSessionCapability remoteSessionCapability)
    {
      using (RemoteSessionNegotiationEventArgs._trace.TraceConstructor((object) this))
        this._remoteSessionCapability = remoteSessionCapability != null ? remoteSessionCapability : throw RemoteSessionNegotiationEventArgs._trace.NewArgumentNullException(nameof (remoteSessionCapability));
    }

    internal RemoteSessionCapability RemoteSessionCapability
    {
      get
      {
        using (RemoteSessionNegotiationEventArgs._trace.TraceProperty())
          return this._remoteSessionCapability;
      }
    }

    internal RemoteDataObject<PSObject> RemoteData
    {
      get => this._remoteObject;
      set => this._remoteObject = value;
    }
  }
}
