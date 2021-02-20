// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.FlowControlNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class FlowControlNode : ParseTreeNode
  {
    private readonly object _argument;
    private readonly bool _inCatch;
    private readonly TokenId _tokenId;

    public FlowControlNode(Token keyword, object argument, bool inCatch)
    {
      this.NodeToken = keyword;
      this._tokenId = keyword.TokenId;
      this._argument = argument;
      this._inCatch = inCatch;
    }

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      object result = this._argument;
      if (this._argument == null)
        result = (object) AutomationNull.Value;
      else if (this._argument is ParseTreeNode)
        result = ((ParseTreeNode) this._argument).Execute(input, (Pipe) null, context);
      switch (this._tokenId)
      {
        case TokenId.ExitToken:
          throw new ExitException(this.NodeToken, (object) this.GetExitCode(result));
        case TokenId.ReturnToken:
          throw new ReturnException(this.NodeToken, result);
        case TokenId.BreakToken:
          throw new BreakException(this.NodeToken, this.GetLabel(result, context));
        case TokenId.ContinueToken:
          throw new ContinueException(this.NodeToken, this.GetLabel(result, context));
        case TokenId.ThrowToken:
          throw (Exception) this.ConvertToException(result, context);
        default:
          return (object) AutomationNull.Value;
      }
    }

    private int GetExitCode(object result)
    {
      int num = 0;
      try
      {
        if (!LanguagePrimitives.IsNull(result))
          num = Parser.ConvertTo<int>(result, this.NodeToken);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
      return num;
    }

    private string GetLabel(object result, ExecutionContext context)
    {
      string stringParser = PSObject.ToStringParser(context, result);
      for (int index = 0; index < stringParser.Length; ++index)
      {
        if (!char.IsLetterOrDigit(stringParser[index]) && stringParser[index] != '_')
          throw InterpreterError.NewInterpreterException((object) stringParser, typeof (RuntimeException), this.NodeToken, "InvalidLabelCharacter", (object) stringParser[index]);
      }
      return stringParser;
    }

    private RuntimeException ConvertToException(
      object result,
      ExecutionContext context)
    {
      result = PSObject.Base(result);
      if (LanguagePrimitives.IsNull(result) && context.CurrentExceptionBeingHandled != null && this._inCatch)
        result = (object) context.CurrentExceptionBeingHandled;
      switch (result)
      {
        case RuntimeException runtimeException3:
          runtimeException3.WasThrownFromThrowStatement = true;
          return runtimeException3;
        case ErrorRecord errorRecord3:
          RuntimeException runtimeException1 = new RuntimeException(errorRecord3.ToString(), errorRecord3.Exception, errorRecord3);
          if (errorRecord3.InvocationInfo == null)
            errorRecord3.SetInvocationInfo(new InvocationInfo((CommandInfo) null, this.NodeToken, context));
          runtimeException1.WasThrownFromThrowStatement = true;
          return runtimeException1;
        case Exception exception2:
          ErrorRecord errorRecord1 = new ErrorRecord(exception2, exception2.Message, ErrorCategory.OperationStopped, (object) null);
          errorRecord1.SetInvocationInfo(new InvocationInfo((CommandInfo) null, this.NodeToken, context));
          return new RuntimeException(exception2.Message, exception2, errorRecord1)
          {
            WasThrownFromThrowStatement = true
          };
        default:
          string str = LanguagePrimitives.IsNull(result) ? "ScriptHalted" : Parser.ConvertTo<string>(result, this.NodeToken);
          Exception exception1 = (Exception) new RuntimeException(str, (Exception) null);
          ErrorRecord errorRecord2 = new ErrorRecord(exception1, str, ErrorCategory.OperationStopped, (object) null);
          errorRecord2.SetInvocationInfo(new InvocationInfo((CommandInfo) null, this.NodeToken, context));
          RuntimeException runtimeException2 = new RuntimeException(str, exception1, errorRecord2);
          runtimeException2.WasThrownFromThrowStatement = true;
          runtimeException2.SetTargetObject(result);
          return runtimeException2;
      }
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      if (!(this._argument is ParseTreeNode parseTreeNode))
        return;
      parseTreeNode.Accept(visitor);
    }
  }
}
