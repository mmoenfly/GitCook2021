// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.InvalidRunspacePoolStateException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation.Runspaces
{
  [Serializable]
  public class InvalidRunspacePoolStateException : SystemException
  {
    private const string ResourceBase = "RunspacePoolStrings";
    [NonSerialized]
    private RunspacePoolState currentState;
    [NonSerialized]
    private RunspacePoolState expectedState;
    [TraceSource("RunspacePool", "Powershell hosting interfaces")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("RunspacePool", "Powershell hosting interfaces");

    public InvalidRunspacePoolStateException()
      : base(ResourceManagerCache.FormatResourceString("RunspacePoolStrings", "InvalidRunspacePoolStateGeneral"))
    {
    }

    public InvalidRunspacePoolStateException(string message)
      : base(message)
    {
    }

    public InvalidRunspacePoolStateException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    internal InvalidRunspacePoolStateException(
      string message,
      RunspacePoolState currentState,
      RunspacePoolState expectedState)
      : base(message)
    {
      using (InvalidRunspacePoolStateException.tracer.TraceConstructor((object) this))
      {
        this.expectedState = expectedState;
        this.currentState = currentState;
      }
    }

    protected InvalidRunspacePoolStateException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      using (InvalidRunspacePoolStateException.tracer.TraceConstructor((object) this))
        ;
    }

    public RunspacePoolState CurrentState
    {
      get
      {
        using (InvalidRunspacePoolStateException.tracer.TraceProperty())
          return this.currentState;
      }
    }

    public RunspacePoolState ExpectedState
    {
      get
      {
        using (InvalidRunspacePoolStateException.tracer.TraceProperty())
          return this.expectedState;
      }
    }

    internal InvalidRunspaceStateException ToInvalidRunspaceStateException() => new InvalidRunspaceStateException(ResourceManagerCache.GetResourceString("Runspace", "InvalidRunspaceStateGeneral"), (Exception) this)
    {
      CurrentState = InvalidRunspacePoolStateException.RunspacePoolStateToRunspaceState(this.CurrentState),
      ExpectedState = InvalidRunspacePoolStateException.RunspacePoolStateToRunspaceState(this.ExpectedState)
    };

    private static RunspaceState RunspacePoolStateToRunspaceState(
      RunspacePoolState state)
    {
      switch (state)
      {
        case RunspacePoolState.BeforeOpen:
          return RunspaceState.BeforeOpen;
        case RunspacePoolState.Opening:
          return RunspaceState.Opening;
        case RunspacePoolState.Opened:
          return RunspaceState.Opened;
        case RunspacePoolState.Closed:
          return RunspaceState.Closed;
        case RunspacePoolState.Closing:
          return RunspaceState.Closing;
        case RunspacePoolState.Broken:
          return RunspaceState.Broken;
        default:
          return RunspaceState.BeforeOpen;
      }
    }
  }
}
