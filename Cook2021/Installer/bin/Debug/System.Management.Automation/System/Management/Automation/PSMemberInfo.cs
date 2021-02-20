// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSMemberInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public abstract class PSMemberInfo
  {
    [TraceSource("ETS", "Extended Type System")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");
    internal PSObject instance;
    internal bool isHidden;
    internal bool isReservedMember;
    internal bool isInstance = true;
    internal string name;
    internal bool shouldSerialize = true;

    internal bool ShouldSerialize
    {
      get => this.shouldSerialize;
      set => this.shouldSerialize = value;
    }

    internal void ReplicateInstance(PSObject particularInstance)
    {
      this.instance = particularInstance;
      if (!(this is PSMemberSet psMemberSet))
        return;
      foreach (PSMemberInfo member in psMemberSet.Members)
        member.ReplicateInstance(particularInstance);
    }

    internal void SetValueNoConversion(object setValue)
    {
      if (!(this is PSProperty psProperty))
        this.Value = setValue;
      else
        psProperty.SetAdaptedValue(setValue, false);
    }

    internal void CloneBaseProperties(PSMemberInfo destiny)
    {
      destiny.name = this.name;
      destiny.isHidden = this.isHidden;
      destiny.isReservedMember = this.isReservedMember;
      destiny.isInstance = this.isInstance;
      destiny.instance = this.instance;
      destiny.shouldSerialize = this.shouldSerialize;
    }

    public abstract PSMemberTypes MemberType { get; }

    public string Name => this.name;

    protected void SetMemberName(string name) => this.name = !string.IsNullOrEmpty(name) ? name : throw PSMemberInfo.tracer.NewArgumentException(nameof (name));

    internal bool IsReservedMember => this.isReservedMember;

    internal bool IsHidden => this.isHidden;

    public bool IsInstance => this.isInstance;

    public abstract object Value { get; set; }

    public abstract string TypeNameOfValue { get; }

    public abstract PSMemberInfo Copy();

    internal bool MatchesOptions(MshMemberMatchOptions options) => (!this.IsHidden || (options & MshMemberMatchOptions.IncludeHidden) != MshMemberMatchOptions.None) && (this.ShouldSerialize || (options & MshMemberMatchOptions.OnlySerializable) == MshMemberMatchOptions.None);
  }
}
