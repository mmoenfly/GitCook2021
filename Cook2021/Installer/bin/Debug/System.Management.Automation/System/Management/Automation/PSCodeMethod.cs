// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSCodeMethod
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace System.Management.Automation
{
  public class PSCodeMethod : PSMethodInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    private MethodInfo[] codeReference;
    private string[] codeReferenceDefinition;
    private MethodInformation[] codeReferenceMethodInformation;

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string overloadDefinition in this.OverloadDefinitions)
      {
        stringBuilder.Append(overloadDefinition);
        stringBuilder.Append(", ");
      }
      stringBuilder.Remove(stringBuilder.Length - 2, 2);
      return stringBuilder.ToString();
    }

    private static void CheckMethodInfo(MethodInfo method)
    {
      ParameterInfo[] parameters = method.GetParameters();
      if (!method.IsStatic || !method.IsPublic || (parameters.Length == 0 || !parameters[0].ParameterType.Equals(typeof (PSObject))))
        throw new ExtendedTypeSystemException("WrongMethodFormat", (Exception) null, "ExtendedTypeSystem", "CodeMethodMethodFormat", new object[0]);
    }

    internal void SetCodeReference(Type type, string methodName)
    {
      MemberInfo[] member = type.GetMember(methodName, MemberTypes.Method, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public);
      this.codeReference = member.Length == 1 ? new MethodInfo[1]
      {
        (MethodInfo) member[0]
      } : throw new ExtendedTypeSystemException("WrongMethodFormatFromTypeTable", (Exception) null, "ExtendedTypeSystem", "CodeMethodMethodFormat", new object[0]);
      this.codeReferenceDefinition = new string[1]
      {
        DotNetAdapter.GetMethodInfoOverloadDefinition((string) null, (MethodBase) this.codeReference[0], 0)
      };
      this.codeReferenceMethodInformation = DotNetAdapter.GetMethodInformationArray((MethodBase[]) this.codeReference);
      PSCodeMethod.CheckMethodInfo(this.codeReference[0]);
    }

    internal PSCodeMethod(string name) => this.name = !string.IsNullOrEmpty(name) ? name : throw PSCodeMethod.tracer.NewArgumentException(nameof (name));

    public PSCodeMethod(string name, MethodInfo codeReference)
    {
      if (string.IsNullOrEmpty(name))
        throw PSCodeMethod.tracer.NewArgumentException(nameof (name));
      if (codeReference == null)
        throw PSCodeMethod.tracer.NewArgumentNullException(nameof (codeReference));
      PSCodeMethod.CheckMethodInfo(codeReference);
      this.name = name;
      this.codeReference = new MethodInfo[1]
      {
        codeReference
      };
      this.codeReferenceDefinition = new string[1]
      {
        DotNetAdapter.GetMethodInfoOverloadDefinition((string) null, (MethodBase) this.codeReference[0], 0)
      };
      this.codeReferenceMethodInformation = DotNetAdapter.GetMethodInformationArray((MethodBase[]) this.codeReference);
    }

    public MethodInfo CodeReference => this.codeReference[0];

    public override PSMemberInfo Copy()
    {
      PSCodeMethod psCodeMethod = new PSCodeMethod(this.name, this.codeReference[0]);
      this.CloneBaseProperties((PSMemberInfo) psCodeMethod);
      return (PSMemberInfo) psCodeMethod;
    }

    public override PSMemberTypes MemberType => PSMemberTypes.CodeMethod;

    public override object Invoke(params object[] arguments)
    {
      object[] objArray = arguments != null ? new object[arguments.Length + 1] : throw PSCodeMethod.tracer.NewArgumentNullException(nameof (arguments));
      objArray[0] = (object) this.instance;
      for (int index = 0; index < arguments.Length; ++index)
        objArray[index + 1] = arguments[index];
      object[] newArguments;
      Adapter.GetBestMethodAndArguments(this.codeReference[0].Name, this.codeReferenceMethodInformation, objArray, out newArguments);
      return DotNetAdapter.AuxiliaryMethodInvoke((object) null, newArguments, this.codeReferenceMethodInformation[0], objArray);
    }

    public override Collection<string> OverloadDefinitions => new Collection<string>()
    {
      this.codeReferenceDefinition[0]
    };

    public override string TypeNameOfValue => typeof (PSCodeMethod).FullName;
  }
}
