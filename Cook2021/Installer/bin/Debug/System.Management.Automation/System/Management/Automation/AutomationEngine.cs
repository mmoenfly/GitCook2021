// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.AutomationEngine
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  internal class AutomationEngine
  {
    internal Parser EngineParser;
    private ExecutionContext _context;
    private CommandDiscovery commandDiscovery;

    internal ExecutionContext Context => this._context;

    internal CommandDiscovery CommandDiscovery => this.commandDiscovery;

    internal AutomationEngine(
      PSHost hostInterface,
      RunspaceConfiguration runspaceConfiguration,
      InitialSessionState iss)
    {
      this._context = runspaceConfiguration == null ? new ExecutionContext(this, hostInterface, iss) : new ExecutionContext(this, hostInterface, runspaceConfiguration);
      this.EngineParser = new Parser();
      this.commandDiscovery = new CommandDiscovery(this._context);
      if (runspaceConfiguration != null)
        runspaceConfiguration.Bind(this._context);
      else
        iss.Bind(this._context, false);
      try
      {
        bool flag1 = true;
        if (this._context.EngineSessionState.ProviderCount > 0)
        {
          if (this._context.EngineSessionState.CurrentDrive == (PSDriveInfo) null)
          {
            bool flag2 = false;
            try
            {
              Collection<PSDriveInfo> drives = this._context.EngineSessionState.GetSingleProvider(this._context.ProviderNames.FileSystem).Drives;
              if (drives != null)
              {
                if (drives.Count > 0)
                {
                  this._context.EngineSessionState.CurrentDrive = drives[0];
                  flag2 = true;
                }
              }
            }
            catch (ProviderNotFoundException ex)
            {
            }
            if (!flag2)
            {
              Collection<PSDriveInfo> collection = this._context.EngineSessionState.Drives((string) null);
              if (collection != null && collection.Count > 0)
              {
                this._context.EngineSessionState.CurrentDrive = collection[0];
              }
              else
              {
                this._context.ReportEngineStartupError((Exception) new ItemNotFoundException(Environment.CurrentDirectory, "PathNotFound"));
                flag1 = false;
              }
            }
          }
          if (flag1)
            this._context.EngineSessionState.SetLocation(Environment.CurrentDirectory, new CmdletProviderContext(this._context));
        }
        this._context.EngineSessionState.SetVariableAtScope((PSVariable) new QuestionMarkVariable(this._context), "global", true, CommandOrigin.Internal);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
    }

    internal string Expand(string s) => (string) this.Execute(StringTokenReader.ExpandStringToFormatExpression((IStringTokenReaderHelper2) new StringTokenReaderRunTimeHelper(this.EngineParser), (Token) null, s), (Array) null);

    internal object Execute(string script) => this.Execute(script, (Array) null);

    internal object Execute(string script, Array input) => this.Execute(this.EngineParser.Parse(script, 0), input);

    internal object Execute(ParseTreeNode ptn, Array input)
    {
      object obj = (object) AutomationNull.Value;
      ActivationRecordBuilder activationRecordBuilder = new ActivationRecordBuilder();
      ptn.Accept((ParseTreeVisitor) activationRecordBuilder);
      ActivationRecord activationRecord = this._context.EngineSessionState.CurrentActivationRecord;
      try
      {
        this._context.EngineSessionState.CurrentActivationRecord = new ActivationRecord(activationRecordBuilder.PipelineSlots, activationRecordBuilder.VariableSlots, this._context.EngineSessionState.CurrentScope);
        obj = ptn.Execute(input, this._context);
      }
      catch (FlowControlException ex)
      {
      }
      finally
      {
        this._context.EngineSessionState.CurrentActivationRecord = activationRecord;
      }
      return obj;
    }

    internal ScriptBlock ParseScriptBlock(string script, bool interactiveCommand)
    {
      ScriptBlockNode scriptBlock = this.EngineParser.ParseScriptBlock(script, interactiveCommand);
      if (interactiveCommand)
        this.EngineParser.SetPreviousFirstLastToken(this._context);
      return (ScriptBlock) scriptBlock.Execute(this._context);
    }
  }
}
