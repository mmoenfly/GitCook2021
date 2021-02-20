// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.InvalidPipelineStateException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation.Runspaces
{
  [Serializable]
  public class InvalidPipelineStateException : SystemException
  {
    [NonSerialized]
    private PipelineState _currentState;
    [NonSerialized]
    private PipelineState _expectedState;
    [TraceSource("InvalidPipelineStateException", "InvalidPipelineStateException")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (InvalidPipelineStateException), nameof (InvalidPipelineStateException));

    public InvalidPipelineStateException()
      : base(ResourceManagerCache.FormatResourceString("Runspace", "InvalidPipelineStateStateGeneral"))
    {
      using (InvalidPipelineStateException._trace.TraceConstructor((object) this))
        ;
    }

    public InvalidPipelineStateException(string message)
      : base(message)
    {
      using (InvalidPipelineStateException._trace.TraceConstructor((object) this))
        ;
    }

    public InvalidPipelineStateException(string message, Exception innerException)
      : base(message, innerException)
    {
      using (InvalidPipelineStateException._trace.TraceConstructor((object) this))
        ;
    }

    internal InvalidPipelineStateException(
      string message,
      PipelineState currentState,
      PipelineState expectedState)
      : base(message)
    {
      using (InvalidPipelineStateException._trace.TraceConstructor((object) this))
      {
        this._expectedState = expectedState;
        this._currentState = currentState;
      }
    }

    private InvalidPipelineStateException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      using (InvalidPipelineStateException._trace.TraceConstructor((object) this))
        ;
    }

    public PipelineState CurrentState
    {
      get
      {
        using (InvalidPipelineStateException._trace.TraceProperty())
          return this._currentState;
      }
    }

    public PipelineState ExpectedState
    {
      get
      {
        using (InvalidPipelineStateException._trace.TraceProperty())
          return this._expectedState;
      }
    }
  }
}
