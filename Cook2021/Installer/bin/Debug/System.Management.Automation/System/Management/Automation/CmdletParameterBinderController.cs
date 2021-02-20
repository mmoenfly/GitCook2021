// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CmdletParameterBinderController
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Text;

namespace System.Management.Automation
{
  internal class CmdletParameterBinderController : ParameterBinderController
  {
    [TraceSource("ParameterBinderController", "Controls the interaction between the command processor and the parameter binder(s).")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ParameterBinderController", "Controls the interaction between the command processor and the parameter binder(s).");
    private Cmdlet cmdlet;
    private CommandMetadata commandMetadata;
    private MshCommandRuntime commandRuntime;
    private ParameterBinderBase dynamicParameterBinder;
    private ReflectionParameterBinder shouldProcessParameterBinder;
    private ReflectionParameterBinder transactionParameterBinder;
    private ReflectionParameterBinder commonParametersBinder;
    private Dictionary<MergedCompiledCommandParameter, CmdletParameterBinderController.DelayedScriptBlockArgument> delayBindScriptBlocks = new Dictionary<MergedCompiledCommandParameter, CmdletParameterBinderController.DelayedScriptBlockArgument>();

    internal CmdletParameterBinderController(
      Cmdlet cmdlet,
      CommandMetadata commandMetadata,
      ParameterBinderBase parameterBinder)
      : base(cmdlet.MyInvocation, cmdlet.Context, parameterBinder, (InternalCommand) cmdlet)
    {
      if (cmdlet == null)
        throw CmdletParameterBinderController.tracer.NewArgumentNullException(nameof (cmdlet));
      if (commandMetadata == null)
        throw CmdletParameterBinderController.tracer.NewArgumentNullException(nameof (commandMetadata));
      this.cmdlet = cmdlet;
      this.commandRuntime = (MshCommandRuntime) cmdlet.CommandRuntime;
      this.commandMetadata = commandMetadata;
      this.AddUnboundParameters(this.BindableParameters.ReplaceMetadata(commandMetadata.StaticCommandParameterMetadata));
      int mappingFromMetadata = (int) this.BindableParameters.GenerateParameterSetMappingFromMetadata(commandMetadata.DefaultParameterSetName);
    }

    internal bool BindCommandLineParameters(Collection<CommandParameterInternal> arguments)
    {
      CmdletParameterBinderController.tracer.WriteLine("Argument count: {0}", (object) arguments.Count);
      bool flag = false;
      if (this.Command != null && this.DefaultParameterBinder != null)
      {
        this.BindCommandLineParametersNoValidation(arguments);
        bool isPipelineInputExpected = !this.commandRuntime.IsClosed || !this.commandRuntime.InputPipe.Empty;
        int validParameterSetCount = isPipelineInputExpected ? this.ValidateParameterSets(true, false) : this.ValidateParameterSets(false, true);
        using (ParameterBinderBase.bindingTracer.TraceScope("MANDATORY PARAMETER CHECK on cmdlet [{0}]", (object) this.commandMetadata.Name))
        {
          Collection<MergedCompiledCommandParameter> missingMandatoryParameters = (Collection<MergedCompiledCommandParameter>) null;
          this.HandleUnboundMandatoryParameters(validParameterSetCount, true, isPipelineInputExpected, out missingMandatoryParameters);
        }
        if (!isPipelineInputExpected)
          this.VerifyParameterSetSelected();
        this.prePipelineProcessingParameterSetFlags = this.currentParameterSetFlag;
      }
      return flag;
    }

    internal void BindCommandLineParametersNoValidation(
      Collection<CommandParameterInternal> arguments)
    {
      foreach (CommandParameterInternal parameterInternal in arguments)
        this.UnboundArguments.Add(parameterInternal);
      CommandMetadata commandMetadata = this.commandMetadata;
      this.ReparseUnboundArguments();
      using (ParameterBinderBase.bindingTracer.TraceScope("BIND NAMED cmd line args [{0}]", (object) this.commandMetadata.Name))
        this.UnboundArguments = this.BindParameters(this.currentParameterSetFlag, this.UnboundArguments, commandMetadata);
      ParameterBindingException originalBindingException = (ParameterBindingException) null;
      ParameterBindingException outgoingBindingException = (ParameterBindingException) null;
      using (ParameterBinderBase.bindingTracer.TraceScope("BIND POSITIONAL cmd line args [{0}]", (object) this.commandMetadata.Name))
      {
        this.UnboundArguments = this.BindPositionalParameters(this.UnboundArguments, this.currentParameterSetFlag, commandMetadata.DefaultParameterSetFlag, commandMetadata.ImplementsDynamicParameters, out outgoingBindingException);
        originalBindingException = outgoingBindingException;
      }
      this.ValidateParameterSets(true, false);
      this.HandleCommandLineDynamicParameters(out outgoingBindingException);
      if (this.UnboundArguments.Count != 0)
      {
        if (originalBindingException == null)
          originalBindingException = outgoingBindingException;
        this.HandleRemainingArguments();
      }
      this.VerifyArgumentsProcessed(originalBindingException);
    }

    private void VerifyArgumentsProcessed(ParameterBindingException originalBindingException)
    {
      if (this.UnboundArguments.Count > 0)
      {
        CommandParameterInternal unboundArgument = this.UnboundArguments[0];
        Type typeSpecified = (Type) null;
        if (unboundArgument.Value2 != null && unboundArgument.Value2 != UnboundParameter.Value)
          typeSpecified = !(unboundArgument.Value2 is Token) ? unboundArgument.Value2.GetType() : ((Token) unboundArgument.Value2).Data.GetType();
        ParameterBindingException bindingException1;
        if (unboundArgument.Value1 is Token)
        {
          Token token = (Token) unboundArgument.Value1;
          bindingException1 = new ParameterBindingException(ErrorCategory.InvalidArgument, this.Command.MyInvocation, token, unboundArgument.Name, (Type) null, typeSpecified, "ParameterBinderStrings", token.TokenId == TokenId.ParameterToken ? "NamedParameterNotFound" : "PositionalParameterNotFound", new object[0]);
        }
        else
        {
          string errorIdAndResourceId = "PositionalParameterNotFound";
          if (this.UnboundArguments[0].Name == null && originalBindingException != null)
          {
            bindingException1 = originalBindingException;
          }
          else
          {
            string parameterName = "$null";
            if (this.UnboundArguments[0].Value1 != null)
            {
              if (this.UnboundArguments[0].Name != null)
                errorIdAndResourceId = "NamedParameterNotFound";
              try
              {
                parameterName = this.UnboundArguments[0].Value1.ToString();
              }
              catch (Exception ex)
              {
                CommandProcessorBase.CheckForSevereException(ex);
                ParameterBindingException bindingException2 = (ParameterBindingException) new ParameterBindingArgumentTransformationException(ex, ErrorCategory.InvalidData, this.InvocationInfo, (Token) null, (string) null, (Type) null, this.UnboundArguments[0].Value1.GetType(), "ParameterBinderStrings", "ParameterArgumentTransformationErrorMessageOnly", new object[1]
                {
                  (object) ex.Message
                });
                CmdletParameterBinderController.tracer.TraceException((Exception) bindingException2);
                throw bindingException2;
              }
            }
            bindingException1 = new ParameterBindingException(ErrorCategory.InvalidArgument, this.Command.MyInvocation, (Token) null, parameterName, (Type) null, typeSpecified, "ParameterBinderStrings", errorIdAndResourceId, new object[0]);
          }
        }
        CmdletParameterBinderController.tracer.TraceException((Exception) bindingException1);
        throw bindingException1;
      }
    }

    private void VerifyParameterSetSelected()
    {
      if (this.BindableParameters.ParameterSetCount <= 1 || this.currentParameterSetFlag != uint.MaxValue)
        return;
      if (((int) this.currentParameterSetFlag & (int) this.commandMetadata.DefaultParameterSetFlag) != 0 && this.commandMetadata.DefaultParameterSetFlag != uint.MaxValue)
      {
        ParameterBinderBase.bindingTracer.WriteLine("{0} valid parameter sets, using the DEFAULT PARAMETER SET: [{0}]", (object) this.BindableParameters.ParameterSetCount, (object) this.commandMetadata.DefaultParameterSetName);
        this.currentParameterSetFlag = this.commandMetadata.DefaultParameterSetFlag;
      }
      else
      {
        ParameterBinderBase.bindingTracer.TraceError("ERROR: {0} valid parameter sets, but NOT DEFAULT PARAMETER SET.", (object) this.BindableParameters.ParameterSetCount);
        this.ThrowAmbiguousParameterSetException(this.currentParameterSetFlag, this.BindableParameters);
      }
    }

    internal override bool RestoreParameter(
      CommandParameterInternal argumentToBind,
      MergedCompiledCommandParameter parameter)
    {
      switch (parameter.BinderAssociation)
      {
        case ParameterBinderAssociation.DeclaredFormalParameters:
          this.DefaultParameterBinder.BindParameter(argumentToBind.Name, argumentToBind.Value2);
          break;
        case ParameterBinderAssociation.DynamicParameters:
          if (this.dynamicParameterBinder != null)
          {
            this.dynamicParameterBinder.BindParameter(argumentToBind.Name, argumentToBind.Value2);
            break;
          }
          break;
        case ParameterBinderAssociation.CommonParameters:
          this.CommonParametersBinder.BindParameter(argumentToBind.Name, argumentToBind.Value2);
          break;
        case ParameterBinderAssociation.ShouldProcessParameters:
          this.ShouldProcessParametersBinder.BindParameter(argumentToBind.Name, argumentToBind.Value2);
          break;
        case ParameterBinderAssociation.TransactionParameters:
          this.TransactionParametersBinder.BindParameter(argumentToBind.Name, argumentToBind.Value2);
          break;
      }
      return true;
    }

    private Collection<CommandParameterInternal> BindParameters(
      uint parameterSets,
      Collection<CommandParameterInternal> arguments,
      CommandMetadata commandMetadata)
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
              CmdletParameterBinderController.tracer.TraceException((Exception) bindingException);
              throw bindingException;
            }
            if (((int) matchingParameter.Parameter.ParameterSetFlags & (int) parameterSets) == 0 && !matchingParameter.Parameter.IsInAllSets)
            {
              string parameterSetName = this.BindableParameters.GetParameterSetName(parameterSets);
              ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.InvalidArgument, this.Command.MyInvocation, (Token) null, parameterInternal.Name, (Type) null, (Type) null, "ParameterBinderStrings", "ParameterNotInParameterSet", new object[1]
              {
                (object) parameterSetName
              });
              CmdletParameterBinderController.tracer.TraceException((Exception) bindingException);
              throw bindingException;
            }
            this.BindParameter(parameterSets, parameterInternal, matchingParameter, ParameterBindingFlags.ShouldCoerceType | ParameterBindingFlags.DelayBindScriptBlock);
          }
          else
            collection.Add(parameterInternal);
        }
      }
      return collection;
    }

    private static bool IsParameterScriptBlockBindable(MergedCompiledCommandParameter parameter)
    {
      bool flag = false;
      Type type = parameter.Parameter.Type;
      if (type == typeof (object))
        flag = true;
      else if (type == typeof (ScriptBlock))
        flag = true;
      else if (type.IsSubclassOf(typeof (ScriptBlock)))
      {
        flag = true;
      }
      else
      {
        ParameterCollectionTypeInformation collectionTypeInformation = parameter.Parameter.CollectionTypeInformation;
        if (collectionTypeInformation.ParameterCollectionType != ParameterCollectionType.NotCollection)
        {
          if (collectionTypeInformation.ElementType == typeof (object))
            flag = true;
          else if (collectionTypeInformation.ElementType == typeof (ScriptBlock))
            flag = true;
          else if (collectionTypeInformation.ElementType.IsSubclassOf(typeof (ScriptBlock)))
            flag = true;
        }
      }
      CmdletParameterBinderController.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal override Collection<CommandParameterInternal> BindParameters(
      Collection<CommandParameterInternal> parameters)
    {
      return this.BindParameters(uint.MaxValue, parameters, this.commandMetadata);
    }

    internal override bool BindParameter(
      uint parameterSets,
      CommandParameterInternal argument,
      MergedCompiledCommandParameter parameter,
      ParameterBindingFlags flags)
    {
      bool flag1 = true;
      if ((flags & ParameterBindingFlags.DelayBindScriptBlock) != ParameterBindingFlags.None && parameter.Parameter.DoesParameterSetTakePipelineInput(parameterSets) && argument.Value2 != null && ((argument.Value2 is ScriptBlock || argument.Value2 is CmdletParameterBinderController.DelayedScriptBlockArgument) && !CmdletParameterBinderController.IsParameterScriptBlockBindable(parameter)))
      {
        if (this.commandRuntime.IsClosed && this.commandRuntime.InputPipe.Empty)
        {
          ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.MetadataError, this.Command.MyInvocation, argument.Token, parameter.Parameter.Name, parameter.Parameter.Type, (Type) null, "ParameterBinderStrings", "ScriptBlockArgumentNoInput", new object[0]);
          CmdletParameterBinderController.tracer.TraceException((Exception) bindingException);
          throw bindingException;
        }
        ParameterBinderBase.bindingTracer.WriteLine("Adding ScriptBlock to delay-bind list for parameter '{0}'", (object) parameter.Parameter.Name);
        if (!(argument.Value2 is CmdletParameterBinderController.DelayedScriptBlockArgument scriptBlockArgument))
        {
          scriptBlockArgument = new CmdletParameterBinderController.DelayedScriptBlockArgument();
          scriptBlockArgument.argument = argument;
          scriptBlockArgument.parameterBinder = this;
        }
        if (!this.delayBindScriptBlocks.ContainsKey(parameter))
          this.delayBindScriptBlocks.Add(parameter, scriptBlockArgument);
        if (parameter.Parameter.ParameterSetFlags != 0U)
          this.currentParameterSetFlag &= parameter.Parameter.ParameterSetFlags;
        if (this.UnboundParameters.ContainsKey(parameter.Parameter.Name))
          this.UnboundParameters.Remove(parameter.Parameter.Name);
        if (!this.BoundParameters.ContainsKey(parameter.Parameter.Name))
          this.BoundParameters.Add(parameter.Parameter.Name, parameter);
        if (!this.BoundArguments.ContainsKey(parameter.Parameter.Name))
          this.BoundArguments.Add(parameter.Parameter.Name, argument);
        if (this.DefaultParameterBinder.RecordBoundParameters && !this.DefaultParameterBinder.CommandLineParameters.ContainsKey(parameter.Parameter.Name))
          this.DefaultParameterBinder.CommandLineParameters.Add(parameter.Parameter.Name, (object) scriptBlockArgument);
        flag1 = false;
      }
      bool flag2 = false;
      if (flag1)
      {
        try
        {
          flag2 = this.BindParameter(argument, parameter, flags);
        }
        catch (Exception ex)
        {
          Exception exception = ex;
          bool flag3 = true;
          if ((flags & ParameterBindingFlags.ShouldCoerceType) == ParameterBindingFlags.None)
          {
            for (; exception != null; exception = exception.InnerException)
            {
              if (exception is PSInvalidCastException)
              {
                flag3 = false;
                break;
              }
            }
          }
          if (flag3)
            throw;
        }
      }
      CmdletParameterBinderController.tracer.WriteLine("result = {0}", (object) flag2);
      return flag2;
    }

    private bool BindParameter(
      CommandParameterInternal argument,
      MergedCompiledCommandParameter parameter,
      ParameterBindingFlags flags)
    {
      bool flag = false;
      switch (parameter.BinderAssociation)
      {
        case ParameterBinderAssociation.DeclaredFormalParameters:
          flag = this.DefaultParameterBinder.BindParameter(argument, parameter.Parameter, flags);
          break;
        case ParameterBinderAssociation.DynamicParameters:
          if (this.dynamicParameterBinder != null)
          {
            flag = this.dynamicParameterBinder.BindParameter(argument, parameter.Parameter, flags);
            break;
          }
          break;
        case ParameterBinderAssociation.CommonParameters:
          flag = this.CommonParametersBinder.BindParameter(argument, parameter.Parameter, flags);
          break;
        case ParameterBinderAssociation.ShouldProcessParameters:
          flag = this.ShouldProcessParametersBinder.BindParameter(argument, parameter.Parameter, flags);
          break;
        case ParameterBinderAssociation.TransactionParameters:
          flag = this.TransactionParametersBinder.BindParameter(argument, parameter.Parameter, flags);
          break;
      }
      if (flag && (flags & ParameterBindingFlags.IsDefaultValue) == ParameterBindingFlags.None)
      {
        if (parameter.Parameter.ParameterSetFlags != 0U)
          this.currentParameterSetFlag &= parameter.Parameter.ParameterSetFlags;
        if (this.UnboundParameters.ContainsKey(parameter.Parameter.Name))
          this.UnboundParameters.Remove(parameter.Parameter.Name);
        if (!this.BoundParameters.ContainsKey(parameter.Parameter.Name))
          this.BoundParameters.Add(parameter.Parameter.Name, parameter);
        if (!this.BoundArguments.ContainsKey(parameter.Parameter.Name))
          this.BoundArguments.Add(parameter.Parameter.Name, argument);
      }
      return flag;
    }

    private void HandleRemainingArguments()
    {
      if (this.UnboundArguments.Count <= 0)
        return;
      MergedCompiledCommandParameter parameter = (MergedCompiledCommandParameter) null;
      foreach (MergedCompiledCommandParameter commandParameter in this.UnboundParameters.Values)
      {
        ParameterSetSpecificMetadata parameterSetData = commandParameter.Parameter.GetParameterSetData(this.currentParameterSetFlag);
        if (parameterSetData != null && parameterSetData.ValueFromRemainingArguments)
        {
          if (parameter != null)
          {
            ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.MetadataError, this.Command.MyInvocation, (Token) null, commandParameter.Parameter.Name, commandParameter.Parameter.Type, (Type) null, "ParameterBinderStrings", "AmbiguousParameterSet", new object[0]);
            CmdletParameterBinderController.tracer.TraceException((Exception) bindingException);
            throw bindingException;
          }
          parameter = commandParameter;
        }
      }
      if (parameter == null)
        return;
      using (ParameterBinderBase.bindingTracer.TraceScope("BIND REMAININGARGUMENTS cmd line args to param: [{0}]", (object) parameter.Parameter.Name))
      {
        ArrayList arrayList = new ArrayList();
        foreach (CommandParameterInternal unboundArgument in this.UnboundArguments)
        {
          if (unboundArgument.Value1 != null && unboundArgument.Value1 != AutomationNull.Value && unboundArgument.Value1 != UnboundParameter.Value)
            arrayList.Add(unboundArgument.Value1);
          if (unboundArgument.Value2 != AutomationNull.Value && unboundArgument.Value2 != UnboundParameter.Value)
            arrayList.Add(unboundArgument.Value2);
        }
        this.BindParameter(new CommandParameterInternal(parameter.Parameter.Name, (object) arrayList), parameter, ParameterBindingFlags.ShouldCoerceType);
        this.UnboundArguments.Clear();
      }
    }

    private void HandleCommandLineDynamicParameters(
      out ParameterBindingException outgoingBindingException)
    {
      outgoingBindingException = (ParameterBindingException) null;
      if (!this.commandMetadata.ImplementsDynamicParameters)
        return;
      using (ParameterBinderBase.bindingTracer.TraceScope("BIND cmd line args to DYNAMIC parameters."))
      {
        CmdletParameterBinderController.tracer.WriteLine("The Cmdlet supports the dynamic parameter interface", new object[0]);
        if (!(this.Command is IDynamicParameters command))
          return;
        if (this.dynamicParameterBinder == null)
        {
          CmdletParameterBinderController.tracer.WriteLine("Getting the bindable object from the Cmdlet", new object[0]);
          object dynamicParameters;
          try
          {
            dynamicParameters = command.GetDynamicParameters();
          }
          catch (Exception ex)
          {
            CommandProcessorBase.CheckForSevereException(ex);
            if (ex is ProviderInvocationException)
            {
              throw;
            }
            else
            {
              ParameterBindingException bindingException = new ParameterBindingException(ex, ErrorCategory.InvalidArgument, this.Command.MyInvocation, (Token) null, (string) null, (Type) null, (Type) null, "ParameterBinderStrings", "GetDynamicParametersException", new object[1]
              {
                (object) ex.Message
              });
              CmdletParameterBinderController.tracer.TraceException((Exception) bindingException);
              throw bindingException;
            }
          }
          if (dynamicParameters != null)
          {
            ParameterBinderBase.bindingTracer.WriteLine("DYNAMIC parameter object: [{0}]", (object) dynamicParameters.GetType());
            CmdletParameterBinderController.tracer.WriteLine("Creating a new parameter binder for the dynamic parameter object", new object[0]);
            InternalParameterMetadata parameterMetadata;
            if (dynamicParameters is RuntimeDefinedParameterDictionary parameterDictionary)
            {
              parameterMetadata = InternalParameterMetadata.Get(parameterDictionary, true, true);
              this.dynamicParameterBinder = (ParameterBinderBase) new RuntimeDefinedParameterBinder(parameterDictionary, this.Command, this.CommandLineParameters);
            }
            else
            {
              parameterMetadata = InternalParameterMetadata.Get(dynamicParameters.GetType(), this.Context, true);
              this.dynamicParameterBinder = (ParameterBinderBase) new ReflectionParameterBinder(dynamicParameters, this.Command as Cmdlet, this.CommandLineParameters);
            }
            this.AddUnboundParameters(this.BindableParameters.AddMetadataForBinder(parameterMetadata, ParameterBinderAssociation.DynamicParameters));
            this.commandMetadata.DefaultParameterSetFlag = this.BindableParameters.GenerateParameterSetMappingFromMetadata(this.commandMetadata.DefaultParameterSetName);
          }
        }
        if (this.dynamicParameterBinder == null)
        {
          CmdletParameterBinderController.tracer.WriteLine("No dynamic parameter object was returned from the Cmdlet", new object[0]);
        }
        else
        {
          if (this.UnboundArguments.Count <= 0)
            return;
          using (ParameterBinderBase.bindingTracer.TraceScope("BIND NAMED args to DYNAMIC parameters"))
          {
            this.ReparseUnboundArguments();
            this.UnboundArguments = this.BindParameters(this.currentParameterSetFlag, this.UnboundArguments, this.commandMetadata);
          }
          using (ParameterBinderBase.bindingTracer.TraceScope("BIND POSITIONAL args to DYNAMIC parameters"))
            this.UnboundArguments = this.BindPositionalParameters(this.UnboundArguments, this.currentParameterSetFlag, this.commandMetadata.DefaultParameterSetFlag, false, out outgoingBindingException);
        }
      }
    }

    private Collection<MergedCompiledCommandParameter> GetMissingMandatoryParameters(
      int validParameterSetCount,
      bool isPipelineInputExpected,
      out Collection<MergedCompiledCommandParameter> missingOptionalParameters)
    {
      Collection<MergedCompiledCommandParameter> collection = new Collection<MergedCompiledCommandParameter>();
      missingOptionalParameters = new Collection<MergedCompiledCommandParameter>();
      uint parameterSetFlag = this.commandMetadata.DefaultParameterSetFlag;
      uint parameterSetFlags = 0;
      Dictionary<uint, ParameterSetPromptingData> promptingData = new Dictionary<uint, ParameterSetPromptingData>();
      bool flag1 = false;
      bool flag2 = false;
      foreach (MergedCompiledCommandParameter parameter in this.UnboundParameters.Values)
      {
        Collection<ParameterSetSpecificMetadata> parameterSetData = parameter.Parameter.GetMatchingParameterSetData(this.currentParameterSetFlag);
        uint num1 = 0;
        bool flag3 = false;
        foreach (ParameterSetSpecificMetadata parameterSetMetadata in parameterSetData)
        {
          uint num2 = this.NewParameterSetPromptingData(promptingData, parameter, parameterSetMetadata, parameterSetFlag, isPipelineInputExpected);
          if (num2 != 0U)
          {
            flag1 = true;
            flag3 = true;
            if (num2 != uint.MaxValue)
            {
              num1 |= this.currentParameterSetFlag & num2;
              parameterSetFlags |= this.currentParameterSetFlag & num1;
            }
            else
              flag2 = true;
          }
        }
        if (!isPipelineInputExpected && flag3)
          collection.Add(parameter);
      }
      if (flag1 && isPipelineInputExpected)
      {
        if (parameterSetFlags == 0U)
          parameterSetFlags = this.currentParameterSetFlag;
        if (flag2)
        {
          uint num = this.BindableParameters.AllParameterSetFlags;
          if (num == 0U)
            num = uint.MaxValue;
          parameterSetFlags = this.currentParameterSetFlag & num;
        }
        if (validParameterSetCount > 1 && parameterSetFlag != 0U && (((int) parameterSetFlag & (int) parameterSetFlags) == 0 && ((int) parameterSetFlag & (int) this.currentParameterSetFlag) != 0))
        {
          uint num = 0;
          foreach (ParameterSetPromptingData setPromptingData in promptingData.Values)
          {
            if (((int) setPromptingData.ParameterSet & (int) this.currentParameterSetFlag) != 0 && ((int) setPromptingData.ParameterSet & (int) parameterSetFlag) == 0 && (!setPromptingData.IsAllSet && setPromptingData.PipelineableMandatoryParameters.Count > 0))
            {
              num = setPromptingData.ParameterSet;
              break;
            }
          }
          if (num == 0U)
          {
            parameterSetFlags = parameterSetFlag;
            this.currentParameterSetFlag = parameterSetFlag;
            ((Cmdlet) this.Command).SetParameterSetName(this.CurrentParameterSetName);
          }
        }
        switch (CmdletParameterBinderController.ValidParameterSetCount(parameterSetFlags))
        {
          case 0:
            this.ThrowAmbiguousParameterSetException(this.currentParameterSetFlag, this.BindableParameters);
            break;
          case 1:
            using (Dictionary<uint, ParameterSetPromptingData>.ValueCollection.Enumerator enumerator = promptingData.Values.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                ParameterSetPromptingData current = enumerator.Current;
                if (((int) current.ParameterSet & (int) parameterSetFlags) != 0 || current.IsAllSet)
                {
                  foreach (MergedCompiledCommandParameter key in current.NonpipelineableMandatoryParameters.Keys)
                    collection.Add(key);
                  if (this.DefaultParameterBinder is ScriptParameterBinder)
                  {
                    foreach (MergedCompiledCommandParameter key in current.NonpipelineableOptionalParameters.Keys)
                      missingOptionalParameters.Add(key);
                  }
                }
              }
              break;
            }
          default:
            bool flag3 = false;
            if (parameterSetFlag != 0U && ((int) parameterSetFlags & (int) parameterSetFlag) != 0)
            {
              bool flag4 = false;
              foreach (ParameterSetPromptingData setPromptingData in promptingData.Values)
              {
                if (!setPromptingData.IsAllSet && !setPromptingData.IsDefaultSet && (setPromptingData.PipelineableMandatoryParameters.Count > 0 && setPromptingData.NonpipelineableMandatoryParameters.Count == 0))
                {
                  flag4 = true;
                  break;
                }
              }
              bool flag5 = false;
              foreach (ParameterSetPromptingData setPromptingData in promptingData.Values)
              {
                if (!setPromptingData.IsAllSet && !setPromptingData.IsDefaultSet && setPromptingData.PipelineableMandatoryByPropertyNameParameters.Count > 0)
                {
                  flag5 = true;
                  break;
                }
              }
              ParameterSetPromptingData setPromptingData1 = (ParameterSetPromptingData) null;
              if (promptingData.TryGetValue(parameterSetFlag, out setPromptingData1))
              {
                bool flag6 = setPromptingData1.PipelineableMandatoryParameters.Count > 0;
                if (setPromptingData1.PipelineableMandatoryByPropertyNameParameters.Count > 0 && !flag5)
                  flag3 = true;
                else if (flag6 && !flag4)
                  flag3 = true;
              }
              if (!flag3 && !flag4)
                flag3 = true;
              if (!flag3)
              {
                ParameterSetPromptingData setPromptingData2 = (ParameterSetPromptingData) null;
                if (promptingData.TryGetValue(uint.MaxValue, out setPromptingData2) && setPromptingData2.NonpipelineableMandatoryParameters.Count > 0)
                  flag3 = true;
              }
              if (flag3)
              {
                parameterSetFlags = parameterSetFlag;
                this.currentParameterSetFlag = parameterSetFlag;
                ((Cmdlet) this.Command).SetParameterSetName(this.CurrentParameterSetName);
                foreach (ParameterSetPromptingData setPromptingData2 in promptingData.Values)
                {
                  if (((int) setPromptingData2.ParameterSet & (int) parameterSetFlags) != 0 || setPromptingData2.IsAllSet)
                  {
                    foreach (MergedCompiledCommandParameter key in setPromptingData2.NonpipelineableMandatoryParameters.Keys)
                      collection.Add(key);
                    if (this.DefaultParameterBinder is ScriptParameterBinder)
                    {
                      foreach (MergedCompiledCommandParameter key in setPromptingData2.NonpipelineableOptionalParameters.Keys)
                        missingOptionalParameters.Add(key);
                    }
                  }
                }
              }
            }
            if (!flag3)
            {
              uint num1 = 0;
              uint num2 = 0;
              bool flag4 = false;
              bool flag5 = false;
              foreach (ParameterSetPromptingData setPromptingData in promptingData.Values)
              {
                if (((int) setPromptingData.ParameterSet & (int) parameterSetFlags) != 0 && !setPromptingData.IsAllSet && setPromptingData.PipelineableMandatoryByValueParameters.Count > 0)
                {
                  if (flag4)
                  {
                    flag5 = true;
                    num1 = 0U;
                    break;
                  }
                  num1 = setPromptingData.ParameterSet;
                  flag4 = true;
                }
              }
              bool flag6 = false;
              bool flag7 = false;
              foreach (ParameterSetPromptingData setPromptingData in promptingData.Values)
              {
                if (((int) setPromptingData.ParameterSet & (int) parameterSetFlags) != 0 && !setPromptingData.IsAllSet && setPromptingData.PipelineableMandatoryByPropertyNameParameters.Count > 0)
                {
                  if (flag6)
                  {
                    flag7 = true;
                    num2 = 0U;
                    break;
                  }
                  num2 = setPromptingData.ParameterSet;
                  flag6 = true;
                }
              }
              uint num3 = 0;
              if (flag4 ^ flag6)
                num3 = flag4 ? num1 : num2;
              if (num3 != 0U)
              {
                uint num4 = num3;
                this.currentParameterSetFlag = num3;
                ((Cmdlet) this.Command).SetParameterSetName(this.CurrentParameterSetName);
                using (Dictionary<uint, ParameterSetPromptingData>.ValueCollection.Enumerator enumerator = promptingData.Values.GetEnumerator())
                {
                  while (enumerator.MoveNext())
                  {
                    ParameterSetPromptingData current = enumerator.Current;
                    if (((int) current.ParameterSet & (int) num4) != 0 || current.IsAllSet)
                    {
                      foreach (MergedCompiledCommandParameter key in current.NonpipelineableMandatoryParameters.Keys)
                        collection.Add(key);
                      if (this.DefaultParameterBinder is ScriptParameterBinder)
                      {
                        foreach (MergedCompiledCommandParameter key in current.NonpipelineableOptionalParameters.Keys)
                          missingOptionalParameters.Add(key);
                      }
                    }
                  }
                  break;
                }
              }
              else
              {
                bool flag8 = false;
                foreach (ParameterSetPromptingData setPromptingData in promptingData.Values)
                {
                  if ((((int) setPromptingData.ParameterSet & (int) parameterSetFlags) != 0 || setPromptingData.IsAllSet) && setPromptingData.NonpipelineableMandatoryParameters.Count > 0)
                    flag8 = true;
                }
                if (flag8)
                {
                  if (num1 != 0U)
                  {
                    uint num4 = num1;
                    this.currentParameterSetFlag = num1;
                    ((Cmdlet) this.Command).SetParameterSetName(this.CurrentParameterSetName);
                    using (Dictionary<uint, ParameterSetPromptingData>.ValueCollection.Enumerator enumerator = promptingData.Values.GetEnumerator())
                    {
                      while (enumerator.MoveNext())
                      {
                        ParameterSetPromptingData current = enumerator.Current;
                        if (((int) current.ParameterSet & (int) num4) != 0 || current.IsAllSet)
                        {
                          foreach (MergedCompiledCommandParameter key in current.NonpipelineableMandatoryParameters.Keys)
                            collection.Add(key);
                          if (this.DefaultParameterBinder is ScriptParameterBinder)
                          {
                            foreach (MergedCompiledCommandParameter key in current.NonpipelineableOptionalParameters.Keys)
                              missingOptionalParameters.Add(key);
                          }
                        }
                      }
                      break;
                    }
                  }
                  else
                  {
                    if (!flag5 && !flag7)
                    {
                      this.ThrowAmbiguousParameterSetException(this.currentParameterSetFlag, this.BindableParameters);
                      break;
                    }
                    break;
                  }
                }
                else
                  break;
              }
            }
            else
              break;
        }
      }
      else if (this.DefaultParameterBinder is ScriptParameterBinder)
      {
        foreach (ParameterSetPromptingData setPromptingData in promptingData.Values)
        {
          if (((int) setPromptingData.ParameterSet & (int) this.currentParameterSetFlag) != 0 || setPromptingData.IsAllSet)
          {
            foreach (MergedCompiledCommandParameter key in setPromptingData.NonpipelineableOptionalParameters.Keys)
              missingOptionalParameters.Add(key);
          }
        }
      }
      return collection;
    }

    private uint NewParameterSetPromptingData(
      Dictionary<uint, ParameterSetPromptingData> promptingData,
      MergedCompiledCommandParameter parameter,
      ParameterSetSpecificMetadata parameterSetMetadata,
      uint defaultParameterSet,
      bool pipelineInputExpected)
    {
      uint num1 = 0;
      uint num2 = parameterSetMetadata.ParameterSetFlag;
      if (num2 == 0U)
        num2 = uint.MaxValue;
      bool isDefaultSet = defaultParameterSet != 0U && ((int) defaultParameterSet & (int) num2) != 0;
      bool flag1 = false;
      if (parameterSetMetadata.IsMandatory)
      {
        num1 |= num2;
        flag1 = true;
      }
      bool flag2 = false;
      if (pipelineInputExpected && (parameterSetMetadata.ValueFromPipeline || parameterSetMetadata.ValueFromPipelineByPropertyName))
        flag2 = true;
      if (flag1)
      {
        ParameterSetPromptingData setPromptingData = (ParameterSetPromptingData) null;
        if (!promptingData.TryGetValue(num2, out setPromptingData))
        {
          setPromptingData = new ParameterSetPromptingData(num2, isDefaultSet);
          promptingData.Add(num2, setPromptingData);
        }
        if (flag2)
        {
          setPromptingData.PipelineableMandatoryParameters[parameter] = parameterSetMetadata;
          if (parameterSetMetadata.ValueFromPipeline)
            setPromptingData.PipelineableMandatoryByValueParameters[parameter] = parameterSetMetadata;
          if (parameterSetMetadata.ValueFromPipelineByPropertyName)
            setPromptingData.PipelineableMandatoryByPropertyNameParameters[parameter] = parameterSetMetadata;
        }
        else
          setPromptingData.NonpipelineableMandatoryParameters[parameter] = parameterSetMetadata;
      }
      else
      {
        if (!flag2 && this.DefaultParameterBinder is ScriptParameterBinder)
        {
          ParameterSetPromptingData setPromptingData = (ParameterSetPromptingData) null;
          if (!promptingData.TryGetValue(num2, out setPromptingData))
          {
            setPromptingData = new ParameterSetPromptingData(num2, isDefaultSet);
            promptingData.Add(num2, setPromptingData);
          }
          setPromptingData.NonpipelineableOptionalParameters[parameter] = parameterSetMetadata;
        }
        num1 = 0U;
      }
      return num1;
    }

    private int ValidateParameterSets(bool prePipelineInput, bool setDefault)
    {
      int num = CmdletParameterBinderController.ValidParameterSetCount(this.currentParameterSetFlag);
      if (num == 0 && this.currentParameterSetFlag != uint.MaxValue)
        this.ThrowAmbiguousParameterSetException(this.currentParameterSetFlag, this.BindableParameters);
      else if (num > 1)
      {
        uint parameterSetFlag = this.commandMetadata.DefaultParameterSetFlag;
        bool flag1 = parameterSetFlag != 0U;
        bool flag2 = this.currentParameterSetFlag == uint.MaxValue;
        bool flag3 = (int) this.currentParameterSetFlag == (int) parameterSetFlag;
        if (flag2 && !flag1)
          num = 1;
        else if (!prePipelineInput && flag3 || flag1 && ((int) this.currentParameterSetFlag & (int) parameterSetFlag) != 0)
        {
          ((Cmdlet) this.Command).SetParameterSetName(this.BindableParameters.GetParameterSetName(parameterSetFlag));
          if (setDefault)
          {
            this.currentParameterSetFlag = this.commandMetadata.DefaultParameterSetFlag;
            num = 1;
          }
        }
        else if (!prePipelineInput || !this.AtLeastOneUnboundValidParameterSetTakesPipelineInput(this.currentParameterSetFlag))
          this.ThrowAmbiguousParameterSetException(this.currentParameterSetFlag, this.BindableParameters);
      }
      else
      {
        if (this.currentParameterSetFlag == uint.MaxValue)
        {
          num = this.BindableParameters.ParameterSetCount > 0 ? this.BindableParameters.ParameterSetCount : 1;
          if (!prePipelineInput || !this.AtLeastOneUnboundValidParameterSetTakesPipelineInput(this.currentParameterSetFlag))
          {
            if (this.commandMetadata.DefaultParameterSetFlag != 0U)
            {
              if (setDefault)
              {
                this.currentParameterSetFlag = this.commandMetadata.DefaultParameterSetFlag;
                num = 1;
              }
            }
            else if (num > 1)
              this.ThrowAmbiguousParameterSetException(this.currentParameterSetFlag, this.BindableParameters);
          }
        }
        ((Cmdlet) this.Command).SetParameterSetName(this.CurrentParameterSetName);
      }
      return num;
    }

    private void ThrowAmbiguousParameterSetException(
      uint parameterSetFlags,
      MergedCommandParameterMetadata bindableParameters)
    {
      ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.InvalidArgument, this.Command.MyInvocation, this.Command.MyInvocation.ScriptToken, (string) null, (Type) null, (Type) null, "ParameterBinderStrings", "AmbiguousParameterSet", new object[0]);
      CmdletParameterBinderController.tracer.TraceException((Exception) bindingException);
      uint parameterSet = 1;
      while (parameterSetFlags != 0U)
      {
        if ((parameterSetFlags & 1U) == 1U)
        {
          string parameterSetName = bindableParameters.GetParameterSetName(parameterSet);
          if (!string.IsNullOrEmpty(parameterSetName))
            ParameterBinderBase.bindingTracer.WriteLine("Remaining valid parameter set: {0}", (object) parameterSetName);
        }
        parameterSetFlags >>= 1;
        parameterSet <<= 1;
      }
      throw bindingException;
    }

    private bool AtLeastOneUnboundValidParameterSetTakesPipelineInput(uint validParameterSetFlags)
    {
      bool flag = false;
      foreach (MergedCompiledCommandParameter commandParameter in this.UnboundParameters.Values)
      {
        if (commandParameter.Parameter.DoesParameterSetTakePipelineInput(validParameterSetFlags))
        {
          flag = true;
          break;
        }
      }
      CmdletParameterBinderController.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    internal bool HandleUnboundMandatoryParameters(
      bool promptForMandatory,
      bool isPipelineInputExpected,
      out Collection<MergedCompiledCommandParameter> missingMandatoryParameters)
    {
      return this.HandleUnboundMandatoryParameters(CmdletParameterBinderController.ValidParameterSetCount(this.currentParameterSetFlag), promptForMandatory, isPipelineInputExpected, out missingMandatoryParameters);
    }

    internal bool HandleUnboundMandatoryParameters(
      int validParameterSetCount,
      bool promptForMandatory,
      bool isPipelineInputExpected,
      out Collection<MergedCompiledCommandParameter> missingMandatoryParameters)
    {
      bool flag = true;
      Collection<MergedCompiledCommandParameter> missingOptionalParameters = (Collection<MergedCompiledCommandParameter>) null;
      missingMandatoryParameters = this.GetMissingMandatoryParameters(validParameterSetCount, isPipelineInputExpected, out missingOptionalParameters);
      if (missingMandatoryParameters.Count > 0)
      {
        if (promptForMandatory)
        {
          if (this.Context.EngineHostInterface == null)
          {
            ParameterBinderBase.bindingTracer.WriteLine("ERROR: host does not support prompting for missing mandatory parameters", new object[0]);
            ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.InvalidArgument, this.Command.MyInvocation, (Token) null, CmdletParameterBinderController.BuildMissingParamsString(missingMandatoryParameters), (Type) null, (Type) null, "ParameterBinderStrings", "MissingMandatoryParameter", new object[0]);
            CmdletParameterBinderController.tracer.TraceException((Exception) bindingException);
            throw bindingException;
          }
          Dictionary<string, PSObject> dictionary = this.PromptForMissingMandatoryParameters(this.CreatePromptDataStructures(missingMandatoryParameters), missingMandatoryParameters);
          using (ParameterBinderBase.bindingTracer.TraceScope("BIND PROMPTED mandatory parameter args"))
          {
            foreach (KeyValuePair<string, PSObject> keyValuePair in dictionary)
              flag = this.BindParameter(new CommandParameterInternal(keyValuePair.Key, (object) keyValuePair.Value), ParameterBindingFlags.ShouldCoerceType | ParameterBindingFlags.ThrowOnParameterNotFound);
            flag = true;
          }
        }
        else
          flag = false;
      }
      if (flag && this.DefaultParameterBinder is ScriptParameterBinder && missingOptionalParameters != null)
      {
        foreach (MergedCompiledCommandParameter parameter in missingOptionalParameters)
          this.BindUnboundScriptParameter(parameter, uint.MaxValue);
      }
      return flag;
    }

    private Dictionary<string, PSObject> PromptForMissingMandatoryParameters(
      Collection<FieldDescription> fieldDescriptionList,
      Collection<MergedCompiledCommandParameter> missingMandatoryParameters)
    {
      Dictionary<string, PSObject> dictionary = (Dictionary<string, PSObject>) null;
      Exception exception = (Exception) null;
      try
      {
        ParameterBinderBase.bindingTracer.WriteLine("PROMPTING for missing mandatory parameters using the host", new object[0]);
        string resourceString = ResourceManagerCache.GetResourceString("ParameterBinderStrings", "PromptMessage");
        InvocationInfo myInvocation = (this.Command as Cmdlet).MyInvocation;
        dictionary = this.Context.EngineHostInterface.UI.Prompt(ResourceManagerCache.FormatResourceString("ParameterBinderStrings", "PromptCaption", (object) myInvocation.MyCommand.Name, (object) myInvocation.PipelinePosition), resourceString, fieldDescriptionList);
      }
      catch (NotImplementedException ex)
      {
        exception = (Exception) ex;
      }
      catch (HostException ex)
      {
        exception = (Exception) ex;
      }
      catch (PSInvalidOperationException ex)
      {
        exception = (Exception) ex;
      }
      if (exception != null)
      {
        ParameterBinderBase.bindingTracer.WriteLine("ERROR: host does not support prompting for missing mandatory parameters", new object[0]);
        ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.InvalidArgument, this.Command.MyInvocation, (Token) null, CmdletParameterBinderController.BuildMissingParamsString(missingMandatoryParameters), (Type) null, (Type) null, "ParameterBinderStrings", "MissingMandatoryParameter", new object[0]);
        CmdletParameterBinderController.tracer.TraceException((Exception) bindingException);
        throw bindingException;
      }
      if (dictionary == null || dictionary.Count == 0)
      {
        ParameterBinderBase.bindingTracer.WriteLine("ERROR: still missing mandatory parameters after PROMPTING", new object[0]);
        ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.InvalidArgument, this.Command.MyInvocation, (Token) null, CmdletParameterBinderController.BuildMissingParamsString(missingMandatoryParameters), (Type) null, (Type) null, "ParameterBinderStrings", "MissingMandatoryParameter", new object[0]);
        CmdletParameterBinderController.tracer.TraceException((Exception) bindingException);
        throw bindingException;
      }
      return dictionary;
    }

    internal static string BuildMissingParamsString(
      Collection<MergedCompiledCommandParameter> missingMandatoryParameters)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (MergedCompiledCommandParameter mandatoryParameter in missingMandatoryParameters)
        stringBuilder.AppendFormat(" {0}", (object) mandatoryParameter.Parameter.Name);
      return stringBuilder.ToString();
    }

    private Collection<FieldDescription> CreatePromptDataStructures(
      Collection<MergedCompiledCommandParameter> missingMandatoryParameters)
    {
      StringBuilder usedHotKeys = new StringBuilder();
      Collection<FieldDescription> collection = new Collection<FieldDescription>();
      foreach (MergedCompiledCommandParameter mandatoryParameter in missingMandatoryParameters)
      {
        ParameterSetSpecificMetadata parameterSetData = mandatoryParameter.Parameter.GetParameterSetData(this.currentParameterSetFlag);
        FieldDescription fieldDescription = new FieldDescription(mandatoryParameter.Parameter.Name);
        Cmdlet command = this.Command as Cmdlet;
        string str = (string) null;
        try
        {
          str = parameterSetData.GetHelpMessage(command);
        }
        catch (InvalidOperationException ex)
        {
        }
        catch (ArgumentException ex)
        {
        }
        if (!string.IsNullOrEmpty(str))
          fieldDescription.HelpMessage = str;
        fieldDescription.SetParameterType(mandatoryParameter.Parameter.Type);
        fieldDescription.Label = CmdletParameterBinderController.BuildLabel(mandatoryParameter.Parameter.Name, usedHotKeys);
        foreach (ValidateArgumentsAttribute validationAttribute in mandatoryParameter.Parameter.ValidationAttributes)
          fieldDescription.Attributes.Add((Attribute) validationAttribute);
        foreach (ArgumentTransformationAttribute transformationAttribute in mandatoryParameter.Parameter.ArgumentTransformationAttributes)
          fieldDescription.Attributes.Add((Attribute) transformationAttribute);
        fieldDescription.IsMandatory = true;
        collection.Add(fieldDescription);
      }
      return collection;
    }

    private static string BuildLabel(string parameterName, StringBuilder usedHotKeys)
    {
      bool flag = false;
      StringBuilder stringBuilder = new StringBuilder(parameterName);
      string str = usedHotKeys.ToString();
      for (int index = 0; index < parameterName.Length; ++index)
      {
        if (char.IsUpper(parameterName[index]) && str.IndexOf(parameterName[index]) == -1)
        {
          stringBuilder.Insert(index, '&');
          usedHotKeys.Append(parameterName[index]);
          flag = true;
          break;
        }
      }
      if (!flag)
      {
        for (int index = 0; index < parameterName.Length; ++index)
        {
          if (char.IsLower(parameterName[index]) && str.IndexOf(parameterName[index]) == -1)
          {
            stringBuilder.Insert(index, '&');
            usedHotKeys.Append(parameterName[index]);
            flag = true;
            break;
          }
        }
      }
      if (!flag)
      {
        for (int index = 0; index < parameterName.Length; ++index)
        {
          if (!char.IsLetter(parameterName[index]) && str.IndexOf(parameterName[index]) == -1)
          {
            stringBuilder.Insert(index, '&');
            usedHotKeys.Append(parameterName[index]);
            flag = true;
            break;
          }
        }
      }
      if (!flag)
        stringBuilder.Insert(0, '&');
      return stringBuilder.ToString();
    }

    internal string CurrentParameterSetName
    {
      get
      {
        string parameterSetName = this.BindableParameters.GetParameterSetName(this.currentParameterSetFlag);
        CmdletParameterBinderController.tracer.WriteLine("CurrentParameterSetName = {0}", (object) parameterSetName);
        return parameterSetName;
      }
    }

    internal bool BindPipelineParameters(PSObject inputToOperateOn)
    {
      bool flag1 = true;
      try
      {
        using (ParameterBinderBase.bindingTracer.TraceScope("BIND PIPELINE object to parameters: [{0}]", (object) this.commandMetadata.Name))
        {
          bool thereWasSomethingToBind = false;
          bool flag2 = this.InvokeAndBindDelayBindScriptBlock(inputToOperateOn, out thereWasSomethingToBind);
          bool flag3 = !thereWasSomethingToBind || thereWasSomethingToBind && flag2;
          bool flag4 = false;
          if (flag3)
            flag4 = this.BindPipelineParametersPrivate(inputToOperateOn);
          flag1 = thereWasSomethingToBind && flag2 || flag4;
        }
      }
      catch (ParameterBindingException ex)
      {
        this.RestoreDefaultParameterValues((IEnumerable<MergedCompiledCommandParameter>) this.ParametersBoundThroughPipelineInput);
        CmdletParameterBinderController.tracer.TraceException((Exception) ex);
        throw;
      }
      try
      {
        this.VerifyParameterSetSelected();
      }
      catch (ParameterBindingException ex)
      {
        this.RestoreDefaultParameterValues((IEnumerable<MergedCompiledCommandParameter>) this.ParametersBoundThroughPipelineInput);
        throw;
      }
      if (!flag1)
        this.RestoreDefaultParameterValues((IEnumerable<MergedCompiledCommandParameter>) this.ParametersBoundThroughPipelineInput);
      return flag1;
    }

    private bool BindPipelineParametersPrivate(PSObject inputToOperateOn)
    {
      ParameterBinderBase.bindingTracer.WriteLine("PIPELINE object TYPE = [{0}]", inputToOperateOn == null || inputToOperateOn == AutomationNull.Value ? (object) "null" : (inputToOperateOn.TypeNames.Count <= 0 || inputToOperateOn.TypeNames[0] == null ? (object) inputToOperateOn.BaseObject.GetType().FullName : (object) inputToOperateOn.TypeNames[0]));
      bool flag = false;
      ParameterBinderBase.bindingTracer.WriteLine("RESTORING pipeline parameter's original values", new object[0]);
      this.RestoreDefaultParameterValues((IEnumerable<MergedCompiledCommandParameter>) this.ParametersBoundThroughPipelineInput);
      this.ParametersBoundThroughPipelineInput.Clear();
      this.currentParameterSetFlag = this.prePipelineProcessingParameterSetFlags;
      uint parameterSetFlag = this.currentParameterSetFlag;
      for (CmdletParameterBinderController.CurrentlyBinding currentlyBinding = CmdletParameterBinderController.CurrentlyBinding.ValueFromPipelineNoCoercion; currentlyBinding <= CmdletParameterBinderController.CurrentlyBinding.ValueFromPipelineByPropertyNameWithCoercion; ++currentlyBinding)
      {
        if (this.BindUnboundParametersForBindingState(inputToOperateOn, currentlyBinding, ref parameterSetFlag))
        {
          this.ValidateParameterSets(true, true);
          parameterSetFlag = this.currentParameterSetFlag;
          flag = true;
        }
      }
      this.ValidateParameterSets(false, true);
      return flag;
    }

    private bool BindUnboundParametersForBindingState(
      PSObject inputToOperateOn,
      CmdletParameterBinderController.CurrentlyBinding currentlyBinding,
      ref uint validParameterSets)
    {
      bool flag = false;
      uint validParameterSets1 = validParameterSets;
      uint parameterSetFlag = this.commandMetadata.DefaultParameterSetFlag;
      if (parameterSetFlag != 0U && ((int) validParameterSets & (int) parameterSetFlag) != 0)
      {
        uint validParameterSets2 = parameterSetFlag;
        flag = this.BindUnboundParametersForBindingStateInParameterSet(inputToOperateOn, currentlyBinding, ref validParameterSets2);
        if (!flag)
          validParameterSets1 &= ~parameterSetFlag;
        else
          validParameterSets1 = parameterSetFlag;
      }
      if (!flag)
      {
        flag = this.BindUnboundParametersForBindingStateInParameterSet(inputToOperateOn, currentlyBinding, ref validParameterSets1);
        if (flag)
          validParameterSets = validParameterSets1;
      }
      CmdletParameterBinderController.tracer.WriteLine("aParameterWasBound = {0}", (object) flag);
      return flag;
    }

    private bool BindUnboundParametersForBindingStateInParameterSet(
      PSObject inputToOperateOn,
      CmdletParameterBinderController.CurrentlyBinding currentlyBinding,
      ref uint validParameterSets)
    {
      bool flag1 = false;
      ScriptParameterBinder defaultParameterBinder = this.DefaultParameterBinder as ScriptParameterBinder;
      foreach (MergedCompiledCommandParameter parameter in new List<MergedCompiledCommandParameter>((IEnumerable<MergedCompiledCommandParameter>) this.UnboundParameters.Values))
      {
        if (((int) validParameterSets & (int) parameter.Parameter.ParameterSetFlags) != 0 || parameter.Parameter.IsInAllSets)
        {
          Collection<ParameterSetSpecificMetadata> parameterSetData = parameter.Parameter.GetMatchingParameterSetData(validParameterSets);
          bool flag2 = false;
          foreach (ParameterSetSpecificMetadata specificMetadata in parameterSetData)
          {
            if (currentlyBinding == CmdletParameterBinderController.CurrentlyBinding.ValueFromPipelineNoCoercion && specificMetadata.ValueFromPipeline)
              flag2 = this.BindValueFromPipeline(inputToOperateOn, parameter, ParameterBindingFlags.None);
            else if (currentlyBinding == CmdletParameterBinderController.CurrentlyBinding.ValueFromPipelineByPropertyNameNoCoercion && specificMetadata.ValueFromPipelineByPropertyName && inputToOperateOn != null)
              flag2 = this.BindValueFromPipelineByPropertyName(inputToOperateOn, parameter, ParameterBindingFlags.None);
            else if (currentlyBinding == CmdletParameterBinderController.CurrentlyBinding.ValueFromPipelineWithCoercion && specificMetadata.ValueFromPipeline)
              flag2 = this.BindValueFromPipeline(inputToOperateOn, parameter, ParameterBindingFlags.ShouldCoerceType);
            else if (currentlyBinding == CmdletParameterBinderController.CurrentlyBinding.ValueFromPipelineByPropertyNameWithCoercion && specificMetadata.ValueFromPipelineByPropertyName && inputToOperateOn != null)
              flag2 = this.BindValueFromPipelineByPropertyName(inputToOperateOn, parameter, ParameterBindingFlags.ShouldCoerceType);
            if (flag2)
            {
              flag1 = true;
              break;
            }
          }
          if (!flag2 && defaultParameterBinder != null)
            this.BindUnboundScriptParameter(parameter, validParameterSets);
        }
      }
      return flag1;
    }

    private bool BindValueFromPipeline(
      PSObject inputToOperateOn,
      MergedCompiledCommandParameter parameter,
      ParameterBindingFlags flags)
    {
      ParameterBinderBase.bindingTracer.WriteLine((flags & ParameterBindingFlags.ShouldCoerceType) != ParameterBindingFlags.None ? "Parameter [{0}] PIPELINE INPUT ValueFromPipeline WITH COERCION" : "Parameter [{0}] PIPELINE INPUT ValueFromPipeline NO COERCION", (object) parameter.Parameter.Name);
      try
      {
        return this.BindPipelineParameter((object) inputToOperateOn, parameter, flags);
      }
      catch (ParameterBindingArgumentTransformationException ex)
      {
        if ((!(ex.InnerException is ArgumentTransformationMetadataException) ? ex.InnerException as PSInvalidCastException : ex.InnerException.InnerException as PSInvalidCastException) != null)
          return false;
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
        return false;
      }
    }

    private bool BindValueFromPipelineByPropertyName(
      PSObject inputToOperateOn,
      MergedCompiledCommandParameter parameter,
      ParameterBindingFlags flags)
    {
      bool flag = false;
      ParameterBinderBase.bindingTracer.WriteLine((flags & ParameterBindingFlags.ShouldCoerceType) != ParameterBindingFlags.None ? "Parameter [{0}] PIPELINE INPUT ValueFromPipelineByPropertyName WITH COERCION" : "Parameter [{0}] PIPELINE INPUT ValueFromPipelineByPropertyName NO COERCION", (object) parameter.Parameter.Name);
      PSMemberInfo property = (PSMemberInfo) inputToOperateOn.Properties[parameter.Parameter.Name];
      if (property == null)
      {
        foreach (string alias in parameter.Parameter.Aliases)
        {
          property = (PSMemberInfo) inputToOperateOn.Properties[alias];
          if (property != null)
            break;
        }
      }
      if (property != null)
      {
        try
        {
          flag = this.BindPipelineParameter(property.Value, parameter, flags);
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
          flag = false;
        }
      }
      return flag;
    }

    private bool InvokeAndBindDelayBindScriptBlock(
      PSObject inputToOperateOn,
      out bool thereWasSomethingToBind)
    {
      thereWasSomethingToBind = false;
      bool flag = true;
      foreach (KeyValuePair<MergedCompiledCommandParameter, CmdletParameterBinderController.DelayedScriptBlockArgument> delayBindScriptBlock in this.delayBindScriptBlocks)
      {
        thereWasSomethingToBind = true;
        CommandParameterInternal parameterInternal = delayBindScriptBlock.Value.argument;
        MergedCompiledCommandParameter key = delayBindScriptBlock.Key;
        ScriptBlock scriptBlock = parameterInternal.Value2 as ScriptBlock;
        Collection<PSObject> collection = (Collection<PSObject>) null;
        Exception innerException = (Exception) null;
        using (ParameterBinderBase.bindingTracer.TraceScope("Invoking delay-bind ScriptBlock"))
        {
          if (delayBindScriptBlock.Value.parameterBinder == this)
          {
            try
            {
              collection = scriptBlock.DoInvoke((object) inputToOperateOn, (object) inputToOperateOn);
              delayBindScriptBlock.Value.evaluatedArgument = collection;
            }
            catch (RuntimeException ex)
            {
              ParameterBinderBase.bindingTracer.TraceException((Exception) ex);
              innerException = (Exception) ex;
            }
            catch (Exception ex)
            {
              ParameterBinderBase.bindingTracer.TraceException(ex);
              throw;
            }
          }
          else
            collection = delayBindScriptBlock.Value.evaluatedArgument;
        }
        if (innerException != null)
        {
          ParameterBindingException bindingException = new ParameterBindingException(innerException, ErrorCategory.InvalidArgument, this.Command.MyInvocation, parameterInternal.Token, key.Parameter.Name, (Type) null, (Type) null, "ParameterBinderStrings", "ScriptBlockArgumentInvocationFailed", new object[1]
          {
            (object) innerException.Message
          });
          CmdletParameterBinderController.tracer.TraceException((Exception) bindingException);
          throw bindingException;
        }
        if (collection == null || collection.Count == 0)
        {
          ParameterBindingException bindingException = new ParameterBindingException(innerException, ErrorCategory.InvalidArgument, this.Command.MyInvocation, parameterInternal.Token, key.Parameter.Name, (Type) null, (Type) null, "ParameterBinderStrings", "ScriptBlockArgumentNoOutput", new object[0]);
          CmdletParameterBinderController.tracer.TraceException((Exception) bindingException);
          throw bindingException;
        }
        object obj = (object) collection;
        if (collection.Count == 1)
          obj = (object) collection[0];
        if (!this.BindParameter(new CommandParameterInternal(parameterInternal.Name, obj), key, ParameterBindingFlags.ShouldCoerceType))
          flag = false;
      }
      CmdletParameterBinderController.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    private static int ValidParameterSetCount(uint parameterSetFlags)
    {
      int num = 0;
      if (parameterSetFlags == uint.MaxValue)
      {
        num = 1;
      }
      else
      {
        for (; parameterSetFlags != 0U; parameterSetFlags >>= 1)
          num += (int) parameterSetFlags & 1;
      }
      CmdletParameterBinderController.tracer.WriteLine("result = {0}", (object) num);
      return num;
    }

    internal override object GetDefaultParameterValue(string name)
    {
      MergedCompiledCommandParameter matchingParameter = this.BindableParameters.GetMatchingParameter(name, false, this.InvocationInfo);
      object obj = (object) null;
      try
      {
        switch (matchingParameter.BinderAssociation)
        {
          case ParameterBinderAssociation.DeclaredFormalParameters:
            obj = this.DefaultParameterBinder.GetDefaultParameterValue(name);
            break;
          case ParameterBinderAssociation.DynamicParameters:
            if (this.dynamicParameterBinder != null)
            {
              obj = this.dynamicParameterBinder.GetDefaultParameterValue(name);
              break;
            }
            break;
          case ParameterBinderAssociation.CommonParameters:
            obj = this.CommonParametersBinder.GetDefaultParameterValue(name);
            break;
          case ParameterBinderAssociation.ShouldProcessParameters:
            obj = this.ShouldProcessParametersBinder.GetDefaultParameterValue(name);
            break;
        }
      }
      catch (GetValueException ex)
      {
        CmdletParameterBinderController.tracer.TraceException((Exception) ex);
        ParameterBindingParameterDefaultValueException defaultValueException = new ParameterBindingParameterDefaultValueException((Exception) ex, ErrorCategory.ReadError, this.Command.MyInvocation, (Token) null, name, (Type) null, (Type) null, "ParameterBinderStrings", "GetDefaultValueFailed", new object[1]
        {
          (object) ex.Message
        });
        CmdletParameterBinderController.tracer.TraceException((Exception) defaultValueException);
        throw defaultValueException;
      }
      return obj;
    }

    internal override InternalCommand Command
    {
      get => (InternalCommand) this.cmdlet;
      set
      {
        this.cmdlet = value as Cmdlet;
        base.Command = value;
      }
    }

    internal ReflectionParameterBinder ShouldProcessParametersBinder
    {
      get
      {
        if (this.shouldProcessParameterBinder == null)
          this.shouldProcessParameterBinder = new ReflectionParameterBinder((object) new ShouldProcessParameters(this.commandRuntime), this.Command as Cmdlet, this.CommandLineParameters);
        return this.shouldProcessParameterBinder;
      }
    }

    internal ReflectionParameterBinder TransactionParametersBinder
    {
      get
      {
        if (this.transactionParameterBinder == null)
          this.transactionParameterBinder = new ReflectionParameterBinder((object) new TransactionParameters(this.commandRuntime), this.Command as Cmdlet, this.CommandLineParameters);
        return this.transactionParameterBinder;
      }
    }

    internal ReflectionParameterBinder CommonParametersBinder
    {
      get
      {
        if (this.commonParametersBinder == null)
          this.commonParametersBinder = new ReflectionParameterBinder((object) new CommonParameters(this.commandRuntime), this.Command as Cmdlet, this.CommandLineParameters);
        return this.commonParametersBinder;
      }
    }

    private enum CurrentlyBinding
    {
      ValueFromPipelineNoCoercion,
      ValueFromPipelineByPropertyNameNoCoercion,
      ValueFromPipelineWithCoercion,
      ValueFromPipelineByPropertyNameWithCoercion,
    }

    private class DelayedScriptBlockArgument
    {
      internal CmdletParameterBinderController parameterBinder;
      internal CommandParameterInternal argument;
      internal Collection<PSObject> evaluatedArgument;

      public override string ToString() => this.argument.Value2.ToString();
    }
  }
}
