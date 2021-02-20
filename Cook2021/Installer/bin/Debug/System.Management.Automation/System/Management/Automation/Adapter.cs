// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Adapter
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace System.Management.Automation
{
  internal abstract class Adapter
  {
    [TraceSource("ETS", "Extended Type System")]
    protected static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    private static Dictionary<CallsiteCacheEntry, MethodInformation> callsiteCache = new Dictionary<CallsiteCacheEntry, MethodInformation>(1024);

    protected virtual Collection<string> GetTypeNameHierarchy(object obj)
    {
      Collection<string> collection = new Collection<string>();
      for (Type type = obj.GetType(); type != null; type = type.BaseType)
        collection.Add(type.FullName);
      return collection;
    }

    protected abstract T GetMember<T>(object obj, string memberName) where T : PSMemberInfo;

    protected abstract PSMemberInfoInternalCollection<T> GetMembers<T>(
      object obj)
      where T : PSMemberInfo;

    protected abstract object PropertyGet(PSProperty property);

    protected abstract void PropertySet(
      PSProperty property,
      object setValue,
      bool convertIfPossible);

    protected abstract bool PropertyIsSettable(PSProperty property);

    protected abstract bool PropertyIsGettable(PSProperty property);

    protected abstract string PropertyType(PSProperty property);

    protected abstract string PropertyToString(PSProperty property);

    protected abstract AttributeCollection PropertyAttributes(PSProperty property);

    protected abstract object MethodInvoke(PSMethod method, object[] arguments);

    protected abstract Collection<string> MethodDefinitions(PSMethod method);

    protected virtual string MethodToString(PSMethod method)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string methodDefinition in this.MethodDefinitions(method))
      {
        stringBuilder.Append(methodDefinition);
        stringBuilder.Append(", ");
      }
      stringBuilder.Remove(stringBuilder.Length - 2, 2);
      return stringBuilder.ToString();
    }

    protected virtual string ParameterizedPropertyType(PSParameterizedProperty property) => throw Adapter.tracer.NewNotSupportedException();

    protected virtual bool ParameterizedPropertyIsSettable(PSParameterizedProperty property) => throw Adapter.tracer.NewNotSupportedException();

    protected virtual bool ParameterizedPropertyIsGettable(PSParameterizedProperty property) => throw Adapter.tracer.NewNotSupportedException();

    protected virtual Collection<string> ParameterizedPropertyDefinitions(
      PSParameterizedProperty property)
    {
      throw Adapter.tracer.NewNotSupportedException();
    }

    protected virtual object ParameterizedPropertyGet(
      PSParameterizedProperty property,
      object[] arguments)
    {
      throw Adapter.tracer.NewNotSupportedException();
    }

    protected virtual void ParameterizedPropertySet(
      PSParameterizedProperty property,
      object setValue,
      object[] arguments)
    {
      throw Adapter.tracer.NewNotSupportedException();
    }

    protected virtual string ParameterizedPropertyToString(PSParameterizedProperty property) => throw Adapter.tracer.NewNotSupportedException();

    private Exception NewException(
      Exception e,
      string errorId,
      string targetErrorId,
      string exceptionResourceId,
      params object[] parameters)
    {
      object[] objArray = new object[parameters.Length + 1];
      for (int index = 0; index < parameters.Length; ++index)
        objArray[index + 1] = parameters[index];
      Exception exception = (Exception) (e as TargetInvocationException);
      if (exception != null)
      {
        Exception innerException = exception.InnerException == null ? exception : exception.InnerException;
        objArray[0] = (object) innerException.Message;
        return (Exception) new ExtendedTypeSystemException(targetErrorId, innerException, "ExtendedTypeSystem", exceptionResourceId, objArray);
      }
      objArray[0] = (object) e.Message;
      return (Exception) new ExtendedTypeSystemException(errorId, e, "ExtendedTypeSystem", exceptionResourceId, objArray);
    }

    internal Collection<string> BaseGetTypeNameHierarchy(object obj)
    {
      try
      {
        return this.GetTypeNameHierarchy(obj);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (!(ex is ExtendedTypeSystemException))
          throw this.NewException(ex, "CatchFromBaseGetTypeNameHierarchy", "CatchFromBaseGetTypeNameHierarchyTI", "ExceptionRetrievingTypeNameHierarchy");
        throw;
      }
    }

    internal T BaseGetMember<T>(object obj, string memberName) where T : PSMemberInfo
    {
      try
      {
        return this.GetMember<T>(obj, memberName);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (ex is ExtendedTypeSystemException)
          throw;
        else
          throw this.NewException(ex, "CatchFromBaseGetMember", "CatchFromBaseGetMemberTI", "ExceptionGettingMember", (object) memberName);
      }
    }

    internal PSMemberInfoInternalCollection<T> BaseGetMembers<T>(
      object obj)
      where T : PSMemberInfo
    {
      try
      {
        return this.GetMembers<T>(obj);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (!(ex is ExtendedTypeSystemException))
          throw this.NewException(ex, "CatchFromBaseGetMembers", "CatchFromBaseGetMembersTI", "ExceptionGettingMembers");
        throw;
      }
    }

    internal object BasePropertyGet(PSProperty property)
    {
      try
      {
        return this.PropertyGet(property);
      }
      catch (TargetInvocationException ex)
      {
        Exception innerException = ex.InnerException == null ? (Exception) ex : ex.InnerException;
        throw new GetValueInvocationException("CatchFromBaseAdapterGetValueTI", innerException, "ExtendedTypeSystem", "ExceptionWhenGetting", new object[2]
        {
          (object) property.Name,
          (object) innerException.Message
        });
      }
      catch (Exception ex)
      {
        if (ex is GetValueException)
        {
          throw;
        }
        else
        {
          CommandProcessorBase.CheckForSevereException(ex);
          throw new GetValueInvocationException("CatchFromBaseAdapterGetValue", ex, "ExtendedTypeSystem", "ExceptionWhenGetting", new object[2]
          {
            (object) property.Name,
            (object) ex.Message
          });
        }
      }
    }

    internal void BasePropertySet(PSProperty property, object setValue, bool convert)
    {
      try
      {
        this.PropertySet(property, setValue, convert);
      }
      catch (TargetInvocationException ex)
      {
        Exception innerException = ex.InnerException == null ? (Exception) ex : ex.InnerException;
        throw new SetValueInvocationException("CatchFromBaseAdapterSetValueTI", innerException, "ExtendedTypeSystem", "ExceptionWhenSetting", new object[2]
        {
          (object) property.Name,
          (object) innerException.Message
        });
      }
      catch (Exception ex)
      {
        if (ex is SetValueException)
        {
          throw;
        }
        else
        {
          CommandProcessorBase.CheckForSevereException(ex);
          throw new SetValueInvocationException("CatchFromBaseAdapterSetValue", ex, "ExtendedTypeSystem", "ExceptionWhenSetting", new object[2]
          {
            (object) property.Name,
            (object) ex.Message
          });
        }
      }
    }

    internal bool BasePropertyIsSettable(PSProperty property)
    {
      try
      {
        return this.PropertyIsSettable(property);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (ex is ExtendedTypeSystemException)
          throw;
        else
          throw this.NewException(ex, "CatchFromBasePropertyIsSettable", "CatchFromBasePropertyIsSettableTI", "ExceptionRetrievingPropertyWriteState", (object) property.Name);
      }
    }

    internal bool BasePropertyIsGettable(PSProperty property)
    {
      try
      {
        return this.PropertyIsGettable(property);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (ex is ExtendedTypeSystemException)
          throw;
        else
          throw this.NewException(ex, "CatchFromBasePropertyIsGettable", "CatchFromBasePropertyIsGettableTI", "ExceptionRetrievingPropertyReadState", (object) property.Name);
      }
    }

    internal string BasePropertyType(PSProperty property)
    {
      try
      {
        return this.PropertyType(property);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (ex is ExtendedTypeSystemException)
          throw;
        else
          throw this.NewException(ex, "CatchFromBasePropertyType", "CatchFromBasePropertyTypeTI", "ExceptionRetrievingPropertyType", (object) property.Name);
      }
    }

    internal string BasePropertyToString(PSProperty property)
    {
      try
      {
        return this.PropertyToString(property);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (ex is ExtendedTypeSystemException)
          throw;
        else
          throw this.NewException(ex, "CatchFromBasePropertyToString", "CatchFromBasePropertyToStringTI", "ExceptionRetrievingPropertyString", (object) property.Name);
      }
    }

    internal AttributeCollection BasePropertyAttributes(PSProperty property)
    {
      try
      {
        return this.PropertyAttributes(property);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (ex is ExtendedTypeSystemException)
          throw;
        else
          throw this.NewException(ex, "CatchFromBasePropertyAttributes", "CatchFromBasePropertyAttributesTI", "ExceptionRetrievingPropertyAttributes", (object) property.Name);
      }
    }

    internal object BaseMethodInvoke(PSMethod method, params object[] arguments)
    {
      try
      {
        return this.MethodInvoke(method, arguments);
      }
      catch (TargetInvocationException ex)
      {
        Exception innerException = ex.InnerException == null ? (Exception) ex : ex.InnerException;
        throw new MethodInvocationException("CatchFromBaseAdapterMethodInvokeTI", innerException, "ExtendedTypeSystem", "MethodInvocationException", new object[3]
        {
          (object) method.Name,
          (object) arguments.Length,
          (object) innerException.Message
        });
      }
      catch (FlowControlException ex)
      {
        throw;
      }
      catch (ScriptCallDepthException ex)
      {
        throw;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        if (ex is MethodException)
        {
          throw;
        }
        else
        {
          CommandProcessorBase.CheckForSevereException(ex);
          if (method.baseObject is SteppablePipeline && (method.Name.Equals("Begin", StringComparison.OrdinalIgnoreCase) || method.Name.Equals("Process", StringComparison.OrdinalIgnoreCase) || method.Name.Equals("End", StringComparison.OrdinalIgnoreCase)))
            throw;
          else
            throw new MethodInvocationException("CatchFromBaseAdapterMethodInvoke", ex, "ExtendedTypeSystem", "MethodInvocationException", new object[3]
            {
              (object) method.Name,
              (object) arguments.Length,
              (object) ex.Message
            });
        }
      }
    }

    internal Collection<string> BaseMethodDefinitions(PSMethod method)
    {
      try
      {
        return this.MethodDefinitions(method);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (ex is ExtendedTypeSystemException)
          throw;
        else
          throw this.NewException(ex, "CatchFromBaseMethodDefinitions", "CatchFromBaseMethodDefinitionsTI", "ExceptionRetrievingMethodDefinitions", (object) method.Name);
      }
    }

    internal string BaseMethodToString(PSMethod method)
    {
      try
      {
        return this.MethodToString(method);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (ex is ExtendedTypeSystemException)
          throw;
        else
          throw this.NewException(ex, "CatchFromBaseMethodToString", "CatchFromBaseMethodToStringTI", "ExceptionRetrievingMethodString", (object) method.Name);
      }
    }

    internal string BaseParameterizedPropertyType(PSParameterizedProperty property)
    {
      try
      {
        return this.ParameterizedPropertyType(property);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (ex is ExtendedTypeSystemException)
          throw;
        else
          throw this.NewException(ex, "CatchFromBaseParameterizedPropertyType", "CatchFromBaseParameterizedPropertyTypeTI", "ExceptionRetrievingParameterizedPropertytype", (object) property.Name);
      }
    }

    internal bool BaseParameterizedPropertyIsSettable(PSParameterizedProperty property)
    {
      try
      {
        return this.ParameterizedPropertyIsSettable(property);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (ex is ExtendedTypeSystemException)
          throw;
        else
          throw this.NewException(ex, "CatchFromBaseParameterizedPropertyIsSettable", "CatchFromBaseParameterizedPropertyIsSettableTI", "ExceptionRetrievingParameterizedPropertyWriteState", (object) property.Name);
      }
    }

    internal bool BaseParameterizedPropertyIsGettable(PSParameterizedProperty property)
    {
      try
      {
        return this.ParameterizedPropertyIsGettable(property);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (ex is ExtendedTypeSystemException)
          throw;
        else
          throw this.NewException(ex, "CatchFromBaseParameterizedPropertyIsGettable", "CatchFromBaseParameterizedPropertyIsGettableTI", "ExceptionRetrievingParameterizedPropertyReadState", (object) property.Name);
      }
    }

    internal Collection<string> BaseParameterizedPropertyDefinitions(
      PSParameterizedProperty property)
    {
      try
      {
        return this.ParameterizedPropertyDefinitions(property);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (ex is ExtendedTypeSystemException)
          throw;
        else
          throw this.NewException(ex, "CatchFromBaseParameterizedPropertyDefinitions", "CatchFromBaseParameterizedPropertyDefinitionsTI", "ExceptionRetrievingParameterizedPropertyDefinitions", (object) property.Name);
      }
    }

    internal object BaseParameterizedPropertyGet(
      PSParameterizedProperty property,
      params object[] arguments)
    {
      try
      {
        return this.ParameterizedPropertyGet(property, arguments);
      }
      catch (TargetInvocationException ex)
      {
        Exception innerException = ex.InnerException == null ? (Exception) ex : ex.InnerException;
        throw new GetValueInvocationException("CatchFromBaseAdapterParameterizedPropertyGetValueTI", innerException, "ExtendedTypeSystem", "ExceptionWhenGetting", new object[2]
        {
          (object) property.Name,
          (object) innerException.Message
        });
      }
      catch (Exception ex)
      {
        if (ex is GetValueException)
        {
          throw;
        }
        else
        {
          CommandProcessorBase.CheckForSevereException(ex);
          throw new GetValueInvocationException("CatchFromBaseParameterizedPropertyAdapterGetValue", ex, "ExtendedTypeSystem", "ExceptionWhenGetting", new object[2]
          {
            (object) property.Name,
            (object) ex.Message
          });
        }
      }
    }

    internal void BaseParameterizedPropertySet(
      PSParameterizedProperty property,
      object setValue,
      params object[] arguments)
    {
      try
      {
        this.ParameterizedPropertySet(property, setValue, arguments);
      }
      catch (TargetInvocationException ex)
      {
        Exception innerException = ex.InnerException == null ? (Exception) ex : ex.InnerException;
        throw new SetValueInvocationException("CatchFromBaseAdapterParameterizedPropertySetValueTI", innerException, "ExtendedTypeSystem", "ExceptionWhenSetting", new object[2]
        {
          (object) property.Name,
          (object) innerException.Message
        });
      }
      catch (Exception ex)
      {
        if (ex is SetValueException)
        {
          throw;
        }
        else
        {
          CommandProcessorBase.CheckForSevereException(ex);
          throw new SetValueInvocationException("CatchFromBaseAdapterParameterizedPropertySetValue", ex, "ExtendedTypeSystem", "ExceptionWhenSetting", new object[2]
          {
            (object) property.Name,
            (object) ex.Message
          });
        }
      }
    }

    internal string BaseParameterizedPropertyToString(PSParameterizedProperty property)
    {
      try
      {
        return this.ParameterizedPropertyToString(property);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        if (ex is ExtendedTypeSystemException)
          throw;
        else
          throw this.NewException(ex, "CatchFromBaseParameterizedPropertyToString", "CatchFromBaseParameterizedPropertyToStringTI", "ExceptionRetrievingParameterizedPropertyString", (object) property.Name);
      }
    }

    private static Type GetArgumentType(object argument)
    {
      if (argument == null)
        return typeof (LanguagePrimitives.Null);
      return argument is PSReference psReference ? Adapter.GetArgumentType(psReference.Value) : argument.GetType();
    }

    internal static ConversionRank GetArgumentConversionRank(
      object argument,
      Type parameterType)
    {
      ConversionRank conversionRank = LanguagePrimitives.GetConversionRank(Adapter.GetArgumentType(argument), parameterType);
      if (conversionRank == ConversionRank.None)
        conversionRank = LanguagePrimitives.GetConversionRank(Adapter.GetArgumentType(PSObject.Base(argument)), parameterType);
      return conversionRank;
    }

    private static ParameterInformation[] ExpandParameters(
      int argCount,
      ParameterInformation[] parameters,
      Type elementType)
    {
      ParameterInformation[] parameterInformationArray = new ParameterInformation[argCount];
      ParameterInformation parameter = parameters[parameters.Length - 1];
      Array.Copy((Array) parameters, (Array) parameterInformationArray, parameters.Length - 1);
      for (int index = parameters.Length - 1; index < argCount; ++index)
        parameterInformationArray[index] = new ParameterInformation(elementType, false, (object) null, false);
      return parameterInformationArray;
    }

    private static int CompareMethods(
      Adapter.OverloadCandidate candidate1,
      Adapter.OverloadCandidate candidate2,
      object[] arguments)
    {
      ParameterInformation[] parameterInformationArray1 = candidate1.expandedParameters != null ? candidate1.expandedParameters : candidate1.parameters;
      ParameterInformation[] parameterInformationArray2 = candidate2.expandedParameters != null ? candidate2.expandedParameters : candidate2.parameters;
      int num1 = 0;
      int length = parameterInformationArray1.Length;
      int index1 = 0;
      while (index1 < parameterInformationArray1.Length)
      {
        if (candidate1.conversionRanks[index1] < candidate2.conversionRanks[index1])
          num1 -= length;
        else if (candidate1.conversionRanks[index1] > candidate2.conversionRanks[index1])
          num1 += length;
        else if (candidate1.conversionRanks[index1] == ConversionRank.UnrelatedArrays)
        {
          Type elementType = Adapter.EffectiveArgumentType(arguments[index1]).GetElementType();
          ConversionRank conversionRank1 = LanguagePrimitives.GetConversionRank(elementType, parameterInformationArray1[index1].parameterType.GetElementType());
          ConversionRank conversionRank2 = LanguagePrimitives.GetConversionRank(elementType, parameterInformationArray2[index1].parameterType.GetElementType());
          if (conversionRank1 < conversionRank2)
            num1 -= length;
          else if (conversionRank1 > conversionRank2)
            num1 += length;
        }
        ++index1;
        --length;
      }
      if (num1 == 0)
      {
        int num2 = parameterInformationArray1.Length;
        int index2 = 0;
        while (index2 < parameterInformationArray1.Length)
        {
          ConversionRank conversionRank1 = candidate1.conversionRanks[index2];
          ConversionRank conversionRank2 = candidate2.conversionRanks[index2];
          if (conversionRank1 >= ConversionRank.NullToValue && conversionRank2 >= ConversionRank.NullToValue && conversionRank1 >= ConversionRank.NumericImplicit == conversionRank2 >= ConversionRank.NumericImplicit)
          {
            if (conversionRank1 >= ConversionRank.NumericImplicit)
              num2 = -num2;
            ConversionRank conversionRank3 = LanguagePrimitives.GetConversionRank(parameterInformationArray1[index2].parameterType, parameterInformationArray2[index2].parameterType);
            ConversionRank conversionRank4 = LanguagePrimitives.GetConversionRank(parameterInformationArray2[index2].parameterType, parameterInformationArray1[index2].parameterType);
            if (conversionRank3 < conversionRank4)
              num1 += num2;
            else if (conversionRank3 > conversionRank4)
              num1 -= num2;
          }
          ++index2;
          num2 = Math.Abs(num2) - 1;
        }
      }
      if (num1 != 0)
        return num1;
      for (int index2 = 0; index2 < parameterInformationArray1.Length; ++index2)
      {
        if (!parameterInformationArray1[index2].parameterType.Equals(parameterInformationArray2[index2].parameterType))
          return 0;
      }
      if (candidate1.expandedParameters != null && candidate2.expandedParameters != null)
        return candidate1.parameters.Length <= candidate2.parameters.Length ? -1 : 1;
      if (candidate1.expandedParameters != null)
        return -1;
      return candidate2.expandedParameters == null ? 0 : 1;
    }

    private static Adapter.OverloadCandidate FindBestCandidate(
      List<Adapter.OverloadCandidate> candidates,
      object[] arguments)
    {
      for (int index1 = 0; index1 < candidates.Count; ++index1)
      {
        int index2 = 0;
        while (index2 < candidates.Count && (index1 == index2 || Adapter.CompareMethods(candidates[index1], candidates[index2], arguments) > 0))
          ++index2;
        if (index2 == candidates.Count)
          return candidates[index1];
      }
      return (Adapter.OverloadCandidate) null;
    }

    internal static MethodInformation FindBestMethod(
      string methodName,
      MethodInformation[] methods,
      object[] arguments,
      out bool expandParamsOnBest)
    {
      if (methods.Length == 1 && !methods[0].hasVarArgs && methods[0].parameters.Length == arguments.Length)
      {
        expandParamsOnBest = false;
        return methods[0];
      }
      List<Adapter.OverloadCandidate> candidates = new List<Adapter.OverloadCandidate>();
      for (int index1 = 0; index1 < methods.Length; ++index1)
      {
        MethodInformation method = methods[index1];
        if (!method.isGeneric)
        {
          ParameterInformation[] parameters = method.parameters;
          if (arguments.Length != parameters.Length)
          {
            if (arguments.Length > parameters.Length)
            {
              if (!method.hasVarArgs)
                continue;
            }
            else if (method.hasOptional || method.hasVarArgs && arguments.Length + 1 == parameters.Length)
            {
              if (method.hasOptional)
              {
                int num = 0;
                for (int index2 = 0; index2 < parameters.Length; ++index2)
                {
                  if (parameters[index2].isOptional)
                    ++num;
                }
                if (arguments.Length + num < parameters.Length)
                  continue;
              }
            }
            else
              continue;
          }
          Adapter.OverloadCandidate overloadCandidate = new Adapter.OverloadCandidate(method, arguments.Length);
          for (int index2 = 0; overloadCandidate != null && index2 < parameters.Length; ++index2)
          {
            ParameterInformation parameterInformation = parameters[index2];
            if (!parameterInformation.isOptional || arguments.Length > index2)
            {
              if (parameterInformation.isParamArray)
              {
                Type elementType = parameterInformation.parameterType.GetElementType();
                if (parameters.Length == arguments.Length)
                {
                  ConversionRank argumentConversionRank1 = Adapter.GetArgumentConversionRank(arguments[index2], parameterInformation.parameterType);
                  ConversionRank argumentConversionRank2 = Adapter.GetArgumentConversionRank(arguments[index2], elementType);
                  if (argumentConversionRank2 > argumentConversionRank1)
                  {
                    overloadCandidate.expandedParameters = Adapter.ExpandParameters(arguments.Length, parameters, elementType);
                    overloadCandidate.conversionRanks[index2] = argumentConversionRank2;
                  }
                  else
                    overloadCandidate.conversionRanks[index2] = argumentConversionRank1;
                  if (overloadCandidate.conversionRanks[index2] == ConversionRank.None)
                    overloadCandidate = (Adapter.OverloadCandidate) null;
                }
                else
                {
                  for (int index3 = index2; index3 < arguments.Length; ++index3)
                  {
                    overloadCandidate.conversionRanks[index3] = Adapter.GetArgumentConversionRank(arguments[index3], elementType);
                    if (overloadCandidate.conversionRanks[index3] == ConversionRank.None)
                    {
                      overloadCandidate = (Adapter.OverloadCandidate) null;
                      break;
                    }
                  }
                  if (overloadCandidate != null)
                    overloadCandidate.expandedParameters = Adapter.ExpandParameters(arguments.Length, parameters, elementType);
                }
              }
              else
              {
                overloadCandidate.conversionRanks[index2] = Adapter.GetArgumentConversionRank(arguments[index2], parameterInformation.parameterType);
                if (overloadCandidate.conversionRanks[index2] == ConversionRank.None)
                  overloadCandidate = (Adapter.OverloadCandidate) null;
              }
            }
            else
              break;
          }
          if (overloadCandidate != null)
            candidates.Add(overloadCandidate);
        }
      }
      if (candidates.Count == 0)
        throw new MethodException("MethodCountCouldNotFindBest", (Exception) null, "ExtendedTypeSystem", "MethodArgumentCountException", new object[2]
        {
          (object) methodName,
          (object) arguments.Length
        });
      Adapter.OverloadCandidate overloadCandidate1 = candidates.Count != 1 ? Adapter.FindBestCandidate(candidates, arguments) : candidates[0];
      if (overloadCandidate1 != null)
      {
        expandParamsOnBest = overloadCandidate1.expandedParameters != null;
        return overloadCandidate1.method;
      }
      throw new MethodException("MethodCountCouldNotFindBest", (Exception) null, "ExtendedTypeSystem", "MethodAmbiguousException", new object[2]
      {
        (object) methodName,
        (object) arguments.Length
      });
    }

    internal static Type EffectiveArgumentType(object arg)
    {
      if (arg == null)
        return typeof (object);
      arg = PSObject.Base(arg);
      if (arg is object[] objArray && objArray.Length > 0 && PSObject.Base(objArray[0]) != null)
      {
        Type type = PSObject.Base(objArray[0]).GetType();
        bool flag = true;
        for (int index = 1; index < objArray.Length; ++index)
        {
          if (objArray[index] == null || !type.Equals(PSObject.Base(objArray[index]).GetType()))
          {
            flag = false;
            break;
          }
        }
        if (flag)
          return type.MakeArrayType();
      }
      return arg.GetType();
    }

    internal static MethodInformation FindCachedMethod(
      Type targetType,
      string methodName,
      object[] arguments,
      CallsiteCacheEntryFlags flags)
    {
      if (targetType == typeof (PSObject) || targetType == typeof (PSCustomObject))
        return (MethodInformation) null;
      CallsiteSignature signature = new CallsiteSignature(targetType, arguments, flags);
      CallsiteCacheEntry key = new CallsiteCacheEntry(methodName, signature);
      MethodInformation methodInformation = (MethodInformation) null;
      lock (Adapter.callsiteCache)
        Adapter.callsiteCache.TryGetValue(key, out methodInformation);
      return methodInformation;
    }

    internal static void CacheMethod(
      MethodInformation mi,
      object target,
      string methodName,
      object[] arguments,
      CallsiteCacheEntryFlags flags)
    {
      Type targetType = (flags & (CallsiteCacheEntryFlags.Static | CallsiteCacheEntryFlags.Constructor)) == CallsiteCacheEntryFlags.None ? target.GetType() : (Type) target;
      if (targetType == typeof (PSObject) || targetType == typeof (PSCustomObject))
        return;
      CallsiteSignature signature = new CallsiteSignature(targetType, arguments, flags);
      CallsiteCacheEntry key = new CallsiteCacheEntry(methodName, signature);
      lock (Adapter.callsiteCache)
      {
        if (Adapter.callsiteCache.ContainsKey(key))
          return;
        if (Adapter.callsiteCache.Count > 2048)
          Adapter.callsiteCache.Clear();
        Adapter.callsiteCache[key] = mi;
      }
    }

    internal static void SetReferences(
      object[] arguments,
      MethodInformation methodInformation,
      object[] originalArguments)
    {
      using (PSObject.memberResolution.TraceScope("Checking for possible references."))
      {
        ParameterInformation[] parameters = methodInformation.parameters;
        for (int index = 0; index < originalArguments.Length && index < parameters.Length && index < arguments.Length; ++index)
        {
          switch (originalArguments[index])
          {
            case PSReference baseObject:
label_4:
              if (parameters[index].isByRef)
              {
                object obj = arguments[index];
                PSObject.memberResolution.WriteLine("Argument '{0}' was a reference so it will be set to \"{1}\".", (object) (index + 1), obj);
                baseObject.Value = obj;
                break;
              }
              break;
            case PSObject psObject:
              if (!(psObject.BaseObject is PSReference baseObject))
                break;
              goto label_4;
          }
        }
      }
    }

    internal static MethodInformation GetBestMethodAndArguments(
      string methodName,
      MethodInformation[] methods,
      object[] arguments,
      out object[] newArguments)
    {
      bool expandParamsOnBest;
      MethodInformation bestMethod = Adapter.FindBestMethod(methodName, methods, arguments, out expandParamsOnBest);
      newArguments = Adapter.GetMethodArgumentsBase(methodName, bestMethod.parameters, arguments, expandParamsOnBest);
      return bestMethod;
    }

    internal static object[] GetMethodArgumentsBase(
      string methodName,
      ParameterInformation[] parameters,
      object[] arguments,
      bool expandParamsOnBest)
    {
      int length1 = parameters.Length;
      if (length1 == 0)
        return new object[0];
      object[] newArguments = new object[length1];
      for (int index = 0; index < length1 - 1; ++index)
      {
        ParameterInformation parameter = parameters[index];
        Adapter.SetNewArgument(methodName, arguments, newArguments, parameter, index);
      }
      ParameterInformation parameter1 = parameters[length1 - 1];
      if (!expandParamsOnBest)
      {
        Adapter.SetNewArgument(methodName, arguments, newArguments, parameter1, length1 - 1);
        return newArguments;
      }
      if (arguments.Length < length1)
      {
        newArguments[length1 - 1] = (object) new ArrayList().ToArray(parameter1.parameterType.GetElementType());
        return newArguments;
      }
      int length2 = arguments.Length - length1 + 1;
      if (length2 == 1 && arguments[arguments.Length - 1] == null)
      {
        newArguments[length1 - 1] = (object) null;
      }
      else
      {
        object[] objArray = new object[length2];
        Type elementType = parameter1.parameterType.GetElementType();
        for (int index = 0; index < length2; ++index)
        {
          int parameterIndex = index + length1 - 1;
          try
          {
            objArray[index] = Adapter.MethodArgumentConvertTo(arguments[parameterIndex], false, parameterIndex, elementType, (IFormatProvider) CultureInfo.InvariantCulture);
          }
          catch (InvalidCastException ex)
          {
            throw new MethodException("MethodArgumentConversionInvalidCastArgument", (Exception) ex, "ExtendedTypeSystem", "MethodArgumentConversionException", new object[5]
            {
              (object) parameterIndex,
              arguments[parameterIndex],
              (object) methodName,
              (object) elementType,
              (object) ex.Message
            });
          }
        }
        try
        {
          newArguments[length1 - 1] = Adapter.MethodArgumentConvertTo((object) objArray, parameter1.isByRef, length1 - 1, parameter1.parameterType, (IFormatProvider) CultureInfo.InvariantCulture);
        }
        catch (InvalidCastException ex)
        {
          throw new MethodException("MethodArgumentConversionParamsConversion", (Exception) ex, "ExtendedTypeSystem", "MethodArgumentConversionException", new object[5]
          {
            (object) (length1 - 1),
            (object) objArray,
            (object) methodName,
            (object) parameter1.parameterType,
            (object) ex.Message
          });
        }
      }
      return newArguments;
    }

    internal static void SetNewArgument(
      string methodName,
      object[] arguments,
      object[] newArguments,
      ParameterInformation parameter,
      int index)
    {
      if (arguments.Length > index)
      {
        try
        {
          newArguments[index] = Adapter.MethodArgumentConvertTo(arguments[index], parameter.isByRef, index, parameter.parameterType, (IFormatProvider) CultureInfo.InvariantCulture);
        }
        catch (InvalidCastException ex)
        {
          throw new MethodException("MethodArgumentConversionInvalidCastArgument", (Exception) ex, "ExtendedTypeSystem", "MethodArgumentConversionException", new object[5]
          {
            (object) index,
            arguments[index],
            (object) methodName,
            (object) parameter.parameterType,
            (object) ex.Message
          });
        }
      }
      else
        newArguments[index] = parameter.defaultValue;
    }

    internal static object MethodArgumentConvertTo(
      object valueToConvert,
      bool isParameterByRef,
      int parameterIndex,
      Type resultType,
      IFormatProvider formatProvider)
    {
      using (PSObject.memberResolution.TraceScope("Method argument conversion."))
      {
        if (resultType == null)
          throw Adapter.tracer.NewArgumentNullException(nameof (resultType));
        bool isArgumentByRef;
        valueToConvert = Adapter.UnReference(valueToConvert, out isArgumentByRef);
        if (isParameterByRef && !isArgumentByRef)
          throw new MethodException("NonRefArgumentToRefParameterMsg", (Exception) null, "ExtendedTypeSystem", "NonRefArgumentToRefParameter", new object[3]
          {
            (object) (parameterIndex + 1),
            (object) typeof (PSReference).FullName,
            (object) "[ref]"
          });
        if (isArgumentByRef && !isParameterByRef)
          throw new MethodException("RefArgumentToNonRefParameterMsg", (Exception) null, "ExtendedTypeSystem", "RefArgumentToNonRefParameter", new object[3]
          {
            (object) (parameterIndex + 1),
            (object) typeof (PSReference).FullName,
            (object) "[ref]"
          });
        return Adapter.PropertySetAndMethodArgumentConvertTo(valueToConvert, resultType, formatProvider);
      }
    }

    internal static object UnReference(object obj, out bool isArgumentByRef)
    {
      isArgumentByRef = false;
      if (obj is PSReference psReference)
      {
        PSObject.memberResolution.WriteLine("Parameter was a reference.", new object[0]);
        isArgumentByRef = true;
        return psReference.Value;
      }
      if (obj is PSObject psObject)
        psReference = psObject.BaseObject as PSReference;
      if (psReference == null)
        return obj;
      PSObject.memberResolution.WriteLine("Parameter was an PSObject containing a reference.", new object[0]);
      isArgumentByRef = true;
      return psReference.Value;
    }

    internal static object PropertySetAndMethodArgumentConvertTo(
      object valueToConvert,
      Type resultType,
      IFormatProvider formatProvider)
    {
      using (PSObject.memberResolution.TraceScope("Converting parameter \"{0}\" to \"{1}\".", valueToConvert, (object) resultType))
      {
        if (resultType == null)
          throw Adapter.tracer.NewArgumentNullException(nameof (resultType));
        if (!(valueToConvert is PSObject psObject) || !resultType.Equals(typeof (object)))
          return LanguagePrimitives.ConvertTo(valueToConvert, resultType, formatProvider);
        PSObject.memberResolution.WriteLine("Parameter was an PSObject and will be converted to System.Object.", new object[0]);
        return PSObject.Base((object) psObject);
      }
    }

    internal static void ResetCaches()
    {
      lock (Adapter.callsiteCache)
        Adapter.callsiteCache.Clear();
    }

    private class OverloadCandidate
    {
      internal MethodInformation method;
      internal ParameterInformation[] parameters;
      internal ParameterInformation[] expandedParameters;
      internal ConversionRank[] conversionRanks;

      internal OverloadCandidate(MethodInformation method, int argCount)
      {
        this.method = method;
        this.parameters = method.parameters;
        this.conversionRanks = new ConversionRank[argCount];
      }
    }
  }
}
