// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ScriptParameterBinder
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal class ScriptParameterBinder : ParameterBinderBase
  {
    [TraceSource("ScriptParameterBinder", "The parameter binder for shell functions")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ScriptParameterBinder), "The parameter binder for shell functions");
    private ScriptBlock script;

    internal ScriptParameterBinder(
      ScriptBlock script,
      InvocationInfo invocationInfo,
      ExecutionContext context,
      InternalCommand command)
      : base(invocationInfo, context, command)
    {
      this.script = script != null ? script : throw ScriptParameterBinder.tracer.NewArgumentNullException(nameof (script));
    }

    internal override object GetDefaultParameterValue(string name) => this.script.RuntimeDefinedParameters.ContainsKey(name) ? this.GetDefaultScriptParameterValue(this.script.RuntimeDefinedParameters[name]) : (object) null;

    internal void BindParameterDefaultValue(string name)
    {
      if (!this.script.RuntimeDefinedParameters.ContainsKey(name))
        return;
      object defaultParameterValue = this.GetDefaultParameterValue(name);
      PSVariable variableAtScope = this.Context.EngineSessionState.GetVariableAtScope(name, "LOCAL");
      if (variableAtScope != null)
        this.Context.EngineSessionState.RemoveVariableAtScope(variableAtScope, "LOCAL");
      this.BindParameter(name, defaultParameterValue);
    }

    internal override void BindParameter(string name, object value)
    {
      if (value == AutomationNull.Value || value == UnboundParameter.Value)
        value = (object) null;
      PSVariable variable = new PSVariable(name, value);
      this.Context.EngineSessionState.SetVariable(variable, false, CommandOrigin.Internal);
      if (!this.script.RuntimeDefinedParameters.ContainsKey(name))
        return;
      RuntimeDefinedParameter definedParameter = this.script.RuntimeDefinedParameters[name];
      variable.AddParameterAttributesNoChecks(definedParameter.Attributes);
    }

    internal void BindParameter(ScopedItemLookupPath variable, object value)
    {
      if (value == AutomationNull.Value)
        value = (object) null;
      this.Context.EngineSessionState.SetVariable(variable, value, true, CommandOrigin.Internal);
    }

    internal object GetDefaultScriptParameterValue(RuntimeDefinedParameter parameter)
    {
      object obj = parameter.Value;
      if (obj is ParseTreeNode parseTreeNode)
      {
        object variable = this.Context.GetVariable("MyInvocation");
        SessionStateInternal engineSessionState = this.Context.EngineSessionState;
        try
        {
          if (this.script.SessionStateInternal != null && this.script.SessionStateInternal != this.Context.EngineSessionState)
            this.Context.EngineSessionState = this.script.SessionStateInternal;
          this.Context.SetVariable("MyInvocation", (object) this.Command.MyInvocation);
          obj = parseTreeNode.Execute(this.Context);
          if (obj == AutomationNull.Value)
            obj = (object) null;
        }
        catch (ReturnException ex)
        {
          obj = ex.Argument;
        }
        catch (TerminateException ex)
        {
          throw;
        }
        catch (FlowControlException ex)
        {
          obj = (object) null;
        }
        finally
        {
          this.Context.EngineSessionState = engineSessionState;
          this.Context.SetVariable("MyInvocation", variable);
        }
      }
      return obj;
    }

    internal ScriptBlock Script => this.script;
  }
}
