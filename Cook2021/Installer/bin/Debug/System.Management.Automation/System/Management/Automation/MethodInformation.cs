// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.MethodInformation
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Reflection;
using System.Reflection.Emit;

namespace System.Management.Automation
{
  internal class MethodInformation
  {
    internal MethodBase method;
    internal string methodDefinition;
    internal ParameterInformation[] parameters;
    internal bool hasVarArgs;
    internal bool hasOptional;
    internal bool isGeneric;
    private bool useReflection;
    private MethodInformation.MethodInvoker methodInvoker;
    private static OpCode[] _ldc = new OpCode[9]
    {
      OpCodes.Ldc_I4_0,
      OpCodes.Ldc_I4_1,
      OpCodes.Ldc_I4_2,
      OpCodes.Ldc_I4_3,
      OpCodes.Ldc_I4_4,
      OpCodes.Ldc_I4_5,
      OpCodes.Ldc_I4_6,
      OpCodes.Ldc_I4_7,
      OpCodes.Ldc_I4_8
    };

    internal MethodInformation(string methodName, MethodBase method, int parametersToIgnore)
    {
      this.method = method;
      this.methodDefinition = DotNetAdapter.GetMethodInfoOverloadDefinition(methodName, method, parametersToIgnore);
      this.isGeneric = method.IsGenericMethod;
      ParameterInfo[] parameters = method.GetParameters();
      int length = parameters.Length - parametersToIgnore;
      this.parameters = new ParameterInformation[length];
      for (int index = 0; index < length; ++index)
      {
        this.parameters[index] = new ParameterInformation(parameters[index]);
        if (parameters[index].IsOptional)
          this.hasOptional = true;
      }
      this.hasVarArgs = false;
      if (length <= 0)
        return;
      ParameterInfo parameterInfo = parameters[length - 1];
      if (this.hasOptional || !parameterInfo.ParameterType.IsArray || parameterInfo.GetCustomAttributes(typeof (ParamArrayAttribute), false).Length == 0)
        return;
      this.hasVarArgs = true;
      this.parameters[length - 1].isParamArray = true;
    }

    internal MethodInformation(bool hasvarargs, bool hasoptional, ParameterInformation[] arguments)
    {
      this.hasVarArgs = hasvarargs;
      this.hasOptional = hasoptional;
      this.parameters = arguments;
    }

    internal object Invoke(object target, object[] arguments)
    {
      if (!this.useReflection)
      {
        if (this.methodInvoker == null)
        {
          if (!(this.method is MethodInfo))
            this.useReflection = true;
          else
            this.methodInvoker = this.GetMethodInvoker(this.method as MethodInfo);
        }
        if (this.methodInvoker != null)
          return this.methodInvoker(target, arguments);
      }
      return this.method.Invoke(target, arguments);
    }

    private static void EmitLdc(ILGenerator emitter, int c)
    {
      if (c < MethodInformation._ldc.Length)
        emitter.Emit(MethodInformation._ldc[c]);
      else
        emitter.Emit(OpCodes.Ldc_I4, c);
    }

    private static bool CompareMethodParameters(MethodInfo method1, MethodInfo method2)
    {
      ParameterInfo[] parameters1 = method1.GetParameters();
      ParameterInfo[] parameters2 = method2.GetParameters();
      if (parameters1.Length != parameters2.Length)
        return false;
      for (int index = 0; index < parameters1.Length; ++index)
      {
        if (!parameters1[index].ParameterType.Equals(parameters2[index].ParameterType))
          return false;
      }
      return true;
    }

    private static Type FindInterfaceForMethod(MethodInfo method, out MethodInfo methodToCall)
    {
      methodToCall = (MethodInfo) null;
      foreach (Type type in method.DeclaringType.GetInterfaces())
      {
        MethodInfo method1 = type.GetMethod(method.Name, BindingFlags.Instance);
        if (method1 != null && MethodInformation.CompareMethodParameters(method1, method))
        {
          methodToCall = method1;
          return type;
        }
      }
      return (Type) null;
    }

