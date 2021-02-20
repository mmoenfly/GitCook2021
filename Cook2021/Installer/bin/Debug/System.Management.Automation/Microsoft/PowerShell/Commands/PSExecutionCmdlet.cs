// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.PSExecutionCmdlet
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  public abstract class PSExecutionCmdlet : PSRemotingBaseCmdlet
  {
    protected const string FilePathComputerNameParameterSet = "FilePathComputerName";
    protected const string FilePathSessionParameterSet = "FilePathRunspace";
    protected const string FilePathUriParameterSet = "FilePathUri";
    private PSObject inputObject = AutomationNull.Value;
    private ScriptBlock scriptBlock;
    private string filePath;
    private object[] args;
    private List<IThrottleOperation> operations = new List<IThrottleOperation>();
    private System.Management.Automation.PowerShell powershell;

    [Parameter(ValueFromPipeline = true)]
    public PSObject InputObject
    {
      get => this.inputObject;
      set => this.inputObject = value;
    }

    public virtual ScriptBlock ScriptBlock
    {
      get => this.scriptBlock;
      set => this.scriptBlock = value;
    }

    [Parameter(Mandatory = true, ParameterSetName = "FilePathComputerName", Position = 1)]
    [Parameter(Mandatory = true, ParameterSetName = "FilePathRunspace", Position = 1)]
    [Parameter(Mandatory = true, ParameterSetName = "FilePathUri", Position = 1)]
    [ValidateNotNull]
    public virtual string FilePath
    {
      get => this.filePath;
      set => this.filePath = value;
    }

    [Alias(new string[] {"Args"})]
    [Parameter]
    public object[] ArgumentList
    {
      get => this.args;
      set => this.args = value;
    }

    protected virtual void CreateHelpersForSpecifiedComputerNames()
    {
      this.ValidateComputerName(this.ResolvedComputerNames);
      for (int index = 0; index < this.ResolvedComputerNames.Length; ++index)
      {
        RemoteRunspace runspace;
        try
        {
          WSManConnectionInfo connectionInfo;
          if (this.CertificateThumbprint == null)
          {
            connectionInfo = new WSManConnectionInfo(this.UseSSL.IsPresent, this.ResolvedComputerNames[index], this.Port, this.ApplicationName, this.ConfigurationName, this.Credential);
          }
          else
          {
            connectionInfo = new WSManConnectionInfo(WSManConnectionInfo.ConstructUri(this.UseSSL.IsPresent ? WSManConnectionInfo.DEFAULT_SSL_SCHEME : string.Empty, this.ResolvedComputerNames[index], this.Port, this.ApplicationName), this.ConfigurationName, this.CertificateThumbprint);
            connectionInfo.UseDefaultWSManPort = true;
          }
          connectionInfo.AuthenticationMechanism = this.Authentication;
          this.UpdateConnectionInfo(connectionInfo);
          runspace = (RemoteRunspace) RunspaceFactory.CreateRunspace((RunspaceConnectionInfo) connectionInfo, this.Host, Utils.GetTypeTableFromExecutionContextTLS(), this.SessionOption.ApplicationArguments);
          runspace.Events.ReceivedEvents.PSEventReceived += new PSEventReceivedEventHandler(this.OnRunspacePSEventReceived);
        }
        catch (UriFormatException ex)
        {
          this.WriteError(new ErrorRecord((Exception) ex, "CreateRemoteRunspaceFailed", ErrorCategory.InvalidArgument, (object) this.ResolvedComputerNames[index]));
          continue;
        }
        Pipeline pipeline = this.CreatePipeline(runspace);
        this.Operations.Add((IThrottleOperation) new ExecutionCmdletHelperComputerName(runspace, pipeline));
      }
    }

    protected void CreateHelpersForSpecifiedRunspaces()
    {
      int length = this.Session.Length;
      RemoteRunspace[] remoteRunspaceArray = new RemoteRunspace[length];
      for (int index = 0; index < length; ++index)
        remoteRunspaceArray[index] = (RemoteRunspace) this.Session[index].Runspace;
      Pipeline[] pipelineArray = new Pipeline[length];
      for (int index = 0; index < length; ++index)
      {
        pipelineArray[index] = this.CreatePipeline(remoteRunspaceArray[index]);
        this.Operations.Add((IThrottleOperation) new ExecutionCmdletHelperRunspace(pipelineArray[index]));
      }
    }

    protected void CreateHelpersForSpecifiedUris()
    {
      for (int index = 0; index < this.ConnectionUri.Length; ++index)
      {
        RemoteRunspace runspace;
        try
        {
          WSManConnectionInfo connectionInfo = this.CertificateThumbprint != null ? new WSManConnectionInfo(this.ConnectionUri[index], this.ConfigurationName, this.CertificateThumbprint) : new WSManConnectionInfo(this.ConnectionUri[index], this.ConfigurationName, this.Credential);
          connectionInfo.AuthenticationMechanism = this.Authentication;
          this.UpdateConnectionInfo(connectionInfo);
          runspace = (RemoteRunspace) RunspaceFactory.CreateRunspace((RunspaceConnectionInfo) connectionInfo, this.Host, Utils.GetTypeTableFromExecutionContextTLS(), this.SessionOption.ApplicationArguments);
          runspace.Events.ReceivedEvents.PSEventReceived += new PSEventReceivedEventHandler(this.OnRunspacePSEventReceived);
        }
        catch (UriFormatException ex)
        {
          this.WriteErrorCreateRemoteRunspaceFailed((Exception) ex, this.ConnectionUri[index]);
          continue;
        }
        catch (InvalidOperationException ex)
        {
          this.WriteErrorCreateRemoteRunspaceFailed((Exception) ex, this.ConnectionUri[index]);
          continue;
        }
        catch (ArgumentException ex)
        {
          this.WriteErrorCreateRemoteRunspaceFailed((Exception) ex, this.ConnectionUri[index]);
          continue;
        }
        Pipeline pipeline = this.CreatePipeline(runspace);
        this.Operations.Add((IThrottleOperation) new ExecutionCmdletHelperComputerName(runspace, pipeline));
      }
    }

    internal Pipeline CreatePipeline(RemoteRunspace remoteRunspace)
    {
      Pipeline pipeline = remoteRunspace.CreatePipeline(this.powershell.Commands.Commands[0].CommandText, true);
      pipeline.Commands.Clear();
      foreach (Command command in (Collection<Command>) this.powershell.Commands.Commands)
        pipeline.Commands.Add(command);
      pipeline.RedirectShellErrorOutputPipe = true;
      return pipeline;
    }

    internal void OnRunspacePSEventReceived(object sender, PSEventArgs e)
    {
      if (this.Events == null)
        return;
      this.Events.AddForwardedEvent(e);
    }

    internal List<IThrottleOperation> Operations => this.operations;

    protected void CloseAllInputStreams()
    {
      foreach (ExecutionCmdletHelper operation in this.Operations)
        operation.Pipeline.Input.Close();
    }

    private void WriteErrorCreateRemoteRunspaceFailed(Exception e, Uri uri) => this.WriteError(new ErrorRecord(e, "CreateRemoteRunspaceFailed", ErrorCategory.InvalidArgument, (object) uri));

    protected ScriptBlock GetScriptBlockFromFile(string filePath)
    {
      if (WildcardPattern.ContainsWildcardCharacters(filePath))
        throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.WildCardErrorFilePathParameter), nameof (filePath));
      string path = filePath.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase) ? new PathResolver().ResolveProviderAndPath(filePath, (PSCmdlet) this, false, "RemotingErrorIdStrings", PSRemotingErrorId.FilePathNotFromFileSystemProvider.ToString()) : throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.FilePathShouldPS1Extension), nameof (filePath));
      ExternalScriptInfo externalScriptInfo = new ExternalScriptInfo(filePath, path, this.Context);
      this.Context.AuthorizationManager.ShouldRunInternal((CommandInfo) externalScriptInfo, CommandOrigin.Internal, (PSHost) this.Context.EngineHostInterface);
      return externalScriptInfo.ScriptBlock;
    }

    protected override void BeginProcessing()
    {
      base.BeginProcessing();
      if (this.filePath != null)
        this.scriptBlock = this.GetScriptBlockFromFile(this.filePath);
      try
      {
        this.powershell = this.scriptBlock.GetPowerShell(this.args);
      }
      catch (ScriptBlockToPowerShellNotSupportedException ex)
      {
        if (this.MyInvocation.ExpectingInput)
          this.scriptBlock = this.scriptBlock.GetWithInputHandlingForInvokeCommand();
        this.powershell = System.Management.Automation.PowerShell.Create().AddScript(this.scriptBlock.ToString());
        if (this.args != null)
        {
          foreach (object obj in this.args)
            this.powershell.AddArgument(obj);
        }
      }
      switch (this.ParameterSetName)
      {
        case "FilePathComputerName":
        case "ComputerName":
          string[] resolvedComputerNames = (string[]) null;
          this.ResolveComputerNames(this.ComputerName, out resolvedComputerNames);
          this.ResolvedComputerNames = resolvedComputerNames;
          this.CreateHelpersForSpecifiedComputerNames();
          break;
        case "FilePathRunspace":
        case "Session":
          this.ValidateRemoteRunspacesSpecified();
          this.CreateHelpersForSpecifiedRunspaces();
          break;
        case "FilePathUri":
        case "Uri":
          this.CreateHelpersForSpecifiedUris();
          break;
      }
    }
  }
}
