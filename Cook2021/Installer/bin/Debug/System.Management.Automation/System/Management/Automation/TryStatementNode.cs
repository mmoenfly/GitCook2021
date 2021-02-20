// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.TryStatementNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  internal sealed class TryStatementNode : ParseTreeNode
  {
    private readonly ParseTreeNode _body;
    private readonly ReadOnlyCollection<ExceptionHandlerNode> _catchBlocks;
    private readonly ParseTreeNode _finally;
    private bool _validated;

    internal TryStatementNode(
      Token token,
      ParseTreeNode body,
      ExceptionHandlerNode[] catches,
      ParseTreeNode finallyBlock)
    {
      this.NodeToken = token;
      this._body = body;
      this._catchBlocks = new ReadOnlyCollection<ExceptionHandlerNode>((IList<ExceptionHandlerNode>) catches);
      this._finally = finallyBlock;
    }

    internal override void Execute(
      Array input,
      Pipe outputPipe,
      ref ArrayList resultList,
      ExecutionContext context)
    {
      this.CheckForInterrupts(context);
      bool enclosingStatementBlock = context.ExceptionHandlerInEnclosingStatementBlock;
      context.ExceptionHandlerInEnclosingStatementBlock = true;
      try
      {
        this._body.Execute(input, outputPipe, ref resultList, context);
      }
      catch (RuntimeException ex)
      {
        ExceptionHandlerNode exceptionHandlerNode = (ExceptionHandlerNode) null;
        Exception exception = ex.InnerException;
        if (exception != null)
          exceptionHandlerNode = ExceptionHandlerNode.GetHandler(this._catchBlocks, exception, context);
        if (exceptionHandlerNode == null || exceptionHandlerNode.ExceptionTypes == null)
        {
          exception = (Exception) ex;
          exceptionHandlerNode = ExceptionHandlerNode.GetHandler(this._catchBlocks, exception, context);
        }
        if (exceptionHandlerNode != null)
        {
          Exception exceptionBeingHandled = context.CurrentExceptionBeingHandled;
          try
          {
            context.CurrentExceptionBeingHandled = exception;
            ErrorRecord errorRecord = ex.ErrorRecord;
            exceptionHandlerNode.Invoke(new ErrorRecord(errorRecord, exception), outputPipe, ref resultList);
          }
          finally
          {
            context.CurrentExceptionBeingHandled = exceptionBeingHandled;
          }
        }
        else
          throw;
      }
      finally
      {
        context.ExceptionHandlerInEnclosingStatementBlock = enclosingStatementBlock;
        if (this._finally != null)
        {
          LocalPipeline currentlyRunningPipeline = (LocalPipeline) context.CurrentRunspace.GetCurrentlyRunningPipeline();
          bool isStopping = currentlyRunningPipeline.Stopper.IsStopping;
          currentlyRunningPipeline.Stopper.IsStopping = false;
          try
          {
            this._finally.Execute(input, outputPipe, ref resultList, context);
          }
          finally
          {
            currentlyRunningPipeline.Stopper.IsStopping = isStopping;
          }
        }
      }
    }

    internal void Validate(Parser parser)
    {
      if (!this._validated && this._catchBlocks.Count > 1)
      {
        Exception exception = (Exception) null;
        try
        {
          for (int index1 = 0; index1 < this._catchBlocks.Count - 1; ++index1)
          {
            ExceptionHandlerNode catchBlock1 = this._catchBlocks[index1];
            for (int index2 = index1 + 1; index2 < this._catchBlocks.Count; ++index2)
            {
              ExceptionHandlerNode catchBlock2 = this._catchBlocks[index2];
              if (catchBlock1.ExceptionTypes == null)
              {
                if (parser != null)
                {
                  parser.ReportException((object) null, typeof (ParseException), catchBlock1.NodeToken, "EmptyCatchNotLast");
                  break;
                }
                exception = (Exception) InterpreterError.NewInterpreterException((object) null, typeof (RuntimeException), catchBlock1.NodeToken, "EmptyCatchNotLast");
                break;
              }
              if (catchBlock2.ExceptionTypes != null)
              {
                foreach (TypeLiteral exceptionType1 in (Collection<TypeLiteral>) catchBlock1.ExceptionTypes)
                {
                  Type type1 = exceptionType1.Type;
                  foreach (TypeLiteral exceptionType2 in (Collection<TypeLiteral>) catchBlock2.ExceptionTypes)
                  {
                    Type type2 = exceptionType2.Type;
                    if (type1 == type2 || type2.IsSubclassOf(type1))
                    {
                      if (parser != null)
                        parser.ReportException((object) null, typeof (ParseException), exceptionType2.Token, "ExceptionTypeAlreadyCaught", (object) type2);
                      else
                        exception = (Exception) InterpreterError.NewInterpreterException((object) null, typeof (RuntimeException), exceptionType2.Token, "ExceptionTypeAlreadyCaught", (object) type2);
                    }
                  }
                }
              }
            }
          }
        }
        catch (ParseException ex)
        {
          throw;
        }
        catch (RuntimeException ex)
        {
          return;
        }
        if (exception != null)
          throw exception;
      }
      this._validated = true;
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      this._body.Accept(visitor);
      foreach (ParseTreeNode catchBlock in this._catchBlocks)
        catchBlock.Accept(visitor);
      if (this._finally == null)
        return;
      this._finally.Accept(visitor);
    }
  }
}
