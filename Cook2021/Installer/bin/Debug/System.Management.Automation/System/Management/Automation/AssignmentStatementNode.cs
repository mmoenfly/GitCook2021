// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.AssignmentStatementNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class AssignmentStatementNode : ParseTreeNode
  {
    private readonly IAssignableParseTreeNode _left;
    private readonly ParseTreeNode _rightHandSide;
    private readonly PowerShellBinaryOperator _operationDelegate;

    public AssignmentStatementNode(
      Token operatorToken,
      IAssignableParseTreeNode left,
      ParseTreeNode right)
    {
      this.NodeToken = operatorToken;
      this._left = left;
      this._rightHandSide = right;
      this._operationDelegate = ((OperatorToken) operatorToken).OperationDelegate;
      this.IsVoidable = true;
    }

    internal ParseTreeNode RightHandSide => this._rightHandSide;

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      object obj1 = (object) null;
      try
      {
        obj1 = this._rightHandSide.Execute(input, (Pipe) null, context);
        if (obj1 == AutomationNull.Value)
          obj1 = (object) null;
        IAssignableValue assignableValue = this._left.GetAssignableValue(input, context);
        object obj2 = this._operationDelegate == null ? obj1 : this._operationDelegate(context, this.NodeToken, assignableValue.GetValue(context), obj1);
        assignableValue.SetValue(obj2, context);
        return obj2;
      }
      catch (ScriptCallDepthException ex)
      {
        throw;
      }
      catch (FlowControlException ex)
      {
        throw;
      }
      catch (RuntimeException ex)
      {
        if (ex.ErrorRecord != null && ex.ErrorRecord.InvocationInfo == null)
          ex.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, this.NodeToken, context));
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        throw InterpreterError.NewInterpreterException(obj1, typeof (RuntimeException), this.NodeToken, "OperatorFailed", (object) this.NodeToken.TokenText, (object) ex.Message);
      }
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      if (this._left is ParseTreeNode left)
        left.Accept(visitor);
      this._rightHandSide.Accept(visitor);
    }
  }
}
