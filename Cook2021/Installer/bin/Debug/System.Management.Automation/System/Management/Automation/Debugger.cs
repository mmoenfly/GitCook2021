// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Debugger
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  public sealed class Debugger
  {
    private bool _inBreakpoint;
    private ExecutionContext _context;
    private Stack<Debugger.ScriptBlockInfo> _runningScriptStack;
    private Dictionary<string, ScriptBreakpointInfo> _scriptPathToBreakpointInfo;
    private Dictionary<string, ScriptDebugInfo> _scriptPathToDebugInfo;
    private Dictionary<int, Breakpoint> _idToBreakpoint;
    private Stack<ParseTreeNode> _statementStack;
    private ScriptBreakpointInfo _globalScriptBreakpointInfo;
    private ScriptDebugInfo _currentScriptDebugInfo;
    private Debugger.SteppingMode _steppingMode;
    private int _stepCallDepth;
    private Stack<Debugger.CallStackInfo> _callStack;
    private bool _evaluatingBreakpointAction;

    internal Debugger(ExecutionContext context)
    {
      this._context = context;
      this._runningScriptStack = new Stack<Debugger.ScriptBlockInfo>();
      this._scriptPathToBreakpointInfo = new Dictionary<string, ScriptBreakpointInfo>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      this._scriptPathToDebugInfo = new Dictionary<string, ScriptDebugInfo>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      this._statementStack = new Stack<ParseTreeNode>();
      this._globalScriptBreakpointInfo = new ScriptBreakpointInfo(context, (string) null);
      this._currentScriptDebugInfo = (ScriptDebugInfo) null;
      this._inBreakpoint = false;
      this._idToBreakpoint = new Dictionary<int, Breakpoint>();
      this._steppingMode = Debugger.SteppingMode.None;
      this._callStack = new Stack<Debugger.CallStackInfo>();
      this._evaluatingBreakpointAction = false;
    }

    public event EventHandler<DebuggerStopEventArgs> DebuggerStop;

    public event EventHandler<BreakpointUpdatedEventArgs> BreakpointUpdated;

    internal bool IsOn => this._idToBreakpoint.Count > 0 && !this._inBreakpoint && !this._evaluatingBreakpointAction;

    internal bool InBreakpoint => this._inBreakpoint;

    private ScriptBreakpointInfo GetScriptBreakpointInfo(string path)
    {
      ScriptBreakpointInfo scriptBreakpointInfo;
      if (!this._scriptPathToBreakpointInfo.TryGetValue(path, out scriptBreakpointInfo))
      {
        scriptBreakpointInfo = new ScriptBreakpointInfo(this._context, path);
        this._scriptPathToBreakpointInfo[path] = scriptBreakpointInfo;
      }
      return scriptBreakpointInfo;
    }

    internal void PopRunning()
    {
      Debugger.ScriptBlockInfo scriptBlockInfo1 = this._runningScriptStack.Pop();
      if (scriptBlockInfo1.DebugInfo != null)
      {
        --scriptBlockInfo1.DebugInfo.NumReferences;
        if (scriptBlockInfo1.DebugInfo.NumReferences == 0)
        {
          if (scriptBlockInfo1.DebugInfo.FromScript)
            this._scriptPathToDebugInfo.Remove(scriptBlockInfo1.DebugInfo.BreakpointInfo.ScriptPath);
          if (scriptBlockInfo1.DebugInfo.BreakpointInfo.NumBreakpoints == 0)
            this._scriptPathToBreakpointInfo.Remove(scriptBlockInfo1.DebugInfo.BreakpointInfo.ScriptPath);
        }
      }
      if (!this.IsOn)
        return;
      if (this._runningScriptStack.Count > 0)
      {
        Debugger.ScriptBlockInfo scriptBlockInfo2 = this._runningScriptStack.Peek();
        if (scriptBlockInfo2.DebugInfo != null && scriptBlockInfo2.DebugInfo.IsRecursive)
          scriptBlockInfo2.DebugInfo.SetBreakpoints(scriptBlockInfo2.ScriptBlock);
        this._currentScriptDebugInfo = scriptBlockInfo2.DebugInfo;
      }
      else
      {
        this._currentScriptDebugInfo = (ScriptDebugInfo) null;
        this._steppingMode = Debugger.SteppingMode.None;
      }
    }

    internal void PushRunning(string path, ScriptBlock running, bool fromScript)
    {
      this._runningScriptStack.Push(new Debugger.ScriptBlockInfo(running));
      if (!this.IsOn)
        return;
      ScriptDebugInfo scriptDebugInfo = (ScriptDebugInfo) null;
      if (this._scriptPathToDebugInfo.TryGetValue(path, out scriptDebugInfo))
      {
        ++scriptDebugInfo.NumReferences;
        if (fromScript)
        {
          scriptDebugInfo.IsRecursive = true;
          scriptDebugInfo.SetBreakpoints(running);
        }
      }
      else
      {
        scriptDebugInfo = new ScriptDebugInfo(this.GetScriptBreakpointInfo(path), running, fromScript);
        if (fromScript)
          this._scriptPathToDebugInfo.Add(path, scriptDebugInfo);
      }
      this._runningScriptStack.Peek().DebugInfo = this._currentScriptDebugInfo = scriptDebugInfo;
    }

    internal void PushMethodCall(InvocationInfo invocationInfo, ScriptBlock scriptBlock)
    {
      this._callStack.Push(new Debugger.CallStackInfo()
      {
        InvocationInfo = invocationInfo,
        ScriptBlock = scriptBlock
      });
      if (scriptBlock.IsScriptBlockForExceptionHandler && this._steppingMode == Debugger.SteppingMode.StepOver)
        this._steppingMode = Debugger.SteppingMode.StepIn;
      if (invocationInfo.InvocationName == "")
        this._steppingMode = Debugger.SteppingMode.None;
      if (this._steppingMode == Debugger.SteppingMode.None || this._steppingMode != Debugger.SteppingMode.StepOut && this._steppingMode != Debugger.SteppingMode.StepOver)
        return;
      ++this._stepCallDepth;
    }

    internal void PushStatement(ParseTreeNode statement)
    {
      this._statementStack.Push(statement);
      if (this._steppingMode == Debugger.SteppingMode.None || statement.SkipDebuggerStep)
        return;
      InvocationInfo invocationInfo = new InvocationInfo((CommandInfo) null, statement.NodeToken);
      if (this._steppingMode == Debugger.SteppingMode.StepIn)
      {
        this.OnDebuggerStop(invocationInfo, new List<Breakpoint>());
      }
      else
      {
        if (this._steppingMode != Debugger.SteppingMode.StepOver && this._steppingMode != Debugger.SteppingMode.StepOut || this._stepCallDepth > 0)
          return;
        this.OnDebuggerStop(invocationInfo, new List<Breakpoint>());
      }
    }

    internal void PopMethodCall()
    {
      if (this._callStack.Pop().InvocationInfo.InvocationName == "" || this._steppingMode == Debugger.SteppingMode.None || this._steppingMode == Debugger.SteppingMode.StepIn || this._steppingMode != Debugger.SteppingMode.StepOut && this._steppingMode != Debugger.SteppingMode.StepOver)
        return;
      --this._stepCallDepth;
    }

    internal void PopStatement() => this._statementStack.Pop();

    internal Breakpoint NewCommandBreakpoint(
      string path,
      string command,
      ScriptBlock action)
    {
      return this.NewCommandBreakpoint(this.GetScriptBreakpointInfo(path), command, action);
    }

    internal Breakpoint NewCommandBreakpoint(string command, ScriptBlock action) => this.NewCommandBreakpoint(this._globalScriptBreakpointInfo, command, action);

    private Breakpoint NewCommandBreakpoint(
      ScriptBreakpointInfo breakpointInfo,
      string command,
      ScriptBlock action)
    {
      Breakpoint breakpoint = breakpointInfo.NewCommandBreakpoint(command, action);
      this._idToBreakpoint[breakpoint.Id] = breakpoint;
      this.OnBreakpointUpdated(new BreakpointUpdatedEventArgs(breakpoint, BreakpointUpdateType.Set));
      return breakpoint;
    }

    internal Breakpoint NewLineBreakpoint(string path, int line, ScriptBlock action)
    {
      Breakpoint breakpoint = this.GetScriptBreakpointInfo(path).NewLineBreakpoint(path, line, action);
      this._idToBreakpoint[breakpoint.Id] = breakpoint;
      this.OnBreakpointUpdated(new BreakpointUpdatedEventArgs(breakpoint, BreakpointUpdateType.Set));
      return breakpoint;
    }

    internal Breakpoint NewVariableBreakpoint(
      string path,
      string variableName,
      VariableAccessMode accessMode,
      ScriptBlock action)
    {
      return this.NewVariableBreakpoint(this.GetScriptBreakpointInfo(path), variableName, accessMode, action);
    }

    internal Breakpoint NewVariableBreakpoint(
      string variableName,
      VariableAccessMode accessMode,
      ScriptBlock action)
    {
      return this.NewVariableBreakpoint(this._globalScriptBreakpointInfo, variableName, accessMode, action);
    }

    private Breakpoint NewVariableBreakpoint(
      ScriptBreakpointInfo breakpointInfo,
      string variableName,
      VariableAccessMode accessMode,
      ScriptBlock action)
    {
      Breakpoint breakpoint = breakpointInfo.NewVariableBreakpoint(variableName, accessMode, action);
      this._idToBreakpoint[breakpoint.Id] = breakpoint;
      this.OnBreakpointUpdated(new BreakpointUpdatedEventArgs(breakpoint, BreakpointUpdateType.Set));
      return breakpoint;
    }

    internal Breakpoint NewStatementBreakpoint(
      string path,
      int line,
      int column,
      ScriptBlock action)
    {
      Breakpoint breakpoint = this.GetScriptBreakpointInfo(path).NewStatementBreakpoint(path, line, column, action);
      this._idToBreakpoint[breakpoint.Id] = breakpoint;
      this.OnBreakpointUpdated(new BreakpointUpdatedEventArgs(breakpoint, BreakpointUpdateType.Set));
      return breakpoint;
    }

    internal void AddBreakpointToRunningScripts(LineBreakpoint breakpoint)
    {
      foreach (Debugger.ScriptBlockInfo runningScript in this._runningScriptStack)
      {
        if (runningScript.DebugInfo != null && breakpoint.Script.Equals(runningScript.ScriptBlock.File, StringComparison.OrdinalIgnoreCase))
        {
          if (breakpoint.Type == Breakpoint.BreakpointType.Line)
            runningScript.DebugInfo.AddLineBreakpoint(breakpoint);
          else
            runningScript.DebugInfo.AddStatementBreakpoint(breakpoint);
          if (runningScript.DebugInfo.FromScript)
            break;
        }
      }
    }

    private void OnBreakpointUpdated(BreakpointUpdatedEventArgs e)
    {
      EventHandler<BreakpointUpdatedEventArgs> breakpointUpdated = this.BreakpointUpdated;
      if (breakpointUpdated == null)
        return;
      breakpointUpdated((object) this, e);
    }

    internal void RemoveBreakpoint(Breakpoint breakpoint)
    {
      if (this._idToBreakpoint.ContainsKey(breakpoint.Id))
        this._idToBreakpoint.Remove(breakpoint.Id);
      ScriptBreakpointInfo scriptBreakpointInfo = (ScriptBreakpointInfo) null;
      if (breakpoint.IsScriptBreakpoint)
      {
        if (!this._scriptPathToBreakpointInfo.TryGetValue(breakpoint.Script, out scriptBreakpointInfo))
          return;
      }
      else
        scriptBreakpointInfo = this._globalScriptBreakpointInfo;
      switch (breakpoint.Type)
      {
        case Breakpoint.BreakpointType.Line:
        case Breakpoint.BreakpointType.Statement:
          LineBreakpoint breakpoint1 = (LineBreakpoint) breakpoint;
          scriptBreakpointInfo.Remove(breakpoint1);
          this.RemoveBreakpointFromRunningScripts(breakpoint1);
          break;
        case Breakpoint.BreakpointType.Variable:
          scriptBreakpointInfo.Remove((VariableBreakpoint) breakpoint);
          break;
        case Breakpoint.BreakpointType.Command:
          scriptBreakpointInfo.Remove((CommandBreakpoint) breakpoint);
          break;
      }
      if (scriptBreakpointInfo.NumBreakpoints == 0 && breakpoint.IsScriptBreakpoint)
        this._scriptPathToBreakpointInfo.Remove(breakpoint.Script);
      this.OnBreakpointUpdated(new BreakpointUpdatedEventArgs(breakpoint, BreakpointUpdateType.Removed));
    }

    private void RemoveBreakpointFromRunningScripts(LineBreakpoint breakpoint)
    {
      foreach (Debugger.ScriptBlockInfo runningScript in this._runningScriptStack)
      {
        if (runningScript.DebugInfo != null && breakpoint.Script.Equals(runningScript.ScriptBlock.File, StringComparison.OrdinalIgnoreCase))
        {
          runningScript.DebugInfo.RemoveBreakpoint(breakpoint);
          if (runningScript.DebugInfo.FromScript)
            break;
        }
      }
    }

    internal void CheckCommand(InvocationInfo invocationInfo)
    {
      List<Breakpoint> breakpoints = (List<Breakpoint>) null;
      this._globalScriptBreakpointInfo.CheckCommand(invocationInfo, ref breakpoints);
      if (this._currentScriptDebugInfo != null)
        this._currentScriptDebugInfo.BreakpointInfo.CheckCommand(invocationInfo, ref breakpoints);
      if (breakpoints == null)
        return;
      this.TriggerBreakpoints(breakpoints, invocationInfo);
    }

    internal void CheckVariableRead(string variableName)
    {
      List<Breakpoint> breakpoints = (List<Breakpoint>) null;
      this._globalScriptBreakpointInfo.CheckVariableRead(variableName, ref breakpoints);
      if (this._currentScriptDebugInfo != null)
        this._currentScriptDebugInfo.BreakpointInfo.CheckVariableRead(variableName, ref breakpoints);
      if (breakpoints == null)
        return;
      this.TriggerVariableBreakpoints(breakpoints);
    }

    internal void CheckVariableWrite(string variableName)
    {
      List<Breakpoint> breakpoints = (List<Breakpoint>) null;
      this._globalScriptBreakpointInfo.CheckVariableWrite(variableName, ref breakpoints);
      if (this._currentScriptDebugInfo != null)
        this._currentScriptDebugInfo.BreakpointInfo.CheckVariableWrite(variableName, ref breakpoints);
      if (breakpoints == null)
        return;
      this.TriggerVariableBreakpoints(breakpoints);
    }

    internal void TriggerVariableBreakpoints(List<Breakpoint> breakpoints) => this.TriggerBreakpoints(breakpoints, this._statementStack.Count == 0 ? (InvocationInfo) null : new InvocationInfo((CommandInfo) null, this._statementStack.Peek().NodeToken));

    internal void CheckForBreakpoints(ParseTreeNode node)
    {
      if (this._currentScriptDebugInfo == null)
        return;
      List<Breakpoint> treeNode = this._currentScriptDebugInfo.CheckParseTreeNode(node);
      if (treeNode == null)
        return;
      this.TriggerBreakpoints(treeNode, new InvocationInfo((CommandInfo) null, node.NodeToken));
    }

    internal Breakpoint GetBreakpoint(int id)
    {
      Breakpoint breakpoint = (Breakpoint) null;
      this._idToBreakpoint.TryGetValue(id, out breakpoint);
      return breakpoint;
    }

    internal List<Breakpoint> GetBreakpoints()
    {
      List<Breakpoint> breakpointList = new List<Breakpoint>();
      foreach (ScriptBreakpointInfo scriptBreakpointInfo in this._scriptPathToBreakpointInfo.Values)
        breakpointList.AddRange((IEnumerable<Breakpoint>) scriptBreakpointInfo.GetBreakpoints());
      breakpointList.AddRange((IEnumerable<Breakpoint>) this._globalScriptBreakpointInfo.GetBreakpoints());
      return breakpointList;
    }

    private void TriggerBreakpoints(List<Breakpoint> breakpoints, InvocationInfo invocationInfo)
    {
      List<Breakpoint> breakpoints1 = new List<Breakpoint>();
      this._evaluatingBreakpointAction = true;
      try
      {
        foreach (Breakpoint breakpoint in breakpoints)
        {
          if (breakpoint.Enabled && breakpoint.Trigger() == Breakpoint.BreakpointAction.Break)
            breakpoints1.Add(breakpoint);
        }
      }
      finally
      {
        this._evaluatingBreakpointAction = false;
      }
      if (breakpoints1.Count <= 0)
        return;
      this.OnDebuggerStop(invocationInfo, breakpoints1);
    }

    internal void EnableBreakpoint(Breakpoint bp)
    {
      bp.EnabledInternal = true;
      this.OnBreakpointUpdated(new BreakpointUpdatedEventArgs(bp, BreakpointUpdateType.Enabled));
    }

    internal void DisableBreakpoint(Breakpoint bp)
    {
      bp.EnabledInternal = false;
      this.OnBreakpointUpdated(new BreakpointUpdatedEventArgs(bp, BreakpointUpdateType.Disabled));
    }

    private void SetStep(string name, Debugger.SteppingMode mode, int stepCallDepth)
    {
      this._steppingMode = mode;
      this._stepCallDepth = stepCallDepth;
    }

    private void OnDebuggerStop(InvocationInfo invocationInfo, List<Breakpoint> breakpoints)
    {
      LocalRunspace currentRunspace = this._context.CurrentRunspace as LocalRunspace;
      if (currentRunspace.PulsePipeline != null && currentRunspace.PulsePipeline == currentRunspace.GetCurrentlyRunningPipeline())
      {
        if (breakpoints.Count > 0)
          this._context.EngineHostInterface.UI.WriteWarningLine(ResourceManagerCache.FormatResourceString("DebuggerStrings", "WarningBreakpointWillNotBeHit", (object) breakpoints[0]));
        else
          this._context.EngineHostInterface.UI.WriteWarningLine(new InvalidOperationException().Message);
      }
      else
      {
        this._steppingMode = Debugger.SteppingMode.None;
        EventHandler<DebuggerStopEventArgs> debuggerStop = this.DebuggerStop;
        if (debuggerStop == null)
          return;
        this._inBreakpoint = true;
        if (invocationInfo != null)
          this._callStack.Push(new Debugger.CallStackInfo()
          {
            InvocationInfo = invocationInfo
          });
        this._context.SetVariable("PSDebugContext", (object) new PSDebugContext(invocationInfo, breakpoints));
        PSLanguageMode languageMode = this._context.LanguageMode;
        bool flag = languageMode != PSLanguageMode.FullLanguage && this._context.UseFullLanguageModeInDebugger;
        if (flag)
          this._context.LanguageMode = PSLanguageMode.FullLanguage;
        RunspaceAvailability runspaceAvailability = this._context.CurrentRunspace.RunspaceAvailability;
        this._context.CurrentRunspace.UpdateRunspaceAvailability(this._context.CurrentRunspace.GetCurrentlyRunningPipeline() != null ? RunspaceAvailability.AvailableForNestedCommand : RunspaceAvailability.Available, true);
        try
        {
          DebuggerStopEventArgs e = new DebuggerStopEventArgs(invocationInfo, breakpoints);
          debuggerStop((object) this, e);
          this.ResumeExecution(e);
        }
        finally
        {
          this._context.CurrentRunspace.UpdateRunspaceAvailability(runspaceAvailability, true);
          if (flag)
            this._context.LanguageMode = languageMode;
          this._context.RemoveVariable("PSDebugContext");
          if (invocationInfo != null)
            this._callStack.Pop();
          this._inBreakpoint = false;
        }
      }
    }

    private void ResumeExecution(DebuggerStopEventArgs e)
    {
      switch (e.ResumeAction)
      {
        case DebuggerResumeAction.StepInto:
          this.SetStep("Step-Into", Debugger.SteppingMode.StepIn, -1);
          break;
        case DebuggerResumeAction.StepOut:
          this.SetStep("Step-Out", Debugger.SteppingMode.StepOut, 1);
          break;
        case DebuggerResumeAction.StepOver:
          this.SetStep("Step-Over", Debugger.SteppingMode.StepOver, 0);
          break;
        case DebuggerResumeAction.Stop:
          throw new TerminateException();
      }
    }

    internal List<CallStackFrame> GetCallStack(
      InvocationInfo getPsCallstackInvocationInfo)
    {
      List<CallStackFrame> callStackFrameList = new List<CallStackFrame>();
      if (this._callStack.Count > 0)
      {
        Debugger.CallStackInfo[] array = this._callStack.ToArray();
        InvocationInfo locationInfo = (InvocationInfo) null;
        int index1 = 0;
        if (Debugger.IsGetPsCallStackCmdlet(array[0].InvocationInfo))
          ++index1;
        else if (getPsCallstackInvocationInfo.ScriptLineNumber != 0)
          locationInfo = getPsCallstackInvocationInfo;
        if (locationInfo == null && index1 < array.Length && array[index1].InvocationInfo.ScriptLineNumber != 0)
          locationInfo = array[index1++].InvocationInfo;
        for (int index2 = index1; index2 < array.Length; ++index2)
        {
          if (array[index2].ScriptBlock == null || !array[index2].ScriptBlock.IsScriptBlockForExceptionHandler)
          {
            callStackFrameList.Add(new CallStackFrame(locationInfo, array[index2].InvocationInfo));
            locationInfo = array[index2].InvocationInfo;
          }
        }
      }
      return callStackFrameList;
    }

    private static bool IsGetPsCallStackCmdlet(InvocationInfo invocationInfo) => invocationInfo.ScriptLineNumber == 0 && invocationInfo.MyCommand != null && (invocationInfo.MyCommand.Context != null && invocationInfo.MyCommand.Context.CurrentCommandProcessor != null) && invocationInfo.MyCommand.Context.CurrentCommandProcessor.Command != null && invocationInfo.MyCommand.Context.CurrentCommandProcessor.Command.GetType().FullName == "Microsoft.PowerShell.Commands.GetPSCallStackCommand";

    internal static string FormatResourceString(string resourceId, params object[] args) => ResourceManagerCache.FormatResourceString("DebuggerStrings", resourceId, args);

    private class ScriptBlockInfo
    {
      public ScriptBlock ScriptBlock;
      public ScriptDebugInfo DebugInfo;

      public ScriptBlockInfo(ScriptBlock scriptBlock) => this.ScriptBlock = scriptBlock;
    }

    private struct CallStackInfo
    {
      public InvocationInfo InvocationInfo;
      public ScriptBlock ScriptBlock;
    }

    private enum SteppingMode
    {
      StepIn,
      StepOver,
      StepOut,
      None,
    }
  }
}
