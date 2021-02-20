// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemoteException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class RemoteException : RuntimeException
  {
    [NonSerialized]
    private PSObject _serializedRemoteException;
    [NonSerialized]
    private PSObject _serializedRemoteInvocationInfo;
    private ErrorRecord _remoteErrorRecord;
    [TraceSource("RemoteException", "RemoteException")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (RemoteException), nameof (RemoteException));

    public RemoteException()
    {
      using (RemoteException._trace.TraceConstructor((object) this))
        ;
    }

    public RemoteException(string message)
      : base(message)
    {
      using (RemoteException._trace.TraceConstructor((object) this))
        ;
    }

    public RemoteException(string message, Exception innerException)
      : base(message, innerException)
    {
      using (RemoteException._trace.TraceConstructor((object) this))
        ;
    }

    internal RemoteException(
      string message,
      PSObject serializedRemoteException,
      PSObject serializedRemoteInvocationInfo)
      : base(message)
    {
      using (RemoteException._trace.TraceConstructor((object) this))
      {
        this._serializedRemoteException = serializedRemoteException;
        this._serializedRemoteInvocationInfo = serializedRemoteInvocationInfo;
      }
    }

    protected RemoteException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      using (RemoteException._trace.TraceConstructor((object) this))
        ;
    }

    public PSObject SerializedRemoteException
    {
      get
      {
        using (RemoteException._trace.TraceProperty())
          return this._serializedRemoteException;
      }
    }

    public PSObject SerializedRemoteInvocationInfo
    {
      get
      {
        using (RemoteException._trace.TraceProperty())
          return this._serializedRemoteInvocationInfo;
      }
    }

    internal void SetRemoteErrorRecord(ErrorRecord remoteError)
    {
      using (RemoteException._trace.TraceMethod())
        this._remoteErrorRecord = remoteError;
    }

    public override ErrorRecord ErrorRecord
    {
      get
      {
        using (RemoteException._trace.TraceProperty())
          return this._remoteErrorRecord != null ? this._remoteErrorRecord : base.ErrorRecord;
      }
    }
  }
}