    private MethodInformation.MethodInvoker GetMethodInvoker(MethodInfo method)
    {
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      MethodInfo methodToCall = method;
      DynamicMethod dynamicMethod = new DynamicMethod(method.Name, typeof (object), new Type[2]
      {
        typeof (object),
        typeof (object[])
      }, typeof (Adapter).Module, true);
      ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
      ParameterInfo[] parameters = method.GetParameters();
      int length = 0;
      if (!method.IsStatic && method.DeclaringType.IsValueType && !method.IsVirtual)
      {
        flag1 = true;
        ++length;
      }
      foreach (ParameterInfo parameterInfo in parameters)
      {
        if (parameterInfo.IsOut || parameterInfo.ParameterType.IsByRef)
        {
          flag2 = true;
          ++length;
        }
      }
      LocalBuilder[] localBuilderArray = (LocalBuilder[]) null;
      if (length > 0)
      {
        if (flag2 && method.ReturnType != typeof (void))
        {
          ++length;
          flag3 = true;
        }
        localBuilderArray = new LocalBuilder[length];
        int index = 0;
        if (flag1)
        {
          Type declaringType = method.DeclaringType;
          localBuilderArray[index] = ilGenerator.DeclareLocal(declaringType);
          ilGenerator.Emit(OpCodes.Ldarg_0);
          ilGenerator.Emit(OpCodes.Unbox_Any, declaringType);
          ilGenerator.Emit(OpCodes.Stloc, localBuilderArray[index]);
          ++index;
        }
        for (int c = 0; c < parameters.Length; ++c)
        {
          Type type = parameters[c].ParameterType;
          if (parameters[c].IsOut || type.IsByRef)
          {
            if (type.IsByRef)
              type = type.GetElementType();
            localBuilderArray[index] = ilGenerator.DeclareLocal(type);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            MethodInformation.EmitLdc(ilGenerator, c);
            ilGenerator.Emit(OpCodes.Ldelem_Ref);
            if (type.IsValueType)
              ilGenerator.Emit(OpCodes.Unbox_Any, type);
            else if (type != typeof (object))
              ilGenerator.Emit(OpCodes.Castclass, type);
            ilGenerator.Emit(OpCodes.Stloc, localBuilderArray[index]);
            ++index;
          }
        }
        if (flag3)
          localBuilderArray[index] = ilGenerator.DeclareLocal(method.ReturnType);
      }
      int index1 = 0;
      if (!method.IsStatic)
      {
        if (method.DeclaringType.IsValueType)
        {
          if (method.IsVirtual)
          {
            Type interfaceForMethod = MethodInformation.FindInterfaceForMethod(method, out methodToCall);
            if (interfaceForMethod == null)
            {
              this.useReflection = true;
              return (MethodInformation.MethodInvoker) null;
            }
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Castclass, interfaceForMethod);
          }
          else
          {
            ilGenerator.Emit(OpCodes.Ldloca, localBuilderArray[index1]);
            ++index1;
          }
        }
        else
          ilGenerator.Emit(OpCodes.Ldarg_0);
      }
      for (int c = 0; c < parameters.Length; ++c)
      {
        Type parameterType = parameters[c].ParameterType;
        if (parameterType.IsByRef)
        {
          ilGenerator.Emit(OpCodes.Ldloca, localBuilderArray[index1]);
          ++index1;
        }
        else if (parameters[c].IsOut)
        {
          ilGenerator.Emit(OpCodes.Ldloc, localBuilderArray[index1]);
          ++index1;
        }
        else
        {
          ilGenerator.Emit(OpCodes.Ldarg_1);
          MethodInformation.EmitLdc(ilGenerator, c);
          ilGenerator.Emit(OpCodes.Ldelem_Ref);
          if (parameterType.IsValueType)
            ilGenerator.Emit(OpCodes.Unbox_Any, parameterType);
          else if (parameterType != typeof (object))
            ilGenerator.Emit(OpCodes.Castclass, parameterType);
        }
      }
      if (method.IsStatic)
        ilGenerator.EmitCall(OpCodes.Call, methodToCall, (Type[]) null);
      else
        ilGenerator.EmitCall(OpCodes.Callvirt, methodToCall, (Type[]) null);
      if (flag3)
        ilGenerator.Emit(OpCodes.Stloc, localBuilderArray[localBuilderArray.Length - 1]);
      if (flag2)
      {
        int index2 = flag1 ? 1 : 0;
        for (int c = 0; c < parameters.Length; ++c)
        {
          Type cls = parameters[c].ParameterType;
          if (parameters[c].IsOut || cls.IsByRef)
          {
            if (cls.IsByRef)
              cls = cls.GetElementType();
            ilGenerator.Emit(OpCodes.Ldarg_1);
            MethodInformation.EmitLdc(ilGenerator, c);
            ilGenerator.Emit(OpCodes.Ldloc, localBuilderArray[index2]);
            if (cls.IsValueType)
              ilGenerator.Emit(OpCodes.Box, cls);
            ilGenerator.Emit(OpCodes.Stelem_Ref);
            ++index2;
          }
        }
      }
      if (method.ReturnType == typeof (void))
      {
        ilGenerator.Emit(OpCodes.Ldnull);
      }
      else
      {
        if (flag3)
          ilGenerator.Emit(OpCodes.Ldloc, localBuilderArray[localBuilderArray.Length - 1]);
        if (method.ReturnType.IsValueType)
          ilGenerator.Emit(OpCodes.Box, method.ReturnType);
      }
      ilGenerator.Emit(OpCodes.Ret);
      return (MethodInformation.MethodInvoker) dynamicMethod.CreateDelegate(typeof (MethodInformation.MethodInvoker));
    }

    private delegate object MethodInvoker(object target, object[] arguments);
  }
}
