// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemoteSessionStateEventArgs
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class RemoteSessionStateEventArgs : EventArgs
  {
    [TraceSource("SessionStateEA", "RemoteSessionStateEventArgs")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("SessionStateEA", nameof (RemoteSessionStateEventArgs));
    private RemoteSessionStateInfo _remoteSessionStateInfo;

    internal RemoteSessionStateEventArgs(RemoteSessionStateInfo remoteSessionStateInfo)
    {
      using (RemoteSessionStateEventArgs._trace.TraceConstructor((object) this))
      {
        if (remoteSessionStateInfo == null)
          RemoteSessionStateEventArgs._trace.NewArgumentNullException(nameof (remoteSessionStateInfo));
        this._remoteSessionStateInfo = remoteSessionStateInfo;
      }
    }

    public RemoteSessionStateInfo SessionStateInfo => this._remoteSessionStateInfo;
  }
}
