// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandBreakpoint
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Reflection;

namespace System.Management.Automation
{
  public class CommandBreakpoint : Breakpoint
  {
    private WildcardPattern _command;
    private string _commandString;

    internal CommandBreakpoint(
      ExecutionContext context,
      string script,
      WildcardPattern command,
      string commandString,
      ScriptBlock action)
      : base(context, script, action, Breakpoint.BreakpointType.Command)
    {
      this._command = command;
      this._commandString = commandString;
    }

    public string Command => this._commandString;

    internal WildcardPattern CommandPattern => this._command;

    public override string ToString()
    {
      Assembly assembly = Assembly.GetAssembly(this.GetType());
      return this.IsScriptBreakpoint ? ResourceManagerCache.FormatResourceString(assembly, "DebuggerStrings", "CommandScriptBreakpointString", (object) this.Script, (object) this.Command) : ResourceManagerCache.FormatResourceString(assembly, "DebuggerStrings", "CommandBreakpointString", (object) this.Command);
    }
  }
}
