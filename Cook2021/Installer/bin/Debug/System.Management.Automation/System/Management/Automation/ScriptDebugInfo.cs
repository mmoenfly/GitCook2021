// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ScriptDebugInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Threading;

namespace System.Management.Automation
{
  internal sealed class ScriptDebugInfo
  {
    private ScriptBreakpointInfo _breakpointInfo;
    private int _numReferences;
    private bool _isRecursive;
    private bool _isAnalyzed;
    private ScriptBlock _script;
    private bool _fromScript;
    private ParseTreeNode[] _lineToPTN;
    private List<StatementListNode> _preorderStatements;
    private Dictionary<ParseTreeNode, List<Breakpoint>> _PTNBreakpoints;
    private Dictionary<Breakpoint, ParseTreeNode> _breakpointToPTN;

    internal ScriptDebugInfo(
      ScriptBreakpointInfo breakpointInfo,
      ScriptBlock script,
      bool fromScript)
    {
      this._breakpointInfo = breakpointInfo;
      this._numReferences = 1;
      this._isRecursive = false;
      this._fromScript = fromScript;
      this.SetBreakpoints(script);
    }

    internal ScriptBreakpointInfo BreakpointInfo => this._breakpointInfo;

    internal bool FromScript => this._fromScript;

    internal int NumReferences
    {
      get => this._numReferences;
      set => this._numReferences = value;
    }

    internal bool IsRecursive
    {
      get => this._isRecursive;
      set => this._isRecursive = value;
    }

    private bool IsBefore(ParseTreeNode node1, ParseTreeNode node2) => node1.NodeToken.Start < node2.NodeToken.Start;

    private List<ParseTreeNode> GetPreorderNodes()
    {
      List<ParseTreeNode> parseTreeNodeList1 = new List<ParseTreeNode>();
      if (this._script.Begin != null)
        parseTreeNodeList1.Add(this._script.Begin);
      if (this._script.Process != null)
        parseTreeNodeList1.Add(this._script.Process);
      if (this._script.End != null)
        parseTreeNodeList1.Add(this._script.End);
      for (int index1 = 0; index1 < parseTreeNodeList1.Count; ++index1)
      {
        for (int index2 = index1 + 1; index2 < parseTreeNodeList1.Count; ++index2)
        {
          if (this.IsBefore(parseTreeNodeList1[index1], parseTreeNodeList1[index2]))
          {
            ParseTreeNode parseTreeNode = parseTreeNodeList1[index1];
            parseTreeNodeList1[index1] = parseTreeNodeList1[index2];
            parseTreeNodeList1[index2] = parseTreeNode;
          }
        }
      }
      List<ParseTreeNode> parseTreeNodeList2 = new List<ParseTreeNode>();
      foreach (ParseTreeNode parseTreeNode in parseTreeNodeList1)
        parseTreeNodeList2.AddRange((IEnumerable<ParseTreeNode>) parseTreeNode.EnumeratePreorder());
      return parseTreeNodeList2;
    }

    private void AnalyzeScript()
    {
      List<ParseTreeNode> preorderNodes = this.GetPreorderNodes();
      int lower;
      int upper;
      ScriptDebugInfo.GetLineBoundaries(preorderNodes, out lower, out upper);
      this._lineToPTN = new ParseTreeNode[upper + 1];
      this._PTNBreakpoints = new Dictionary<ParseTreeNode, List<Breakpoint>>();
      this._breakpointToPTN = new Dictionary<Breakpoint, ParseTreeNode>();
      this._preorderStatements = new List<StatementListNode>();
      if (!this._fromScript && this._script.FunctionDeclarationNode != null)
        this.AssignLinesToFunctionDeclaration(this._script.FunctionDeclarationNode);
      for (int index = 0; index < preorderNodes.Count; ++index)
      {
        if (preorderNodes[index].NodeToken != null)
        {
          if (preorderNodes[index] is StatementListNode statementListNode)
            this._preorderStatements.Add(statementListNode);
          if (preorderNodes[index] is FunctionDeclarationNode functionNode)
          {
            this.AssignLinesToFunctionDeclaration(functionNode);
          }
          else
          {
            if (preorderNodes[index].SkipDebuggerStep && index + 1 < preorderNodes.Count)
            {
              ParseTreeNode parseTreeNode = preorderNodes[index + 1];
              if (parseTreeNode.NodeToken != null && parseTreeNode.NodeToken.LineNumber == preorderNodes[index].NodeToken.LineNumber)
                continue;
            }
            if (this._lineToPTN[preorderNodes[index].NodeToken.LineNumber] == null)
              this._lineToPTN[preorderNodes[index].NodeToken.LineNumber] = preorderNodes[index];
          }
        }
      }
      int index1 = upper;
      for (int index2 = index1 - 1; index2 >= lower; --index2)
      {
        if (this._lineToPTN[index2] == null)
          this._lineToPTN[index2] = this._lineToPTN[index1];
        else
          index1 = index2;
      }
      this._isAnalyzed = true;
    }

    private void AssignLinesToFunctionDeclaration(FunctionDeclarationNode functionNode)
    {
      ParseTreeNode firstClause = this.GetFirstClause(functionNode);
      int lineNumberInBody = ScriptDebugInfo.GetMinimumLineNumberInBody(functionNode);
      for (int lineNumber = functionNode.NodeToken.LineNumber; lineNumber < lineNumberInBody; ++lineNumber)
        this._lineToPTN[lineNumber] = firstClause;
    }

