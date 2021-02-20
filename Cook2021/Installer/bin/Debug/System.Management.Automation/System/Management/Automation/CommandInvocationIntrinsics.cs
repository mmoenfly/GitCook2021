// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandInvocationIntrinsics
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  public class CommandInvocationIntrinsics
  {
    [TraceSource("CommandInvocationIntrinsics", "CommandInvocationIntrinsics")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (CommandInvocationIntrinsics), nameof (CommandInvocationIntrinsics));
    private ExecutionContext _context;
    private PSCmdlet _cmdlet;
    private MshCommandRuntime commandRuntime;

    internal CommandInvocationIntrinsics(ExecutionContext context, PSCmdlet cmdlet)
    {
      this._context = context;
      this._cmdlet = cmdlet;
      this.commandRuntime = cmdlet.CommandRuntime as MshCommandRuntime;
    }

    internal CommandInvocationIntrinsics(ExecutionContext context) => this._context = context;

    public string ExpandString(string source)
    {
      if (this._cmdlet != null)
        this._cmdlet.ThrowIfStopping();
      return this._context.Engine.Expand(source);
    }

    public CommandInfo GetCommand(string commandName, CommandTypes type)
    {
      CommandInfo commandInfo = (CommandInfo) null;
      CommandSearcher commandSearcher = new CommandSearcher(commandName, SearchResolutionOptions.None, type, this._context);
      while (true)
      {
        try
        {
          if (!commandSearcher.MoveNext())
            break;
        }
        catch (ArgumentException ex)
        {
          continue;
        }
        catch (PathTooLongException ex)
        {
          continue;
        }
        catch (FileLoadException ex)
        {
          continue;
        }
        catch (MetadataException ex)
        {
          continue;
        }
        catch (FormatException ex)
        {
          continue;
        }
        commandInfo = ((IEnumerator<CommandInfo>) commandSearcher).Current;
      }
      return commandInfo;
    }

    public CmdletInfo GetCmdlet(string commandName) => CommandInvocationIntrinsics.GetCmdlet(commandName, this._context);

    internal static CmdletInfo GetCmdlet(string commandName, ExecutionContext context)
    {
      CmdletInfo cmdletInfo = (CmdletInfo) null;
      CommandSearcher commandSearcher = new CommandSearcher(commandName, SearchResolutionOptions.None, CommandTypes.Cmdlet, context);
      while (true)
      {
        try
        {
          if (!commandSearcher.MoveNext())
            break;
        }
        catch (ArgumentException ex)
        {
          continue;
        }
        catch (PathTooLongException ex)
        {
          continue;
        }
        catch (FileLoadException ex)
        {
          continue;
        }
        catch (MetadataException ex)
        {
          continue;
        }
        catch (FormatException ex)
        {
          continue;
        }
        cmdletInfo = ((IEnumerator) commandSearcher).Current as CmdletInfo;
      }
      return cmdletInfo;
    }

    public List<CmdletInfo> GetCmdlets() => this.GetCmdlets("*");

    public List<CmdletInfo> GetCmdlets(string pattern)
    {
      if (pattern == null)
        throw CommandInvocationIntrinsics.tracer.NewArgumentNullException(nameof (pattern));
      List<CmdletInfo> cmdletInfoList = new List<CmdletInfo>();
      CommandSearcher commandSearcher = new CommandSearcher(pattern, SearchResolutionOptions.CommandNameIsPattern, CommandTypes.Cmdlet, this._context);
      while (true)
      {
        do
        {
          try
          {
            if (!commandSearcher.MoveNext())
              goto label_11;
          }
          catch (ArgumentException ex)
          {
          }
          catch (PathTooLongException ex)
          {
          }
          catch (FileLoadException ex)
          {
          }
          catch (MetadataException ex)
          {
          }
          catch (FormatException ex)
          {
          }
        }
        while (!(((IEnumerator) commandSearcher).Current is CmdletInfo current));
        cmdletInfoList.Add(current);
      }
label_11:
      return cmdletInfoList;
    }

    public List<string> GetCommandName(string name, bool nameIsPattern, bool returnFullName)
    {
      if (name == null)
        throw CommandInvocationIntrinsics.tracer.NewArgumentNullException(nameof (name));
      List<CommandInfo> commandInfoList = new List<CommandInfo>();
      List<string> stringList = new List<string>();
      CommandSearcher commandSearcher = new CommandSearcher(name, nameIsPattern ? SearchResolutionOptions.ResolveAliasPatterns | SearchResolutionOptions.ResolveFunctionPatterns | SearchResolutionOptions.CommandNameIsPattern : SearchResolutionOptions.None, CommandTypes.All, this._context);
label_3:
      string extension;
      while (true)
      {
        do
        {
          do
          {
            try
            {
              if (!commandSearcher.MoveNext())
                goto label_28;
            }
            catch (ArgumentException ex)
            {
              goto label_3;
            }
            catch (PathTooLongException ex)
            {
              goto label_3;
            }
            catch (FileLoadException ex)
            {
              goto label_3;
            }
            catch (MetadataException ex)
            {
              goto label_3;
            }
            catch (FormatException ex)
            {
              goto label_3;
            }
          }
          while (!(((IEnumerator) commandSearcher).Current is CommandInfo current));
          if (current.CommandType == CommandTypes.Application)
            extension = Path.GetExtension(current.Name);
          else
            goto label_23;
        }
        while (string.IsNullOrEmpty(extension));
        break;
label_23:
        if (current.CommandType == CommandTypes.ExternalScript)
        {
          if (returnFullName)
            stringList.Add(current.Definition);
          else
            stringList.Add(current.Name);
        }
        else
          stringList.Add(current.Name);
      }
      using (IEnumerator<string> enumerator = this._context.CommandDiscovery.PathExtensions.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          if (enumerator.Current.Equals(extension, StringComparison.OrdinalIgnoreCase))
          {
            if (returnFullName)
              stringList.Add(current.Definition);
            else
              stringList.Add(current.Name);
          }
        }
        goto label_3;
      }
label_28:
      return stringList;
    }

    public Collection<PSObject> InvokeScript(string script) => this.InvokeScript(script, true, PipelineResultTypes.None, (IList) null);

    public Collection<PSObject> InvokeScript(string script, params object[] args) => this.InvokeScript(script, true, PipelineResultTypes.None, (IList) args);

    public Collection<PSObject> InvokeScript(
      SessionState sessionState,
      ScriptBlock scriptBlock,
      params object[] args)
    {
      if (scriptBlock == null)
        throw CommandInvocationIntrinsics.tracer.NewArgumentNullException(nameof (scriptBlock));
      if (sessionState == null)
        throw CommandInvocationIntrinsics.tracer.NewArgumentNullException(nameof (sessionState));
      SessionStateInternal engineSessionState = this._context.EngineSessionState;
      try
      {
        this._context.EngineSessionState = sessionState.Internal;
        return this.InvokeScript(scriptBlock, false, PipelineResultTypes.None, (IList) null, args);
      }
      finally
      {
        this._context.EngineSessionState = engineSessionState;
      }
    }

    public Collection<PSObject> InvokeScript(
      string script,
      bool useNewScope,
      PipelineResultTypes writeToPipeline,
      IList input,
      params object[] args)
    {
      if (script == null)
        throw new ArgumentNullException(nameof (script));
      return this.InvokeScript(ScriptBlock.Create(this._context, script), useNewScope, writeToPipeline, input, args);
    }

    private Collection<PSObject> InvokeScript(
      ScriptBlock sb,
      bool useNewScope,
      PipelineResultTypes writeToPipeline,
      IList input,
      params object[] args)
    {
      if (this._cmdlet != null)
        this._cmdlet.ThrowIfStopping();
      Cmdlet contextCmdlet = (Cmdlet) null;
      bool writeErrors = false;
      if ((writeToPipeline & PipelineResultTypes.Output) == PipelineResultTypes.Output)
      {
        contextCmdlet = (Cmdlet) this._cmdlet;
        writeToPipeline &= ~PipelineResultTypes.Output;
      }
      if ((writeToPipeline & PipelineResultTypes.Error) == PipelineResultTypes.Error)
      {
        writeErrors = true;
        writeToPipeline &= ~PipelineResultTypes.Error;
      }
      if (writeToPipeline != PipelineResultTypes.None)
        throw CommandInvocationIntrinsics.tracer.NewNotImplementedException();
      object obj = sb.InvokeUsingCmdlet(contextCmdlet, useNewScope, writeErrors, (object) AutomationNull.Value, (object) input, (object) AutomationNull.Value, args);
      if (obj == AutomationNull.Value)
        return new Collection<PSObject>();
      if (obj is Collection<PSObject> collection)
        return collection;
      Collection<PSObject> collection1 = new Collection<PSObject>();
      IEnumerator enumerator = LanguagePrimitives.GetEnumerator(obj);
      if (enumerator != null)
      {
        while (enumerator.MoveNext())
        {
          object current = enumerator.Current;
          collection1.Add(LanguagePrimitives.AsPSObjectOrNull(current));
        }
      }
      else
        collection1.Add(LanguagePrimitives.AsPSObjectOrNull(obj));
      return collection1;
    }

    public ScriptBlock NewScriptBlock(string scriptText)
    {
      if (this.commandRuntime != null)
        this.commandRuntime.ThrowIfStopping();
      return ScriptBlock.Create(this._context, scriptText);
    }
  }
}
