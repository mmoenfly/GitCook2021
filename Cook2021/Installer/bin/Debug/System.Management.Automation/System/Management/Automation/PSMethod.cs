// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSMethod
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public class PSMethod : PSMethodInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    internal object adapterData;
    private Adapter adapter;
    internal object baseObject;
    private bool isSpecial;

    public override string ToString() => this.adapter.BaseMethodToString(this);

    internal PSMethod(string name, Adapter adapter, object baseObject, object adapterData)
    {
      this.name = !string.IsNullOrEmpty(name) ? name : throw PSMethod.tracer.NewArgumentException(nameof (name));
      this.adapterData = adapterData;
      this.adapter = adapter;
      this.baseObject = baseObject;
    }

    internal PSMethod(
      string name,
      Adapter adapter,
      object baseObject,
      object adapterData,
      bool isSpecial)
      : this(name, adapter, baseObject, adapterData)
    {
      this.isSpecial = isSpecial;
    }

    public override PSMemberInfo Copy()
    {
      PSMethod psMethod = new PSMethod(this.name, this.adapter, this.baseObject, this.adapterData, this.isSpecial);
      this.CloneBaseProperties((PSMemberInfo) psMethod);
      return (PSMemberInfo) psMethod;
    }

    public override PSMemberTypes MemberType => PSMemberTypes.Method;

    public override object Invoke(params object[] arguments) => arguments != null ? this.adapter.BaseMethodInvoke(this, arguments) : throw PSMethod.tracer.NewArgumentNullException(nameof (arguments));

    public override Collection<string> OverloadDefinitions => this.adapter.BaseMethodDefinitions(this);

    public override string TypeNameOfValue => typeof (PSMethod).FullName;

    internal bool IsSpecial => this.isSpecial;
  }
}
