// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ScriptBlockNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal sealed class ScriptBlockNode : ParseTreeNode
  {
    private readonly ParameterDeclarationNode _parameterDeclarationNode;
    private readonly ParseTreeNode _begin;
    private readonly ParseTreeNode _process;
    private readonly ParseTreeNode _end;
    private readonly ParseTreeNode _dynamicParams;
    private readonly bool _isFilter;
    private readonly List<AttributeNode> _attributes;
    internal readonly List<Token> _helpComments;
    private readonly List<List<Token>> _parameterComments;
    private readonly int _variableSlots;
    private readonly int _pipelineSlots;

    internal ScriptBlockNode(
      ParameterDeclarationNode parameterDeclaration,
      ParseTreeNode body,
      bool isFilter,
      List<AttributeNode> attributes,
      List<Token> helpComments,
      List<List<Token>> parameterComments)
    {
      this._parameterDeclarationNode = parameterDeclaration;
      if (isFilter)
        this._process = body;
      else
        this._end = body;
      this._isFilter = isFilter;
      this._attributes = attributes;
      this._helpComments = helpComments;
      this._parameterComments = parameterComments;
      ActivationRecordBuilder activationRecordBuilder = this.BuildActivationRecord();
      this._pipelineSlots = activationRecordBuilder.PipelineSlots;
      this._variableSlots = activationRecordBuilder.VariableSlots;
      this.ValidAttributeArgument = true;
    }

    internal ScriptBlockNode(
      ParameterDeclarationNode parameterDeclaration,
      ParseTreeNode begin,
      ParseTreeNode process,
      ParseTreeNode end,
      ParseTreeNode dynamicParams,
      List<AttributeNode> attributes,
      List<Token> helpComments,
      List<List<Token>> parameterComments)
    {
      this._parameterDeclarationNode = parameterDeclaration;
      this._begin = begin;
      this._process = process;
      this._end = end;
      this._dynamicParams = dynamicParams;
      this._attributes = attributes;
      this._helpComments = helpComments;
      this._parameterComments = parameterComments;
      ActivationRecordBuilder activationRecordBuilder = this.BuildActivationRecord();
      this._pipelineSlots = activationRecordBuilder.PipelineSlots;
      this._variableSlots = activationRecordBuilder.VariableSlots;
      this.ValidAttributeArgument = true;
    }

    internal override object Execute(Array input, Pipe outputPipe, ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      ScriptBlock scriptBlock = this.BuildNewScriptBlock();
      scriptBlock.SessionStateInternal = context.EngineSessionState;
      return (object) scriptBlock;
    }

    internal override object GetConstValue() => (object) this.BuildNewScriptBlock();

    internal ScriptBlock BuildNewScriptBlock() => this.BuildNewScriptBlock((FunctionDeclarationNode) null);

    internal ScriptBlock BuildNewScriptBlock(FunctionDeclarationNode functionDeclaration)
    {
      Token token = (Token) null;
      if (this._begin != null)
        token = this._begin.NodeToken;
      else if (this._process != null)
        token = this._process.NodeToken;
      else if (this._end != null)
        token = this._end.NodeToken;
      return new ScriptBlock(token, functionDeclaration, this._parameterDeclarationNode, this._begin, this._process, this._end, this._dynamicParams, this._isFilter, this._attributes, this._helpComments, this._parameterComments, this._pipelineSlots, this._variableSlots);
    }

    internal ActivationRecordBuilder BuildActivationRecord()
    {
      ActivationRecordBuilder activationRecordBuilder = new ActivationRecordBuilder();
      this.AcceptInternal((ParseTreeVisitor) activationRecordBuilder, true);
      return activationRecordBuilder;
    }

    internal ParseTreeNode DynamicParams => this._dynamicParams;

    internal ParseTreeNode Begin => this._begin;

    internal ParseTreeNode Process => this._process;

    internal ParseTreeNode End => this._end;

    private void AcceptInternal(ParseTreeVisitor visitor, bool topLevel)
    {
      if (!topLevel && visitor.SkipInvokableScriptBlocks)
        return;
      visitor.Visit(this);
      if (this._parameterDeclarationNode != null)
        this._parameterDeclarationNode.Accept(visitor);
      if (this._begin != null)
        this._begin.Accept(visitor);
      if (this._process != null)
        this._process.Accept(visitor);
      if (this._end != null)
        this._end.Accept(visitor);
      if (this._dynamicParams == null)
        return;
      this._dynamicParams.Accept(visitor);
    }

    internal override void Accept(ParseTreeVisitor visitor) => this.AcceptInternal(visitor, false);
  }
}
