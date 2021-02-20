// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RedirectionNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  internal class RedirectionNode : ParseTreeNode
  {
    private readonly ParseTreeNode _location;
    private readonly Token _token;

    internal RedirectionNode(Token token)
      : this(token, (ParseTreeNode) null)
    {
    }

    internal RedirectionNode(Token token, ParseTreeNode location)
    {
      this._token = token;
      this.NodeToken = token;
      this._location = location;
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      if (this._location == null)
        return;
      this._location.Accept(visitor);
    }

    internal ParseTreeNode Location => this._location;

    internal Token Token => this._token;

    internal bool IsError => this._token.TokenText[0] == '2';

    internal static bool IsMerging(Token token)
    {
      switch (token.TokenText)
      {
        case "2>&1":
        case "1>&2":
          return true;
        default:
          return false;
      }
    }

    internal bool Merging => RedirectionNode.IsMerging(this._token);

    internal bool Appending
    {
      get
      {
        switch (this._token.TokenText)
        {
          case "1>>":
          case "2>>":
          case ">>":
            return true;
          default:
            return false;
        }
      }
    }

    internal Pipe GetRedirectionPipe(ExecutionContext context)
    {
      if (this._location == null)
        return (Pipe) null;
      string stringParser = PSObject.ToStringParser(context, this._location.Execute(context));
      if (string.IsNullOrEmpty(stringParser))
        return new Pipe() { NullPipe = true };
      PipelineProcessor outputPipeline = this.BuildRedirectionPipeline(stringParser, context);
      return new Pipe(context, outputPipeline);
    }

    private PipelineProcessor BuildRedirectionPipeline(
      string path,
      ExecutionContext context)
    {
      CommandProcessorBase command = context.CreateCommand("out-file");
      command.AddParameter("-encoding", (object) "unicode");
      if (this.Appending)
        command.AddParameter("-append", (object) true);
      command.AddParameter("-filepath", (object) path);
      PipelineProcessor pipelineProcessor = new PipelineProcessor();
      pipelineProcessor.Add(command);
      try
      {
        pipelineProcessor.StartStepping(true);
      }
      catch (RuntimeException ex)
      {
        if (ex.ErrorRecord.Exception is ArgumentException)
          throw InterpreterError.NewInterpreterExceptionWithInnerException((object) null, typeof (RuntimeException), this._token, "RedirectionFailed", ex.ErrorRecord.Exception, (object) path, (object) ex.ErrorRecord.Exception.Message);
        ex.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, this._token, context));
        throw ex;
      }
      return pipelineProcessor;
    }
  }
}
