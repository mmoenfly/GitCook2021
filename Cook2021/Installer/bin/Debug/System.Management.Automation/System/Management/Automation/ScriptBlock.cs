// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ScriptBlock
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace System.Management.Automation
{
  public class ScriptBlock
  {
    private static readonly object[] _emptyArray = new object[0];
    private ParseTreeNode _begin;
    private ParseTreeNode _process;
    private ParseTreeNode _end;
    private ParseTreeNode _dynamicParams;
    private bool _isFilter;
    private bool _isSynthesized;
    private bool _isScriptBlockForExceptionHandler;
    private FunctionDeclarationNode _functionDeclaration;
    private int _pipelineSlots = -1;
    private int _variableSlots = -1;
    private Token _token;
    private string _script;
    private bool _usesCmdletBinding;
    private List<AttributeNode> _attributeNodes;
    private List<Attribute> _attributes;
    private ParameterDeclarationNode _parameterDeclaration;
    private MergedCommandParameterMetadata _parameterMetadata;
    private RuntimeDefinedParameterDictionary _runtimeDefinedParameters;
    private List<RuntimeDefinedParameter> _runtimeDefinedParameterList;
    private bool _initialized;
    private SessionStateInternal _sessionStateInternal;
    internal List<Token> _helpComments;
    internal List<List<Token>> _parameterHelpComments;
    private HelpInfo _helpInfo;
    private string _helpFile;
    [TraceSource("ScriptBlock", "Traces the execution of a ScriptBlock")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ScriptBlock), "Traces the execution of a ScriptBlock");

    internal ScriptBlock(
      Token token,
      FunctionDeclarationNode functionDeclaration,
      ParameterDeclarationNode parameterDeclaration,
      ParseTreeNode begin,
      ParseTreeNode process,
      ParseTreeNode end,
      ParseTreeNode dynamicParams,
      bool isFilter,
      List<AttributeNode> attributeNodes,
      List<Token> helpComments,
      List<List<Token>> parameterComments,
      int pipelineSlots,
      int variableSlots)
    {
      this._token = token;
      this._functionDeclaration = functionDeclaration;
      this._parameterDeclaration = parameterDeclaration;
      this._begin = begin;
      this._process = process;
      this._end = end;
      this._dynamicParams = dynamicParams;
      this._isFilter = isFilter;
      this._attributeNodes = attributeNodes;
      this._helpComments = helpComments;
      this._parameterHelpComments = parameterComments;
      this._pipelineSlots = pipelineSlots;
      this._variableSlots = variableSlots;
    }

    internal static ScriptBlock CreateSynthesized(ParseTreeNode body, Token token) => new ScriptBlock(token, (FunctionDeclarationNode) null, (ParameterDeclarationNode) null, (ParseTreeNode) null, (ParseTreeNode) null, body, (ParseTreeNode) null, false, (List<AttributeNode>) null, (List<Token>) null, (List<List<Token>>) null, -1, -1)
    {
      _isSynthesized = true
    };

    internal static ScriptBlock CreateExceptionHandler(
      ParseTreeNode body,
      Token token,
      int pipelineSlots,
      int variableSlots)
    {
      return new ScriptBlock(token, (FunctionDeclarationNode) null, (ParameterDeclarationNode) null, (ParseTreeNode) null, (ParseTreeNode) null, body, (ParseTreeNode) null, false, (List<AttributeNode>) null, (List<Token>) null, (List<List<Token>>) null, pipelineSlots, variableSlots)
      {
        _isScriptBlockForExceptionHandler = true
      };
    }

    internal static ScriptBlock Create(ExecutionContext context, string script)
    {
      ScriptBlock scriptBlock = ScriptBlock.Create(context.Engine.EngineParser, script);
      if (context.EngineSessionState != null && context.EngineSessionState.Module != null)
        scriptBlock.SessionStateInternal = context.EngineSessionState;
      return scriptBlock;
    }

    public static ScriptBlock Create(string script) => ScriptBlock.Create(new Parser(), script);

    internal static ScriptBlock Create(Parser parser, string script)
    {
      ScriptBlock scriptBlock = parser.ParseScriptBlock(script, false).BuildNewScriptBlock();
      scriptBlock._script = script;
      return scriptBlock;
    }

    internal ScriptBlock Clone() => this.Clone(false);

    internal ScriptBlock Clone(bool cloneHelpInfo)
    {
      ScriptBlock scriptBlock = new ScriptBlock(this._token, this._functionDeclaration, this._parameterDeclaration, this._begin, this._process, this._end, this._dynamicParams, this._isFilter, this._attributeNodes, this._helpComments, this._parameterHelpComments, this._pipelineSlots, this._variableSlots);
      scriptBlock._isScriptBlockForExceptionHandler = this._isScriptBlockForExceptionHandler;
      scriptBlock._isSynthesized = this._isSynthesized;
      scriptBlock._parameterMetadata = this._parameterMetadata;
      scriptBlock._usesCmdletBinding = this._usesCmdletBinding;
      scriptBlock._attributes = this._attributes;
      scriptBlock._runtimeDefinedParameters = this._runtimeDefinedParameters;
      scriptBlock._runtimeDefinedParameterList = this._runtimeDefinedParameterList;
      scriptBlock._initialized = this._initialized;
      if (cloneHelpInfo)
        scriptBlock._helpInfo = this._helpInfo;
      return scriptBlock;
    }

    internal ParseTreeNode Begin => this._begin;

    internal ParseTreeNode Process => this._process;

    internal ParseTreeNode End => this._end;

    internal ParseTreeNode DynamicParams => this._dynamicParams;

    public bool IsFilter
    {
      get => this._isFilter;
      set
      {
        this._isFilter = value;
        if (this._isFilter)
        {
          if (this._begin != null || this._process != null || this._end == null)
            return;
          this._process = this._end;
          this._end = (ParseTreeNode) null;
        }
        else
        {
          if (this._begin != null || this._process == null || this._end != null)
            return;
          this._end = this._process;
          this._process = (ParseTreeNode) null;
        }
      }
    }

    internal bool IsSynthesized => this._isSynthesized;

    internal bool IsScriptBlockForExceptionHandler => this._isScriptBlockForExceptionHandler;

    internal FunctionDeclarationNode FunctionDeclarationNode => this._functionDeclaration;

    internal int PipelineSlots => this._pipelineSlots;

    internal int VariableSlots => this._variableSlots;

    private ExecutionContext GetContextFromTLS()
    {
      ExecutionContext executionContextFromTls = LocalPipeline.GetExecutionContextFromTLS();
      if (executionContextFromTls == null)
      {
        string str = ErrorCategoryInfo.Ellipsize(Thread.CurrentThread.CurrentUICulture, this.ToString());
        PSInvalidOperationException operationException = ScriptBlock.tracer.NewInvalidOperationException("Parser", "ScriptBlockDelegateInvokedFromWrongThread", (object) str);
        operationException.SetErrorId("ScriptBlockDelegateInvokedFromWrongThread");
        throw operationException;
      }
      return executionContextFromTls;
    }

    internal Token Token => this._token;

    public PSToken StartPosition => this._token != null ? new PSToken(this._token) : (PSToken) null;

    public string File => this._token != null ? this._token.File : "";

    public override string ToString()
    {
      if (this._script != null)
        return this._script;
      StringBuilder stringBuilder = new StringBuilder();
      if (this._parameterDeclaration != null)
      {
        stringBuilder.Append(this._parameterDeclaration.ToString());
        stringBuilder.Append('\n');
      }
      if (this._begin != null)
        stringBuilder.AppendFormat("begin {{\n{0}\n}}\n", (object) this._begin.ToString());
      if (this._process != null)
      {
        if (this._isFilter && this._begin == null && this._end == null)
          stringBuilder.Append(this._process.ToString());
        else
          stringBuilder.AppendFormat("process {{\n{0}\n}}\n", (object) this._process.ToString());
      }
      if (this._end != null)
      {
        if (this._begin == null && this._process == null && !this._isFilter)
          stringBuilder.Append(this._end.ToString());
        else
          stringBuilder.AppendFormat("end {{\n{0}\n}}\n", (object) this._end.ToString());
      }
      this._script = stringBuilder.ToString();
      if (this._script == null)
        this._script = "";
      return this._script;
    }

    private ParseTreeNode GetCodeToInvoke()
    {
      if (this._begin != null || this._end != null && this._process != null)
        throw ScriptBlock.tracer.NewInvalidOperationException("AutomationExceptions", "ScriptBlockInvokeOnOneClauseOnly");
      return this._process != null ? this._process : this._end;
    }

    internal ReadOnlyCollection<PSTypeName> OutputType
    {
      get
      {
        List<PSTypeName> psTypeNameList = new List<PSTypeName>();
        foreach (Attribute attribute in this.Attributes)
        {
          if (attribute is OutputTypeAttribute outputTypeAttribute)
            psTypeNameList.AddRange((IEnumerable<PSTypeName>) outputTypeAttribute.Type);
        }
        return new ReadOnlyCollection<PSTypeName>((IList<PSTypeName>) psTypeNameList);
      }
    }

    internal CmdletBindingAttribute CmdletBindingAttribute
    {
      get
      {
        if (!this._initialized)
          this.InitializeAttributesAndParameters();
        if (this._usesCmdletBinding)
        {
          foreach (Attribute attribute in this._attributes)
          {
            if (attribute is CmdletBindingAttribute bindingAttribute)
              return bindingAttribute;
          }
        }
        return (CmdletBindingAttribute) null;
      }
    }

    internal bool UsesCmdletBinding
    {
      get
      {
        if (!this._initialized)
          this.InitializeAttributesAndParameters();
        return this._usesCmdletBinding;
      }
    }

    internal List<AttributeNode> AttributeNodes => this._attributeNodes;

    public List<Attribute> Attributes
    {
      get
      {
        if (!this._initialized)
          this.InitializeAttributesAndParameters();
        return this._attributes;
      }
    }

    internal ParameterDeclarationNode ParameterDeclaration => this._parameterDeclaration;

    internal MergedCommandParameterMetadata ParameterMetadata
    {
      get
      {
        if (this._parameterMetadata == null)
          this._parameterMetadata = new CommandMetadata(this, "", LocalPipeline.GetExecutionContextFromTLS()).StaticCommandParameterMetadata;
        return this._parameterMetadata;
      }
    }

    internal RuntimeDefinedParameterDictionary RuntimeDefinedParameters
    {
      get
      {
        if (!this._initialized)
          this.InitializeAttributesAndParameters();
        return this._runtimeDefinedParameters;
      }
    }

    internal List<RuntimeDefinedParameter> RuntimeDefinedParameterList
    {
      get
      {
        if (!this._initialized)
          this.InitializeAttributesAndParameters();
        return this._runtimeDefinedParameterList;
      }
    }

    private void InitializeAttributesAndParameters()
    {
      if (this._attributeNodes != null)
      {
        this._attributes = new List<Attribute>(this._attributeNodes.Count);
        foreach (AttributeNode attributeNode in this._attributeNodes)
        {
          Attribute attribute = attributeNode.GetAttribute();
          this._attributes.Add(attribute);
          if (attribute is CmdletBindingAttribute)
            this._usesCmdletBinding = true;
        }
      }
      if (this._parameterDeclaration == null)
      {
        this._runtimeDefinedParameters = new RuntimeDefinedParameterDictionary();
        this._runtimeDefinedParameterList = new List<RuntimeDefinedParameter>();
      }
      else
      {
        this._parameterDeclaration.InitializeRuntimeDefinedParameters(ref this._usesCmdletBinding);
        this._runtimeDefinedParameters = this._parameterDeclaration.RuntimeDefinedParameters;
        this._runtimeDefinedParameterList = this._parameterDeclaration.RuntimeDefinedParameterList;
      }
      this._initialized = true;
    }

    public SteppablePipeline GetSteppablePipeline() => this.GetSteppablePipeline(CommandOrigin.Internal);

    public SteppablePipeline GetSteppablePipeline(CommandOrigin commandOrigin)
    {
      PipelineNode simplePipeline = this.GetSimplePipeline((Action<string>) (errorId =>
      {
        throw ScriptBlock.tracer.NewInvalidOperationException("AutomationExceptions", errorId);
      }));
      ExecutionContext contextFromTls = this.GetContextFromTLS();
      ActivationRecord activationRecord = contextFromTls.EngineSessionState.CurrentActivationRecord;
      try
      {
        if (!this.IsSynthesized)
          contextFromTls.EngineSessionState.CurrentActivationRecord = new ActivationRecord(this.PipelineSlots, this.VariableSlots, contextFromTls.EngineSessionState.CurrentScope);
        return simplePipeline.GetSteppablePipeline(contextFromTls, commandOrigin);
      }
      finally
      {
        contextFromTls.EngineSessionState.CurrentActivationRecord = activationRecord;
      }
    }

    public object InvokeReturnAsIs(params object[] args)
    {
      lock (((RunspaceBase) this.GetContextFromTLS().CurrentRunspace).SyncRoot)
        return this.DoInvokeReturnAsIs((object) AutomationNull.Value, (object) AutomationNull.Value, args);
    }

    public Collection<PSObject> Invoke(params object[] args)
    {
      lock (((RunspaceBase) this.GetContextFromTLS().CurrentRunspace).SyncRoot)
        return this.DoInvoke((object) AutomationNull.Value, (object) AutomationNull.Value, args);
    }

    internal object DoInvokeReturnAsIs(object dollarUnder, object input, params object[] args)
    {
      ArrayList resultList = (ArrayList) null;
      this.InvokeWithPipe(true, false, dollarUnder, input, (object) AutomationNull.Value, (Pipe) null, ref resultList, args);
      return this.GetRawResult(resultList);
    }

    internal Collection<PSObject> DoInvoke(
      object dollarUnder,
      object input,
      params object[] args)
    {
      ArrayList resultList = (ArrayList) null;
      this.InvokeWithPipe(true, false, dollarUnder, input, (object) AutomationNull.Value, (Pipe) null, ref resultList, args);
      return this.GetWrappedResult(resultList);
    }

    internal object InvokeUsingCmdlet(
      Cmdlet contextCmdlet,
      bool UseLocalScope,
      bool writeErrors,
      object dollarUnder,
      object input,
      object scriptThis,
      params object[] args)
    {
      ArrayList resultList = (ArrayList) null;
      Pipe outputPipe = ((MshCommandRuntime) contextCmdlet?.CommandRuntime).OutputPipe;
      this.InvokeWithPipe(UseLocalScope, writeErrors, dollarUnder, input, scriptThis, outputPipe, ref resultList, args);
      return this.GetRawResult(resultList);
    }

    internal void InvokeWithPipe(
      bool useLocalScope,
      bool writeErrors,
      object dollarUnder,
      object input,
      object scriptThis,
      Pipe outputPipe,
      ref ArrayList resultList,
      params object[] args)
    {
      ExecutionContext contextFromTls = this.GetContextFromTLS();
      if (contextFromTls.CurrentPipelineStopping)
        throw new PipelineStoppedException();
      ParseTreeNode codeToInvoke = this.GetCodeToInvoke();
      if (codeToInvoke == null)
        return;
      InvocationInfo invocationInfo = new InvocationInfo((CommandInfo) null, codeToInvoke.NodeToken, contextFromTls);
      contextFromTls.Debugger.PushMethodCall(invocationInfo, this);
      bool flag = false;
      ScriptInvocationContext oldScriptContext = (ScriptInvocationContext) null;
      Pipe pipe = (Pipe) null;
      CommandOrigin scopeOrigin = contextFromTls.EngineSessionState.currentScope.ScopeOrigin;
      Exception exception = (Exception) null;
      SessionStateInternal engineSessionState = contextFromTls.EngineSessionState;
      ActivationRecord oldActivationRecord = (ActivationRecord) null;
      try
      {
        ScriptInvocationContext scriptContext = new ScriptInvocationContext(useLocalScope, scriptThis, dollarUnder, input, args);
        this.EnterScope(contextFromTls, scriptContext, out oldScriptContext, out oldActivationRecord);
        pipe = contextFromTls.ShellFunctionErrorOutputPipe;
        if (!writeErrors)
          contextFromTls.ShellFunctionErrorOutputPipe = (Pipe) null;
        contextFromTls.EngineSessionState.currentScope.ScopeOrigin = CommandOrigin.Internal;
        if (!string.IsNullOrEmpty(this.File))
        {
          contextFromTls.Debugger.PushRunning(this.File, this, false);
          flag = true;
        }
        codeToInvoke.Execute((Array) null, outputPipe, ref resultList, contextFromTls);
      }
      catch (ReturnException ex)
      {
        if (!this._isScriptBlockForExceptionHandler)
          ParseTreeNode.AppendResult(contextFromTls, ex.Argument, (Pipe) null, ref resultList);
        else
          exception = (Exception) ex;
      }
      finally
      {
        if (flag)
          contextFromTls.Debugger.PopRunning();
        contextFromTls.ShellFunctionErrorOutputPipe = pipe;
        contextFromTls.EngineSessionState.currentScope.ScopeOrigin = scopeOrigin;
        try
        {
          this.LeaveScope(contextFromTls, oldScriptContext, engineSessionState, oldActivationRecord);
        }
        finally
        {
          contextFromTls.Debugger.PopMethodCall();
        }
      }
      if (exception != null)
        throw exception;
    }

    private void EnterScope(
      ExecutionContext context,
      ScriptInvocationContext scriptContext,
      out ScriptInvocationContext oldScriptContext,
      out ActivationRecord oldActivationRecord)
    {
      oldScriptContext = (ScriptInvocationContext) null;
      if (this._sessionStateInternal != null)
        context.EngineSessionState = this._sessionStateInternal;
      context.IncrementScopeDepth(ExecutionContext.FeaturesThatNeedDepthHandling.ScriptScope);
      if (scriptContext.CreateScope)
      {
        SessionStateScope sessionStateScope = context.EngineSessionState.NewScope(false);
        context.EngineSessionState.CurrentScope = sessionStateScope;
      }
      else
      {
        object[] args = (object[]) LanguagePrimitives.ConvertTo(context.ArgsVariable, typeof (object[]), (IFormatProvider) CultureInfo.InvariantCulture);
        oldScriptContext = new ScriptInvocationContext(scriptContext.CreateScope, context.ScriptThisVariable, context.UnderbarVariable, context.InputVariable, args);
      }
      oldActivationRecord = context.EngineSessionState.CurrentActivationRecord;
      if (!this.IsSynthesized)
        context.EngineSessionState.CurrentActivationRecord = new ActivationRecord(this.PipelineSlots, this.VariableSlots, context.EngineSessionState.CurrentScope);
      object[] objArray = this.BindArguments(context, scriptContext);
      if (scriptContext.DollarThis != AutomationNull.Value)
        context.ScriptThisVariable = scriptContext.DollarThis;
      if (scriptContext.DollarBar != AutomationNull.Value)
        context.UnderbarVariable = scriptContext.DollarBar;
      if (scriptContext.DollarInput != AutomationNull.Value)
        context.InputVariable = (object) LanguagePrimitives.GetEnumerator(scriptContext.DollarInput);
      context.ArgsVariable = (object) objArray;
    }

    private object[] BindArguments(ExecutionContext context, ScriptInvocationContext scriptContext)
    {
      if (this.ParameterDeclaration == null)
        return scriptContext.Args;
      List<RuntimeDefinedParameter> definedParameterList = this.RuntimeDefinedParameterList;
      for (int index = 0; index < definedParameterList.Count; ++index)
      {
        RuntimeDefinedParameter parameter = definedParameterList[index];
        if (index < scriptContext.Args.Length)
          this.BindArgument(context, scriptContext, parameter, scriptContext.Args[index], false);
        else
          this.BindArgument(context, scriptContext, parameter);
      }
      scriptContext.BoundParameters.SetPSBoundParametersVariable(context);
      int length = scriptContext.Args.Length - definedParameterList.Count;
      if (length <= 0)
        return ScriptBlock._emptyArray;
      object[] objArray = new object[length];
      for (int index = 0; index < length; ++index)
        objArray[index] = scriptContext.Args[index + definedParameterList.Count];
      return objArray;
    }

    private void BindArgument(
      ExecutionContext context,
      ScriptInvocationContext scriptContext,
      RuntimeDefinedParameter parameter)
    {
      object obj = parameter.Value;
      if (obj is ParseTreeNode parseTreeNode)
      {
        try
        {
          obj = parseTreeNode.Execute(context);
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
      }
      this.BindArgument(context, scriptContext, parameter, obj, true);
    }

    private void BindArgument(
      ExecutionContext context,
      ScriptInvocationContext scriptContext,
      RuntimeDefinedParameter parameter,
      object value,
      bool isFromDefaultValue)
    {
      if (!scriptContext.CreateScope)
        scriptContext.BackupVariables[parameter.Name] = context.EngineSessionState.GetVariableAtScope(parameter.Name, "local");
      PSVariable variable = new PSVariable(parameter.Name, value, ScopedItemOptions.None, parameter.Attributes);
      context.EngineSessionState.SetVariable(variable, false, CommandOrigin.Internal);
      if (isFromDefaultValue)
        return;
      scriptContext.BoundParameters.Add(parameter.Name, value);
      scriptContext.BoundParameters.MarkAsBoundPositionally(parameter.Name);
    }

    private void LeaveScope(
      ExecutionContext context,
      ScriptInvocationContext oldScriptContext,
      SessionStateInternal oldSessionState,
      ActivationRecord oldActivationRecord)
    {
      context.DecrementScopeDepth(ExecutionContext.FeaturesThatNeedDepthHandling.ScriptScope);
      if (oldScriptContext == null)
      {
        context.EngineSessionState.RemoveScope(context.EngineSessionState.CurrentScope);
      }
      else
      {
        context.ScriptThisVariable = oldScriptContext.DollarThis;
        context.UnderbarVariable = oldScriptContext.DollarBar;
        context.InputVariable = oldScriptContext.DollarInput;
        context.ArgsVariable = (object) oldScriptContext.Args;
        foreach (string key in oldScriptContext.BackupVariables.Keys)
        {
          if (oldScriptContext.BackupVariables[key] != null)
            context.EngineSessionState.SetVariable(oldScriptContext.BackupVariables[key], false, CommandOrigin.Internal);
          else
            context.EngineSessionState.RemoveVariable(key);
        }
      }
      context.EngineSessionState.CurrentActivationRecord = oldActivationRecord;
      context.EngineSessionState = oldSessionState;
    }

    internal SessionState SessionState
    {
      get
      {
        if (this._sessionStateInternal == null)
        {
          ExecutionContext executionContextFromTls = LocalPipeline.GetExecutionContextFromTLS();
          if (executionContextFromTls != null)
            this._sessionStateInternal = executionContextFromTls.EngineSessionState.PublicSessionState.Internal;
        }
        return this._sessionStateInternal != null ? this._sessionStateInternal.PublicSessionState : (SessionState) null;
      }
      set => this._sessionStateInternal = value != null ? value.Internal : throw ScriptBlock.tracer.NewArgumentNullException(nameof (value));
    }

    public PSModuleInfo Module => this._sessionStateInternal != null ? this._sessionStateInternal.Module : (PSModuleInfo) null;

    internal SessionStateInternal SessionStateInternal
    {
      get => this._sessionStateInternal;
      set => this._sessionStateInternal = value;
    }

    internal Delegate GetDelegate(Type delegateType)
    {
      MethodInfo method1 = delegateType.GetMethod("Invoke");
      ParameterInfo[] parameters = method1.GetParameters();
      Type returnType = method1.ReturnType;
      if (parameters.Length <= 5)
      {
        bool flag = !returnType.Equals(typeof (void));
        Type[] typeArray = new Type[parameters.Length + (flag ? 1 : 0)];
        int num = 0;
        if (flag)
          typeArray[num++] = returnType;
        for (int index = 0; index < parameters.Length; ++index)
          typeArray[num++] = parameters[index].ParameterType;
        string str = flag ? "InvokeAsDelegateR" : "InvokeAsDelegate";
        MethodInfo[] methods = typeof (ScriptBlock).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
        MethodInfo method2 = (MethodInfo) null;
        foreach (MethodInfo methodInfo in methods)
        {
          if (methodInfo.Name.Equals(str, StringComparison.Ordinal) && methodInfo.GetParameters().Length == parameters.Length)
          {
            method2 = methodInfo;
            break;
          }
        }
        if (typeArray.Length > 0)
          method2 = method2.MakeGenericMethod(typeArray);
        return Delegate.CreateDelegate(delegateType, (object) this, method2);
      }
      Type[] parameterTypes = new Type[parameters.Length + 1];
      parameterTypes[0] = typeof (ScriptBlock);
      for (int index = 0; index < parameters.Length; ++index)
        parameterTypes[index + 1] = parameters[index].ParameterType;
      DynamicMethod dynamicMethod = new DynamicMethod("InvokeAsDelegate", returnType, parameterTypes, typeof (ScriptBlock).Module, true);
      ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldnull);
      ilGenerator.Emit(OpCodes.Ldnull);
      ilGenerator.Emit(OpCodes.Ldc_I4, parameters.Length);
      ilGenerator.Emit(OpCodes.Newarr, typeof (object));
      for (int index = 0; index < parameters.Length; ++index)
      {
        ilGenerator.Emit(OpCodes.Dup);
        ilGenerator.Emit(OpCodes.Ldc_I4, index);
        ilGenerator.Emit(OpCodes.Ldarg, index + 1);
        if (parameterTypes[index + 1].IsValueType)
          ilGenerator.Emit(OpCodes.Box, parameters[index].ParameterType);
        ilGenerator.Emit(OpCodes.Stelem_Ref);
      }
      BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
      if (returnType.Equals(typeof (void)))
      {
        MethodInfo method2 = typeof (ScriptBlock).GetMethod("InvokeAsDelegateHelper", bindingAttr);
        ilGenerator.Emit(OpCodes.Call, method2);
        ilGenerator.Emit(OpCodes.Pop);
      }
      else
      {
        MethodInfo meth = typeof (ScriptBlock).GetMethod("InvokeAsDelegateHelperT", bindingAttr).MakeGenericMethod(returnType);
        ilGenerator.Emit(OpCodes.Call, meth);
      }
      ilGenerator.Emit(OpCodes.Ret);
      return dynamicMethod.CreateDelegate(delegateType, (object) this);
    }

    internal object InvokeAsDelegateHelper(
      object dollarUnder,
      object dollarThis,
      params object[] args)
    {
      lock (((RunspaceBase) this.GetContextFromTLS().CurrentRunspace).SyncRoot)
      {
        ArrayList resultList = (ArrayList) null;
        this.InvokeWithPipe(false, true, dollarUnder, (object) null, dollarThis, (Pipe) null, ref resultList, args);
        return this.GetRawResult(resultList);
      }
    }

    internal T InvokeAsDelegateHelperT<T>(
      object dollarUnder,
      object dollarThis,
      params object[] args)
    {
      return (T) LanguagePrimitives.ConvertTo(this.InvokeAsDelegateHelper(dollarUnder, dollarThis, args), typeof (T), (IFormatProvider) CultureInfo.InvariantCulture);
    }

    internal void InvokeAsDelegate() => this.InvokeAsDelegateHelper((object) null, (object) null);

    internal void InvokeAsDelegate<T>(T t) => this.InvokeAsDelegateHelper((object) null, (object) null, (object) t);

    internal void InvokeAsDelegate<T1, T2>(T1 t1, T2 t2) => this.InvokeAsDelegateHelper((object) t2, (object) t1, (object) t1, (object) t2);

    internal void InvokeAsDelegate<T1, T2, T3>(T1 t1, T2 t2, T3 t3) => this.InvokeAsDelegateHelper((object) null, (object) null, (object) t1, (object) t2, (object) t3);

    internal void InvokeAsDelegate<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) => this.InvokeAsDelegateHelper((object) null, (object) null, (object) t1, (object) t2, (object) t3, (object) t4);

    internal void InvokeAsDelegate<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) => this.InvokeAsDelegateHelper((object) null, (object) null, (object) t1, (object) t2, (object) t3, (object) t4, (object) t5);

    internal R InvokeAsDelegateR<R>() => (R) LanguagePrimitives.ConvertTo(this.InvokeAsDelegateHelper((object) null, (object) null), typeof (R), (IFormatProvider) CultureInfo.InvariantCulture);

    internal R InvokeAsDelegateR<R, T>(T t) => (R) LanguagePrimitives.ConvertTo(this.InvokeAsDelegateHelper((object) null, (object) null, (object) t), typeof (R), (IFormatProvider) CultureInfo.InvariantCulture);

    internal R InvokeAsDelegateR<R, T1, T2>(T1 t1, T2 t2) => (R) LanguagePrimitives.ConvertTo(this.InvokeAsDelegateHelper((object) null, (object) null, (object) t1, (object) t2), typeof (R), (IFormatProvider) CultureInfo.InvariantCulture);

    internal R InvokeAsDelegateR<R, T1, T2, T3>(T1 t1, T2 t2, T3 t3) => (R) LanguagePrimitives.ConvertTo(this.InvokeAsDelegateHelper((object) null, (object) null, (object) t1, (object) t2, (object) t3), typeof (R), (IFormatProvider) CultureInfo.InvariantCulture);

    internal R InvokeAsDelegateR<R, T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) => (R) LanguagePrimitives.ConvertTo(this.InvokeAsDelegateHelper((object) null, (object) null, (object) t1, (object) t2, (object) t3, (object) t4), typeof (R), (IFormatProvider) CultureInfo.InvariantCulture);

    internal R InvokeAsDelegateR<R, T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) => (R) LanguagePrimitives.ConvertTo(this.InvokeAsDelegateHelper((object) null, (object) null, (object) t1, (object) t2, (object) t3, (object) t4, (object) t5), typeof (R), (IFormatProvider) CultureInfo.InvariantCulture);

    private object GetRawResult(ArrayList result)
    {
      if (result == null)
        return (object) AutomationNull.Value;
      return result.Count == 1 ? (object) LanguagePrimitives.AsPSObjectOrNull(result[0]) : (object) LanguagePrimitives.AsPSObjectOrNull((object) result.ToArray());
    }

    private Collection<PSObject> GetWrappedResult(ArrayList result)
    {
      if (result == null || result.Count == 0)
        return new Collection<PSObject>();
      Collection<PSObject> collection = new Collection<PSObject>();
      foreach (object obj in result)
        collection.Add(LanguagePrimitives.AsPSObjectOrNull(obj));
      return collection;
    }

    internal List<Token> HelpComments
    {
      get => this._helpComments;
      set => this._helpComments = value;
    }

    internal List<List<Token>> ParameterHelpComments
    {
      get => this._parameterHelpComments;
      set => this._parameterHelpComments = value;
    }

    internal string GetHelpFile(ExecutionContext context, CommandInfo commandInfo)
    {
      this.AnalyzeHelpComments(context, commandInfo);
      return this._helpFile;
    }

    internal HelpInfo GetHelpInfo(ExecutionContext context, CommandInfo commandInfo)
    {
      this.AnalyzeHelpComments(context, commandInfo);
      return this._helpInfo;
    }

    private void AnalyzeHelpComments(ExecutionContext context, CommandInfo commandInfo)
    {
      if (this._helpComments == null)
        return;
      this._helpInfo = HelpCommentsParser.CreateFromComments(context, commandInfo, this._helpComments, this._parameterHelpComments, out this._helpFile);
      this._helpComments = (List<Token>) null;
      this._parameterHelpComments = (List<List<Token>>) null;
    }

    private void ValidateVariableIsDeclared(ExecutionContext context, string variableName)
    {
      string namespaceSpecificString = new ScopedItemLookupPath(variableName).LookupPath.NamespaceSpecificString;
      if ((this._parameterDeclaration == null || !this.RuntimeDefinedParameters.ContainsKey(namespaceSpecificString)) && !namespaceSpecificString.Equals(ExecutionContext.ArgsVariablePath.LookupPath.NamespaceSpecificString, StringComparison.OrdinalIgnoreCase))
        throw new ScriptBlockToPowerShellNotSupportedException("CantConvertWithUndeclaredVariables", (Exception) null, "AutomationExceptions", "CantConvertWithUndeclaredVariables", new object[1]
        {
          (object) variableName
        });
    }

    private void ValidateTranslationToPowerShellAllowed(
      ExecutionContext context,
      ParseTreeNode node)
    {
      if (node == null)
        throw ScriptBlock.tracer.NewArgumentNullException(nameof (node));
      foreach (ParseTreeNode parseTreeNode in node.EnumeratePreorder())
      {
        if (parseTreeNode is ScriptBlockNode)
          throw new ScriptBlockToPowerShellNotSupportedException("CantConvertWithScriptBlocks", (Exception) null, "AutomationExceptions", "CantConvertWithScriptBlocks", new object[0]);
        if (parseTreeNode is CommandNode commandNode)
        {
          foreach (object element in commandNode.Elements)
          {
            if (element is Token token && token.Is(TokenId.SplattedVariableToken))
              this.ValidateVariableIsDeclared(context, token.TokenText);
          }
        }
        if (parseTreeNode is VariableDereferenceNode variableDereferenceNode && !variableDereferenceNode.IsPredefinedConstantVariable)
          this.ValidateVariableIsDeclared(context, variableDereferenceNode.VariableName);
      }
    }

    private PipelineNode GetSimplePipeline(Action<string> errorHandler)
    {
      if (this._begin != null || this._process != null)
      {
        if (errorHandler != null)
          errorHandler("CanConvertOneClauseOnly");
        return (PipelineNode) null;
      }
      if (!(this._end is StatementListNode end) || end.Statements == null || end.Statements.Length < 1)
      {
        if (errorHandler != null)
          errorHandler("CantConvertEmptyPipeline");
        return (PipelineNode) null;
      }
      if (end.Statements.Length != 1)
      {
        if (errorHandler != null)
          errorHandler("CanOnlyConvertOnePipeline");
        return (PipelineNode) null;
      }
      if (end.Traps != null)
      {
        if (errorHandler != null)
          errorHandler("CantConvertScriptBlockWithTrap");
        return (PipelineNode) null;
      }
      if (!(end.Statements[0] is PipelineNode statement))
      {
        if (errorHandler != null)
          errorHandler("CanOnlyConvertOnePipeline");
        return (PipelineNode) null;
      }
      if (statement.Commands != null && statement.Commands.Count != 0)
        return statement;
      if (errorHandler != null)
        errorHandler("CantConvertEmptyPipeline");
      return (PipelineNode) null;
    }

    private List<CommandNode> GetCommandNodesForPowerShellConversion(
      ExecutionContext context)
    {
      PipelineNode simplePipeline = this.GetSimplePipeline((Action<string>) (errorId =>
      {
        throw new ScriptBlockToPowerShellNotSupportedException(errorId, (Exception) null, "AutomationExceptions", errorId, new object[0]);
      }));
      this.ValidateTranslationToPowerShellAllowed(context, (ParseTreeNode) (this._end as StatementListNode));
      return simplePipeline.Commands;
    }

    internal PowerShell GetPowerShell(
      ExecutionContext context,
      bool? useLocalScope,
      params object[] args)
    {
      CommandOrigin? nullable = new CommandOrigin?();
      ScriptInvocationContext oldScriptContext = (ScriptInvocationContext) null;
      SessionStateInternal oldSessionState = context != null ? context.EngineSessionState : throw ScriptBlock.tracer.NewInvalidOperationException("AutomationExceptions", "CantConvertScriptBlockWithNoContext");
      ActivationRecord oldActivationRecord = (ActivationRecord) null;
      try
      {
        ScriptInvocationContext scriptContext = new ScriptInvocationContext(true, (object) null, (object) null, (object) null, args);
        this.EnterScope(context, scriptContext, out oldScriptContext, out oldActivationRecord);
        nullable = new CommandOrigin?(context.EngineSessionState.currentScope.ScopeOrigin);
        context.EngineSessionState.currentScope.ScopeOrigin = CommandOrigin.Internal;
        PowerShell powerShell = (PowerShell) null;
        foreach (CommandNode commandNode in this.GetCommandNodesForPowerShellConversion(context))
        {
          Command runspaceCommand = commandNode.ConvertToRunspaceCommand(context, useLocalScope);
          if (powerShell == null)
          {
            powerShell = PowerShell.Create();
            powerShell.Commands = new PSCommand(runspaceCommand);
          }
          else
            powerShell.AddCommand(runspaceCommand);
        }
        return powerShell;
      }
      finally
      {
        if (nullable.HasValue)
          context.EngineSessionState.currentScope.ScopeOrigin = nullable.Value;
        this.LeaveScope(context, oldScriptContext, oldSessionState, oldActivationRecord);
      }
    }

    public PowerShell GetPowerShell(params object[] args) => this.GetPowerShell(LocalPipeline.GetExecutionContextFromTLS(), new bool?(), args);

    internal bool IsUsingDollarInput()
    {
      PipelineNode simplePipeline = this.GetSimplePipeline((Action<string>) null);
      if (simplePipeline != null)
      {
        BuildPreOrderNodesListVisitor nodesListVisitor = new BuildPreOrderNodesListVisitor(ParseTreeVisitorOptions.SkipInvokableScriptBlocks);
        simplePipeline.Accept((ParseTreeVisitor) nodesListVisitor);
        foreach (ParseTreeNode preOrderNode in nodesListVisitor.GetPreOrderNodes())
        {
          if (preOrderNode is VariableDereferenceNode variableDereferenceNode && new ScopedItemLookupPath(variableDereferenceNode.VariableName).LookupPath.NamespaceSpecificString.Equals(ExecutionContext.InputVariablePath.LookupPath.NamespaceSpecificString, StringComparison.OrdinalIgnoreCase))
            return true;
        }
      }
      return false;
    }

    internal ScriptBlock GetWithInputHandlingForInvokeCommand()
    {
      ScriptBlock scriptBlock = this.Clone();
      PipelineNode simplePipeline = scriptBlock.GetSimplePipeline((Action<string>) null);
      if (simplePipeline == null || simplePipeline.Commands[0].IsExpression || this.IsUsingDollarInput())
        return this;
      StringBuilder stringBuilder = new StringBuilder();
      if (scriptBlock.ParameterDeclaration != null)
        stringBuilder.Append(this._parameterDeclaration.ToString());
      stringBuilder.Append("$input | ");
      stringBuilder.Append(this._end.ToString());
      scriptBlock._script = stringBuilder.ToString();
      CommandNode commandNode = new CommandNode((ParseTreeNode) new VariableDereferenceNode("input", 2), (Token) null);
      simplePipeline.Commands.Insert(0, commandNode);
      return scriptBlock;
    }

    public ScriptBlock GetNewClosure()
    {
      PSModuleInfo psModuleInfo = new PSModuleInfo(true);
      psModuleInfo.CaptureLocals();
      return psModuleInfo.NewBoundScriptBlock(this);
    }
  }
}
