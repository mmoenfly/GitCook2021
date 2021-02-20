// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemoteDataEventArgs
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Remoting;

namespace System.Management.Automation
{
  internal sealed class RemoteDataEventArgs : EventArgs
  {
    [TraceSource("RDEA", "RemoteDataEventArgs")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("RDEA", nameof (RemoteDataEventArgs));
    private RemoteDataObject<PSObject> _rcvdData;

    internal RemoteDataEventArgs(RemoteDataObject<PSObject> receivedData)
    {
      using (RemoteDataEventArgs._trace.TraceConstructor((object) this))
        this._rcvdData = receivedData != null ? receivedData : throw RemoteDataEventArgs._trace.NewArgumentNullException(nameof (receivedData));
    }

    public RemoteDataObject<PSObject> ReceivedData
    {
      get
      {
        using (RemoteDataEventArgs._trace.TraceProperty())
          return this._rcvdData;
      }
    }
  }
}
