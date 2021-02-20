// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSPropertyInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public abstract class PSPropertyInfo : PSMemberInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");

    public abstract bool IsSettable { get; }

    public abstract bool IsGettable { get; }

    internal Exception NewSetValueException(Exception e, string errorId) => (Exception) new SetValueInvocationException(errorId, e, "ExtendedTypeSystem", "ExceptionWhenSetting", new object[2]
    {
      (object) this.Name,
      (object) e.Message
    });

    internal Exception NewGetValueException(Exception e, string errorId) => (Exception) new GetValueInvocationException(errorId, e, "ExtendedTypeSystem", "ExceptionWhenSetting", new object[2]
    {
      (object) this.Name,
      (object) e.Message
    });
  }
}
