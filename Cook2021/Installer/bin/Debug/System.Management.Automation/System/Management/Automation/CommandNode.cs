// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandNode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  internal class CommandNode : ParseTreeNode
  {
    private ArrayList _elements = new ArrayList();
    private readonly ParseTreeNode _expression;
    private RedirectionNode _errorRedirection;
    private RedirectionNode _outputRedirection;

    internal CommandNode()
    {
    }

    internal CommandNode(ParseTreeNode expression, Token token)
    {
      this._expression = expression;
      this.NodeToken = token;
    }

    public ArrayList Elements => this._elements;

    internal ParseTreeNode Expression => this._expression;

    internal bool IsPureExpression => this._expression != null && this._errorRedirection == null && this._outputRedirection == null;

    internal bool IsValid(Parser parser)
    {
      if (this._expression != null)
        return true;
      if (this._elements.Count == 0)
        return false;
      if (this._elements[this._elements.Count - 1] is Token element && element.Is(TokenId.CommaToken))
        parser.ReportException((object) null, typeof (IncompleteParseException), element, "MissingExpression", (object) ',');
      this.NodeToken = this._elements[0] as Token;
      return true;
    }

    internal override void Accept(ParseTreeVisitor visitor)
    {
      visitor.Visit(this);
      if (this._expression != null)
      {
        this._expression.Accept(visitor);
      }
      else
      {
        for (int index = 0; index < this._elements.Count; ++index)
        {
          switch (this._elements[index])
          {
            case ParseTreeNode parseTreeNode:
              parseTreeNode.Accept(visitor);
              break;
            case Token node:
              visitor.Visit(node, index);
              break;
          }
        }
      }
      if (this._outputRedirection != null)
        this._outputRedirection.Accept(visitor);
      if (this._errorRedirection == null)
        return;
      this._errorRedirection.Accept(visitor);
    }

    internal RedirectionNode ErrorRedirection => this._errorRedirection;

    internal RedirectionNode OutputRedirection => this._outputRedirection;

    internal void AddRedirection(Parser parser, RedirectionNode redirection)
    {
      if (redirection == null)
        return;
      if (redirection.IsError)
      {
        if (this._errorRedirection != null)
          parser.ReportException((object) null, typeof (ParseException), redirection.Token, "ErrorAlreadyRedirected");
        this._errorRedirection = redirection;
      }
      else
      {
        if (this._outputRedirection != null)
          parser.ReportException((object) null, typeof (ParseException), redirection.Token, "OutputAlreadyRedirected");
        this._outputRedirection = redirection;
      }
    }

    internal CommandProcessorBase AddToPipeline(
      PipelineProcessor pipeline,
      ExecutionContext context)
    {
      if (pipeline == null)
        throw ParseTreeNode.tracer.NewArgumentNullException(nameof (pipeline));
      int index;
      CommandProcessorBase commandProcessor = this.CreateCommandProcessor(out index, context);
      foreach (CommandParameterInternal parametersAndArgument in (IEnumerable<CommandParameterInternal>) this.BindParametersAndArguments(index, context))
        commandProcessor.AddParameter(parametersAndArgument);
      string helpTarget;
      HelpCategory helpCategory;
      if (commandProcessor.IsHelpRequested(out helpTarget, out helpCategory))
      {
        commandProcessor = CommandProcessorBase.CreateGetHelpCommandProcessor(context, helpTarget, helpCategory);
        commandProcessor.Command.MyInvocation.ScriptToken = (Token) this._elements[0];
      }
      pipeline.Add(commandProcessor);
      this.BindRedirectionPipes(commandProcessor, pipeline, context);
      return commandProcessor;
    }

    private CommandProcessorBase CreateCommandProcessor(
      out int index,
      ExecutionContext context)
    {
      index = 0;
      CommandProcessorBase commandProcessorBase = (CommandProcessorBase) null;
      if (this._expression != null)
        return (CommandProcessorBase) new ScriptCommandProcessor(ScriptBlock.CreateSynthesized(this._expression, this.NodeToken), context, false);
      bool createScope;
      SessionStateInternal commandSessionState;
      bool forceSessionState;
      object commandName = this.GetCommandName(out createScope, out index, out commandSessionState, out forceSessionState, context);
      switch (commandName)
      {
        case ScriptBlock scriptblock:
          commandProcessorBase = CommandDiscovery.CreateCommandProcessorForScript(scriptblock, context, createScope);
          if (commandProcessorBase.CommandSessionState == null || forceSessionState)
          {
            commandProcessorBase.CommandSessionState = commandSessionState;
            break;
          }
          break;
        case CommandInfo commandInfo:
          commandProcessorBase = context.CommandDiscovery.LookupCommandProcessor(commandInfo, context.EngineSessionState.currentScope.ScopeOrigin, new bool?(false));
          break;
        case string stringParser:
label_7:
          if (string.IsNullOrEmpty(stringParser))
            throw InterpreterError.NewInterpreterException(commandName, typeof (RuntimeException), (Token) this._elements[0], "BadExpression", (object) ((Token) this._elements[0]).TokenText);
          try
          {
            if (commandSessionState != context.EngineSessionState)
            {
              SessionStateInternal engineSessionState = context.EngineSessionState;
              try
              {
                context.EngineSessionState = commandSessionState;
                commandProcessorBase = context.CreateCommand(stringParser);
                break;
              }
              finally
              {
                context.EngineSessionState = engineSessionState;
              }
            }
            else
            {
              commandProcessorBase = context.CreateCommand(stringParser);
              break;
            }
          }
          catch (ParseException ex)
          {
            throw;
          }
          catch (RuntimeException ex)
          {
            if (ex.ErrorRecord.InvocationInfo == null)
              ex.ErrorRecord.SetInvocationInfo(new InvocationInfo((CommandInfo) null, (Token) this._elements[0], context));
            throw;
          }
        default:
          stringParser = PSObject.ToStringParser(context, commandName);
          goto label_7;
      }
      if (commandProcessorBase.CommandSessionState == null || forceSessionState)
        commandProcessorBase.CommandSessionState = commandSessionState;
      InternalCommand command = commandProcessorBase.Command;
      command.MyInvocation.ScriptToken = (Token) this._elements[0];
      if (context.IsStrictVersion(2) && this._elements.Count == index + 1 && (this._elements[index - 1] is Token && this._elements[index] is ArrayLiteralNode))
      {
        Token element = (Token) this._elements[index - 1];
        if (element.Script[element.End] == '(')
          throw InterpreterError.NewInterpreterException((object) null, typeof (RuntimeException), (Token) this._elements[index - 1], "StrictModeFunctionCallWithParens");
      }
      if (createScope)
      {
        switch (command)
        {
          case ScriptCommand _:
          case PSScriptCmdlet _:
            commandProcessorBase.UseLocalScope = true;
            commandProcessorBase.CommandScope = commandProcessorBase.CommandSessionState == null ? context.EngineSessionState.NewScope(commandProcessorBase.FromScriptFile) : commandProcessorBase.CommandSessionState.NewScope(commandProcessorBase.FromScriptFile);
            goto label_27;
        }
      }
      commandProcessorBase.UseLocalScope = false;
      commandProcessorBase.CommandScope = commandProcessorBase.CommandSessionState.CurrentScope;
label_27:
      commandProcessorBase.Command.CallingToken = (Token) this._elements[0];
      return commandProcessorBase;
    }

    private object GetCommandName(
      out bool createScope,
      out int index,
      out SessionStateInternal commandSessionState,
      out bool forceSessionState,
      ExecutionContext context)
    {
      createScope = true;
      index = 0;
      forceSessionState = false;
      commandSessionState = context.EngineSessionState;
      if (this._elements.Count == 0)
        return (object) null;
      Token element = (Token) this._elements[index++];
      if (element.Is(TokenId.DotToken))
        createScope = false;
      else if (!element.Is(TokenId.AmpersandToken))
        return (object) element.TokenText;
      for (int index1 = 0; index1 < 2; ++index1)
      {
        ParseTreeNode parseTreeNode = (ParseTreeNode) null;
        if (index < this._elements.Count)
          parseTreeNode = this._elements[index++] as ParseTreeNode;
        object obj = parseTreeNode != null ? PSObject.Base(parseTreeNode.Execute(context)) : throw InterpreterError.NewInterpreterException((object) null, typeof (RuntimeException), element, "MissingExpression", (object) element.TokenText);
        if (!(obj is PSModuleInfo psModuleInfo))
          return obj;
        if (psModuleInfo.ModuleType == ModuleType.Binary && psModuleInfo.SessionState == null)
          throw InterpreterError.NewInterpreterException((object) null, typeof (RuntimeException), element, "CantInvokeInBinaryModule", (object) psModuleInfo.Name);
        commandSessionState = psModuleInfo.SessionState.Internal;
        forceSessionState = true;
      }
      return (object) null;
    }

    private void ValidateNoFunctionCalls()
    {
      foreach (ParseTreeNode parseTreeNode in this.EnumeratePreorder())
      {
        if (parseTreeNode is PipelineNode)
          throw new ScriptBlockToPowerShellNotSupportedException("CantConvertWithCommandInvocations", (Exception) null, "AutomationExceptions", "CantConvertWithCommandInvocations", new object[1]
          {
            (object) parseTreeNode.ToString()
          });
      }
    }

    private Command CreateRunspaceCommand(
      out int index,
      ExecutionContext context,
      bool? useLocalScope)
    {
      bool createScope;
      object commandName = this.GetCommandName(out createScope, out index, out SessionStateInternal _, out bool _, context);
      if (!createScope)
        throw new ScriptBlockToPowerShellNotSupportedException("CantConvertWithDotSourcing", (Exception) null, "AutomationExceptions", "CantConvertWithDotSourcing", new object[0]);
      string command = !(commandName is CommandInfo commandInfo) ? commandName as string : commandInfo.Name;
      return !string.IsNullOrEmpty(command) ? new Command(command, false, useLocalScope) : throw new ScriptBlockToPowerShellNotSupportedException("CantConvertWithScriptBlockInvocation", (Exception) null, "AutomationExceptions", "CantConvertWithScriptBlockInvocation", new object[0]);
    }

    internal Command ConvertToRunspaceCommand(ExecutionContext context, bool? useLocalScope)
    {
      this.ValidateNoFunctionCalls();
      int index;
      Command runspaceCommand = this.CreateRunspaceCommand(out index, context, useLocalScope);
      this.BindRedirectionPipes(runspaceCommand);
      foreach (CommandParameterInternal parametersAndArgument in (IEnumerable<CommandParameterInternal>) this.BindParametersAndArguments(index, context))
      {
        CommandParameter commandParameter = CommandParameter.FromCommandParameterInternal(parametersAndArgument);
        runspaceCommand.Parameters.Add(commandParameter);
      }
      return runspaceCommand;
    }

    private void BindRedirectionPipes(
      CommandProcessorBase commandProcessor,
      PipelineProcessor pipeline,
      ExecutionContext context)
    {
      if (this._outputRedirection != null)
      {
        commandProcessor.CommandRuntime.OutputPipe = this._outputRedirection.GetRedirectionPipe(context);
        if (commandProcessor.CommandRuntime.OutputPipe != null && commandProcessor.CommandRuntime.OutputPipe.PipelineProcessor != null)
          pipeline.AddRedirectionPipe(commandProcessor.CommandRuntime.OutputPipe.PipelineProcessor);
      }
      if (this._errorRedirection != null)
      {
        if (this._errorRedirection.Merging)
        {
          commandProcessor.CommandRuntime.MergeMyErrorOutputWithSuccess = true;
        }
        else
        {
          commandProcessor.CommandRuntime.ErrorOutputPipe = this._errorRedirection.GetRedirectionPipe(context);
          if (commandProcessor.CommandRuntime.ErrorOutputPipe == null || commandProcessor.CommandRuntime.ErrorOutputPipe.PipelineProcessor == null)
            return;
          pipeline.AddRedirectionPipe(commandProcessor.CommandRuntime.ErrorOutputPipe.PipelineProcessor);
        }
      }
      else if (context.ShellFunctionErrorOutputPipe != null)
        commandProcessor.CommandRuntime.ErrorOutputPipe = context.ShellFunctionErrorOutputPipe;
      else
        commandProcessor.CommandRuntime.ErrorOutputPipe.ExternalWriter = context.ExternalErrorOutput;
    }

    private void BindRedirectionPipes(Command command, RedirectionNode redirectionNode)
    {
      if (command == null)
        throw ParseTreeNode.tracer.NewArgumentNullException(nameof (command));
      if (redirectionNode == null)
        return;
      if (redirectionNode.Appending || !redirectionNode.Merging || redirectionNode.Location != null)
        throw new ScriptBlockToPowerShellNotSupportedException("CanConvertOneOutputErrorRedir", (Exception) null, "AutomationExceptions", "CanConvertOneOutputErrorRedir", new object[0]);
      PipelineResultTypes myResult;
      PipelineResultTypes toResult;
      if (redirectionNode.IsError)
      {
        myResult = PipelineResultTypes.Error;
        toResult = PipelineResultTypes.Output;
      }
      else
      {
        myResult = PipelineResultTypes.Output;
        toResult = PipelineResultTypes.Error;
      }
      command.MergeMyResults(myResult, toResult);
    }

    private void BindRedirectionPipes(Command command)
    {
      this.BindRedirectionPipes(command, this._errorRedirection);
      this.BindRedirectionPipes(command, this._outputRedirection);
    }

    private List<CommandParameterInternal> BindParametersAndArguments(
      int index,
      ExecutionContext context)
    {
      List<CommandParameterInternal> listOfParameters = new List<CommandParameterInternal>();
      bool flag = false;
      while (index < this._elements.Count)
      {
        if (!flag && this.IsMinusMinus(index))
        {
          flag = true;
          ++index;
        }
        else
        {
          Token parameter = (Token) null;
          if (!flag && this.IsParameter(index))
          {
            parameter = this._elements[index++] as Token;
            if (!parameter.TokenText.EndsWith(":", StringComparison.Ordinal))
            {
              listOfParameters.Add(new CommandParameterInternal((object) parameter));
              continue;
            }
          }
          ArrayList args = this.ReadArguments(ref index, context);
          this.AddArgument(listOfParameters, parameter, args, context);
        }
      }
      return listOfParameters;
    }

    private bool IsMinusMinus(int index) => this._elements[index] is Token element && element.Is(TokenId.MinusMinusToken);

    private bool IsParameter(int index)
    {
      if (!(this._elements[index++] is Token element) || !element.Is(TokenId.ParameterToken))
        return false;
      if (index < this._elements.Count && this._elements[index] is Token element && element.Is(TokenId.CommaToken))
        return false;
      if (element.TokenText.EndsWith(":", StringComparison.Ordinal) && index >= this._elements.Count)
        throw InterpreterError.NewInterpreterException((object) element, typeof (RuntimeException), element, "ParameterRequiresArgument", (object) element);
      return true;
    }

    private ArrayList ReadArguments(ref int index, ExecutionContext context)
    {
      ArrayList arrayList = new ArrayList();
      Token errToken = (Token) null;
      while (index < this._elements.Count)
      {
        Token element1 = this._elements[index] as Token;
        if (element1 != null && element1.Is(TokenId.CommaToken))
          throw InterpreterError.NewInterpreterException((object) element1.TokenText, typeof (RuntimeException), element1, "MissingArgument");
        if (element1 != null && element1.Is(TokenId.SplattedVariableToken))
          errToken = element1;
        object obj = element1 == null || !element1.Is(TokenId.ParameterToken) ? (element1 == null || !element1.Is(TokenId.NumberToken) ? (!(this._elements[index] is ParseTreeNode element) ? (object) element1 : element.Execute(context)) : (object) ParserOps.WrappedNumber(element1.Data, element1.TokenText)) : (object) element1.TokenText;
        arrayList.Add(obj);
        ++index;
        if (index < this._elements.Count && this._elements[index] is Token element && element.Is(TokenId.CommaToken))
          ++index;
        else
          break;
      }
      if (errToken != null && arrayList.Count > 1)
        throw InterpreterError.NewInterpreterException((object) errToken.TokenText, typeof (RuntimeException), errToken, "SplattingNotPermittedInArgumentList", (object) errToken);
      return arrayList;
    }

    private void AddArgument(
      List<CommandParameterInternal> listOfParameters,
      Token parameter,
      ArrayList args,
      ExecutionContext context)
    {
      object obj1 = (object) null;
      if (parameter == null && (args == null || args.Count == 0))
        return;
      if (args != null && args.Count > 0)
      {
        if (args.Count > 1)
        {
          obj1 = (object) args.ToArray();
          ParseTreeNode.tracer.WriteLine("Binding {0} arguments to parameter {0}", (object) ((Array) obj1).Length, parameter == null ? (object) "positional param" : (object) parameter.ToString());
        }
        else
        {
          obj1 = args[0];
          if (obj1 is Token token && token.Is(TokenId.SplattedVariableToken))
          {
            object obj2 = PSObject.Base(context.GetVariable(token.TokenText));
            switch (obj2)
            {
              case IDictionary dictionary:
                IDictionaryEnumerator enumerator1 = dictionary.GetEnumerator();
                try
                {
                  while (enumerator1.MoveNext())
                  {
                    DictionaryEntry current = (DictionaryEntry) enumerator1.Current;
                    string name = current.Key.ToString();
                    object obj3 = current.Value;
                    listOfParameters.Add(new CommandParameterInternal(name, obj3));
                  }
                  return;
                }
                finally
                {
                  if (enumerator1 is IDisposable disposable)
                    disposable.Dispose();
                }
              case IEnumerable enumerable:
                IEnumerator enumerator2 = enumerable.GetEnumerator();
                try
                {
                  while (enumerator2.MoveNext())
                  {
                    object current = enumerator2.Current;
                    listOfParameters.Add(new CommandParameterInternal(current));
                  }
                  return;
                }
                finally
                {
                  if (enumerator2 is IDisposable disposable)
                    disposable.Dispose();
                }
              default:
                listOfParameters.Add(new CommandParameterInternal(obj2));
                return;
            }
          }
          else
            ParseTreeNode.tracer.WriteLine("Binding single argument to parameter {0}", parameter == null ? (object) "positional param" : (object) parameter.ToString());
        }
      }
      if (parameter == null)
        listOfParameters.Add(new CommandParameterInternal(obj1));
      else
        listOfParameters.Add(new CommandParameterInternal(parameter, obj1));
    }
  }
}
