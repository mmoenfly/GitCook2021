// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PipelineNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class PipelineNode : ParseTreeNode
  {
    private int _activationRecordSlot = -1;
    internal readonly List<CommandNode> Commands = new List<CommandNode>();

    public PipelineNode(Token nodeToken) => this.NodeToken = nodeToken;

    internal void SetActivationRecordSlot(int slot) => this._activationRecordSlot = slot;

    internal bool ExecutionFailed(ExecutionContext context) => context.EngineSessionState.CurrentActivationRecord.GetExecutionFailed(this._activationRecordSlot);

    public void Add(CommandNode pElement) => this.Commands.Add(pElement);

    internal override void Execute(
      Array input,
      Pipe outputPipe,
      ref ArrayList resultList,
      ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      if (this.Commands.Count == 0)
        return;
      object input1 = input == null ? (object) AutomationNull.Value : (object) input;
      int num = 0;
      CommandNode command = this.Commands[0];
      if (command.IsPureExpression)
      {
        command.CheckForInterrupts(context);
        input1 = !(command.Expression is ExpressionNode expression) || !expression.IsRangeExpression() ? command.Expression.Execute(input, (Pipe) null, context) : (object) expression.GetEnumeratorForRangeExpression(context);
        if (input1 == AutomationNull.Value)
          return;
        ++num;
      }
      PipelineProcessor pipelineProcessor = new PipelineProcessor();
      try
      {
        CommandProcessorBase commandProcessorBase = (CommandProcessorBase) null;
        for (int index = num; index < this.Commands.Count; ++index)
          commandProcessorBase = this.Commands[index].AddToPipeline(pipelineProcessor, context);
        if (commandProcessorBase != null && !commandProcessorBase.CommandRuntime.OutputPipe.IsRedirected)
        {
          if (outputPipe != null)
          {
            pipelineProcessor.LinkPipelineSuccessOutput(outputPipe);
          }
          else
          {
            if (resultList == null)
              resultList = new ArrayList();
            pipelineProcessor.LinkPipelineSuccessOutput(new Pipe(resultList));
          }
        }
        context.PushPipelineProcessor(pipelineProcessor);
        try
        {
          pipelineProcessor.SynchronousExecuteEnumerate(input1, (Hashtable) null, true);
        }
        finally
        {
          context.PopPipelineProcessor();
        }
      }
      finally
      {
        context.EngineSessionState.CurrentActivationRecord.SetExecutionFailed(this._activationRecordSlot, pipelineProcessor.ExecutionFailed);
        pipelineProcessor.Dispose();
      }
    }

    internal SteppablePipeline GetSteppablePipeline(
      ExecutionContext context,
      CommandOrigin commandOrigin)
    {
      if (this.Commands.Count == 0)
        return (SteppablePipeline) null;
      if (this.Commands[0].IsPureExpression)
        throw ParseTreeNode.tracer.NewInvalidOperationException("AutomationExceptions", "CantConvertEmptyPipeline");
      PipelineProcessor pipeline1 = new PipelineProcessor();
      for (int index = 0; index < this.Commands.Count; ++index)
      {
        CommandProcessorBase pipeline2 = this.Commands[index].AddToPipeline(pipeline1, context);
        pipeline2.Command.CommandOriginInternal = commandOrigin;
        pipeline2.CommandScope.ScopeOrigin = commandOrigin;
        pipeline2.Command.MyInvocation.CommandOrigin = commandOrigin;
      }
      return new SteppablePipeline(context, pipeline1);
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      foreach (ParseTreeNode command in this.Commands)
        command.Accept(visitor);
    }
  }
}
