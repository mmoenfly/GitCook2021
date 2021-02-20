// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RuntimeDefinedParameterBinder
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal class RuntimeDefinedParameterBinder : ParameterBinderBase
  {
    [TraceSource("RuntimeDefinedParameterBinder", "The parameter binder for runtime defined parameters which are declared through the RuntimeDefinedParameterDictionary.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (RuntimeDefinedParameterBinder), "The parameter binder for runtime defined parameters which are declared through the RuntimeDefinedParameterDictionary.");

    internal RuntimeDefinedParameterBinder(
      RuntimeDefinedParameterDictionary target,
      InternalCommand command,
      CommandLineParameters commandLineParameters)
      : base((object) target, command.MyInvocation, command.Context, command)
    {
      using (RuntimeDefinedParameterBinder.tracer.TraceConstructor((object) this))
      {
        foreach (string key in target.Keys)
        {
          RuntimeDefinedParameter definedParameter = target[key];
          string parameterName = definedParameter == null ? (string) null : definedParameter.Name;
          if (definedParameter == null || key != parameterName)
          {
            ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.InvalidArgument, command.MyInvocation, (Token) null, parameterName, (Type) null, (Type) null, "ParameterBinderStrings", "RuntimeDefinedParameterNameMismatch", new object[1]
            {
              (object) key
            });
            RuntimeDefinedParameterBinder.tracer.TraceException((Exception) bindingException);
            throw bindingException;
          }
        }
        this.CommandLineParameters = commandLineParameters;
      }
    }

    internal RuntimeDefinedParameterDictionary Target
    {
      get
      {
        using (RuntimeDefinedParameterBinder.tracer.TraceProperty())
          return base.Target as RuntimeDefinedParameterDictionary;
      }
      set
      {
        using (RuntimeDefinedParameterBinder.tracer.TraceProperty())
          this.Target = (object) value;
      }
    }

    internal override object GetDefaultParameterValue(string name)
    {
      using (RuntimeDefinedParameterBinder.tracer.TraceMethod(name, new object[0]))
      {
        object obj = (object) null;
        if (this.Target.ContainsKey(name))
        {
          RuntimeDefinedParameter definedParameter = this.Target[name];
          if (definedParameter != null)
            obj = definedParameter.Value;
        }
        return obj;
      }
    }

    internal override void BindParameter(string name, object value)
    {
      using (RuntimeDefinedParameterBinder.tracer.TraceMethod())
      {
        if (string.IsNullOrEmpty(name))
          throw RuntimeDefinedParameterBinder.tracer.NewArgumentException(nameof (name));
        this.Target[name].Value = value;
        this.CommandLineParameters.Add(name, value);
      }
    }
  }
}