    private ParseTreeNode GetFirstClause(FunctionDeclarationNode functionNode)
    {
      if (functionNode.Body.Begin != null)
        return functionNode.Body.Begin;
      if (functionNode.Body.Process != null)
        return functionNode.Body.Process;
      return functionNode.Body.End != null ? functionNode.Body.End : (ParseTreeNode) null;
    }

    private static int GetMinimumLineNumberInBody(FunctionDeclarationNode functionNode)
    {
      int num = int.MaxValue;
      if (functionNode.Body.DynamicParams != null && functionNode.Body.DynamicParams.NodeToken.LineNumber < num)
        num = functionNode.Body.DynamicParams.NodeToken.LineNumber;
      if (functionNode.Body.Begin != null && functionNode.Body.Begin.NodeToken.LineNumber < num)
        num = functionNode.Body.Begin.NodeToken.LineNumber;
      if (functionNode.Body.Process != null && functionNode.Body.Process.NodeToken.LineNumber < num)
        num = functionNode.Body.Process.NodeToken.LineNumber;
      if (functionNode.Body.End != null && functionNode.Body.End.NodeToken.LineNumber < num)
        num = functionNode.Body.End.NodeToken.LineNumber;
      return num == int.MaxValue ? -1 : num;
    }

    private static void GetLineBoundaries(
      List<ParseTreeNode> parseTreeNodes,
      out int lower,
      out int upper)
    {
      lower = int.MaxValue;
      upper = 0;
      foreach (ParseTreeNode parseTreeNode in parseTreeNodes)
      {
        if (parseTreeNode.NodeToken != null)
        {
          if (parseTreeNode.NodeToken.LineNumber < lower)
            lower = parseTreeNode.NodeToken.LineNumber;
          if (parseTreeNode.NodeToken.LineNumber > upper)
            upper = parseTreeNode.NodeToken.LineNumber;
        }
      }
    }

    internal void SetBreakpoints(ScriptBlock script)
    {
      this._script = script;
      this._isAnalyzed = false;
      this.AnalyzeScript();
      foreach (LineBreakpoint lineBreakpoint in this._breakpointInfo.LineBreakpoints)
        this.AddLineBreakpoint(lineBreakpoint);
      foreach (LineBreakpoint statementBreakpoint in this._breakpointInfo.StatementBreakpoints)
        this.AddStatementBreakpoint(statementBreakpoint);
    }

    internal void AddLineBreakpoint(LineBreakpoint breakpoint)
    {
      if (!this._isAnalyzed)
        this.AnalyzeScript();
      if (breakpoint.Line < 0 || breakpoint.Line >= this._lineToPTN.Length)
      {
        if (!this._fromScript)
          return;
        this._breakpointInfo.Context.EngineHostInterface.UI.WriteWarningLine(string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, ResourceManagerCache.GetResourceString("DebuggerStrings", "WarningBreakpointWillNotBeHit"), (object) breakpoint.Id));
      }
      else
      {
        ParseTreeNode key = this._lineToPTN[breakpoint.Line];
        if (key == null)
          return;
        if (this._PTNBreakpoints.ContainsKey(key))
          this._PTNBreakpoints[key].Add((Breakpoint) breakpoint);
        else
          this._PTNBreakpoints[key] = new List<Breakpoint>()
          {
            (Breakpoint) breakpoint
          };
        this._breakpointToPTN[(Breakpoint) breakpoint] = key;
      }
    }

    internal void AddStatementBreakpoint(LineBreakpoint breakpoint)
    {
      if (!this._isAnalyzed)
        this.AnalyzeScript();
      StatementListNode statementListNode = (StatementListNode) null;
      ParseTreeNode key = (ParseTreeNode) null;
      for (int index = 0; this._preorderStatements.Count > index; ++index)
      {
        if (this.IsBefore(this._preorderStatements[index].NodeToken, breakpoint))
          statementListNode = this._preorderStatements[index];
      }
      for (int index = 0; statementListNode != null && statementListNode.Statements.Length > index; ++index)
      {
        if (this.IsBefore(statementListNode.Statements[index].NodeToken, breakpoint))
          key = statementListNode.Statements[index];
      }
      if (key == null)
        return;
      if (this._PTNBreakpoints.ContainsKey(key))
        this._PTNBreakpoints[key].Add((Breakpoint) breakpoint);
      else
        this._PTNBreakpoints[key] = new List<Breakpoint>()
        {
          (Breakpoint) breakpoint
        };
      this._breakpointToPTN[(Breakpoint) breakpoint] = key;
    }

    private bool IsBefore(Token token, LineBreakpoint breakpoint)
    {
      if (token.LineNumber < breakpoint.Line)
        return true;
      return token.LineNumber == breakpoint.Line && token.StartOffsetInLine <= breakpoint.Column;
    }

    internal void RemoveBreakpoint(LineBreakpoint breakpoint)
    {
      ParseTreeNode key = (ParseTreeNode) null;
      if (!this._breakpointToPTN.TryGetValue((Breakpoint) breakpoint, out key))
        return;
      this._PTNBreakpoints[key].Remove((Breakpoint) breakpoint);
      this._breakpointToPTN.Remove((Breakpoint) breakpoint);
    }

    internal List<Breakpoint> CheckParseTreeNode(ParseTreeNode node)
    {
      List<Breakpoint> breakpointList = (List<Breakpoint>) null;
      return this._PTNBreakpoints != null && this._PTNBreakpoints.TryGetValue(node, out breakpointList) ? breakpointList : (List<Breakpoint>) null;
    }
  }
}
