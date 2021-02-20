// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParameterBinderController
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal abstract class ParameterBinderController
  {
    [TraceSource("ParameterBinderController", "Controls the interaction between the command processor and the parameter binder(s).")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ParameterBinderController), "Controls the interaction between the command processor and the parameter binder(s).");
    private InternalCommand command;
    private ExecutionContext context;
    private ParameterBinderBase parameterBinder;
    private InvocationInfo invocationInfo;
    private MergedCommandParameterMetadata bindableParameters = new MergedCommandParameterMetadata();
    private Dictionary<string, MergedCompiledCommandParameter> unboundParameters = new Dictionary<string, MergedCompiledCommandParameter>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, MergedCompiledCommandParameter> boundParameters = new Dictionary<string, MergedCompiledCommandParameter>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private Collection<CommandParameterInternal> _unboundArguments = new Collection<CommandParameterInternal>();
    private Dictionary<string, CommandParameterInternal> boundArguments = new Dictionary<string, CommandParameterInternal>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private Collection<MergedCompiledCommandParameter> parametersBoundThroughPipelineInput = new Collection<MergedCompiledCommandParameter>();
    private Dictionary<string, CommandParameterInternal> defaultParameterValues = new Dictionary<string, CommandParameterInternal>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    internal uint currentParameterSetFlag = uint.MaxValue;
    internal uint prePipelineProcessingParameterSetFlags = uint.MaxValue;

    internal ParameterBinderController(
      InvocationInfo invocationInfo,
      ExecutionContext context,
      ParameterBinderBase parameterBinder,
      InternalCommand command)
    {
      if (invocationInfo == null)
        throw ParameterBinderController.tracer.NewArgumentNullException(nameof (invocationInfo));
      if (parameterBinder == null)
        throw ParameterBinderController.tracer.NewArgumentNullException(nameof (parameterBinder));
      if (context == null)
        throw ParameterBinderController.tracer.NewArgumentNullException(nameof (context));
      this.command = command;
      this.parameterBinder = parameterBinder;
      this.context = context;
      this.invocationInfo = invocationInfo;
    }

    internal virtual InternalCommand Command
    {
      get => this.command;
      set => this.command = value;
    }

    internal ExecutionContext Context => this.context;

    internal ParameterBinderBase DefaultParameterBinder
    {
      get => this.parameterBinder;
      set => this.parameterBinder = value != null ? value : throw ParameterBinderController.tracer.NewArgumentNullException("parameterBinder");
    }

    internal InvocationInfo InvocationInfo => this.invocationInfo;

    internal MergedCommandParameterMetadata BindableParameters => this.bindableParameters;

    protected Dictionary<string, MergedCompiledCommandParameter> UnboundParameters => this.unboundParameters;

    protected void AddUnboundParameters(
      Collection<MergedCompiledCommandParameter> newParameters)
    {
      foreach (MergedCompiledCommandParameter newParameter in newParameters)
        this.unboundParameters.Add(newParameter.Parameter.Name, newParameter);
    }

    protected Dictionary<string, MergedCompiledCommandParameter> BoundParameters => this.boundParameters;

    internal CommandLineParameters CommandLineParameters => this.parameterBinder.CommandLineParameters;

    protected Collection<CommandParameterInternal> UnboundArguments
    {
      get => this._unboundArguments;
      set => this._unboundArguments = value;
    }

    protected Dictionary<string, CommandParameterInternal> BoundArguments
    {
      get => this.boundArguments;
      set => this.boundArguments = value;
    }

    internal void ReparseUnboundArguments()
    {
      Collection<CommandParameterInternal> collection = new Collection<CommandParameterInternal>();
      for (int index = 0; index < this._unboundArguments.Count; ++index)
      {
        CommandParameterInternal unboundArgument1 = this._unboundArguments[index];
        if (unboundArgument1.IsValidPair)
          collection.Add(unboundArgument1);
        else if (unboundArgument1.Value1 == null || !ParameterBinderController.IsParameterToken(unboundArgument1.Value1) && !unboundArgument1.TreatLikeToken)
        {
          collection.Add(unboundArgument1);
        }
        else
        {
          Token token = unboundArgument1.Value1 as Token;
          string str = unboundArgument1.Name;
          if (token != null)
            str = token.Data as string;
          if (string.IsNullOrEmpty(str))
          {
            collection.Add(unboundArgument1);
          }
          else
          {
            MergedCompiledCommandParameter matchingParameter = this.bindableParameters.GetMatchingParameter(str, false, this.InvocationInfo);
            if (matchingParameter == null)
              collection.Add(unboundArgument1);
            else if (ParameterBinderController.IsSwitchAndSetValue(str, unboundArgument1, matchingParameter.Parameter))
              collection.Add(unboundArgument1);
            else if (this._unboundArguments.Count - 1 > index)
            {
              CommandParameterInternal unboundArgument2 = this._unboundArguments[index + 1];
              if (unboundArgument2.IsValidPair || ParameterBinderController.IsParameterToken(unboundArgument2.Value1) || unboundArgument2.TreatLikeToken)
              {
                bool flag = false;
                if (!string.IsNullOrEmpty(unboundArgument2.Name) && this.bindableParameters.GetMatchingParameter(unboundArgument2.Name, false, this.InvocationInfo) != null)
                  flag = true;
                if (flag || unboundArgument2.IsValidPair)
                {
                  ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.InvalidArgument, this.InvocationInfo, token, matchingParameter.Parameter.Name, matchingParameter.Parameter.Type, (Type) null, "ParameterBinderStrings", "MissingArgument", new object[0]);
                  ParameterBinderController.tracer.TraceException((Exception) bindingException);
                  throw bindingException;
                }
                ++index;
                unboundArgument1.Name = matchingParameter.Parameter.Name;
                unboundArgument1.Value2 = unboundArgument2.Value1;
                unboundArgument1.IsValidPair = true;
                collection.Add(unboundArgument1);
              }
              else
              {
                ++index;
                unboundArgument1.Name = matchingParameter.Parameter.Name;
                unboundArgument1.Value2 = unboundArgument2.Value1;
                unboundArgument1.IsValidPair = true;
                collection.Add(unboundArgument1);
              }
            }
            else
            {
              ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.InvalidArgument, this.InvocationInfo, token, matchingParameter.Parameter.Name, matchingParameter.Parameter.Type, (Type) null, "ParameterBinderStrings", "MissingArgument", new object[0]);
              ParameterBinderController.tracer.TraceException((Exception) bindingException);
              throw bindingException;
            }
          }
        }
      }
      this._unboundArguments = collection;
    }

    private static bool IsSwitchAndSetValue(
      string argumentName,
      CommandParameterInternal argument,
      CompiledCommandParameter matchingParameter)
    {
      bool flag = false;
      if (matchingParameter.Type == typeof (SwitchParameter))
      {
        argument.Name = argumentName;
        argument.Value2 = (object) SwitchParameter.Present;
        argument.IsValidPair = true;
        flag = true;
      }
      ParameterBinderController.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal static bool ArgumentLooksLikeParameter(object arg)
    {
      bool flag = false;
      string str = arg as string;
      if (!string.IsNullOrEmpty(str))
        flag = SpecialCharacters.IsDash(str[0]);
      return flag;
    }

    internal static void AddArgumentsToCommandProcessor(
      CommandProcessorBase commandProcessor,
      object[] arguments)
    {
      if (arguments == null)
        return;
      for (int index = 0; index < arguments.Length; ++index)
      {
        CommandParameterInternal parameter;
        if (ParameterBinderController.ArgumentLooksLikeParameter(arguments[index]))
        {
          parameter = index >= arguments.Length - 1 || !ParameterBinderController.ArgumentLooksLikeParameter(arguments[index + 1]) ? (index != arguments.Length - 1 ? new CommandParameterInternal((string) arguments[index], arguments[index + 1]) : new CommandParameterInternal(arguments[index], true)) : new CommandParameterInternal(arguments[index], true);
          ++index;
        }
        else
          parameter = new CommandParameterInternal(arguments[index]);
        commandProcessor.AddParameter(parameter);
      }
    }

    internal virtual bool BindParameter(
      CommandParameterInternal argument,
      ParameterBindingFlags flags)
    {
      bool flag = false;
      MergedCompiledCommandParameter matchingParameter = this.BindableParameters.GetMatchingParameter(argument.Name, (flags & ParameterBindingFlags.ThrowOnParameterNotFound) != ParameterBindingFlags.None, this.InvocationInfo);
      if (matchingParameter != null)
      {
        if (this.BoundParameters.ContainsKey(matchingParameter.Parameter.Name))
        {
          ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.InvalidArgument, this.InvocationInfo, argument.Token, argument.Name, (Type) null, (Type) null, "ParameterBinderStrings", "ParameterAlreadyBound", new object[0]);
          ParameterBinderController.tracer.TraceException((Exception) bindingException);
          throw bindingException;
        }
        flags &= ~ParameterBindingFlags.DelayBindScriptBlock;
        flag = this.BindParameter(this.currentParameterSetFlag, argument, matchingParameter, flags);
      }
      return flag;
    }

    internal abstract Collection<CommandParameterInternal> BindParameters(
      Collection<CommandParameterInternal> parameters);

    internal virtual bool BindParameter(
      uint parameterSets,
      CommandParameterInternal argument,
      MergedCompiledCommandParameter parameter,
      ParameterBindingFlags flags)
    {
      bool flag = false;
      if (parameter.BinderAssociation == ParameterBinderAssociation.DeclaredFormalParameters)
        flag = this.DefaultParameterBinder.BindParameter(argument, parameter.Parameter, flags);
      if (flag)
      {
        this.UnboundParameters.Remove(parameter.Parameter.Name);
        this.BoundParameters.Add(parameter.Parameter.Name, parameter);
      }
      return flag;
    }

    internal Collection<CommandParameterInternal> BindPositionalParameters(
      Collection<CommandParameterInternal> unboundArguments,
      uint validParameterSets,
      uint defaultParameterSet,
      bool ignoreArgumentsThatLookLikeParameters,
      out ParameterBindingException outgoingBindingException)
    {
      Collection<CommandParameterInternal> nonPositionalArguments = new Collection<CommandParameterInternal>();
      outgoingBindingException = (ParameterBindingException) null;
      if (unboundArguments.Count > 0)
      {
        List<CommandParameterInternal> unboundArgumentsCollection = new List<CommandParameterInternal>((IEnumerable<CommandParameterInternal>) unboundArguments);
        SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>> positionalParameters;
        try
        {
          positionalParameters = this.EvaluateUnboundPositionalParameters();
        }
        catch (InvalidOperationException ex)
        {
          ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.InvalidArgument, this.InvocationInfo, (Token) null, (string) null, (Type) null, (Type) null, "ParameterBinderStrings", "AmbiguousPositionalParameterNoName", new object[0]);
          ParameterBinderController.tracer.TraceException((Exception) bindingException);
          throw bindingException;
        }
        if (positionalParameters.Count > 0)
        {
          int unboundArgumentsIndex = 0;
          foreach (Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> nextPositionalParameters in positionalParameters.Values)
          {
            if (nextPositionalParameters.Count != 0)
            {
              CommandParameterInternal positionalArgument = ParameterBinderController.GetNextPositionalArgument(unboundArgumentsCollection, nonPositionalArguments, ref unboundArgumentsIndex);
              if (positionalArgument != null)
              {
                bool flag = false;
                if (defaultParameterSet != 0U && ((int) validParameterSets & (int) defaultParameterSet) != 0)
                  flag = this.BindPositionalParametersInSet(defaultParameterSet, nextPositionalParameters, positionalArgument, ParameterBindingFlags.DelayBindScriptBlock, out outgoingBindingException);
                if (!flag)
                  flag = this.BindPositionalParametersInSet(validParameterSets, nextPositionalParameters, positionalArgument, ParameterBindingFlags.DelayBindScriptBlock, out outgoingBindingException);
                if (!flag && defaultParameterSet != 0U && ((int) validParameterSets & (int) defaultParameterSet) != 0)
                  flag = this.BindPositionalParametersInSet(defaultParameterSet, nextPositionalParameters, positionalArgument, ParameterBindingFlags.ShouldCoerceType | ParameterBindingFlags.DelayBindScriptBlock, out outgoingBindingException);
                if (!flag)
                  flag = this.BindPositionalParametersInSet(validParameterSets, nextPositionalParameters, positionalArgument, ParameterBindingFlags.ShouldCoerceType | ParameterBindingFlags.DelayBindScriptBlock, out outgoingBindingException);
                if (!flag)
                  nonPositionalArguments.Add(positionalArgument);
                else if ((int) validParameterSets != (int) this.currentParameterSetFlag)
                {
                  validParameterSets = this.currentParameterSetFlag;
                  ParameterBinderController.UpdatePositionalDictionary(positionalParameters, validParameterSets);
                }
              }
              else
                break;
            }
          }
          for (int index = unboundArgumentsIndex; index < unboundArgumentsCollection.Count; ++index)
            nonPositionalArguments.Add(unboundArgumentsCollection[index]);
        }
        else
          nonPositionalArguments = unboundArguments;
      }
      return nonPositionalArguments;
    }

    private static void UpdatePositionalDictionary(
      SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>> positionalParameterDictionary,
      uint validParameterSets)
    {
      Collection<MergedCompiledCommandParameter> collection1 = new Collection<MergedCompiledCommandParameter>();
      foreach (Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> dictionary in positionalParameterDictionary.Values)
      {
        Collection<MergedCompiledCommandParameter> collection2 = new Collection<MergedCompiledCommandParameter>();
        foreach (PositionalCommandParameter commandParameter in dictionary.Values)
        {
          Collection<ParameterSetSpecificMetadata> parameterSetData = commandParameter.ParameterSetData;
          for (int index = parameterSetData.Count - 1; index >= 0; --index)
          {
            if (((int) parameterSetData[index].ParameterSetFlag & (int) validParameterSets) == 0 && !parameterSetData[index].IsInAllSets)
              parameterSetData.RemoveAt(index);
          }
          if (parameterSetData.Count == 0)
            collection2.Add(commandParameter.Parameter);
        }
        foreach (MergedCompiledCommandParameter key in collection2)
          dictionary.Remove(key);
      }
    }

    private bool BindPositionalParametersInSet(
      uint validParameterSets,
      Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> nextPositionalParameters,
      CommandParameterInternal argument,
      ParameterBindingFlags flags,
      out ParameterBindingException bindingException)
    {
      bool flag1 = false;
      bindingException = (ParameterBindingException) null;
      foreach (PositionalCommandParameter commandParameter in nextPositionalParameters.Values)
      {
        foreach (ParameterSetSpecificMetadata specificMetadata in commandParameter.ParameterSetData)
        {
          if (((int) validParameterSets & (int) specificMetadata.ParameterSetFlag) != 0 || specificMetadata.IsInAllSets)
          {
            string name = commandParameter.Parameter.Parameter.Name;
            bool flag2;
            try
            {
              CommandParameterInternal parameterInternal = new CommandParameterInternal(name, argument.Value1);
              flag2 = this.BindParameter(validParameterSets, parameterInternal, commandParameter.Parameter, flags);
            }
            catch (ParameterBindingArgumentTransformationException ex)
            {
              throw;
            }
            catch (ParameterBindingValidationException ex)
            {
              throw;
            }
            catch (ParameterBindingParameterDefaultValueException ex)
            {
              throw;
            }
            catch (ParameterBindingException ex)
            {
              flag2 = false;
              bindingException = ex;
            }
            if (flag2)
            {
              flag1 = flag2;
              this.CommandLineParameters.MarkAsBoundPositionally(name);
            }
          }
        }
      }
      ParameterBinderController.tracer.TraceMethod("result = {0}", (object) flag1);
      return flag1;
    }

    private static CommandParameterInternal GetNextPositionalArgument(
      List<CommandParameterInternal> unboundArgumentsCollection,
      Collection<CommandParameterInternal> nonPositionalArguments,
      ref int unboundArgumentsIndex)
    {
      CommandParameterInternal parameterInternal = (CommandParameterInternal) null;
      while (unboundArgumentsIndex < unboundArgumentsCollection.Count)
      {
        CommandParameterInternal unboundArguments1 = unboundArgumentsCollection[unboundArgumentsIndex++];
        if (!unboundArguments1.IsValidPair && !ParameterBinderController.IsParameterToken(unboundArguments1.Value1))
        {
          parameterInternal = unboundArguments1;
          break;
        }
        nonPositionalArguments.Add(unboundArguments1);
        if (unboundArgumentsCollection.Count - 1 >= unboundArgumentsIndex)
        {
          CommandParameterInternal unboundArguments2 = unboundArgumentsCollection[unboundArgumentsIndex];
          if (!unboundArguments2.IsValidPair && !ParameterBinderController.IsParameterToken(unboundArguments2.Value1))
          {
            nonPositionalArguments.Add(unboundArguments2);
            ++unboundArgumentsIndex;
          }
        }
      }
      return parameterInternal;
    }

    private static bool IsParameterToken(object value)
    {
      bool flag = false;
      if (value is Token token && token.Is(TokenId.ParameterToken))
        flag = true;
      return flag;
    }

    internal object DecodeValue(
      object value,
      Type parameterType,
      ParameterCollectionTypeInformation parameterCollectionType)
    {
      return this.DefaultParameterBinder.DecodeValue(value, parameterType, parameterCollectionType, true);
    }

    private SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>> EvaluateUnboundPositionalParameters()
    {
      SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>> result = new SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>>();
      if (this.UnboundParameters.Count > 0)
      {
        foreach (MergedCompiledCommandParameter parameter in this.UnboundParameters.Values)
        {
          if (((int) parameter.Parameter.ParameterSetFlags & (int) this.currentParameterSetFlag) != 0 || parameter.Parameter.IsInAllSets)
          {
            foreach (ParameterSetSpecificMetadata parameterSetData in parameter.Parameter.GetMatchingParameterSetData(this.currentParameterSetFlag))
            {
              if (!parameterSetData.ValueFromRemainingArguments)
              {
                int position = parameterSetData.Position;
                if (position != int.MinValue)
                  ParameterBinderController.AddNewPosition(result, position, parameter, parameterSetData);
              }
            }
          }
        }
      }
      return result;
    }

    private static void AddNewPosition(
      SortedDictionary<int, Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>> result,
      int positionInParameterSet,
      MergedCompiledCommandParameter parameter,
      ParameterSetSpecificMetadata parameterSetData)
    {
      if (result.ContainsKey(positionInParameterSet))
      {
        Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> positionalCommandParameters = result[positionInParameterSet];
        if (ParameterBinderController.ContainsPositionalParameterInSet(positionalCommandParameters, parameter, parameterSetData.ParameterSetFlag))
          throw ParameterBinderController.tracer.NewInvalidOperationException();
        if (positionalCommandParameters.ContainsKey(parameter))
          positionalCommandParameters[parameter].ParameterSetData.Add(parameterSetData);
        else
          positionalCommandParameters.Add(parameter, new PositionalCommandParameter(parameter)
          {
            ParameterSetData = {
              parameterSetData
            }
          });
      }
      else
        result.Add(positionInParameterSet, new Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter>()
        {
          {
            parameter,
            new PositionalCommandParameter(parameter)
            {
              ParameterSetData = {
                parameterSetData
              }
            }
          }
        });
    }

    private static bool ContainsPositionalParameterInSet(
      Dictionary<MergedCompiledCommandParameter, PositionalCommandParameter> positionalCommandParameters,
      MergedCompiledCommandParameter parameter,
      uint parameterSet)
    {
      bool flag = false;
      foreach (KeyValuePair<MergedCompiledCommandParameter, PositionalCommandParameter> commandParameter in positionalCommandParameters)
      {
        if (commandParameter.Key != parameter)
        {
          foreach (ParameterSetSpecificMetadata specificMetadata in commandParameter.Value.ParameterSetData)
          {
            if (((int) specificMetadata.ParameterSetFlag & (int) parameterSet) != 0 || (int) specificMetadata.ParameterSetFlag == (int) parameterSet)
            {
              flag = true;
              break;
            }
          }
          if (flag)
            break;
        }
      }
      ParameterBinderController.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal bool BindPipelineParameter(
      object parameterValue,
      MergedCompiledCommandParameter parameter,
      ParameterBindingFlags flags)
    {
      bool flag = false;
      if (parameterValue != AutomationNull.Value)
      {
        ParameterBinderController.tracer.WriteLine("Adding PipelineParameter name={0}; value={1}", (object) parameter.Parameter.Name, parameterValue == null ? (object) "null" : parameterValue);
        this.BackupDefaultParameter(parameter);
        CommandParameterInternal parameterInternal = new CommandParameterInternal(parameter.Parameter.Name, parameterValue);
        flags &= ~ParameterBindingFlags.DelayBindScriptBlock;
        flag = this.BindParameter(this.currentParameterSetFlag, parameterInternal, parameter, flags);
        if (flag)
          this.parametersBoundThroughPipelineInput.Add(parameter);
      }
      return flag;
    }

    internal Collection<MergedCompiledCommandParameter> ParametersBoundThroughPipelineInput => this.parametersBoundThroughPipelineInput;

    internal virtual void RestoreDefaultParameterValues(
      IEnumerable<MergedCompiledCommandParameter> parameters)
    {
      if (parameters == null)
        throw ParameterBinderController.tracer.NewArgumentNullException(nameof (parameters));
      foreach (MergedCompiledCommandParameter parameter in parameters)
      {
        if (parameter != null)
        {
          CommandParameterInternal argumentToBind = (CommandParameterInternal) null;
          foreach (CommandParameterInternal parameterInternal in this.defaultParameterValues.Values)
          {
            if (string.Equals(parameter.Parameter.Name, parameterInternal.Name, StringComparison.OrdinalIgnoreCase))
            {
              argumentToBind = parameterInternal;
              break;
            }
          }
          if (argumentToBind != null)
          {
            Exception innerException = (Exception) null;
            try
            {
              this.RestoreParameter(argumentToBind, parameter);
            }
            catch (SetValueException ex)
            {
              ParameterBinderController.tracer.TraceException((Exception) ex);
              innerException = (Exception) ex;
            }
            if (innerException != null)
            {
              Type typeSpecified = argumentToBind.Value2 == null ? (Type) null : argumentToBind.Value2.GetType();
              ParameterBindingException bindingException = new ParameterBindingException(innerException, ErrorCategory.WriteError, this.InvocationInfo, argumentToBind.Token, parameter.Parameter.Name, parameter.Parameter.Type, typeSpecified, "ParameterBinderStrings", "ParameterBindingFailed", new object[1]
              {
                (object) innerException.Message
              });
              ParameterBinderController.tracer.TraceException((Exception) bindingException);
              throw bindingException;
            }
            if (this.boundParameters.ContainsKey(parameter.Parameter.Name))
              this.boundParameters.Remove(parameter.Parameter.Name);
            if (!this.unboundParameters.ContainsKey(parameter.Parameter.Name))
              this.unboundParameters.Add(parameter.Parameter.Name, parameter);
            if (this.boundArguments.ContainsKey(parameter.Parameter.Name))
              this.boundArguments.Remove(parameter.Parameter.Name);
          }
          else
          {
            if (!this.boundParameters.ContainsKey(parameter.Parameter.Name))
              this.boundParameters.Add(parameter.Parameter.Name, parameter);
            if (this.unboundParameters.ContainsKey(parameter.Parameter.Name))
              this.unboundParameters.Remove(parameter.Parameter.Name);
            if (!this.boundArguments.ContainsKey(parameter.Parameter.Name))
              this.boundArguments.Add(argumentToBind.Name, argumentToBind);
          }
        }
      }
    }

    internal virtual bool RestoreParameter(
      CommandParameterInternal argumentToBind,
      MergedCompiledCommandParameter parameter)
    {
      this.DefaultParameterBinder.BindParameter(argumentToBind.Name, argumentToBind.Value2);
      return true;
    }

    internal virtual Dictionary<string, CommandParameterInternal> DefaultParameterValues => this.defaultParameterValues;

    internal void BackupDefaultParameters()
    {
      foreach (CommandParameterInternal defaultParameterValue in this.BackupDefaultParameterValues())
        this.defaultParameterValues.Add(defaultParameterValue.Name, defaultParameterValue);
    }

    internal void BackupDefaultParameter(MergedCompiledCommandParameter parameter)
    {
      if (this.defaultParameterValues.ContainsKey(parameter.Parameter.Name))
        return;
      object defaultParameterValue = this.GetDefaultParameterValue(parameter.Parameter.Name);
      this.defaultParameterValues.Add(parameter.Parameter.Name, new CommandParameterInternal(parameter.Parameter.Name, defaultParameterValue));
    }

    internal virtual Collection<CommandParameterInternal> BackupDefaultParameterValues() => new Collection<CommandParameterInternal>();

    internal virtual object GetDefaultParameterValue(string name) => this.DefaultParameterBinder.GetDefaultParameterValue(name);

    internal void BindUnboundScriptParameter(
      MergedCompiledCommandParameter parameter,
      uint parameterSets)
    {
      ScriptParameterBinder defaultParameterBinder = (ScriptParameterBinder) this.DefaultParameterBinder;
      ScriptBlock script = defaultParameterBinder.Script;
      if (!script.RuntimeDefinedParameters.ContainsKey(parameter.Parameter.Name))
        return;
      bool recordBoundParameters = defaultParameterBinder.RecordBoundParameters;
      try
      {
        defaultParameterBinder.RecordBoundParameters = false;
        RuntimeDefinedParameter definedParameter = script.RuntimeDefinedParameters[parameter.Parameter.Name];
        object scriptParameterValue = defaultParameterBinder.GetDefaultScriptParameterValue(definedParameter);
        CommandParameterInternal parameterInternal = new CommandParameterInternal(parameter.Parameter.Name, scriptParameterValue);
        ParameterBindingFlags flags = ParameterBindingFlags.IsDefaultValue;
        if (definedParameter.IsSet)
          flags |= ParameterBindingFlags.ShouldCoerceType;
        this.BindParameter(parameterSets, parameterInternal, parameter, flags);
      }
      finally
      {
        defaultParameterBinder.RecordBoundParameters = recordBoundParameters;
      }
    }
  }
}
