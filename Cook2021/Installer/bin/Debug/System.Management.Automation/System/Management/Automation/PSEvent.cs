// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSEvent
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Reflection;
using System.Text;

namespace System.Management.Automation
{
  public class PSEvent : PSMemberInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    internal EventInfo baseEvent;

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(this.baseEvent.ToString());
      stringBuilder.Append("(");
      int num = 0;
      foreach (ParameterInfo parameter in this.baseEvent.EventHandlerType.GetMethod("Invoke").GetParameters())
      {
        if (num > 0)
          stringBuilder.Append(", ");
        stringBuilder.Append(parameter.ParameterType.ToString());
        ++num;
      }
      stringBuilder.Append(")");
      return stringBuilder.ToString();
    }

    internal PSEvent(EventInfo baseEvent)
    {
      this.baseEvent = baseEvent;
      this.name = baseEvent.Name;
    }

    public override PSMemberInfo Copy()
    {
      PSEvent psEvent = new PSEvent(this.baseEvent);
      this.CloneBaseProperties((PSMemberInfo) psEvent);
      return (PSMemberInfo) psEvent;
    }

    public override PSMemberTypes MemberType => PSMemberTypes.Event;

    public override sealed object Value
    {
      get => (object) this.baseEvent;
      set => throw new ExtendedTypeSystemException("CannotChangePSEventInfoValue", (Exception) null, "ExtendedTypeSystem", "CannotSetValueForMemberType", new object[1]
      {
        (object) this.GetType().FullName
      });
    }

    public override string TypeNameOfValue => typeof (PSEvent).FullName;
  }
}
