// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemoteSessionStateMachineEventArgs
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Remoting;

namespace System.Management.Automation
{
  internal class RemoteSessionStateMachineEventArgs : EventArgs
  {
    [TraceSource("StateMachineEA", "RemoteSessionStateMachineEventArgs")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("StateMachineEA", nameof (RemoteSessionStateMachineEventArgs));
    private RemoteSessionEvent _stateEvent;
    private RemoteSessionCapability _capability;
    private RemoteDataObject<PSObject> _remoteObject;
    private Exception _reason;

    internal RemoteSessionStateMachineEventArgs(RemoteSessionEvent stateEvent)
      : this(stateEvent, (Exception) null)
    {
      using (RemoteSessionStateMachineEventArgs._trace.TraceConstructor((object) this))
        ;
    }

    internal RemoteSessionStateMachineEventArgs(RemoteSessionEvent stateEvent, Exception reason)
    {
      using (RemoteSessionStateMachineEventArgs._trace.TraceConstructor((object) this, (object) reason))
      {
        this._stateEvent = stateEvent;
        this._reason = reason;
      }
    }

    internal RemoteSessionEvent StateEvent
    {
      get
      {
        using (RemoteSessionStateMachineEventArgs._trace.TraceProperty())
          return this._stateEvent;
      }
    }

    internal Exception Reason
    {
      get
      {
        using (RemoteSessionStateMachineEventArgs._trace.TraceProperty())
          return this._reason;
      }
    }

    internal RemoteSessionCapability RemoteSessionCapability
    {
      get
      {
        using (RemoteSessionStateMachineEventArgs._trace.TraceProperty())
          return this._capability;
      }
      set
      {
        using (RemoteSessionStateMachineEventArgs._trace.TraceProperty())
          this._capability = value;
      }
    }

    internal RemoteDataObject<PSObject> RemoteData
    {
      get
      {
        using (RemoteSessionStateMachineEventArgs._trace.TraceProperty())
          return this._remoteObject;
      }
      set
      {
        using (RemoteSessionStateMachineEventArgs._trace.TraceProperty())
          this._remoteObject = value;
      }
    }
  }
}
