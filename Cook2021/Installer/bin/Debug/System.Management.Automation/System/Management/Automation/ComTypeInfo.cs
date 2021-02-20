// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ComTypeInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation.Internal;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace System.Management.Automation
{
  internal class ComTypeInfo
  {
    [TraceSource("COM", "Tracing for COM interop calls")]
    protected static PSTraceSource tracer = PSTraceSource.GetTracer("COM", "Tracing for COM interop calls");
    private Dictionary<string, ComProperty> properties;
    private Dictionary<string, ComMethod> methods;
    private ITypeInfo typeinfo;
    private Guid guid = Guid.Empty;

    internal ComTypeInfo(ITypeInfo info)
    {
      using (ComTypeInfo.tracer.TraceConstructor((object) this))
      {
        this.typeinfo = info;
        this.properties = new Dictionary<string, ComProperty>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        this.methods = new Dictionary<string, ComMethod>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        if (this.typeinfo == null)
          return;
        this.Initialize();
      }
    }

    public Dictionary<string, ComProperty> Properties
    {
      get
      {
        using (ComTypeInfo.tracer.TraceProperty((object) this.properties))
          return this.properties;
      }
    }

    public Dictionary<string, ComMethod> Methods
    {
      get
      {
        using (ComTypeInfo.tracer.TraceProperty((object) this.methods))
          return this.methods;
      }
    }

    public string Clsid => this.guid.ToString();

    private static int FindFirstUserMethod(System.Runtime.InteropServices.ComTypes.TYPEATTR typeattr)
    {
      int num = 0;
      if (typeattr.cFuncs >= (short) 7)
        num = 7;
      return num;
    }

    private void Initialize()
    {
      using (ComTypeInfo.tracer.TraceMethod())
      {
        if (this.typeinfo == null)
          return;
        System.Runtime.InteropServices.ComTypes.TYPEATTR typeAttr = ComTypeInfo.GetTypeAttr(this.typeinfo);
        this.guid = typeAttr.guid;
        for (int firstUserMethod = ComTypeInfo.FindFirstUserMethod(typeAttr); firstUserMethod < (int) typeAttr.cFuncs; ++firstUserMethod)
        {
          System.Runtime.InteropServices.ComTypes.FUNCDESC funcDesc = ComTypeInfo.GetFuncDesc(this.typeinfo, firstUserMethod);
          string nameFromFuncDesc = ComUtil.GetNameFromFuncDesc(this.typeinfo, funcDesc);
          switch (funcDesc.invkind)
          {
            case System.Runtime.InteropServices.ComTypes.INVOKEKIND.INVOKE_FUNC:
              this.AddMethod(nameFromFuncDesc, firstUserMethod);
              break;
            case System.Runtime.InteropServices.ComTypes.INVOKEKIND.INVOKE_PROPERTYGET:
            case System.Runtime.InteropServices.ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT:
            case System.Runtime.InteropServices.ComTypes.INVOKEKIND.INVOKE_PROPERTYPUTREF:
              this.AddProperty(nameFromFuncDesc, funcDesc, firstUserMethod);
              break;
          }
        }
      }
    }

    internal static ComTypeInfo GetDispatchTypeInfo(object comObject)
    {
      ComTypeInfo comTypeInfo = (ComTypeInfo) null;
      if (comObject is IDispatch dispatch)
      {
        ITypeInfo ppTInfo = (ITypeInfo) null;
        dispatch.GetTypeInfo(0, 0, out ppTInfo);
        if (ppTInfo != null)
        {
          System.Runtime.InteropServices.ComTypes.TYPEATTR typeAttr = ComTypeInfo.GetTypeAttr(ppTInfo);
          if (typeAttr.typekind == System.Runtime.InteropServices.ComTypes.TYPEKIND.TKIND_INTERFACE)
            ppTInfo = ComTypeInfo.GetDispatchTypeInfoFromCustomInterfaceTypeInfo(ppTInfo);
          if (typeAttr.typekind == System.Runtime.InteropServices.ComTypes.TYPEKIND.TKIND_COCLASS)
            ppTInfo = ComTypeInfo.GetDispatchTypeInfoFromCoClassTypeInfo(ppTInfo);
          comTypeInfo = new ComTypeInfo(ppTInfo);
        }
      }
      return comTypeInfo;
    }

    private void AddProperty(string strName, System.Runtime.InteropServices.ComTypes.FUNCDESC funcdesc, int index)
    {
      ComProperty comProperty;
      if (this.properties.ContainsKey(strName))
      {
        comProperty = this.properties[strName];
      }
      else
      {
        comProperty = new ComProperty(this.typeinfo, strName);
        this.properties[strName] = comProperty;
      }
      comProperty?.UpdateFuncDesc(funcdesc, index);
    }

    private void AddMethod(string strName, int index)
    {
      ComMethod comMethod;
      if (this.methods.ContainsKey(strName))
      {
        comMethod = this.methods[strName];
      }
      else
      {
        comMethod = new ComMethod(this.typeinfo, strName);
        this.methods[strName] = comMethod;
      }
      comMethod?.AddFuncDesc(index);
    }

    [ArchitectureSensitive]
    internal static System.Runtime.InteropServices.ComTypes.TYPEATTR GetTypeAttr(
      ITypeInfo typeinfo)
    {
      IntPtr ppTypeAttr;
      typeinfo.GetTypeAttr(out ppTypeAttr);
      System.Runtime.InteropServices.ComTypes.TYPEATTR structure = (System.Runtime.InteropServices.ComTypes.TYPEATTR) Marshal.PtrToStructure(ppTypeAttr, typeof (System.Runtime.InteropServices.ComTypes.TYPEATTR));
      typeinfo.ReleaseTypeAttr(ppTypeAttr);
      return structure;
    }

    [ArchitectureSensitive]
    internal static System.Runtime.InteropServices.ComTypes.FUNCDESC GetFuncDesc(
      ITypeInfo typeinfo,
      int index)
    {
      IntPtr ppFuncDesc;
      typeinfo.GetFuncDesc(index, out ppFuncDesc);
      System.Runtime.InteropServices.ComTypes.FUNCDESC structure = (System.Runtime.InteropServices.ComTypes.FUNCDESC) Marshal.PtrToStructure(ppFuncDesc, typeof (System.Runtime.InteropServices.ComTypes.FUNCDESC));
      typeinfo.ReleaseFuncDesc(ppFuncDesc);
      return structure;
    }

    internal static ITypeInfo GetDispatchTypeInfoFromCustomInterfaceTypeInfo(
      ITypeInfo typeinfo)
    {
      ITypeInfo ppTI = (ITypeInfo) null;
      try
      {
        int href;
        typeinfo.GetRefTypeOfImplType(-1, out href);
        typeinfo.GetRefTypeInfo(href, out ppTI);
      }
      catch (COMException ex)
      {
        if (ex.ErrorCode != -2147319765)
          throw;
      }
      return ppTI;
    }

    internal static ITypeInfo GetDispatchTypeInfoFromCoClassTypeInfo(ITypeInfo typeinfo)
    {
      int cImplTypes = (int) ComTypeInfo.GetTypeAttr(typeinfo).cImplTypes;
      ITypeInfo ppTI = (ITypeInfo) null;
      for (int index = 0; index < cImplTypes; ++index)
      {
        int href;
        typeinfo.GetRefTypeOfImplType(index, out href);
        typeinfo.GetRefTypeInfo(href, out ppTI);
        System.Runtime.InteropServices.ComTypes.TYPEATTR typeAttr = ComTypeInfo.GetTypeAttr(ppTI);
        if (typeAttr.typekind == System.Runtime.InteropServices.ComTypes.TYPEKIND.TKIND_DISPATCH)
          return ppTI;
        if ((typeAttr.wTypeFlags & System.Runtime.InteropServices.ComTypes.TYPEFLAGS.TYPEFLAG_FDUAL) != (System.Runtime.InteropServices.ComTypes.TYPEFLAGS) 0)
        {
          ppTI = ComTypeInfo.GetDispatchTypeInfoFromCustomInterfaceTypeInfo(ppTI);
          if (ComTypeInfo.GetTypeAttr(ppTI).typekind == System.Runtime.InteropServices.ComTypes.TYPEKIND.TKIND_DISPATCH)
            return ppTI;
        }
      }
      return (ITypeInfo) null;
    }
  }
}
