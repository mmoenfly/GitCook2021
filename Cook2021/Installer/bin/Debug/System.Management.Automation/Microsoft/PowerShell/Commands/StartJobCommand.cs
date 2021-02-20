// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.StartJobCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Start", "Job", DefaultParameterSetName = "ComputerName")]
  public class StartJobCommand : PSExecutionCmdlet, IDisposable
  {
    [TraceSource("JobCmdlets", "Job related cmdlets")]
    internal static readonly PSTraceSource tracer = PSTraceSource.GetTracer("JobCmdlets", "Job related cmdlets");
    private string name;
    private ScriptBlock initScript;
    private bool shouldRunAs32;
    private bool firstProcessRecord = true;

    [Parameter(ValueFromPipelineByPropertyName = true)]
    public string Name
    {
      get => this.name;
      set
      {
        using (StartJobCommand.tracer.TraceProperty(value, new object[0]))
        {
          if (string.IsNullOrEmpty(value))
            return;
          this.name = value;
        }
      }
    }

    [Parameter(Mandatory = true, ParameterSetName = "ComputerName", Position = 0)]
    [Alias(new string[] {"Command"})]
    public override ScriptBlock ScriptBlock
    {
      get => base.ScriptBlock;
      set => base.ScriptBlock = value;
    }

    public override PSSession[] Session => (PSSession[]) null;

    public override string[] ComputerName => (string[]) null;

    [System.Management.Automation.Credential]
    [Parameter]
    public override PSCredential Credential
    {
      get => base.Credential;
      set => base.Credential = value;
    }

    public override int Port => 0;

    public override SwitchParameter UseSSL => (SwitchParameter) false;

    public override string ConfigurationName
    {
      get => base.ConfigurationName;
      set => base.ConfigurationName = value;
    }

    public override int ThrottleLimit => 0;

    public override string ApplicationName => (string) null;

    public override Uri[] ConnectionUri => (Uri[]) null;

    [Parameter(ParameterSetName = "FilePathComputerName", Position = 0)]
    [Alias(new string[] {"PSPath"})]
    public override string FilePath
    {
      get => base.FilePath;
      set => base.FilePath = value;
    }

    [Parameter]
    public override AuthenticationMechanism Authentication
    {
      get => base.Authentication;
      set => base.Authentication = value;
    }

    public override string CertificateThumbprint
    {
      get => base.CertificateThumbprint;
      set => base.CertificateThumbprint = value;
    }

    public override SwitchParameter AllowRedirection => (SwitchParameter) false;

    public override PSSessionOption SessionOption
    {
      get => base.SessionOption;
      set => base.SessionOption = value;
    }

    [Parameter(Position = 1)]
    public ScriptBlock InitializationScript
    {
      get => this.initScript;
      set => this.initScript = value;
    }

    [Parameter]
    public SwitchParameter RunAs32
    {
      get => (SwitchParameter) this.shouldRunAs32;
      set => this.shouldRunAs32 = (bool) value;
    }

    protected override void BeginProcessing()
    {
      this.SkipWinRMCheck = true;
      base.BeginProcessing();
    }

    protected override void CreateHelpersForSpecifiedComputerNames()
    {
      NewProcessConnectionInfo processConnectionInfo = new NewProcessConnectionInfo(this.Credential);
      processConnectionInfo.RunAs32 = this.shouldRunAs32;
      processConnectionInfo.InitializationScript = this.initScript;
      processConnectionInfo.AuthenticationMechanism = this.Authentication;
      RemoteRunspace runspace = (RemoteRunspace) RunspaceFactory.CreateRunspace((RunspaceConnectionInfo) processConnectionInfo, this.Host, Utils.GetTypeTableFromExecutionContextTLS());
      runspace.Events.ReceivedEvents.PSEventReceived += new PSEventReceivedEventHandler(((PSExecutionCmdlet) this).OnRunspacePSEventReceived);
      Pipeline pipeline = this.CreatePipeline(runspace);
      this.Operations.Add((IThrottleOperation) new ExecutionCmdletHelperComputerName(runspace, pipeline));
    }

    protected override void ProcessRecord()
    {
      if (this.firstProcessRecord)
      {
        this.firstProcessRecord = false;
        PSRemotingJob psRemotingJob = new PSRemotingJob(this.ResolvedComputerNames, this.Operations, this.ScriptBlock.ToString(), this.ThrottleLimit, this.name);
        this.JobRepository.Add((Job) psRemotingJob);
        this.WriteObject((object) psRemotingJob);
      }
      if (this.InputObject == AutomationNull.Value)
        return;
      foreach (ExecutionCmdletHelper operation in this.Operations)
        operation.Pipeline.Input.Write((object) this.InputObject);
    }

    protected override void EndProcessing()
    {
      using (StartJobCommand.tracer.TraceMethod())
        this.CloseAllInputStreams();
    }

    public void Dispose()
    {
      using (StartJobCommand.tracer.TraceMethod())
      {
        this.Dispose(true);
        GC.SuppressFinalize((object) this);
      }
    }

    private void Dispose(bool disposing)
    {
      using (StartJobCommand.tracer.TraceMethod())
      {
        if (!disposing)
          return;
        this.CloseAllInputStreams();
      }
    }
  }
}
