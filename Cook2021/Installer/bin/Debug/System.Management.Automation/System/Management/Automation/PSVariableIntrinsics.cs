// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSVariableIntrinsics
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public sealed class PSVariableIntrinsics
  {
    [TraceSource("PSVariableCommandAPI", "The APIs that are exposed to the Cmdlet base class for manipulating location in session state")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PSVariableCommandAPI", "The APIs that are exposed to the Cmdlet base class for manipulating location in session state");
    private SessionStateInternal sessionState;

    private PSVariableIntrinsics()
    {
    }

    internal PSVariableIntrinsics(SessionStateInternal sessionState)
    {
      using (PSVariableIntrinsics.tracer.TraceConstructor((object) this))
        this.sessionState = sessionState != null ? sessionState : throw PSVariableIntrinsics.tracer.NewArgumentException(nameof (sessionState));
    }

    public PSVariable Get(string name) => this.sessionState.GetVariable(name);

    internal PSVariable GetAtScope(string name, string scope) => this.sessionState.GetVariableAtScope(name, scope);

    public object GetValue(string name) => this.sessionState.GetVariableValue(name);

    public object GetValue(string name, object defaultValue) => this.sessionState.GetVariableValue(name, defaultValue);

    internal object GetValueAtScope(string name, string scope) => this.sessionState.GetVariableValueAtScope(name, scope);

    public void Set(string name, object value) => this.sessionState.SetVariableValue(name, value, CommandOrigin.Internal);

    public void Set(PSVariable variable) => this.sessionState.SetVariable(variable, false, CommandOrigin.Internal);

    internal void SetAtScope(string name, object value, string scope) => this.sessionState.SetVariableAtScope(name, value, scope, false, CommandOrigin.Internal);

    internal void SetAtScope(PSVariable variable, string scope) => this.sessionState.SetVariableAtScope(variable, scope, false, CommandOrigin.Internal);

    public void Remove(string name) => this.sessionState.RemoveVariable(name);

    public void Remove(PSVariable variable) => this.sessionState.RemoveVariable(variable);

    internal void RemoveAtScope(string name, string scope) => this.sessionState.RemoveVariableAtScope(name, scope);

    internal void RemoveAtScope(PSVariable variable, string scope) => this.sessionState.RemoveVariableAtScope(variable, scope);
  }
}
