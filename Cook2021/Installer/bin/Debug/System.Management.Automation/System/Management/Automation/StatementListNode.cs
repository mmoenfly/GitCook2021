// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.StatementListNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Internal.Host;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;

namespace System.Management.Automation
{
  internal sealed class StatementListNode : ParseTreeNode
  {
    private ParseTreeNode[] _statements;
    private readonly ReadOnlyCollection<ExceptionHandlerNode> _traps;
    private Token _end;

    internal StatementListNode(
      Token start,
      Token end,
      ParseTreeNode[] statements,
      ExceptionHandlerNode[] traps)
    {
      this.NodeToken = start;
      this._end = end;
      this._statements = statements;
      if (traps != null)
        this._traps = new ReadOnlyCollection<ExceptionHandlerNode>((IList<ExceptionHandlerNode>) traps);
      if (statements.Length != 1)
        return;
      this.ValidAttributeArgument = statements[0].ValidAttributeArgument;
    }

    internal override bool SkipDebuggerStep => true;

    public override string ToString() => this.NodeToken.Script.Substring(this.NodeToken.Start, this._end.End - this.NodeToken.Start);

    internal bool IsEmpty => this._statements.Length == 0;

    internal ParseTreeNode[] Statements => this._statements;

    internal ReadOnlyCollection<ExceptionHandlerNode> Traps => this._traps;

    internal override void Execute(
      Array input,
      Pipe outputPipe,
      ref ArrayList resultList,
      ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      bool enclosingStatementBlock = context.ExceptionHandlerInEnclosingStatementBlock;
      if (this._traps != null)
        context.ExceptionHandlerInEnclosingStatementBlock = true;
      try
      {
        for (int index = 0; index < this._statements.Length; ++index)
        {
          ParseTreeNode statement = this._statements[index];
          context.Debugger.PushStatement(statement);
          try
          {
            this.ExecuteStatement(statement, input, outputPipe, ref resultList, context);
          }
          finally
          {
            context.Debugger.PopStatement();
          }
        }
      }
      finally
      {
        context.ExceptionHandlerInEnclosingStatementBlock = enclosingStatementBlock;
      }
    }

    private void ExecuteStatement(
      ParseTreeNode statement,
      Array input,
      Pipe outputPipe,
      ref ArrayList resultList,
      ExecutionContext context)
    {
      ScriptTrace.TraceLine(context, statement);
      Exception e = (Exception) null;
      try
      {
        try
        {
          if (statement.IsVoidable)
            statement.Execute(input, (Pipe) null, context);
          else
            statement.Execute(input, outputPipe, ref resultList, context);
          ParseTreeNode parseTreeNode = statement;
          while (parseTreeNode is AssignmentStatementNode assignmentStatementNode)
            parseTreeNode = assignmentStatementNode.RightHandSide;
          if (parseTreeNode.IsExpression)
            context.QuestionMarkVariableValue = true;
          else if (parseTreeNode is PipelineNode pipelineNode)
            context.QuestionMarkVariableValue = !pipelineNode.ExecutionFailed(context);
        }
        catch (COMException ex)
        {
          throw InterpreterError.NewInterpreterExceptionWithInnerException((object) null, typeof (RuntimeException), statement.NodeToken, "COMException", (Exception) ex, (object) ex.Message);
        }
        catch (InvalidComObjectException ex)
        {
          throw InterpreterError.NewInterpreterExceptionWithInnerException((object) null, typeof (RuntimeException), statement.NodeToken, "InvalidComObjectException", (Exception) ex, (object) ex.Message);
        }
      }
      catch (ReturnException ex)
      {
        if (resultList == null || resultList.Count == 0)
        {
          e = (Exception) ex;
        }
        else
        {
          ParseTreeNode.AppendResult(context, ex.Argument, (Pipe) null, ref resultList);
          ex.SetArgument((object) resultList.ToArray());
          resultList = (ArrayList) null;
          e = (Exception) ex;
        }
      }
      catch (RuntimeException ex)
      {
        e = this.HandleException(statement.NodeToken, ex, outputPipe, ref resultList, context);
      }
      if (e != null)
      {
        RuntimeException.LockStackTrace(e);
        throw e;
      }
    }

    internal override object GetConstValue() => this._statements[0].GetConstValue();

    private Exception HandleException(
      Token statementToken,
      RuntimeException rte,
      Pipe outputPipe,
      ref ArrayList resultList,
      ExecutionContext context)
    {
      this.SetErrorVariables(statementToken, rte, context, outputPipe);
      context.QuestionMarkVariableValue = false;
      if (this._traps == null && !this.NeedToQueryForActionPreference(rte, context))
        return (Exception) rte;
      ActionPreference actionPreference = this._traps == null ? this.QueryForAction(rte, rte.Message, context) : this.ProcessTraps(rte, outputPipe, ref resultList, context);
      context.QuestionMarkVariableValue = false;
      if (actionPreference == ActionPreference.SilentlyContinue)
        return (Exception) null;
      if (actionPreference == ActionPreference.Stop)
      {
        rte.SuppressPromptInInterpreter = true;
        return (Exception) rte;
      }
      if (this._traps == null && rte.WasThrownFromThrowStatement)
        return (Exception) rte;
      bool flag = this.ReportErrorRecord(statementToken, rte, context);
      context.QuestionMarkVariableValue = false;
      return !flag ? (Exception) rte : (Exception) null;
    }

