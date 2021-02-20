// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ComUtil
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace System.Management.Automation
{
  internal class ComUtil
  {
    internal static string GetMethodSignatureFromFuncDesc(
      ITypeInfo typeinfo,
      System.Runtime.InteropServices.ComTypes.FUNCDESC funcdesc,
      bool isPropertyPut)
    {
      StringBuilder stringBuilder = new StringBuilder();
      string nameFromFuncDesc = ComUtil.GetNameFromFuncDesc(typeinfo, funcdesc);
      if (!isPropertyPut)
      {
        string stringFromTypeDesc = ComUtil.GetStringFromTypeDesc(typeinfo, funcdesc.elemdescFunc.tdesc);
        stringBuilder.Append(stringFromTypeDesc + " ");
      }
      stringBuilder.Append(nameFromFuncDesc);
      stringBuilder.Append(" (");
      IntPtr lprgelemdescParam = funcdesc.lprgelemdescParam;
      int num1 = Marshal.SizeOf(typeof (System.Runtime.InteropServices.ComTypes.ELEMDESC));
      for (int index = 0; index < (int) funcdesc.cParams; ++index)
      {
        System.Runtime.InteropServices.ComTypes.ELEMDESC elemdesc = new System.Runtime.InteropServices.ComTypes.ELEMDESC();
        int num2 = index * num1;
        elemdesc = (System.Runtime.InteropServices.ComTypes.ELEMDESC) Marshal.PtrToStructure(IntPtr.Size != 4 ? (IntPtr) (lprgelemdescParam.ToInt64() + (long) num2) : (IntPtr) (lprgelemdescParam.ToInt32() + num2), typeof (System.Runtime.InteropServices.ComTypes.ELEMDESC));
        string stringFromTypeDesc = ComUtil.GetStringFromTypeDesc(typeinfo, elemdesc.tdesc);
        if (index == 0 && isPropertyPut)
        {
          stringBuilder.Insert(0, stringFromTypeDesc + " ");
        }
        else
        {
          stringBuilder.Append(stringFromTypeDesc);
          if (index < (int) funcdesc.cParams - 1)
            stringBuilder.Append(", ");
        }
      }
      stringBuilder.Append(")");
      return stringBuilder.ToString();
    }

    internal static string GetNameFromFuncDesc(ITypeInfo typeinfo, System.Runtime.InteropServices.ComTypes.FUNCDESC funcdesc)
    {
      string strName;
      typeinfo.GetDocumentation(funcdesc.memid, out strName, out string _, out int _, out string _);
      return strName;
    }

    private static string GetStringFromCustomType(ITypeInfo typeinfo, IntPtr refptr)
    {
      int hRef = (int) (long) refptr;
      ITypeInfo ppTI;
      typeinfo.GetRefTypeInfo(hRef, out ppTI);
      if (ppTI == null)
        return "UnknownCustomtype";
      string strName;
      ppTI.GetDocumentation(-1, out strName, out string _, out int _, out string _);
      return strName;
    }

    private static string GetStringFromTypeDesc(ITypeInfo typeinfo, System.Runtime.InteropServices.ComTypes.TYPEDESC typedesc)
    {
      if (typedesc.vt == (short) 26)
      {
        System.Runtime.InteropServices.ComTypes.TYPEDESC structure = (System.Runtime.InteropServices.ComTypes.TYPEDESC) Marshal.PtrToStructure(typedesc.lpValue, typeof (System.Runtime.InteropServices.ComTypes.TYPEDESC));
        return ComUtil.GetStringFromTypeDesc(typeinfo, structure);
      }
      if (typedesc.vt == (short) 27)
      {
        System.Runtime.InteropServices.ComTypes.TYPEDESC structure = (System.Runtime.InteropServices.ComTypes.TYPEDESC) Marshal.PtrToStructure(typedesc.lpValue, typeof (System.Runtime.InteropServices.ComTypes.TYPEDESC));
        return "SAFEARRAY(" + ComUtil.GetStringFromTypeDesc(typeinfo, structure) + ")";
      }
      if (typedesc.vt == (short) 29)
        return ComUtil.GetStringFromCustomType(typeinfo, typedesc.lpValue);
      switch ((VarEnum) typedesc.vt)
      {
        case VarEnum.VT_EMPTY:
          return "";
        case VarEnum.VT_I2:
          return "short";
        case VarEnum.VT_I4:
        case VarEnum.VT_INT:
        case VarEnum.VT_HRESULT:
          return "int";
        case VarEnum.VT_R4:
          return "float";
        case VarEnum.VT_R8:
          return "double";
        case VarEnum.VT_CY:
          return "currency";
        case VarEnum.VT_DATE:
          return "Date";
        case VarEnum.VT_BSTR:
        case VarEnum.VT_LPSTR:
        case VarEnum.VT_LPWSTR:
          return "string";
        case VarEnum.VT_DISPATCH:
          return "IDispatch";
        case VarEnum.VT_BOOL:
          return "bool";
        case VarEnum.VT_VARIANT:
          return "Variant";
        case VarEnum.VT_UNKNOWN:
          return "IUnknown";
        case VarEnum.VT_DECIMAL:
          return "decimal";
        case VarEnum.VT_I1:
          return "char";
        case VarEnum.VT_UI1:
          return "byte";
        case VarEnum.VT_UI2:
          return "ushort";
        case VarEnum.VT_UI4:
        case VarEnum.VT_UINT:
          return "uint";
        case VarEnum.VT_I8:
          return "int64";
        case VarEnum.VT_UI8:
          return "uint64";
        case VarEnum.VT_VOID:
          return "void";
        case VarEnum.VT_CLSID:
          return "clsid";
        case VarEnum.VT_ARRAY:
          return "object[]";
        default:
          return "Unknown!";
      }
    }

    internal static Type GetTypeFromTypeDesc(System.Runtime.InteropServices.ComTypes.TYPEDESC typedesc)
    {
      Type type;
      switch ((VarEnum) typedesc.vt)
      {
        case VarEnum.VT_I2:
          type = typeof (short);
          break;
        case VarEnum.VT_I4:
        case VarEnum.VT_INT:
        case VarEnum.VT_HRESULT:
          type = typeof (int);
          break;
        case VarEnum.VT_R4:
          type = typeof (float);
          break;
        case VarEnum.VT_R8:
          type = typeof (double);
          break;
        case VarEnum.VT_CY:
        case VarEnum.VT_DECIMAL:
          type = typeof (Decimal);
          break;
        case VarEnum.VT_DATE:
          type = typeof (DateTime);
          break;
        case VarEnum.VT_BSTR:
        case VarEnum.VT_LPSTR:
        case VarEnum.VT_LPWSTR:
          type = typeof (string);
          break;
        case VarEnum.VT_BOOL:
          type = typeof (bool);
          break;
        case VarEnum.VT_I1:
          type = typeof (sbyte);
          break;
        case VarEnum.VT_UI1:
          type = typeof (byte);
          break;
        case VarEnum.VT_UI2:
          type = typeof (ushort);
          break;
        case VarEnum.VT_UI4:
        case VarEnum.VT_UINT:
          type = typeof (uint);
          break;
        case VarEnum.VT_I8:
          type = typeof (long);
          break;
        case VarEnum.VT_UI8:
          type = typeof (ulong);
          break;
        case VarEnum.VT_VOID:
          type = typeof (void);
          break;
        case VarEnum.VT_CLSID:
          type = typeof (Guid);
          break;
        case VarEnum.VT_ARRAY:
          type = typeof (object[]);
          break;
        default:
          type = typeof (object);
          break;
      }
      return type;
    }

    private static ComMethodInformation GetMethodInformation(
      System.Runtime.InteropServices.ComTypes.FUNCDESC funcdesc,
      bool skipLastParameter)
    {
      Type typeFromTypeDesc = ComUtil.GetTypeFromTypeDesc(funcdesc.elemdescFunc.tdesc);
      ParameterInformation[] parameterInformation1 = ComUtil.GetParameterInformation(funcdesc, skipLastParameter);
      bool hasoptional = false;
      foreach (ParameterInformation parameterInformation2 in parameterInformation1)
      {
        if (parameterInformation2.isOptional)
        {
          hasoptional = true;
          break;
        }
      }
      return new ComMethodInformation(false, hasoptional, parameterInformation1, typeFromTypeDesc);
    }

    internal static ParameterInformation[] GetParameterInformation(
      System.Runtime.InteropServices.ComTypes.FUNCDESC funcdesc,
      bool skipLastParameter)
    {
      int cParams = (int) funcdesc.cParams;
      if (skipLastParameter)
        --cParams;
      ParameterInformation[] parameterInformationArray = new ParameterInformation[cParams];
      IntPtr lprgelemdescParam = funcdesc.lprgelemdescParam;
      int num1 = Marshal.SizeOf(typeof (System.Runtime.InteropServices.ComTypes.ELEMDESC));
      for (int index = 0; index < cParams; ++index)
      {
        System.Runtime.InteropServices.ComTypes.ELEMDESC elemdesc = new System.Runtime.InteropServices.ComTypes.ELEMDESC();
        int num2 = index * num1;
        elemdesc = (System.Runtime.InteropServices.ComTypes.ELEMDESC) Marshal.PtrToStructure(IntPtr.Size != 4 ? (IntPtr) (lprgelemdescParam.ToInt64() + (long) num2) : (IntPtr) (lprgelemdescParam.ToInt32() + num2), typeof (System.Runtime.InteropServices.ComTypes.ELEMDESC));
        Type typeFromTypeDesc = ComUtil.GetTypeFromTypeDesc(elemdesc.tdesc);
        object defaultValue = (object) null;
        bool isOptional;
        if ((elemdesc.desc.paramdesc.wParamFlags & System.Runtime.InteropServices.ComTypes.PARAMFLAG.PARAMFLAG_FOPT) != System.Runtime.InteropServices.ComTypes.PARAMFLAG.PARAMFLAG_NONE)
        {
          isOptional = true;
          defaultValue = Type.Missing;
        }
        else
          isOptional = false;
        bool isByRef = false;
        if ((elemdesc.desc.paramdesc.wParamFlags & System.Runtime.InteropServices.ComTypes.PARAMFLAG.PARAMFLAG_FOUT) != System.Runtime.InteropServices.ComTypes.PARAMFLAG.PARAMFLAG_NONE)
          isByRef = true;
        parameterInformationArray[index] = new ParameterInformation(typeFromTypeDesc, isOptional, defaultValue, isByRef);
      }
      return parameterInformationArray;
    }

    internal static ComMethodInformation[] GetMethodInformationArray(
      ITypeInfo typeInfo,
      Collection<int> methods,
      bool skipLastParameters)
    {
      int count = methods.Count;
      int num = 0;
      ComMethodInformation[] methodInformationArray = new ComMethodInformation[count];
      foreach (int method in methods)
      {
        IntPtr ppFuncDesc;
        typeInfo.GetFuncDesc(method, out ppFuncDesc);
        System.Runtime.InteropServices.ComTypes.FUNCDESC structure = (System.Runtime.InteropServices.ComTypes.FUNCDESC) Marshal.PtrToStructure(ppFuncDesc, typeof (System.Runtime.InteropServices.ComTypes.FUNCDESC));
        methodInformationArray[num++] = ComUtil.GetMethodInformation(structure, skipLastParameters);
        typeInfo.ReleaseFuncDesc(ppFuncDesc);
      }
      return methodInformationArray;
    }

    internal static ParameterModifier[] GetModifiers(
      ParameterInformation[] parameters)
    {
      int length = parameters.Length;
      if (parameters.Length == 0)
        return (ParameterModifier[]) null;
      ParameterModifier parameterModifier = new ParameterModifier(length);
      for (int index = 0; index < length; ++index)
        parameterModifier[index] = parameters[index].isByRef;
      return new ParameterModifier[1]{ parameterModifier };
    }
  }
}
