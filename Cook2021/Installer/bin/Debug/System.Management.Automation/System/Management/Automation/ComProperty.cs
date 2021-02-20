// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ComProperty
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace System.Management.Automation
{
  internal class ComProperty
  {
    [TraceSource("COM", "Tracing for COM interop calls")]
    protected static PSTraceSource tracer = PSTraceSource.GetTracer("COM", "Tracing for COM interop calls");
    private bool hasSetter;
    private bool hasSetterByRef;
    private bool hasGetter;
    private int setterIndex;
    private int setterByRefIndex;
    private int getterIndex;
    private ITypeInfo typeInfo;
    private string name;
    private bool isparameterizied;
    private Type cachedType;

    internal ComProperty(ITypeInfo typeinfo, string name)
    {
      using (ComProperty.tracer.TraceConstructor((object) this))
      {
        this.typeInfo = typeinfo;
        this.name = name;
      }
    }

    internal string Name
    {
      get
      {
        using (ComProperty.tracer.TraceProperty(this.name, new object[0]))
          return this.name;
      }
    }

    internal Type Type
    {
      get
      {
        this.cachedType = (Type) null;
        if (this.cachedType == null)
        {
          IntPtr ppFuncDesc = IntPtr.Zero;
          try
          {
            this.typeInfo.GetFuncDesc(this.GetFuncDescIndex(), out ppFuncDesc);
            System.Runtime.InteropServices.ComTypes.FUNCDESC structure = (System.Runtime.InteropServices.ComTypes.FUNCDESC) Marshal.PtrToStructure(ppFuncDesc, typeof (System.Runtime.InteropServices.ComTypes.FUNCDESC));
            this.cachedType = !this.hasGetter ? ComUtil.GetParameterInformation(structure, false)[0].parameterType : ComUtil.GetTypeFromTypeDesc(structure.elemdescFunc.tdesc);
          }
          finally
          {
            if (ppFuncDesc != IntPtr.Zero)
              this.typeInfo.ReleaseFuncDesc(ppFuncDesc);
          }
        }
        return this.cachedType;
      }
    }

    private int GetFuncDescIndex()
    {
      if (this.hasGetter)
        return this.getterIndex;
      return this.hasSetter ? this.setterIndex : this.setterByRefIndex;
    }

    internal bool IsParameterized => this.isparameterizied;

    internal int ParamCount => 0;

    internal bool IsSettable
    {
      get
      {
        using (ComProperty.tracer.TraceProperty())
          return this.hasSetter | this.hasSetterByRef;
      }
    }

    internal bool IsGettable
    {
      get
      {
        using (ComProperty.tracer.TraceProperty())
          return this.hasGetter;
      }
    }

    internal object GetValue(object target)
    {
      using (ComProperty.tracer.TraceMethod())
      {
        Type type = target.GetType();
        try
        {
          return type.InvokeMember(this.name, BindingFlags.IgnoreCase | BindingFlags.GetProperty, (Binder) null, target, (object[]) null, CultureInfo.CurrentCulture);
        }
        catch (TargetInvocationException ex)
        {
          CommandProcessorBase.CheckForSevereException(ex.InnerException);
          if (ex.InnerException is COMException innerException)
          {
            if (innerException.ErrorCode == -2147352573)
              goto label_8;
          }
          throw;
        }
        catch (COMException ex)
        {
          if (ex.ErrorCode != -2147352570)
            throw;
        }
label_8:
        return (object) null;
      }
    }

    internal object GetValue(object target, object[] arguments)
    {
      using (ComProperty.tracer.TraceMethod())
      {
        Type type = target.GetType();
        try
        {
          object[] newArguments;
          MethodInformation methodAndArguments = Adapter.GetBestMethodAndArguments(this.Name, (MethodInformation[]) ComUtil.GetMethodInformationArray(this.typeInfo, new Collection<int>()
          {
            this.getterIndex
          }, false), arguments, out newArguments);
          object obj = type.InvokeMember(this.name, BindingFlags.IgnoreCase | BindingFlags.GetProperty, (Binder) null, target, newArguments, ComUtil.GetModifiers(methodAndArguments.parameters), CultureInfo.CurrentCulture, (string[]) null);
          Adapter.SetReferences(newArguments, methodAndArguments, arguments);
          return obj;
        }
        catch (TargetInvocationException ex)
        {
          CommandProcessorBase.CheckForSevereException(ex.InnerException);
          if (ex.InnerException is COMException innerException)
          {
            if (innerException.ErrorCode == -2147352573)
              goto label_8;
          }
          throw;
        }
        catch (COMException ex)
        {
          if (ex.ErrorCode != -2147352570)
            throw;
        }
label_8:
        return (object) null;
      }
    }

    internal void SetValue(object target, object setValue)
    {
      using (ComProperty.tracer.TraceMethod())
      {
        Type type = target.GetType();
        object[] args = new object[1];
        setValue = Adapter.PropertySetAndMethodArgumentConvertTo(setValue, this.Type, (IFormatProvider) CultureInfo.InvariantCulture);
        args[0] = setValue;
        try
        {
          type.InvokeMember(this.name, BindingFlags.IgnoreCase | BindingFlags.SetProperty, (Binder) null, target, args, CultureInfo.CurrentCulture);
        }
        catch (TargetInvocationException ex)
        {
          CommandProcessorBase.CheckForSevereException(ex.InnerException);
          if (ex.InnerException is COMException innerException && innerException.ErrorCode == -2147352573)
            return;
          throw;
        }
        catch (COMException ex)
        {
          if (ex.ErrorCode == -2147352570)
            return;
          throw;
        }
      }
    }

    internal void SetValue(object target, object setValue, object[] arguments)
    {
      using (ComProperty.tracer.TraceMethod())
      {
        object[] newArguments;
        MethodInformation methodAndArguments = Adapter.GetBestMethodAndArguments(this.Name, (MethodInformation[]) ComUtil.GetMethodInformationArray(this.typeInfo, new Collection<int>()
        {
          this.hasSetterByRef ? this.setterByRefIndex : this.setterIndex
        }, true), arguments, out newArguments);
        Type type = target.GetType();
        object[] objArray = new object[newArguments.Length + 1];
        for (int index = 0; index < newArguments.Length; ++index)
          objArray[index] = newArguments[index];
        objArray[newArguments.Length] = Adapter.PropertySetAndMethodArgumentConvertTo(setValue, this.Type, (IFormatProvider) CultureInfo.InvariantCulture);
        try
        {
          type.InvokeMember(this.name, BindingFlags.IgnoreCase | BindingFlags.SetProperty, (Binder) null, target, objArray, ComUtil.GetModifiers(methodAndArguments.parameters), CultureInfo.CurrentCulture, (string[]) null);
          Adapter.SetReferences(objArray, methodAndArguments, arguments);
        }
        catch (TargetInvocationException ex)
        {
          CommandProcessorBase.CheckForSevereException(ex.InnerException);
          if (ex.InnerException is COMException innerException && innerException.ErrorCode == -2147352573)
            return;
          throw;
        }
        catch (COMException ex)
        {
          if (ex.ErrorCode == -2147352570)
            return;
          throw;
        }
      }
    }

    internal void UpdateFuncDesc(System.Runtime.InteropServices.ComTypes.FUNCDESC desc, int index)
    {
      using (ComProperty.tracer.TraceMethod())
      {
        switch (desc.invkind)
        {
          case System.Runtime.InteropServices.ComTypes.INVOKEKIND.INVOKE_PROPERTYGET:
            this.hasGetter = true;
            this.getterIndex = index;
            if (desc.cParams <= (short) 0)
              break;
            this.isparameterizied = true;
            break;
          case System.Runtime.InteropServices.ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT:
            this.hasSetter = true;
            this.setterIndex = index;
            if (desc.cParams <= (short) 1)
              break;
            this.isparameterizied = true;
            break;
          case System.Runtime.InteropServices.ComTypes.INVOKEKIND.INVOKE_PROPERTYPUTREF:
            this.setterByRefIndex = index;
            this.hasSetterByRef = true;
            if (desc.cParams <= (short) 1)
              break;
            this.isparameterizied = true;
            break;
        }
      }
    }

    internal string GetDefinition()
    {
      IntPtr ppFuncDesc = IntPtr.Zero;
      try
      {
        this.typeInfo.GetFuncDesc(this.GetFuncDescIndex(), out ppFuncDesc);
        return ComUtil.GetMethodSignatureFromFuncDesc(this.typeInfo, (System.Runtime.InteropServices.ComTypes.FUNCDESC) Marshal.PtrToStructure(ppFuncDesc, typeof (System.Runtime.InteropServices.ComTypes.FUNCDESC)), !this.hasGetter);
      }
      finally
      {
        if (ppFuncDesc != IntPtr.Zero)
          this.typeInfo.ReleaseFuncDesc(ppFuncDesc);
      }
    }

    public override string ToString()
    {
      using (ComProperty.tracer.TraceMethod())
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(this.GetDefinition());
        stringBuilder.Append(" ");
        if (this.hasGetter)
          stringBuilder.Append("{get} ");
        if (this.hasSetter)
          stringBuilder.Append("{set} ");
        if (this.hasSetterByRef)
          stringBuilder.Append("{set by ref}");
        return stringBuilder.ToString();
      }
    }
  }
}
