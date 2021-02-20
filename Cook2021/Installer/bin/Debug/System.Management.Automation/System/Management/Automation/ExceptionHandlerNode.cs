// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ExceptionHandlerNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class ExceptionHandlerNode : ParseTreeNode
  {
    private readonly ExceptionTypeList _exceptionTypes;
    private readonly ParseTreeNode _body;
    private readonly bool _isCatchBlock;
    private readonly int _pipelineSlots;
    private readonly int _variableSlots;

    internal ExceptionTypeList ExceptionTypes => this._exceptionTypes;

    internal ExceptionHandlerNode(Token token, TypeLiteral exceptionType, ParseTreeNode body)
      : this(token, exceptionType != null ? new ExceptionTypeList() : (ExceptionTypeList) null, body, false)
    {
      if (exceptionType == null)
        return;
      this._exceptionTypes.Add(exceptionType);
    }

    internal ExceptionHandlerNode(
      Token token,
      ExceptionTypeList exceptionTypeList,
      ParseTreeNode body,
      bool isCatchBlock)
    {
      this.NodeToken = token;
      this._exceptionTypes = exceptionTypeList;
      this._isCatchBlock = isCatchBlock;
      this._body = body;
      ActivationRecordBuilder activationRecordBuilder = new ActivationRecordBuilder();
      if (this._body != null)
      {
        this._body.Accept((ParseTreeVisitor) activationRecordBuilder);
        this._pipelineSlots = activationRecordBuilder.PipelineSlots;
        this._variableSlots = activationRecordBuilder.VariableSlots;
      }
      else
      {
        this._pipelineSlots = 0;
        this._variableSlots = 0;
      }
    }

    internal void Invoke(ErrorRecord errorRecord, Pipe outputPipe, ref ArrayList resultList) => ScriptBlock.CreateExceptionHandler(this._body, this.NodeToken, this._pipelineSlots, this._variableSlots).InvokeWithPipe(!this._isCatchBlock, true, (object) errorRecord, (object) AutomationNull.Value, (object) AutomationNull.Value, outputPipe, ref resultList);

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context) => (object) AutomationNull.Value;

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      if (visitor.SkipInvokableScriptBlocks)
        return;
      this._body.Accept(visitor);
    }

    internal static ExceptionHandlerNode GetHandler(
      ReadOnlyCollection<ExceptionHandlerNode> handlers,
      Exception exception,
      ExecutionContext context)
    {
      ExceptionTypeList.Comparer comparer1 = new ExceptionTypeList.Comparer(ExceptionTypeList.CompareEqual);
      Type matchedType = (Type) null;
      foreach (ExceptionHandlerNode handler in handlers)
      {
        if (handler.ExceptionTypes != null && handler.ExceptionTypes.Matches(comparer1, exception.GetType(), out matchedType))
        {
          ScriptTrace.Trace(context, 1, "TrapOnExceptionMatch", (object) exception.GetType());
          return handler;
        }
      }
      ExceptionTypeList.Comparer comparer2 = new ExceptionTypeList.Comparer(ExceptionTypeList.CompareSubclass);
      foreach (ExceptionHandlerNode handler in handlers)
      {
        if (handler.ExceptionTypes != null && handler.ExceptionTypes.Matches(comparer2, exception.GetType(), out matchedType))
        {
          ScriptTrace.Trace(context, 1, "TrapOnSubclassMatch", (object) matchedType, (object) exception.GetType());
          return handler;
        }
      }
      foreach (ExceptionHandlerNode handler in handlers)
      {
        if (handler.ExceptionTypes == null)
        {
          ScriptTrace.Trace(context, 1, "TrapOnGenericException", (object) exception.GetType());
          return handler;
        }
      }
      return (ExceptionHandlerNode) null;
    }
  }
}
