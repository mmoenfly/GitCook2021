// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ReflectionParameterBinder
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal class ReflectionParameterBinder : ParameterBinderBase
  {
    [TraceSource("ReflectionParameterBinder", "The parameter binder for real CLR objects that have properties and fields decorated with the parameter attributes.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ReflectionParameterBinder), "The parameter binder for real CLR objects that have properties and fields decorated with the parameter attributes.");

    internal ReflectionParameterBinder(object target, Cmdlet command)
      : base((object) LanguagePrimitives.AsPSObjectOrNull(target), command.MyInvocation, command.Context, (InternalCommand) command)
    {
      using (ReflectionParameterBinder.tracer.TraceConstructor((object) this))
        ;
    }

    internal ReflectionParameterBinder(
      object target,
      Cmdlet command,
      CommandLineParameters commandLineParameters)
      : base((object) LanguagePrimitives.AsPSObjectOrNull(target), command.MyInvocation, command.Context, (InternalCommand) command)
    {
      using (ReflectionParameterBinder.tracer.TraceConstructor((object) this))
        this.CommandLineParameters = commandLineParameters;
    }

    internal PSObject Target
    {
      get
      {
        using (ReflectionParameterBinder.tracer.TraceProperty())
        {
          this.Target = (object) LanguagePrimitives.AsPSObjectOrNull(base.Target);
          return (PSObject) base.Target;
        }
      }
      set
      {
        using (ReflectionParameterBinder.tracer.TraceProperty())
          this.Target = (object) LanguagePrimitives.AsPSObjectOrNull((object) value);
      }
    }

    internal override object GetDefaultParameterValue(string name)
    {
      using (ReflectionParameterBinder.tracer.TraceMethod(name, new object[0]))
        return this.Target.Properties[name].Value;
    }

    internal override void BindParameter(string name, object value)
    {
      using (ReflectionParameterBinder.tracer.TraceMethod())
      {
        if (string.IsNullOrEmpty(name))
          throw ReflectionParameterBinder.tracer.NewArgumentException(nameof (name));
        this.Target.Properties[name]?.SetValueNoConversion(value);
      }
    }
  }
}
