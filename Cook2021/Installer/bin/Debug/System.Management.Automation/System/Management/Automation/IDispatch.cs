// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.IDispatch
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace System.Management.Automation
{
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("00020400-0000-0000-c000-000000000046")]
  internal interface IDispatch
  {
    [MethodImpl(MethodImplOptions.PreserveSig)]
    int GetTypeInfoCount(out int info);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int GetTypeInfo(int iTInfo, int lcid, out ITypeInfo ppTInfo);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int GetIDsOfNames(
      [In] ref Guid iid_null,
      [MarshalAs(UnmanagedType.LPWStr), In] string[] rgszNames,
      int cNames,
      int lcid,
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2), Out] int[] rgDispId);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int Invoke(
      int dispIdMember,
      [In] ref Guid iid_null,
      int lcid,
      int wFlags,
      [MarshalAs(UnmanagedType.LPArray), In] System.Runtime.InteropServices.ComTypes.DISPPARAMS[] pDispParams,
      [MarshalAs(UnmanagedType.Struct)] out object pVarResult,
      ref System.Runtime.InteropServices.ComTypes.EXCEPINFO pExcepInfo,
      [MarshalAs(UnmanagedType.LPArray, SizeConst = 1), Out] int[] puArgErr);
  }
}
