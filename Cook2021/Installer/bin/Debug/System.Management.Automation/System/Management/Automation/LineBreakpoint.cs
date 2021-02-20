// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.LineBreakpoint
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Reflection;

namespace System.Management.Automation
{
  public class LineBreakpoint : Breakpoint
  {
    private int _column;
    private int _line;

    internal LineBreakpoint(ExecutionContext context, string script, int line, ScriptBlock action)
      : base(context, script, action, Breakpoint.BreakpointType.Line)
    {
      this._line = line;
      this._column = 0;
    }

    internal LineBreakpoint(
      ExecutionContext context,
      string script,
      int line,
      int column,
      ScriptBlock action)
      : base(context, script, action, Breakpoint.BreakpointType.Statement)
    {
      this._line = line;
      this._column = column;
    }

    public int Column => this._column;

    public int Line => this._line;

    public override string ToString()
    {
      Assembly assembly = Assembly.GetAssembly(this.GetType());
      return this.Type == Breakpoint.BreakpointType.Line ? ResourceManagerCache.FormatResourceString(assembly, "DebuggerStrings", "LineBreakpointString", (object) this.Script, (object) this.Line) : ResourceManagerCache.FormatResourceString(assembly, "DebuggerStrings", "StatementBreakpointString", (object) this.Script, (object) this.Line, (object) this.Column);
    }
  }
}
