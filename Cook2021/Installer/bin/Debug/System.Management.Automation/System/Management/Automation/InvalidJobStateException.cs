// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.InvalidJobStateException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Remoting;
using System.Runtime.Serialization;

namespace System.Management.Automation
{
  [Serializable]
  public class InvalidJobStateException : SystemException
  {
    [NonSerialized]
    private JobState currState;
    [TraceSource("PSJob", "Job APIs")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PSJob", "Job APIs");

    public InvalidJobStateException()
      : base(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.InvalidJobStateGeneral))
    {
    }

    public InvalidJobStateException(string message)
      : base(message)
    {
    }

    public InvalidJobStateException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    internal InvalidJobStateException(JobState currentState)
      : base(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.InvalidJobStateGeneral))
    {
      using (InvalidJobStateException.tracer.TraceConstructor((object) this))
        this.currState = currentState;
    }

    protected InvalidJobStateException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      using (InvalidJobStateException.tracer.TraceConstructor((object) this))
        ;
    }

    public JobState CurrentState
    {
      get
      {
        using (InvalidJobStateException.tracer.TraceProperty())
          return this.currState;
      }
    }
  }
}
