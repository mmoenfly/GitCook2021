// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ExpressionNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal class ExpressionNode : ParseTreeNode
  {
    private ArrayList _inFixItems = new ArrayList();
    private ArrayList _postFixItems = new ArrayList();
    private bool _completed;

    internal ExpressionNode() => this.IsExpression = true;

    internal void Add(ParseTreeNode operand) => this._inFixItems.Add((object) operand);

    internal void Add(OperatorToken opToken, ParseTreeNode operand)
    {
      if (this.NodeToken == null)
        this.NodeToken = (Token) opToken;
      this._inFixItems.Add((object) opToken);
      this._inFixItems.Add((object) operand);
    }

    internal void Complete(bool inDataSection)
    {
      if (this._completed)
        return;
      this.ConvertToPostFix(inDataSection);
      this._completed = true;
    }

    internal void ConvertToPostFix(bool inDataSection)
    {
      Stack<OperatorToken> stack = new Stack<OperatorToken>();
      foreach (object inFixItem in this._inFixItems)
      {
        if (!(inFixItem is OperatorToken operatorToken))
        {
          ParseTreeNode parseTreeNode = inFixItem as ParseTreeNode;
          if (parseTreeNode.IsConstant)
            this._postFixItems.Add(PSObject.Base(parseTreeNode.GetConstValue()));
          else
            this._postFixItems.Add(inFixItem);
        }
        else
        {
          while (stack.Count > 0 && operatorToken.Precedence <= stack.Peek().Precedence)
            this.PopOpStack(stack, inDataSection, this._postFixItems);
          stack.Push(operatorToken);
        }
      }
      while (stack.Count > 0)
        this.PopOpStack(stack, inDataSection, this._postFixItems);
      this._inFixItems.Clear();
    }

    private void PopOpStack(Stack<OperatorToken> stack, bool inDataSection, ArrayList output)
    {
      OperatorToken op = stack.Pop();
      object left = output[output.Count - 2];
      object right = output[output.Count - 1];
      if (!inDataSection && this.CanReduce(op) && (this.OperandSafeToReduce(left) && this.OperandSafeToReduce(right)))
      {
        output[output.Count - 2] = this.ExecuteOp((ExecutionContext) null, left, op, right);
        output.RemoveAt(output.Count - 1);
      }
      else
        output.Add((object) op);
    }

    private bool OperandSafeToReduce(object obj)
    {
      switch (obj)
      {
        case Token _:
        case ParseTreeNode _:
          return false;
        case IEnumerable _:
          return false;
        default:
          return true;
      }
    }

    private bool CanReduce(OperatorToken op) => !op.Is(TokenId.RangeOperatorToken) && (!(op is ComparisonToken comparisonToken) || !string.Equals(comparisonToken.ComparisonName, "-match", StringComparison.Ordinal) && !string.Equals(comparisonToken.ComparisonName, "-notmatch", StringComparison.Ordinal));

    internal ParseTreeNode GetConstantWrapperNode() => this._postFixItems.Count == 1 && !(this._postFixItems[0] is ParseTreeNode) ? (ParseTreeNode) new ConstantNode(this.NodeToken, this._postFixItems[0]) : (ParseTreeNode) null;

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      if (this._postFixItems == null)
        return (object) null;
      Stack stack = new Stack();
      int count = this._postFixItems.Count;
      for (int index = 0; index < count; ++index)
      {
        object postFixItem = this._postFixItems[index];
        switch (postFixItem)
        {
          case OperatorToken op:
            object right = stack.Pop();
            object left = stack.Pop();
            stack.Push(this.ExecuteOp(context, left, op, right));
            break;
          case ParseTreeNode parseTreeNode:
            stack.Push(parseTreeNode.Execute((Array) null, (Pipe) null, context));
            break;
          default:
            stack.Push(postFixItem);
            break;
        }
      }
      return stack.Pop();
    }

    private object ExecuteOp(
      ExecutionContext context,
      object left,
      OperatorToken op,
      object right)
    {
      Exception exception;
      try
      {
        return op.OperationDelegate(context, (Token) op, left, right);
      }
      catch (ArgumentException ex)
      {
        exception = (Exception) InterpreterError.NewInterpreterException((object) null, typeof (RuntimeException), (Token) op, "BadOperatorArgument", (object) op.TokenText, (object) ex.Message);
      }
      catch (RuntimeException ex)
      {
        if (ex.ErrorRecord != null && ex.ErrorRecord.InvocationInfo == null)
          ex.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, (Token) op, context));
        exception = (Exception) ex;
      }
      catch (ScriptCallDepthException ex)
      {
        exception = (Exception) ex;
      }
      catch (FlowControlException ex)
      {
        exception = (Exception) ex;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        exception = (Exception) InterpreterError.NewInterpreterException((object) null, typeof (RuntimeException), (Token) op, "OperatorFailed", (object) op.TokenText, (object) ex.Message);
      }
      throw exception;
    }

    private object ExecuteOperand(ArrayList postfixList, int index, ExecutionContext context)
    {
      object postfix = postfixList[index];
      return !(postfix is ParseTreeNode parseTreeNode) ? postfix : parseTreeNode.Execute((Array) null, (Pipe) null, context);
    }

    internal bool IsRangeExpression() => this._postFixItems.Count == 3 && (this._postFixItems[2] as OperatorToken).Is(TokenId.RangeOperatorToken);

    internal IEnumerator GetEnumeratorForRangeExpression(ExecutionContext context)
    {
      if (!this.IsRangeExpression())
        return (IEnumerator) null;
      OperatorToken postFixItem = this._postFixItems[2] as OperatorToken;
      Exception exception;
      try
      {
        return (IEnumerator) new RangeEnumerator(ParserOps.FixNum(this.ExecuteOperand(this._postFixItems, 0, context), (Token) postFixItem), ParserOps.FixNum(this.ExecuteOperand(this._postFixItems, 1, context), (Token) postFixItem));
      }
      catch (ArgumentException ex)
      {
        exception = (Exception) InterpreterError.NewInterpreterException((object) null, typeof (RuntimeException), (Token) postFixItem, "BadOperatorArgument", (object) postFixItem.TokenText, (object) ex.Message);
      }
      catch (RuntimeException ex)
      {
        if (ex.ErrorRecord != null && ex.ErrorRecord.InvocationInfo == null)
          ex.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, (Token) postFixItem, context));
        exception = (Exception) ex;
      }
      catch (ScriptCallDepthException ex)
      {
        exception = (Exception) ex;
      }
      catch (FlowControlException ex)
      {
        exception = (Exception) ex;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        exception = (Exception) InterpreterError.NewInterpreterException((object) null, typeof (RuntimeException), (Token) postFixItem, "OperatorFailed", (object) postFixItem.TokenText, (object) ex.Message);
      }
      if (exception != null)
        throw exception;
      return (IEnumerator) null;
    }

    internal void RestrictedLanguageCheck(Parser parser)
    {
      for (int index = 0; index < this._postFixItems.Count; ++index)
      {
        if (this._postFixItems[index] is OperatorToken postFixItem && !postFixItem.IsValidInRestrictedLanguage)
          parser.ReportException((object) postFixItem, typeof (ParseException), (Token) postFixItem, "OperatorNotSupportedInDataSection", (object) postFixItem.TokenText);
      }
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      foreach (object postFixItem in this._postFixItems)
      {
        if (postFixItem is ParseTreeNode parseTreeNode)
          parseTreeNode.Accept(visitor);
      }
    }
  }
}
