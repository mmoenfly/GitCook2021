// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ScriptBreakpointInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  internal sealed class ScriptBreakpointInfo
  {
    private ExecutionContext _context;
    private string _scriptPath;
    private List<Breakpoint> _lineBreakpoints;
    private List<Breakpoint> _statementBreakpoints;
    private Dictionary<WildcardPattern, List<Breakpoint>> _commandBreakpoints;
    private Dictionary<string, List<Breakpoint>> _variableWriteBreakpoints;
    private Dictionary<string, List<Breakpoint>> _variableReadBreakpoints;
    private int _numBreakpoints;

    internal ScriptBreakpointInfo(ExecutionContext context, string scriptPath)
    {
      this._context = context;
      this._scriptPath = scriptPath;
      this._lineBreakpoints = new List<Breakpoint>();
      this._statementBreakpoints = new List<Breakpoint>();
      this._commandBreakpoints = new Dictionary<WildcardPattern, List<Breakpoint>>();
      this._variableWriteBreakpoints = new Dictionary<string, List<Breakpoint>>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      this._variableReadBreakpoints = new Dictionary<string, List<Breakpoint>>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      this._numBreakpoints = 0;
    }

    internal ExecutionContext Context => this._context;

    internal List<Breakpoint> LineBreakpoints => this._lineBreakpoints;

    internal List<Breakpoint> StatementBreakpoints => this._statementBreakpoints;

    internal string ScriptPath => this._scriptPath;

    internal int NumBreakpoints => this._numBreakpoints;

    private void AddBreakpoint<KeyType>(
      Dictionary<KeyType, List<Breakpoint>> breakpointDictionary,
      KeyType key,
      Breakpoint breakpoint)
    {
      List<Breakpoint> breakpointList = (List<Breakpoint>) null;
      breakpointDictionary.TryGetValue(key, out breakpointList);
      if (breakpointList == null)
      {
        breakpointList = new List<Breakpoint>();
        breakpointDictionary[key] = breakpointList;
      }
      breakpointList.Add(breakpoint);
    }

    internal Breakpoint NewCommandBreakpoint(string command, ScriptBlock action)
    {
      ++this._numBreakpoints;
      WildcardPattern wildcardPattern = new WildcardPattern(command, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
      CommandBreakpoint commandBreakpoint = new CommandBreakpoint(this._context, this._scriptPath, wildcardPattern, command, action);
      this.AddBreakpoint<WildcardPattern>(this._commandBreakpoints, wildcardPattern, (Breakpoint) commandBreakpoint);
      return (Breakpoint) commandBreakpoint;
    }

    internal Breakpoint NewLineBreakpoint(string path, int line, ScriptBlock action)
    {
      ++this._numBreakpoints;
      LineBreakpoint breakpoint = new LineBreakpoint(this._context, path, line, action);
      this._context.Debugger.AddBreakpointToRunningScripts(breakpoint);
      this._lineBreakpoints.Add((Breakpoint) breakpoint);
      return (Breakpoint) breakpoint;
    }

    internal Breakpoint NewStatementBreakpoint(
      string path,
      int line,
      int column,
      ScriptBlock action)
    {
      ++this._numBreakpoints;
      LineBreakpoint breakpoint = new LineBreakpoint(this._context, path, line, column, action);
      this._context.Debugger.AddBreakpointToRunningScripts(breakpoint);
      this._statementBreakpoints.Add((Breakpoint) breakpoint);
      return (Breakpoint) breakpoint;
    }

    internal Breakpoint NewVariableBreakpoint(
      string variableName,
      VariableAccessMode accessMode,
      ScriptBlock action)
    {
      ++this._numBreakpoints;
      VariableBreakpoint variableBreakpoint = new VariableBreakpoint(this._context, this._scriptPath, variableName, accessMode, action);
      if (accessMode == VariableAccessMode.Write || accessMode == VariableAccessMode.ReadWrite)
        this.AddBreakpoint<string>(this._variableWriteBreakpoints, variableName, (Breakpoint) variableBreakpoint);
      if (accessMode == VariableAccessMode.ReadWrite || accessMode == VariableAccessMode.Read)
        this.AddBreakpoint<string>(this._variableReadBreakpoints, variableName, (Breakpoint) variableBreakpoint);
      return (Breakpoint) variableBreakpoint;
    }

    private void RemoveDictionaryBreakpoint<KeyType>(
      Dictionary<KeyType, List<Breakpoint>> dictionary,
      KeyType key,
      Breakpoint breakpoint)
    {
      if (!dictionary.ContainsKey(key))
        return;
      List<Breakpoint> breakpointList = dictionary[key];
      if (breakpointList.Contains(breakpoint))
        --this._numBreakpoints;
      breakpointList.Remove(breakpoint);
      if (breakpointList.Count != 0)
        return;
      dictionary.Remove(key);
    }

    internal void Remove(CommandBreakpoint breakpoint) => this.RemoveDictionaryBreakpoint<WildcardPattern>(this._commandBreakpoints, breakpoint.CommandPattern, (Breakpoint) breakpoint);

    internal void Remove(LineBreakpoint breakpoint)
    {
      List<Breakpoint> breakpointList = breakpoint.Type != Breakpoint.BreakpointType.Line ? this._statementBreakpoints : this._lineBreakpoints;
      if (!breakpointList.Contains((Breakpoint) breakpoint))
        return;
      breakpointList.Remove((Breakpoint) breakpoint);
      --this._numBreakpoints;
    }

    internal void Remove(VariableBreakpoint breakpoint)
    {
      if (breakpoint.AccessMode == VariableAccessMode.Write || breakpoint.AccessMode == VariableAccessMode.ReadWrite)
        this.RemoveDictionaryBreakpoint<string>(this._variableWriteBreakpoints, breakpoint.Variable, (Breakpoint) breakpoint);
      if (breakpoint.AccessMode != VariableAccessMode.Read && breakpoint.AccessMode != VariableAccessMode.ReadWrite)
        return;
      this.RemoveDictionaryBreakpoint<string>(this._variableReadBreakpoints, breakpoint.Variable, (Breakpoint) breakpoint);
    }

    internal void CheckCommand(InvocationInfo invocationInfo, ref List<Breakpoint> breakpoints)
    {
      foreach (WildcardPattern key in this._commandBreakpoints.Keys)
      {
        if (key.IsMatch(invocationInfo.MyCommand.Name) || key.IsMatch(invocationInfo.InvocationName))
          this.Append(this._commandBreakpoints[key], ref breakpoints);
      }
    }

    internal void CheckVariableRead(string variableName, ref List<Breakpoint> breakpoints)
    {
      List<Breakpoint> source = (List<Breakpoint>) null;
      if (!this._variableReadBreakpoints.TryGetValue(variableName, out source))
        return;
      this.Append(source, ref breakpoints);
    }

    internal void CheckVariableWrite(string variableName, ref List<Breakpoint> breakpoints)
    {
      List<Breakpoint> source = (List<Breakpoint>) null;
      if (!this._variableWriteBreakpoints.TryGetValue(variableName, out source))
        return;
      this.Append(source, ref breakpoints);
    }

    private void Append(List<Breakpoint> source, ref List<Breakpoint> target)
    {
      if (target == null)
        target = new List<Breakpoint>();
      target.AddRange((IEnumerable<Breakpoint>) source);
    }

    internal List<Breakpoint> GetBreakpoints()
    {
      List<Breakpoint> breakpointList1 = new List<Breakpoint>();
      breakpointList1.AddRange((IEnumerable<Breakpoint>) this._lineBreakpoints);
      breakpointList1.AddRange((IEnumerable<Breakpoint>) this._statementBreakpoints);
      foreach (List<Breakpoint> breakpointList2 in this._commandBreakpoints.Values)
        breakpointList1.AddRange((IEnumerable<Breakpoint>) breakpointList2);
      foreach (List<Breakpoint> breakpointList2 in this._variableWriteBreakpoints.Values)
        breakpointList1.AddRange((IEnumerable<Breakpoint>) breakpointList2);
      foreach (List<Breakpoint> breakpointList2 in this._variableReadBreakpoints.Values)
      {
        foreach (Breakpoint breakpoint in breakpointList2)
        {
          if ((breakpoint as VariableBreakpoint).AccessMode != VariableAccessMode.ReadWrite)
            breakpointList1.Add(breakpoint);
        }
      }
      return breakpointList1;
    }
  }
}
