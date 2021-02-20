// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ComMethod
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Internal;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace System.Management.Automation
{
  internal class ComMethod
  {
    [TraceSource("COM", "Tracing for COM interop calls")]
    protected static PSTraceSource tracer = PSTraceSource.GetTracer("COM", "Tracing for COM interop calls");
    private Collection<int> methods = new Collection<int>();
    private ITypeInfo typeInfo;
    private string name;

    internal ComMethod(ITypeInfo typeinfo, string name)
    {
      this.typeInfo = typeinfo;
      this.name = name;
    }

    internal string Name => this.name;

    internal void AddFuncDesc(int index) => this.methods.Add(index);

    internal Collection<string> MethodDefinitions()
    {
      Collection<string> collection = new Collection<string>();
      foreach (int method in this.methods)
      {
        IntPtr ppFuncDesc;
        this.typeInfo.GetFuncDesc(method, out ppFuncDesc);
        string signatureFromFuncDesc = ComUtil.GetMethodSignatureFromFuncDesc(this.typeInfo, (System.Runtime.InteropServices.ComTypes.FUNCDESC) Marshal.PtrToStructure(ppFuncDesc, typeof (System.Runtime.InteropServices.ComTypes.FUNCDESC)), false);
        collection.Add(signatureFromFuncDesc);
        this.typeInfo.ReleaseFuncDesc(ppFuncDesc);
      }
      return collection;
    }

    internal object InvokeMethod(PSMethod method, object[] arguments)
    {
      Type type = method.baseObject.GetType();
      BindingFlags invokeAttr = BindingFlags.IgnoreCase | BindingFlags.InvokeMethod;
      try
      {
        object[] newArguments;
        ComMethodInformation methodAndArguments = (ComMethodInformation) Adapter.GetBestMethodAndArguments(this.Name, (MethodInformation[]) ComUtil.GetMethodInformationArray(this.typeInfo, this.methods, false), arguments, out newArguments);
        object obj = type.InvokeMember(this.Name, invokeAttr, (Binder) null, method.baseObject, newArguments, ComUtil.GetModifiers(methodAndArguments.parameters), CultureInfo.CurrentCulture, (string[]) null);
        Adapter.SetReferences(newArguments, (MethodInformation) methodAndArguments, arguments);
        return methodAndArguments.ReturnType != typeof (void) ? obj : (object) AutomationNull.Value;
      }
      catch (TargetInvocationException ex)
      {
        CommandProcessorBase.CheckForSevereException(ex.InnerException);
        if (ex.InnerException is COMException innerException)
        {
          if (innerException.ErrorCode == -2147352573)
            goto label_9;
        }
        string str = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
        throw new MethodInvocationException("ComMethodTargetInvocation", (Exception) ex, "ExtendedTypeSystem", "MethodInvocationException", new object[3]
        {
          (object) method.Name,
          (object) arguments.Length,
          (object) str
        });
      }
      catch (COMException ex)
      {
        if (ex.ErrorCode != -2147352570)
          throw new MethodInvocationException("ComMethodCOMException", (Exception) ex, "ExtendedTypeSystem", "MethodInvocationException", new object[3]
          {
            (object) method.Name,
            (object) arguments.Length,
            (object) ex.Message
          });
      }
label_9:
      return (object) null;
    }
  }
}
