// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.InvalidRunspaceStateException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation.Runspaces
{
  [Serializable]
  public class InvalidRunspaceStateException : SystemException
  {
    [NonSerialized]
    private RunspaceState _currentState;
    [NonSerialized]
    private RunspaceState _expectedState;
    [TraceSource("InvalidRunspaceStateException", "InvalidRunspaceStateException")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (InvalidRunspaceStateException), nameof (InvalidRunspaceStateException));

    public InvalidRunspaceStateException()
      : base(ResourceManagerCache.FormatResourceString("Runspace", "InvalidRunspaceStateGeneral"))
    {
    }

    public InvalidRunspaceStateException(string message)
      : base(message)
    {
    }

    public InvalidRunspaceStateException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    internal InvalidRunspaceStateException(
      string message,
      RunspaceState currentState,
      RunspaceState expectedState)
      : base(message)
    {
      this._expectedState = expectedState;
      this._currentState = currentState;
    }

    protected InvalidRunspaceStateException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public RunspaceState CurrentState
    {
      get => this._currentState;
      internal set => this._currentState = value;
    }

    public RunspaceState ExpectedState
    {
      get => this._expectedState;
      internal set => this._expectedState = value;
    }
  }
}
