// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParseTreeNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal abstract class ParseTreeNode
  {
    internal Token NodeToken;
    private ParseTreeNode.Flags _flags;
    [TraceSource("Parser", "Parser")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer("Parser", "Parser");

    internal bool IsExpression
    {
      get => (this._flags & ParseTreeNode.Flags.IsExpression) != (ParseTreeNode.Flags) 0;
      set => this._flags = value ? this._flags | ParseTreeNode.Flags.IsExpression : this._flags & ~ParseTreeNode.Flags.IsExpression;
    }

    internal bool IsVoidable
    {
      get => (this._flags & ParseTreeNode.Flags.IsVoidable) != (ParseTreeNode.Flags) 0;
      set => this._flags = value ? this._flags | ParseTreeNode.Flags.IsVoidable : this._flags & ~ParseTreeNode.Flags.IsVoidable;
    }

    internal bool IsConstant
    {
      get => (this._flags & ParseTreeNode.Flags.IsConstant) != (ParseTreeNode.Flags) 0;
      set => this._flags = value ? this._flags | ParseTreeNode.Flags.IsConstant | ParseTreeNode.Flags.ValidAttributeArgument : this._flags & ~(ParseTreeNode.Flags.IsConstant | ParseTreeNode.Flags.ValidAttributeArgument);
    }

    internal bool ValidAttributeArgument
    {
      get => (this._flags & ParseTreeNode.Flags.ValidAttributeArgument) != (ParseTreeNode.Flags) 0;
      set => this._flags = value ? this._flags | ParseTreeNode.Flags.ValidAttributeArgument : this._flags & ~ParseTreeNode.Flags.ValidAttributeArgument;
    }

    internal bool IsInternalCode
    {
      get => (this._flags & ParseTreeNode.Flags.IsInternalCode) != (ParseTreeNode.Flags) 0;
      set => this._flags = value ? this._flags | ParseTreeNode.Flags.IsInternalCode : this._flags & ~ParseTreeNode.Flags.IsInternalCode;
    }

    internal virtual bool SkipDebuggerStep => false;

    internal virtual object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      ArrayList resultList = (ArrayList) null;
      this.Execute(input, outputPipe, ref resultList, context);
      if (resultList == null || resultList.Count == 0)
        return (object) AutomationNull.Value;
      return resultList.Count == 1 ? resultList[0] : (object) resultList.ToArray();
    }

    internal virtual void Execute(
      Array input,
      Pipe outputPipe,
      ref ArrayList resultList,
      ExecutionContext context)
    {
      object result = this.Execute(input, outputPipe, context);
      if (this.IsVoidable)
        return;
      ParseTreeNode.AppendResult(context, result, outputPipe, ref resultList);
    }

    public object Execute(ExecutionContext context) => this.Execute((Array) null, (Pipe) null, context);

    public object Execute(Array arguments, ExecutionContext context) => this.Execute(arguments, (Pipe) null, context);

    internal virtual object GetConstValue() => throw ParseTreeNode.tracer.NewInvalidOperationException();

    internal static object InvokeGetConstValue(ParseTreeNode ptn) => ptn.GetConstValue();

    public override string ToString() => this.NodeToken == null ? "<null token>" : this.NodeToken.TokenText;

    internal static void AppendResult(
      ExecutionContext context,
      object result,
      Pipe outputPipe,
      ref ArrayList resultList)
    {
      if (result == AutomationNull.Value)
        return;
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(result);
      if (outputPipe != null)
      {
        if (enumerator != null)
        {
          while (ParserOps.MoveNext(context, (Token) null, enumerator))
          {
            object obj = ParserOps.Current((Token) null, enumerator);
            if (obj != AutomationNull.Value)
              ParseTreeNode.AddOutputObjectToPipe(outputPipe, obj);
          }
        }
        else
          ParseTreeNode.AddOutputObjectToPipe(outputPipe, result);
      }
      else
      {
        if (resultList == null)
          resultList = new ArrayList();
        if (enumerator != null)
        {
          while (ParserOps.MoveNext(context, (Token) null, enumerator))
          {
            object obj = ParserOps.Current((Token) null, enumerator);
            if (obj != AutomationNull.Value)
              resultList.Add(obj);
          }
        }
        else
          resultList.Add(result);
      }
    }

    private static void AddOutputObjectToPipe(Pipe pipe, object obj)
    {
      pipe.UpdateScriptCmdletVariable(ScriptCmdletVariable.Output, obj);
      pipe.Add(obj);
    }

    internal List<ParseTreeNode> EnumeratePreorder()
    {
      BuildPreOrderNodesListVisitor nodesListVisitor = new BuildPreOrderNodesListVisitor();
      this.Accept((ParseTreeVisitor) nodesListVisitor);
      return nodesListVisitor.GetPreOrderNodes();
    }

    internal abstract void Accept(ParseTreeVisitor visitor);

    internal void CheckForInterrupts(ExecutionContext context)
    {
      if (context.Events != null)
        context.Events.ProcessPendingActions();
      if (context.Debugger == null || !context.Debugger.IsOn)
        return;
      context.Debugger.CheckForBreakpoints(this);
    }

    [System.Flags]
    private enum Flags
    {
      IsExpression = 1,
      IsVoidable = 2,
      IsConstant = 4,
      ValidAttributeArgument = 8,
      IsInternalCode = 16, // 0x00000010
    }

    internal delegate object ParseTreeNodeExecutor(ParseTreeNode ptn);

    internal class InvokeExecute
    {
      private Array input;
      private Pipe outputPipe;
      private ExecutionContext context;

      internal InvokeExecute(Array input, Pipe outputPipe, ExecutionContext context)
      {
        this.input = input;
        this.outputPipe = outputPipe;
        this.context = context;
      }

      internal object Invoke(ParseTreeNode ptn) => ptn.Execute(this.input, this.outputPipe, this.context);
    }
  }
}
