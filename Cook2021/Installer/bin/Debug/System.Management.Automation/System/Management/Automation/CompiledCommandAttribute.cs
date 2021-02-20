// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CompiledCommandAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class CompiledCommandAttribute
  {
    [TraceSource("CompiledCommandAttribute", "The metadata associated with an attribute that is attached to a bindable object in MSH.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (CompiledCommandAttribute), "The metadata associated with an attribute that is attached to a bindable object in MSH.");
    private Type type;
    private Attribute attribute;

    internal CompiledCommandAttribute(Attribute attribute)
    {
      using (CompiledCommandAttribute.tracer.TraceConstructor((object) this))
      {
        this.attribute = attribute != null ? attribute : throw CompiledCommandAttribute.tracer.NewArgumentNullException(nameof (attribute));
        this.type = attribute.GetType();
      }
    }

    internal Type Type
    {
      get
      {
        using (CompiledCommandAttribute.tracer.TraceProperty())
        {
          Type type = this.type;
          return this.type;
        }
      }
    }

    internal Attribute Instance
    {
      get
      {
        using (CompiledCommandAttribute.tracer.TraceProperty())
        {
          Attribute attribute = this.attribute;
          return this.attribute;
        }
      }
    }
  }
}
