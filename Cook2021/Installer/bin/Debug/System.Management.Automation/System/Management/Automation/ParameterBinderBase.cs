// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParameterBinderBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Globalization;
using System.Management.Automation.Internal;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Management.Automation
{
  internal abstract class ParameterBinderBase
  {
    [TraceSource("ParameterBinderBase", "A abstract helper class for the CommandProcessor that binds parameters to the specified object.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ParameterBinderBase), "A abstract helper class for the CommandProcessor that binds parameters to the specified object.");
    [TraceSource("ParameterBinding", "Traces the process of binding the arguments to the parameters of cmdlets, scripts, and applications.")]
    internal static PSTraceSource bindingTracer = PSTraceSource.GetTracer("ParameterBinding", "Traces the process of binding the arguments to the parameters of cmdlets, scripts, and applications.", false);
    private object target;
    private CommandLineParameters commandLineParameters;
    internal bool RecordBoundParameters = true;
    private InvocationInfo invocationInfo;
    private ExecutionContext context;
    private InternalCommand command;
    private EngineIntrinsics engine;

    internal ParameterBinderBase(
      object target,
      InvocationInfo invocationInfo,
      ExecutionContext context,
      InternalCommand command)
    {
      if (target == null)
        throw ParameterBinderBase.tracer.NewArgumentNullException(nameof (target));
      if (invocationInfo == null)
        throw ParameterBinderBase.tracer.NewArgumentNullException(nameof (invocationInfo));
      if (context == null)
        throw ParameterBinderBase.tracer.NewArgumentNullException(nameof (context));
      ParameterBinderBase.bindingTracer.ShowHeaders = false;
      this.command = command;
      this.target = target;
      this.invocationInfo = invocationInfo;
      this.context = context;
      this.engine = context.EngineIntrinsics;
    }

    internal ParameterBinderBase(
      InvocationInfo invocationInfo,
      ExecutionContext context,
      InternalCommand command)
    {
      if (invocationInfo == null)
        throw ParameterBinderBase.tracer.NewArgumentNullException(nameof (invocationInfo));
      if (context == null)
        throw ParameterBinderBase.tracer.NewArgumentNullException(nameof (context));
      ParameterBinderBase.bindingTracer.ShowHeaders = false;
      this.command = command;
      this.invocationInfo = invocationInfo;
      this.context = context;
      this.engine = context.EngineIntrinsics;
    }

    internal object Target
    {
      get => this.target;
      set => this.target = value;
    }

    internal CommandLineParameters CommandLineParameters
    {
      set => this.commandLineParameters = value;
      get
      {
        if (this.commandLineParameters == null)
          this.commandLineParameters = new CommandLineParameters();
        return this.commandLineParameters;
      }
    }

    internal abstract object GetDefaultParameterValue(string name);

    internal abstract void BindParameter(string name, object value);

    internal virtual bool BindParameter(
      CommandParameterInternal parameter,
      CompiledCommandParameter parameterMetadata,
      ParameterBindingFlags flags)
    {
      bool flag = false;
      bool coerceTypeIfNeeded = (flags & ParameterBindingFlags.ShouldCoerceType) != ParameterBindingFlags.None;
      if (parameter == null)
        throw ParameterBinderBase.tracer.NewArgumentNullException(nameof (parameter));
      if (parameterMetadata == null)
        throw ParameterBinderBase.tracer.NewArgumentNullException(nameof (parameterMetadata));
      using (ParameterBinderBase.bindingTracer.TraceScope("BIND arg [{0}] to parameter [{1}]", parameter.Value2, (object) parameterMetadata.Name))
      {
        parameter.Name = parameterMetadata.Name;
        object parameterValue = this.DecodeValue(parameter.Value2, parameterMetadata.Type, parameterMetadata.CollectionTypeInformation, coerceTypeIfNeeded);
        ScriptParameterBinder scriptParameterBinder = this as ScriptParameterBinder;
        bool bindingScriptCmdlet = false;
        if (scriptParameterBinder != null)
          bindingScriptCmdlet = scriptParameterBinder.Script.UsesCmdletBinding;
        foreach (ArgumentTransformationAttribute transformationAttribute in parameterMetadata.ArgumentTransformationAttributes)
        {
          using (ParameterBinderBase.bindingTracer.TraceScope("Executing DATA GENERATION metadata: [{0}]", (object) transformationAttribute.GetType()))
          {
            try
            {
              if (transformationAttribute is ArgumentTypeConverterAttribute converterAttribute)
              {
                if (coerceTypeIfNeeded)
                  parameterValue = converterAttribute.Transform(this.engine, parameterValue, true, bindingScriptCmdlet);
              }
              else
                parameterValue = transformationAttribute.Transform(this.engine, parameterValue);
              ParameterBinderBase.bindingTracer.WriteLine("result returned from DATA GENERATION: {0}", parameterValue);
            }
            catch (Exception ex)
            {
              CommandProcessorBase.CheckForSevereException(ex);
              ParameterBinderBase.bindingTracer.WriteLine("ERROR: DATA GENERATION: {0}", (object) ex.Message);
              ParameterBindingException bindingException = (ParameterBindingException) new ParameterBindingArgumentTransformationException(ex, ErrorCategory.InvalidData, this.InvocationInfo, parameter.Token, parameterMetadata.Name, parameterMetadata.Type, parameterValue == null ? (Type) null : parameterValue.GetType(), "ParameterBinderStrings", "ParameterArgumentTransformationError", new object[1]
              {
                (object) ex.Message
              });
              ParameterBinderBase.tracer.TraceException((Exception) bindingException);
              throw bindingException;
            }
          }
        }
        if (coerceTypeIfNeeded)
          parameterValue = this.CoerceTypeAsNeeded(parameter, parameterMetadata.Name, parameterMetadata.Type, parameterMetadata.CollectionTypeInformation, parameterValue);
        else if (!this.ShouldContinueUncoercedBind(parameter, parameterMetadata, ref parameterValue))
          goto label_47;
        if ((flags & ParameterBindingFlags.IsDefaultValue) == ParameterBindingFlags.None)
        {
          foreach (ValidateArgumentsAttribute validationAttribute in parameterMetadata.ValidationAttributes)
          {
            using (ParameterBinderBase.bindingTracer.TraceScope("Executing VALIDATION metadata: [{0}]", (object) validationAttribute.GetType()))
            {
              try
              {
                validationAttribute.InternalValidate(parameterValue, this.engine);
              }
              catch (Exception ex)
              {
                CommandProcessorBase.CheckForSevereException(ex);
                ParameterBinderBase.bindingTracer.WriteLine("ERROR: VALIDATION FAILED: {0}", (object) ex.Message);
                ParameterBindingValidationException validationException = new ParameterBindingValidationException(ex, ErrorCategory.InvalidData, this.InvocationInfo, parameter.Token, parameterMetadata.Name, parameterMetadata.Type, parameterValue == null ? (Type) null : parameterValue.GetType(), "ParameterBinderStrings", "ParameterArgumentValidationError", new object[1]
                {
                  (object) ex.Message
                });
                ParameterBinderBase.tracer.TraceException((Exception) validationException);
                throw validationException;
              }
              ParameterBinderBase.tracer.WriteLine("Validation attribute on {0} returned {1}.", (object) parameterMetadata.Name, (object) flag);
            }
          }
          if (ParameterBinderBase.IsParameterMandatory(parameterMetadata))
            this.ValidateNullOrEmptyArgument(parameter, parameterMetadata, parameterMetadata.Type, parameterValue, true);
        }
        Exception innerException = (Exception) null;
        try
        {
          this.BindParameter(parameter.Name, parameterValue);
          flag = true;
        }
        catch (SetValueException ex)
        {
          ParameterBinderBase.tracer.TraceException((Exception) ex);
          innerException = (Exception) ex;
        }
        if (innerException != null)
        {
          Type typeSpecified = parameterValue == null ? (Type) null : parameterValue.GetType();
          ParameterBindingException bindingException = new ParameterBindingException(innerException, ErrorCategory.WriteError, this.InvocationInfo, parameter.Token, parameterMetadata.Name, parameterMetadata.Type, typeSpecified, "ParameterBinderStrings", "ParameterBindingFailed", new object[1]
          {
            (object) innerException.Message
          });
          ParameterBinderBase.tracer.TraceException((Exception) bindingException);
          throw bindingException;
        }
label_47:
        ParameterBinderBase.bindingTracer.WriteLine("BIND arg [{0}] to param [{1}] {2}", parameterValue, (object) parameter.Name, flag ? (object) "SUCCESSFUL" : (object) "SKIPPED");
        if (flag)
        {
          if (this.RecordBoundParameters)
            this.CommandLineParameters.Add(parameter.Name, parameterValue);
          if (this.Command.commandRuntime is MshCommandRuntime commandRuntime && commandRuntime.LogPipelineExecutionDetail)
            commandRuntime.PipelineProcessor.LogExecutionParameterBinding(this.InvocationInfo, parameter.Name, parameterValue == null ? "" : parameterValue.ToString());
        }
        return flag;
      }
    }

    private void ValidateNullOrEmptyArgument(
      CommandParameterInternal parameter,
      CompiledCommandParameter parameterMetadata,
      Type argumentType,
      object parameterValue,
      bool recurseIntoCollections)
    {
      if (parameterValue == null && argumentType != typeof (bool?))
      {
        if (!parameterMetadata.AllowsNullArgument)
        {
          ParameterBinderBase.bindingTracer.WriteLine("ERROR: Argument cannot be null", new object[0]);
          ParameterBindingValidationException validationException = new ParameterBindingValidationException(ErrorCategory.InvalidData, this.InvocationInfo, parameter.Token, parameterMetadata.Name, argumentType, parameterValue == null ? (Type) null : parameterValue.GetType(), "ParameterBinderStrings", "ParameterArgumentValidationErrorNullNotAllowed", new object[0]);
          ParameterBinderBase.tracer.TraceException((Exception) validationException);
          throw validationException;
        }
      }
      else if (argumentType == typeof (string))
      {
        if ((parameterValue as string).Length == 0 && !parameterMetadata.AllowsEmptyStringArgument)
        {
          ParameterBinderBase.bindingTracer.WriteLine("ERROR: Argument cannot be an empty string", new object[0]);
          ParameterBindingValidationException validationException = new ParameterBindingValidationException(ErrorCategory.InvalidData, this.InvocationInfo, parameter.Token, parameterMetadata.Name, parameterMetadata.Type, parameterValue == null ? (Type) null : parameterValue.GetType(), "ParameterBinderStrings", "ParameterArgumentValidationErrorEmptyStringNotAllowed", new object[0]);
          ParameterBinderBase.tracer.TraceException((Exception) validationException);
          throw validationException;
        }
      }
      else
      {
        if (!recurseIntoCollections)
          return;
        switch (parameterMetadata.CollectionTypeInformation.ParameterCollectionType)
        {
          case ParameterCollectionType.IList:
          case ParameterCollectionType.Array:
          case ParameterCollectionType.ICollectionGeneric:
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(parameterValue);
            bool flag = true;
            while (ParserOps.MoveNext((ExecutionContext) null, (Token) null, enumerator))
            {
              object parameterValue1 = ParserOps.Current((Token) null, enumerator);
              flag = false;
              this.ValidateNullOrEmptyArgument(parameter, parameterMetadata, parameterMetadata.CollectionTypeInformation.ElementType, parameterValue1, false);
            }
            if (!flag || parameterMetadata.AllowsEmptyCollectionArgument)
              break;
            ParameterBinderBase.bindingTracer.WriteLine("ERROR: Argument cannot be an empty collection", new object[0]);
            ParameterBindingValidationException validationException = new ParameterBindingValidationException(ErrorCategory.InvalidData, this.InvocationInfo, parameter.Token, parameterMetadata.Name, parameterMetadata.Type, parameterValue == null ? (Type) null : parameterValue.GetType(), "ParameterBinderStrings", parameterMetadata.CollectionTypeInformation.ParameterCollectionType == ParameterCollectionType.Array ? "ParameterArgumentValidationErrorEmptyArrayNotAllowed" : "ParameterArgumentValidationErrorEmptyCollectionNotAllowed", new object[0]);
            ParameterBinderBase.tracer.TraceException((Exception) validationException);
            throw validationException;
        }
      }
    }

    private static bool IsParameterMandatory(CompiledCommandParameter parameterMetadata)
    {
      bool flag = false;
      foreach (ParameterSetSpecificMetadata specificMetadata in parameterMetadata.ParameterSetData.Values)
      {
        if (specificMetadata.IsMandatory)
        {
          flag = true;
          break;
        }
      }
      ParameterBinderBase.tracer.WriteLine("isMandatory = {0}", (object) flag);
      return flag;
    }

    private bool ShouldContinueUncoercedBind(
      CommandParameterInternal parameter,
      CompiledCommandParameter parameterMetadata,
      ref object parameterValue)
    {
      bool flag = false;
      Type type1 = parameterMetadata.Type;
      if (parameterValue == null)
      {
        flag = !LanguagePrimitives.IsBooleanType(type1) && type1 != typeof (string);
      }
      else
      {
        Type type2 = parameterValue.GetType();
        if (type2 == type1)
          flag = true;
        else if (type2.IsSubclassOf(type1))
          flag = true;
        else if (type1.IsAssignableFrom(type2))
        {
          flag = true;
        }
        else
        {
          if (parameterValue is PSObject && !((PSObject) parameterValue).immediateBaseObjectIsEmpty)
          {
            parameterValue = ((PSObject) parameterValue).BaseObject;
            Type type3 = parameterValue.GetType();
            if (type3 == type1 || type3.IsSubclassOf(type1))
            {
              flag = true;
              goto label_14;
            }
          }
          if (parameterMetadata.CollectionTypeInformation.ParameterCollectionType != ParameterCollectionType.NotCollection)
          {
            bool coercionRequired = false;
            object obj = this.EncodeCollection(parameter, parameterMetadata.Name, parameterMetadata.CollectionTypeInformation, type1, parameterValue, false, out coercionRequired);
            if (obj != null && !coercionRequired)
            {
              parameterValue = obj;
              flag = true;
            }
          }
        }
      }
label_14:
      return flag;
    }

    internal object DecodeValue(
      object value,
      Type parameterType,
      ParameterCollectionTypeInformation parameterCollectionType,
      bool coerceTypeIfNeeded)
    {
      object obj1 = value;
      if (obj1 != null)
      {
        IEnumerator enumerator = LanguagePrimitives.GetEnumerator(value);
        if (enumerator != null)
        {
          bool flag = false;
          ArrayList arrayList = new ArrayList();
          while (ParserOps.MoveNext((ExecutionContext) null, (Token) null, enumerator))
          {
            object obj2 = ParserOps.Current((Token) null, enumerator);
            if (obj2 is Token token)
            {
              object obj3 = this.DecodeToken(token, parameterType, parameterCollectionType, coerceTypeIfNeeded);
              arrayList.Add(obj3);
              flag = true;
            }
            else
              arrayList.Add(obj2);
          }
          if (flag)
          {
            obj1 = (object) arrayList.ToArray();
          }
          else
          {
            obj1 = value;
            if (value is IEnumerator)
            {
              try
              {
                enumerator.Reset();
              }
              catch (Exception ex)
              {
                CommandProcessorBase.CheckForSevereException(ex);
                obj1 = arrayList.Count != 1 || parameterCollectionType.ParameterCollectionType != ParameterCollectionType.NotCollection ? (object) arrayList.ToArray() : arrayList[0];
              }
            }
          }
        }
        else if (value is Token token)
          obj1 = this.DecodeToken(token, parameterType, parameterCollectionType, coerceTypeIfNeeded);
      }
      return obj1;
    }

    protected virtual object DecodeToken(
      Token token,
      Type parameterType,
      ParameterCollectionTypeInformation parameterCollectionType,
      bool coerceTypeIfNeeded)
    {
      return !token.Is(TokenId.NumberToken) ? (object) token.TokenText : (parameterType != typeof (string) || !coerceTypeIfNeeded ? (parameterCollectionType.ParameterCollectionType == ParameterCollectionType.NotCollection || parameterCollectionType.ElementType != typeof (string) ? (object) ParserOps.WrappedNumber(token.Data, token.TokenText) : (object) token.TokenText) : (object) token.TokenText);
    }

    internal InvocationInfo InvocationInfo => this.invocationInfo;

    internal ExecutionContext Context => this.context;

    internal InternalCommand Command => this.command;

    private object CoerceTypeAsNeeded(
      CommandParameterInternal argument,
      string parameterName,
      Type toType,
      ParameterCollectionTypeInformation collectionTypeInfo,
      object currentValue)
    {
      if (argument == null)
        throw ParameterBinderBase.tracer.NewArgumentNullException(nameof (argument));
      if (toType == null)
        throw ParameterBinderBase.tracer.NewArgumentNullException(nameof (toType));
      if (collectionTypeInfo == null)
        collectionTypeInfo = new ParameterCollectionTypeInformation(toType);
      object result = currentValue;
      using (ParameterBinderBase.bindingTracer.TraceScope("COERCE arg to [{0}]", (object) toType))
      {
        Type type1 = (Type) null;
        try
        {
          if (ParameterBinderBase.IsNullParameterValue(currentValue))
          {
            result = this.HandleNullParameterForSpecialTypes(argument, parameterName, toType, currentValue);
          }
          else
          {
            type1 = currentValue.GetType();
            if (toType.IsAssignableFrom(type1))
            {
              ParameterBinderBase.bindingTracer.WriteLine("Parameter and arg types the same, no coercion is needed.", new object[0]);
              result = currentValue;
            }
            else
            {
              ParameterBinderBase.bindingTracer.WriteLine("Trying to convert argument value from {0} to {1}", (object) type1, (object) toType);
              if (toType == typeof (PSObject))
              {
                if (this.command != null && currentValue == this.command.CurrentPipelineObject.BaseObject)
                  currentValue = (object) this.command.CurrentPipelineObject;
                ParameterBinderBase.bindingTracer.WriteLine("The parameter is of type [{0}] and the argument is an PSObject, so the parameter value is the argument value wrapped into an PSObject.", (object) toType);
                result = (object) LanguagePrimitives.AsPSObjectOrNull(currentValue);
              }
              else if (toType == typeof (string) && type1 == typeof (PSObject) && (PSObject) currentValue == AutomationNull.Value)
              {
                ParameterBinderBase.bindingTracer.WriteLine("CONVERT a null PSObject to a null string.", new object[0]);
                result = (object) null;
              }
              else if (toType == typeof (bool) || toType == typeof (SwitchParameter) || toType == typeof (bool?))
              {
                Type type2;
                if (type1 == typeof (PSObject))
                {
                  currentValue = ((PSObject) currentValue).BaseObject;
                  if (currentValue is SwitchParameter switchParameter)
                    currentValue = (object) switchParameter.IsPresent;
                  type2 = currentValue.GetType();
                }
                else
                  type2 = type1;
                if (type2 == typeof (bool))
                  result = !LanguagePrimitives.IsBooleanType(toType) ? (object) new SwitchParameter((bool) currentValue) : ParserOps.BoolToObject((bool) currentValue);
                else if (type2 == typeof (int))
                  result = (int) LanguagePrimitives.ConvertTo(currentValue, typeof (int), (IFormatProvider) CultureInfo.InvariantCulture) == 0 ? (!LanguagePrimitives.IsBooleanType(toType) ? (object) new SwitchParameter(false) : ParserOps.BoolToObject(false)) : (!LanguagePrimitives.IsBooleanType(toType) ? (object) new SwitchParameter(true) : ParserOps.BoolToObject(true));
                else if (LanguagePrimitives.IsNumeric(Type.GetTypeCode(type2)))
                {
                  result = (double) LanguagePrimitives.ConvertTo(currentValue, typeof (double), (IFormatProvider) CultureInfo.InvariantCulture) == 0.0 ? (!LanguagePrimitives.IsBooleanType(toType) ? (object) new SwitchParameter(false) : ParserOps.BoolToObject(false)) : (!LanguagePrimitives.IsBooleanType(toType) ? (object) new SwitchParameter(true) : ParserOps.BoolToObject(true));
                }
                else
                {
                  ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.InvalidArgument, this.InvocationInfo, argument.Token, parameterName, toType, type1, "ParameterBinderStrings", "CannotConvertArgument", new object[2]
                  {
                    (object) type2,
                    (object) ""
                  });
                  ParameterBinderBase.tracer.TraceException((Exception) bindingException);
                  throw bindingException;
                }
              }
              else
              {
                if (collectionTypeInfo.ParameterCollectionType == ParameterCollectionType.ICollectionGeneric || collectionTypeInfo.ParameterCollectionType == ParameterCollectionType.IList)
                {
                  object obj = PSObject.Base(currentValue);
                  if (obj != null)
                  {
                    switch (LanguagePrimitives.GetConversionRank(obj.GetType(), toType))
                    {
                      case ConversionRank.ImplicitCast:
                      case ConversionRank.ExplicitCast:
                      case ConversionRank.Constructor:
                        if (LanguagePrimitives.TryConvertTo(currentValue, toType, (IFormatProvider) Thread.CurrentThread.CurrentCulture, out result))
                          goto label_45;
                        else
                          break;
                    }
                  }
                }
                if (collectionTypeInfo.ParameterCollectionType != ParameterCollectionType.NotCollection)
                {
                  ParameterBinderBase.bindingTracer.WriteLine("ENCODING arg into collection", new object[0]);
                  bool coercionRequired = false;
                  result = this.EncodeCollection(argument, parameterName, collectionTypeInfo, toType, currentValue, collectionTypeInfo.ElementType != null, out coercionRequired);
                }
                else
                {
                  if (ParameterBinderBase.GetIList(currentValue) != null && toType != typeof (object) && (toType != typeof (PSObject) && toType != typeof (PSListModifier)) && ((!toType.IsGenericType || toType.GetGenericTypeDefinition() != typeof (PSListModifier<>)) && !toType.IsEnum))
                    throw new NotSupportedException();
                  ParameterBinderBase.bindingTracer.WriteLine("CONVERT arg type to param type using LanguagePrimitives.ConvertTo", new object[0]);
                  result = LanguagePrimitives.ConvertTo(currentValue, toType, (IFormatProvider) Thread.CurrentThread.CurrentCulture);
                  ParameterBinderBase.bindingTracer.WriteLine("CONVERT SUCCESSFUL using LanguagePrimitives.ConvertTo: [{0}]", result == null ? (object) "null" : (object) result.ToString());
                }
              }
            }
          }
        }
        catch (NotSupportedException ex)
        {
          ParameterBinderBase.bindingTracer.TraceError("ERROR: COERCE FAILED: arg [{0}] could not be converted to the parameter type [{1}]", result == null ? (object) "null" : result, (object) toType);
          Token token = argument.Token;
          ParameterBindingException bindingException = new ParameterBindingException((Exception) ex, ErrorCategory.InvalidArgument, this.InvocationInfo, token, parameterName, toType, type1, "ParameterBinderStrings", "CannotConvertArgument", new object[2]
          {
            result == null ? (object) "null" : result,
            (object) ex.Message
          });
          ParameterBinderBase.tracer.TraceException((Exception) bindingException);
          throw bindingException;
        }
        catch (PSInvalidCastException ex)
        {
          ParameterBinderBase.bindingTracer.TraceError("ERROR: COERCE FAILED: arg [{0}] could not be converted to the parameter type [{1}]", result == null ? (object) "null" : result, (object) toType);
          Token token = argument.Token;
          ParameterBindingException bindingException = new ParameterBindingException((Exception) ex, ErrorCategory.InvalidArgument, this.InvocationInfo, token, parameterName, toType, type1, "ParameterBinderStrings", "CannotConvertArgumentNoMessage", new object[1]
          {
            (object) ex.Message
          });
          ParameterBinderBase.tracer.TraceException((Exception) bindingException);
          throw bindingException;
        }
      }
label_45:
      return result;
    }

    private static bool IsNullParameterValue(object currentValue)
    {
      bool flag = false;
      if (currentValue == null || currentValue == AutomationNull.Value || currentValue == UnboundParameter.Value)
        flag = true;
      return flag;
    }

    private object HandleNullParameterForSpecialTypes(
      CommandParameterInternal argument,
      string parameterName,
      Type toType,
      object currentValue)
    {
      if (toType == typeof (bool))
      {
        ParameterBinderBase.bindingTracer.WriteLine("ERROR: No argument is specified for parameter and parameter type is BOOL", new object[0]);
        ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.InvalidArgument, this.InvocationInfo, argument.Token, parameterName, toType, (Type) null, "ParameterBinderStrings", "ParameterArgumentValidationErrorNullNotAllowed", new object[1]
        {
          (object) ""
        });
        ParameterBinderBase.tracer.TraceException((Exception) bindingException);
        throw bindingException;
      }
      object obj;
      if (toType == typeof (SwitchParameter))
      {
        ParameterBinderBase.bindingTracer.WriteLine("Arg is null or not present, parameter type is SWITCHPARAMTER, value is true.", new object[0]);
        obj = (object) SwitchParameter.Present;
      }
      else
      {
        if (currentValue == UnboundParameter.Value)
        {
          ParameterBinderBase.bindingTracer.TraceError("ERROR: No argument was specified for the parameter and the parameter is not of type bool");
          ParameterBindingException bindingException = new ParameterBindingException(ErrorCategory.InvalidArgument, this.InvocationInfo, argument.Token, parameterName, toType, (Type) null, "ParameterBinderStrings", "MissingArgument", new object[0]);
          ParameterBinderBase.tracer.TraceException((Exception) bindingException);
          throw bindingException;
        }
        ParameterBinderBase.bindingTracer.WriteLine("Arg is null, parameter type not bool or SwitchParameter, value is null.", new object[0]);
        obj = (object) null;
      }
      return obj;
    }

    private object EncodeCollection(
      CommandParameterInternal argument,
      string parameterName,
      ParameterCollectionTypeInformation collectionTypeInformation,
      Type toType,
      object currentValue,
      bool coerceElementTypeIfNeeded,
      out bool coercionRequired)
    {
      object obj1 = (object) null;
      coercionRequired = false;
      ParameterBinderBase.bindingTracer.WriteLine("Binding collection parameter {0}: argument type [{1}], parameter type [{2}], collection type {3}, element type [{4}], {5}", (object) parameterName, currentValue == null ? (object) "null" : (object) currentValue.GetType().Name, (object) toType, (object) collectionTypeInformation.ParameterCollectionType, (object) collectionTypeInformation.ElementType, coerceElementTypeIfNeeded ? (object) "coerceElementType" : (object) "no coerceElementType");
      if (currentValue != null)
      {
        int length = 1;
        Type type1 = collectionTypeInformation.ElementType;
        IList ilist = ParameterBinderBase.GetIList(currentValue);
        if (ilist != null)
        {
          length = ilist.Count;
          ParameterBinderBase.tracer.WriteLine("current value is an IList with {0} elements", (object) length);
          ParameterBinderBase.bindingTracer.WriteLine("Arg is IList with {0} elements", (object) length);
        }
        object obj2 = (object) null;
        IList list = (IList) null;
        MethodInfo methodInfo = (MethodInfo) null;
        bool flag1 = toType == typeof (Array);
        if (collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.Array || flag1)
        {
          if (flag1)
            type1 = typeof (object);
          ParameterBinderBase.bindingTracer.WriteLine("Creating array with element type [{0}] and {1} elements", (object) type1, (object) length);
          obj2 = (object) (Array) (list = (IList) Array.CreateInstance(type1, length));
        }
        else if (collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.IList || collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.ICollectionGeneric)
        {
          ParameterBinderBase.bindingTracer.WriteLine("Creating collection [{0}]", (object) toType);
          bool flag2 = false;
          Exception exception = (Exception) null;
          try
          {
            obj2 = Activator.CreateInstance(toType, BindingFlags.Default, (Binder) null, new object[0], CultureInfo.InvariantCulture);
            if (collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.IList)
            {
              list = (IList) obj2;
            }
            else
            {
              Type elementType = collectionTypeInformation.ElementType;
              BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod;
              Exception innerException = (Exception) null;
              try
              {
                methodInfo = toType.GetMethod("Add", bindingAttr, (Binder) null, new Type[1]
                {
                  elementType
                }, (ParameterModifier[]) null);
              }
              catch (AmbiguousMatchException ex)
              {
                ParameterBinderBase.bindingTracer.WriteLine("Ambiguous match to Add(T) for type " + toType.FullName + ": " + ex.Message, new object[0]);
                innerException = (Exception) ex;
              }
              catch (ArgumentException ex)
              {
                ParameterBinderBase.bindingTracer.WriteLine("ArgumentException matching Add(T) for type " + toType.FullName + ": " + ex.Message, new object[0]);
                innerException = (Exception) ex;
              }
              if (methodInfo == null)
              {
                ParameterBindingException bindingException = new ParameterBindingException(innerException, ErrorCategory.InvalidArgument, this.InvocationInfo, argument.Token, parameterName, toType, currentValue == null ? (Type) null : currentValue.GetType(), "ParameterBinderStrings", "CannotExtractAddMethod", new object[1]
                {
                  innerException == null ? (object) "" : (object) innerException.Message
                });
                ParameterBinderBase.bindingTracer.TraceException((Exception) bindingException);
                ParameterBinderBase.tracer.TraceException((Exception) bindingException);
                throw bindingException;
              }
            }
          }
          catch (ArgumentException ex)
          {
            flag2 = true;
            exception = (Exception) ex;
          }
          catch (NotSupportedException ex)
          {
            flag2 = true;
            exception = (Exception) ex;
          }
          catch (TargetInvocationException ex)
          {
            flag2 = true;
            exception = (Exception) ex;
          }
          catch (MethodAccessException ex)
          {
            flag2 = true;
            exception = (Exception) ex;
          }
          catch (MemberAccessException ex)
          {
            flag2 = true;
            exception = (Exception) ex;
          }
          catch (InvalidComObjectException ex)
          {
            flag2 = true;
            exception = (Exception) ex;
          }
          catch (COMException ex)
          {
            flag2 = true;
            exception = (Exception) ex;
          }
          catch (TypeLoadException ex)
          {
            flag2 = true;
            exception = (Exception) ex;
          }
          if (flag2)
          {
            ParameterBinderBase.bindingTracer.TraceException(exception);
            ParameterBinderBase.tracer.TraceException(exception);
            ParameterBindingException bindingException = new ParameterBindingException(exception, ErrorCategory.InvalidArgument, this.InvocationInfo, argument.Token, parameterName, toType, currentValue.GetType(), "ParameterBinderStrings", "CannotConvertArgument", new object[2]
            {
              obj1 == null ? (object) "null" : obj1,
              exception == null ? (object) "" : (object) exception.Message
            });
            ParameterBinderBase.bindingTracer.TraceException((Exception) bindingException);
            ParameterBinderBase.tracer.TraceException((Exception) bindingException);
            throw bindingException;
          }
        }
        else
          goto label_65;
        if (ilist != null)
        {
          int num = 0;
          ParameterBinderBase.bindingTracer.WriteLine("Argument type {0} is IList", (object) currentValue.GetType());
          foreach (object currentValue1 in (IEnumerable) ilist)
          {
            object obj3 = PSObject.Base(currentValue1);
            if (coerceElementTypeIfNeeded)
            {
              ParameterBinderBase.bindingTracer.WriteLine("COERCE collection element from type {0} to type {1}", currentValue1 == null ? (object) "null" : (object) currentValue1.GetType().Name, (object) type1);
              obj3 = this.CoerceTypeAsNeeded(argument, parameterName, type1, (ParameterCollectionTypeInformation) null, currentValue1);
            }
            else if (type1 != null)
            {
              if (obj3 != null)
              {
                Type type2 = obj3.GetType();
                Type c = type1;
                if (type2 != c)
                {
                  if (!type2.IsSubclassOf(c))
                  {
                    ParameterBinderBase.bindingTracer.WriteLine("COERCION REQUIRED: Did not attempt to coerce collection element from type {0} to type {1}", currentValue1 == null ? (object) "null" : (object) currentValue1.GetType().Name, (object) type1);
                    coercionRequired = true;
                    break;
                  }
                }
              }
            }
            try
            {
              if (collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.Array || flag1)
              {
                ParameterBinderBase.bindingTracer.WriteLine("Adding element of type {0} to array position {1}", obj3 == null ? (object) "null" : (object) obj3.GetType().Name, (object) num);
                list[num++] = obj3;
              }
              else if (collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.IList)
              {
                ParameterBinderBase.bindingTracer.WriteLine("Adding element of type {0} via IList.Add", obj3 == null ? (object) "null" : (object) obj3.GetType().Name);
                list.Add(obj3);
              }
              else
              {
                ParameterBinderBase.bindingTracer.WriteLine("Adding element of type {0} via ICollection<T>::Add()", obj3 == null ? (object) "null" : (object) obj3.GetType().Name);
                methodInfo.Invoke(obj2, new object[1]
                {
                  obj3
                });
              }
            }
            catch (Exception ex)
            {
              Exception exception = ex;
              CommandProcessorBase.CheckForSevereException(exception);
              ParameterBinderBase.bindingTracer.TraceException(exception);
              ParameterBinderBase.tracer.TraceException(exception);
              if (exception is TargetInvocationException && exception.InnerException != null)
                exception = exception.InnerException;
              ParameterBindingException bindingException = new ParameterBindingException(exception, ErrorCategory.InvalidArgument, this.InvocationInfo, argument.Token, parameterName, toType, obj3 == null ? (Type) null : obj3.GetType(), "ParameterBinderStrings", "CannotConvertArgument", new object[2]
              {
                obj3 == null ? (object) "null" : obj3,
                (object) exception.Message
              });
              ParameterBinderBase.bindingTracer.TraceException((Exception) bindingException);
              ParameterBinderBase.tracer.TraceException((Exception) bindingException);
              throw bindingException;
            }
          }
        }
        else
        {
          ParameterBinderBase.bindingTracer.WriteLine("Argument type {0} is not IList, treating this as scalar", currentValue == null ? (object) "null" : (object) currentValue.GetType().Name);
          if (type1 != null)
          {
            if (coerceElementTypeIfNeeded)
            {
              ParameterBinderBase.bindingTracer.WriteLine("Coercing scalar arg value to type {1}", (object) type1);
              currentValue = this.CoerceTypeAsNeeded(argument, parameterName, type1, (ParameterCollectionTypeInformation) null, currentValue);
            }
            else
            {
              Type type2 = currentValue.GetType();
              Type c = type1;
              if (type2 != c)
              {
                if (!type2.IsSubclassOf(c))
                {
                  ParameterBinderBase.bindingTracer.WriteLine("COERCION REQUIRED: Did not coerce scalar arg value to type {1}", (object) type1);
                  coercionRequired = true;
                  goto label_65;
                }
              }
            }
          }
          try
          {
            if (collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.Array || flag1)
            {
              ParameterBinderBase.bindingTracer.WriteLine("Adding scalar element of type {0} to array position {1}", currentValue == null ? (object) "null" : (object) currentValue.GetType().Name, (object) 0);
              list[0] = currentValue;
            }
            else if (collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.IList)
            {
              ParameterBinderBase.bindingTracer.WriteLine("Adding scalar element of type {0} via IList.Add", currentValue == null ? (object) "null" : (object) currentValue.GetType().Name);
              list.Add(currentValue);
            }
            else
            {
              ParameterBinderBase.bindingTracer.WriteLine("Adding scalar element of type {0} via ICollection<T>::Add()", currentValue == null ? (object) "null" : (object) currentValue.GetType().Name);
              methodInfo.Invoke(obj2, new object[1]
              {
                currentValue
              });
            }
          }
          catch (Exception ex)
          {
            Exception exception = ex;
            CommandProcessorBase.CheckForSevereException(exception);
            ParameterBinderBase.bindingTracer.TraceException(exception);
            ParameterBinderBase.tracer.TraceException(exception);
            if (exception is TargetInvocationException && exception.InnerException != null)
              exception = exception.InnerException;
            ParameterBindingException bindingException = new ParameterBindingException(exception, ErrorCategory.InvalidArgument, this.InvocationInfo, argument.Token, parameterName, toType, currentValue == null ? (Type) null : currentValue.GetType(), "ParameterBinderStrings", "CannotConvertArgument", new object[2]
            {
              currentValue == null ? (object) "null" : currentValue,
              (object) exception.Message
            });
            ParameterBinderBase.bindingTracer.TraceException((Exception) bindingException);
            ParameterBinderBase.tracer.TraceException((Exception) bindingException);
            throw bindingException;
          }
        }
        if (!coercionRequired)
          obj1 = obj2;
      }
label_65:
      return obj1;
    }

    internal static IList GetIList(object value)
    {
      switch (value)
      {
        case IList baseObject:
          ParameterBinderBase.tracer.WriteLine("argument is IList", new object[0]);
          break;
        case PSObject psObject:
          if (psObject.BaseObject is IList baseObject)
          {
            ParameterBinderBase.tracer.WriteLine("argument is PSObject with BaseObject as IList", new object[0]);
            break;
          }
          break;
      }
      return baseObject;
    }
  }
}
