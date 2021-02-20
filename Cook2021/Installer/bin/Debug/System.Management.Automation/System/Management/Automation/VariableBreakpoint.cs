// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.VariableBreakpoint
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Reflection;

namespace System.Management.Automation
{
  public class VariableBreakpoint : Breakpoint
  {
    private VariableAccessMode _accessMode;
    private string _variable;

    internal VariableBreakpoint(
      ExecutionContext context,
      string script,
      string variable,
      VariableAccessMode accessMode,
      ScriptBlock action)
      : base(context, script, action, Breakpoint.BreakpointType.Variable)
    {
      this._variable = variable;
      this._accessMode = accessMode;
    }

    public VariableAccessMode AccessMode => this._accessMode;

    public string Variable => this._variable;

    public override string ToString()
    {
      Assembly assembly = Assembly.GetAssembly(this.GetType());
      return this.IsScriptBreakpoint ? ResourceManagerCache.FormatResourceString(assembly, "DebuggerStrings", "VariableScriptBreakpointString", (object) this.Script, (object) this.Variable, (object) this.AccessMode) : ResourceManagerCache.FormatResourceString(assembly, "DebuggerStrings", "VariableBreakpointString", (object) this.Variable, (object) this.AccessMode);
    }
  }
}
