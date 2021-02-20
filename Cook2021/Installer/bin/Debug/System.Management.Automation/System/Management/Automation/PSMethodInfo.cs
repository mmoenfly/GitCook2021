// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSMethodInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public abstract class PSMethodInfo : PSMemberInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");

    public abstract object Invoke(params object[] arguments);

    public abstract Collection<string> OverloadDefinitions { get; }

    public override sealed object Value
    {
      get => (object) this;
      set => throw new ExtendedTypeSystemException("CannotChangePSMethodInfoValue", (Exception) null, "ExtendedTypeSystem", "CannotSetValueForMemberType", new object[1]
      {
        (object) this.GetType().FullName
      });
    }
  }
}