    private bool NeedToQueryForActionPreference(RuntimeException rte, ExecutionContext context) => !context.ExceptionHandlerInEnclosingStatementBlock && context.ShellFunctionErrorOutputPipe != null && (!context.CurrentPipelineStopping && !rte.SuppressPromptInInterpreter) && !(rte is PipelineStoppedException);

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      if (!visitor.SkipInvokableScriptBlocks && this._traps != null)
      {
        foreach (ParseTreeNode trap in this._traps)
          trap.Accept(visitor);
      }
      foreach (ParseTreeNode statement in this._statements)
        statement.Accept(visitor);
    }

    private ActionPreference ProcessTraps(
      RuntimeException rte,
      Pipe outputPipe,
      ref ArrayList resultList,
      ExecutionContext context)
    {
      ExceptionHandlerNode trap = (ExceptionHandlerNode) null;
      Exception exception = (Exception) null;
      if (rte.InnerException != null)
      {
        trap = ExceptionHandlerNode.GetHandler(this._traps, rte.InnerException, context);
        exception = rte.InnerException;
      }
      if (trap == null)
      {
        trap = ExceptionHandlerNode.GetHandler(this._traps, (Exception) rte, context);
        exception = (Exception) rte;
      }
      return trap != null ? this.TrapException(trap, rte, exception, outputPipe, ref resultList, context) : ActionPreference.Stop;
    }

    private ActionPreference TrapException(
      ExceptionHandlerNode trap,
      RuntimeException runtimeException,
      Exception exception,
      Pipe outputPipe,
      ref ArrayList resultList,
      ExecutionContext context)
    {
      try
      {
        ErrorRecord errorRecord = runtimeException.ErrorRecord;
        trap.Invoke(new ErrorRecord(errorRecord, exception), outputPipe, ref resultList);
        return this.QueryForAction(runtimeException, exception.Message, context);
      }
      catch (ContinueException ex)
      {
        return ActionPreference.SilentlyContinue;
      }
      catch (BreakException ex)
      {
        return ActionPreference.Stop;
      }
    }

    private ActionPreference QueryForAction(
      RuntimeException rte,
      string message,
      ExecutionContext context)
    {
      ActionPreference enumPreference = context.GetEnumPreference<ActionPreference>(InitialSessionState.errorActionPreferenceVariablePath, ActionPreference.Continue, out bool _);
      return enumPreference != ActionPreference.Inquire || rte.SuppressPromptInInterpreter ? enumPreference : this.InquireForActionPreference(message, context);
    }

    private ActionPreference InquireForActionPreference(
      string message,
      ExecutionContext context)
    {
      InternalHostUserInterface ui = (InternalHostUserInterface) context.EngineHostInterface.UI;
      Collection<ChoiceDescription> choices = new Collection<ChoiceDescription>();
      string resourceString1 = ResourceManagerCache.GetResourceString("Parser", "ContinueLabel");
      string resourceString2 = ResourceManagerCache.GetResourceString("Parser", "ContinueHelpMessage");
      string resourceString3 = ResourceManagerCache.GetResourceString("Parser", "SilentlyContinueLabel");
      string resourceString4 = ResourceManagerCache.GetResourceString("Parser", "SilentlyContinueHelpMessage");
      string resourceString5 = ResourceManagerCache.GetResourceString("Parser", "BreakLabel");
      string resourceString6 = ResourceManagerCache.GetResourceString("Parser", "BreakHelpMessage");
      string resourceString7 = ResourceManagerCache.GetResourceString("Parser", "SuspendLabel");
      string helpMessage = ResourceManagerCache.FormatResourceString("Parser", "SuspendHelpMessage");
      choices.Add(new ChoiceDescription(resourceString1, resourceString2));
      choices.Add(new ChoiceDescription(resourceString3, resourceString4));
      choices.Add(new ChoiceDescription(resourceString5, resourceString6));
      choices.Add(new ChoiceDescription(resourceString7, helpMessage));
      string resourceString8 = ResourceManagerCache.GetResourceString("Parser", "ExceptionActionPromptCaption");
      int num;
      while ((num = ui.PromptForChoice(resourceString8, message, choices, 0)) == 3)
        context.EngineHostInterface.EnterNestedPrompt();
      if (num == 0)
        return ActionPreference.Continue;
      return num == 1 ? ActionPreference.SilentlyContinue : ActionPreference.Stop;
    }

    private void SetErrorVariables(
      Token statementToken,
      RuntimeException rte,
      ExecutionContext context,
      Pipe outputPipe)
    {
      string str = (string) null;
      Exception exception = (Exception) rte;
      for (int index = 0; exception != null && index++ < 10; exception = exception.InnerException)
      {
        if (!string.IsNullOrEmpty(exception.StackTrace))
          str = exception.StackTrace;
      }
      context.SetVariable("global:StackTrace", (object) str);
      if (rte.ErrorRecord.InvocationInfo == null)
        rte.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, statementToken, context));
      ErrorRecord errorRecord = new ErrorRecord(rte.ErrorRecord, (Exception) rte);
      if (rte is PipelineStoppedException)
        return;
      outputPipe?.UpdateScriptCmdletVariable(ScriptCmdletVariable.Error, (object) errorRecord);
      context.AppendDollarError((object) errorRecord);
    }

    internal bool ReportErrorRecord(
      Token statementToken,
      RuntimeException rte,
      ExecutionContext context)
    {
      if (context.ShellFunctionErrorOutputPipe == null)
        return false;
      if (rte.ErrorRecord.InvocationInfo == null)
        rte.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, statementToken, context));
      PSObject psObject = PSObject.AsPSObject((object) new ErrorRecord(rte.ErrorRecord, (Exception) rte));
      PSNoteProperty psNoteProperty = new PSNoteProperty("writeErrorStream", (object) true);
      psObject.Properties.Add((PSPropertyInfo) psNoteProperty);
      context.ShellFunctionErrorOutputPipe.Add((object) psObject);
      return true;
    }
  }
}
