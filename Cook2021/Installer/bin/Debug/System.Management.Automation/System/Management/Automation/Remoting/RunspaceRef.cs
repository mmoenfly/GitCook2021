// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.RunspaceRef
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation.Remoting
{
  internal class RunspaceRef
  {
    private ObjectRef<Runspace> _runspaceRef;

    internal RunspaceRef(Runspace runspace) => this._runspaceRef = new ObjectRef<Runspace>(runspace);

    internal void Revert() => this._runspaceRef.Revert();

    internal Runspace Runspace => this._runspaceRef.Value;

    internal bool IsRunspaceOverridden => this._runspaceRef.IsOverridden;

    private PSCommand ParsePsCommandUsingScriptBlock(string line, bool? useLocalScope)
    {
      try
      {
        ExecutionContext executionContext = this._runspaceRef.OldValue.ExecutionContext;
        return ScriptBlock.Create(executionContext, line).GetPowerShell(executionContext, useLocalScope).Commands;
      }
      catch (ScriptBlockToPowerShellNotSupportedException ex)
      {
        CommandProcessorBase.CheckForSevereException((Exception) ex);
      }
      catch (RuntimeException ex)
      {
        CommandProcessorBase.CheckForSevereException((Exception) ex);
      }
      return (PSCommand) null;
    }

    internal PSCommand CreatePsCommand(string line, bool isScript, bool? useNewScope) => !this.IsRunspaceOverridden ? this.CreatePsCommandNotOverriden(line, isScript, useNewScope) : this.ParsePsCommandUsingScriptBlock(line, useNewScope) ?? this.CreatePsCommandNotOverriden(line, isScript, useNewScope);

    private PSCommand CreatePsCommandNotOverriden(
      string line,
      bool isScript,
      bool? useNewScope)
    {
      PSCommand psCommand = new PSCommand();
      if (isScript)
      {
        if (useNewScope.HasValue)
          psCommand.AddScript(line, useNewScope.Value);
        else
          psCommand.AddScript(line);
      }
      else if (useNewScope.HasValue)
        psCommand.AddCommand(line, useNewScope.Value);
      else
        psCommand.AddCommand(line);
      return psCommand;
    }

    internal Pipeline CreatePipeline(
      string line,
      bool addToHistory,
      bool useNestedPipelines)
    {
      Pipeline pipeline = (Pipeline) null;
      if (this.IsRunspaceOverridden)
      {
        PSCommand usingScriptBlock = this.ParsePsCommandUsingScriptBlock(line, new bool?());
        if (usingScriptBlock != null)
        {
          pipeline = useNestedPipelines ? this._runspaceRef.Value.CreateNestedPipeline(usingScriptBlock.Commands[0].CommandText, addToHistory) : this._runspaceRef.Value.CreatePipeline(usingScriptBlock.Commands[0].CommandText, addToHistory);
          pipeline.Commands.Clear();
          foreach (Command command in (Collection<Command>) usingScriptBlock.Commands)
            pipeline.Commands.Add(command);
        }
      }
      if (pipeline == null)
        pipeline = useNestedPipelines ? this._runspaceRef.Value.CreateNestedPipeline(line, addToHistory) : this._runspaceRef.Value.CreatePipeline(line, addToHistory);
      pipeline.SetHistoryString(line);
      return pipeline;
    }

    internal Pipeline CreatePipeline() => this._runspaceRef.Value.CreatePipeline();

    internal Pipeline CreateNestedPipeline() => this._runspaceRef.Value.CreateNestedPipeline();

    internal void Override(RemoteRunspace remoteRunspace)
    {
      bool isRunspacePushed = false;
      this.Override(remoteRunspace, (object) null, out isRunspacePushed);
    }

    internal void Override(
      RemoteRunspace remoteRunspace,
      object syncObject,
      out bool isRunspacePushed)
    {
      try
      {
        if (syncObject != null)
        {
          lock (syncObject)
          {
            this._runspaceRef.Override((Runspace) remoteRunspace);
            isRunspacePushed = true;
          }
        }
        else
        {
          this._runspaceRef.Override((Runspace) remoteRunspace);
          isRunspacePushed = true;
        }
        using (PowerShell powerShell = PowerShell.Create())
        {
          powerShell.AddCommand("Get-Command");
          powerShell.AddParameter("Name", (object) new string[2]
          {
            "Out-Default",
            "Exit-PSSession"
          });
          powerShell.Runspace = this._runspaceRef.Value;
          bool flag = this._runspaceRef.Value.GetRemoteProtocolVersion() == RemotingConstants.ProtocolVersionWin7RC;
          powerShell.IsGetCommandMetadataSpecialPipeline = !flag;
          int num = flag ? 2 : 3;
          Collection<PSObject> collection = powerShell.Invoke();
          if (powerShell.Streams.Error.Count > 0 || collection.Count < num)
            throw RemoteHostExceptions.NewRemoteRunspaceDoesNotSupportPushRunspaceException();
        }
      }
      catch (Exception ex)
      {
        this._runspaceRef.Revert();
        throw;
      }
    }
  }
}
