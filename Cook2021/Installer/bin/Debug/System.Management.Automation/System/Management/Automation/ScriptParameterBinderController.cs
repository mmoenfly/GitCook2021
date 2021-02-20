// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ScriptParameterBinderController
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal class ScriptParameterBinderController : ParameterBinderController
  {
    [TraceSource("ParameterBinderController", "Controls the interaction between the command processor and the parameter binder(s).")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ParameterBinderController", "Controls the interaction between the command processor and the parameter binder(s).");
    private bool useLocalScope;
    private ScriptBlock script;
    private List<object> dollarArgs = new List<object>();
    private Dictionary<ScopedItemLookupPath, object> defaultValues = new Dictionary<ScopedItemLookupPath, object>();

    internal ScriptParameterBinderController(
      ScriptBlock script,
      InvocationInfo invocationInfo,
      ExecutionContext context,
      InternalCommand command,
      bool useLocalScope)
      : base(invocationInfo, context, (ParameterBinderBase) new ScriptParameterBinder(script, invocationInfo, context, command), command)
    {
      this.useLocalScope = useLocalScope;
      this.script = script;
      this.AddUnboundParameters(this.BindableParameters.ReplaceMetadata(script.ParameterMetadata));
    }

    internal bool UseLocalScope
    {
      get => this.useLocalScope;
      set => this.useLocalScope = value;
    }

    internal ScriptBlock Script => this.script;

    internal List<object> DollarArgs => this.dollarArgs;

    internal bool BindCommandLineParameters(Collection<CommandParameterInternal> arguments)
    {
      ScriptParameterBinderController.tracer.WriteLine("Argument count: {0}", (object) arguments.Count);
      bool flag = true;
      foreach (CommandParameterInternal parameterInternal in arguments)
        this.UnboundArguments.Add(parameterInternal);
      this.ReparseUnboundArguments();
      this.UnboundArguments = this.BindParameters(this.UnboundArguments);
      ParameterBindingException outgoingBindingException = new ParameterBindingException();
      this.UnboundArguments = this.BindPositionalParameters(this.UnboundArguments, uint.MaxValue, uint.MaxValue, false, out outgoingBindingException);
      try
      {
        this.DefaultParameterBinder.RecordBoundParameters = false;
        this.BindUnboundParameters();
        this.HandleRemainingArguments(this.UnboundArguments);
      }
      finally
      {
        this.DefaultParameterBinder.RecordBoundParameters = true;
      }
      return flag;
    }

    internal override bool BindParameter(
      CommandParameterInternal argument,
      ParameterBindingFlags flags)
    {
      this.DefaultParameterBinder.BindParameter(argument.Name, argument.Value2);
      return true;
    }

    internal void BindParameter(ScopedItemLookupPath variablePath, object value) => ((ScriptParameterBinder) this.DefaultParameterBinder).BindParameter(variablePath, value);

    internal override Collection<CommandParameterInternal> BindParameters(
      Collection<CommandParameterInternal> arguments)
    {
      Collection<CommandParameterInternal> collection = new Collection<CommandParameterInternal>();
      foreach (CommandParameterInternal parameterInternal in arguments)
      {
        if (string.IsNullOrEmpty(parameterInternal.Name))
        {
          collection.Add(parameterInternal);
        }
        else
        {
          MergedCompiledCommandParameter matchingParameter = this.BindableParameters.GetMatchingParameter(parameterInternal.Name, false, this.InvocationInfo);
          if (matchingParameter != null)
          {
            if (this.BoundParameters.ContainsKey(matchingParameter.Parameter.Name))
            {
              ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.InvalidArgument, this.InvocationInfo, parameterInternal.Token, parameterInternal.Name, (Type) null, (Type) null, "ParameterBinderStrings", "ParameterAlreadyBound", new object[0]);
              ScriptParameterBinderController.tracer.TraceException((Exception) bindingException);
              throw bindingException;
            }
            this.BindParameter(uint.MaxValue, parameterInternal, matchingParameter, ParameterBindingFlags.ShouldCoerceType);
          }
          else
            collection.Add(parameterInternal);
        }
      }
      return collection;
    }

    internal void BindUnboundParameters()
    {
      foreach (MergedCompiledCommandParameter parameter in new List<MergedCompiledCommandParameter>((IEnumerable<MergedCompiledCommandParameter>) this.UnboundParameters.Values))
        this.BindUnboundScriptParameter(parameter, uint.MaxValue);
    }

    internal ArrayList HandleRemainingArguments(
      Collection<CommandParameterInternal> arguments)
    {
      ArrayList arrayList = new ArrayList();
      Type type = typeof (object);
      ParameterCollectionTypeInformation parameterCollectionType = new ParameterCollectionTypeInformation(type);
      foreach (CommandParameterInternal parameterInternal in arguments)
      {
        if (parameterInternal.Value1 is string str && str.Equals("$args", StringComparison.OrdinalIgnoreCase) && parameterInternal.IsValidPair)
        {
          object obj = this.DefaultParameterBinder.DecodeValue(parameterInternal.Value2, type, parameterCollectionType, true);
          if (obj is object[])
            arrayList.AddRange((ICollection) (obj as object[]));
          else
            arrayList.Add(obj);
        }
        else
        {
          object obj1 = this.DefaultParameterBinder.DecodeValue(parameterInternal.Value1, type, parameterCollectionType, true);
          arrayList.Add(obj1);
          if (parameterInternal.IsValidPair)
          {
            object obj2 = this.DefaultParameterBinder.DecodeValue(parameterInternal.Value2, type, parameterCollectionType, true);
            arrayList.Add(obj2);
          }
        }
      }
      object[] array = arrayList.ToArray();
      this.BindParameter(ExecutionContext.ArgsVariablePath, (object) array);
      this.DollarArgs.AddRange((IEnumerable<object>) array);
      return arrayList;
    }

    internal override Collection<CommandParameterInternal> BackupDefaultParameterValues()
    {
      Collection<CommandParameterInternal> collection = new Collection<CommandParameterInternal>();
      if (!this.useLocalScope)
      {
        this.defaultValues.Add(ExecutionContext.InputVariablePath, this.Context.InputVariable);
        this.defaultValues.Add(ExecutionContext.ArgsVariablePath, this.Context.ArgsVariable);
        this.defaultValues.Add(ExecutionContext.UnderbarVariablePath, this.Context.UnderbarVariable);
      }
      return collection;
    }

    internal override void RestoreDefaultParameterValues(
      IEnumerable<MergedCompiledCommandParameter> arguments)
    {
      foreach (KeyValuePair<ScopedItemLookupPath, object> defaultValue in this.defaultValues)
        ((ScriptParameterBinder) this.DefaultParameterBinder).BindParameter(defaultValue.Key, defaultValue.Value);
    }
  }
}
