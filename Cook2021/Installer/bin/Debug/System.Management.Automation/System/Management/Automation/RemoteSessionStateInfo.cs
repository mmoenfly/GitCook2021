// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemoteSessionStateInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class RemoteSessionStateInfo
  {
    [TraceSource("SessionStateInfo", "RemoteSessionStateInfo")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("SessionStateInfo", nameof (RemoteSessionStateInfo));
    private RemoteSessionState _state;
    private Exception _reason;

    internal RemoteSessionStateInfo(RemoteSessionState state)
      : this(state, (Exception) null)
    {
    }

    internal RemoteSessionStateInfo(RemoteSessionState state, Exception reason)
    {
      this._state = state;
      this._reason = reason;
    }

    internal RemoteSessionStateInfo(RemoteSessionStateInfo sessionStateInfo)
    {
      using (RemoteSessionStateInfo._trace.TraceConstructor((object) this))
      {
        this._state = sessionStateInfo.State;
        this._reason = sessionStateInfo.Reason;
      }
    }

    internal RemoteSessionState State
    {
      get
      {
        using (RemoteSessionStateInfo._trace.TraceProperty())
          return this._state;
      }
    }

    internal Exception Reason
    {
      get
      {
        using (RemoteSessionStateInfo._trace.TraceProperty())
          return this._reason;
      }
    }
  }
}
