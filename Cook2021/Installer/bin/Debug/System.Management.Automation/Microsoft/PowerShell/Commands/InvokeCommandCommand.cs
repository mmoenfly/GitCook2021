// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.InvokeCommandCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Invoke", "Command", DefaultParameterSetName = "InProcess")]
  public class InvokeCommandCommand : PSExecutionCmdlet, IDisposable
  {
    private const string InProcParameterSet = "InProcess";
    private bool asjob;
    private bool hideComputerName;
    private string name = string.Empty;
    private ThrottleManager throttleManager = new ThrottleManager();
    private ManualResetEvent operationsComplete = new ManualResetEvent(true);
    private PSInvokeExpressionSyncJob job;
    private SteppablePipeline steppablePipeline;
    private bool pipelineinvoked;
    private bool inputStreamClosed;
    private PSDataCollection<object> input = new PSDataCollection<object>();
    private bool needToCollect;
    private List<PipelineWriter> inputWriters = new List<PipelineWriter>();
    private object jobSyncObject = new object();
    private bool nojob;
    private Guid instanceId = Guid.NewGuid();
    private bool propagateErrors;

    [Parameter(ParameterSetName = "Session", Position = 0, ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "FilePathRunspace", Position = 0, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public override PSSession[] Session
    {
      get => base.Session;
      set => base.Session = value;
    }

    [Alias(new string[] {"Cn"})]
    [Parameter(ParameterSetName = "FilePathComputerName", Position = 0, ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "ComputerName", Position = 0, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName
    {
      get => base.ComputerName;
      set => base.ComputerName = value;
    }

    [Parameter(ParameterSetName = "ComputerName", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "Uri", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "FilePathComputerName", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "FilePathUri", ValueFromPipelineByPropertyName = true)]
    [System.Management.Automation.Credential]
    public override PSCredential Credential
    {
      get => base.Credential;
      set => base.Credential = value;
    }

    [ValidateRange(1, 65535)]
    [Parameter(ParameterSetName = "FilePathComputerName")]
    [Parameter(ParameterSetName = "ComputerName")]
    public override int Port
    {
      get => base.Port;
      set => base.Port = value;
    }

    [Parameter(ParameterSetName = "ComputerName")]
    [Parameter(ParameterSetName = "FilePathComputerName")]
    public override SwitchParameter UseSSL
    {
      get => base.UseSSL;
      set => base.UseSSL = value;
    }

    [Parameter(ParameterSetName = "ComputerName", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "Uri", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "FilePathComputerName", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "FilePathUri", ValueFromPipelineByPropertyName = true)]
    public override string ConfigurationName
    {
      get => base.ConfigurationName;
      set => base.ConfigurationName = value;
    }

    [Parameter(ParameterSetName = "ComputerName", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "FilePathComputerName", ValueFromPipelineByPropertyName = true)]
    public override string ApplicationName
    {
      get => base.ApplicationName;
      set => base.ApplicationName = value;
    }

    [Parameter(ParameterSetName = "Uri")]
    [Parameter(ParameterSetName = "Session")]
    [Parameter(ParameterSetName = "ComputerName")]
    [Parameter(ParameterSetName = "FilePathComputerName")]
    [Parameter(ParameterSetName = "FilePathRunspace")]
    [Parameter(ParameterSetName = "FilePathUri")]
    public override int ThrottleLimit
    {
      set => base.ThrottleLimit = value;
      get => base.ThrottleLimit;
    }

    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "FilePathUri", Position = 0, ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "Uri", Position = 0, ValueFromPipelineByPropertyName = true)]
    [Alias(new string[] {"URI", "CU"})]
    public override Uri[] ConnectionUri
    {
      get => base.ConnectionUri;
      set => base.ConnectionUri = value;
    }

    [Parameter(ParameterSetName = "Session")]
    [Parameter(ParameterSetName = "ComputerName")]
    [Parameter(ParameterSetName = "Uri")]
    [Parameter(ParameterSetName = "FilePathComputerName")]
    [Parameter(ParameterSetName = "FilePathRunspace")]
    [Parameter(ParameterSetName = "FilePathUri")]
    public SwitchParameter AsJob
    {
      get => (SwitchParameter) this.asjob;
      set => this.asjob = (bool) value;
    }

    [Parameter(ParameterSetName = "Session")]
    [Parameter(ParameterSetName = "ComputerName")]
    [Parameter(ParameterSetName = "Uri")]
    [Parameter(ParameterSetName = "FilePathComputerName")]
    [Parameter(ParameterSetName = "FilePathRunspace")]
    [Parameter(ParameterSetName = "FilePathUri")]
    [Alias(new string[] {"HCN"})]
    public SwitchParameter HideComputerName
    {
      get => (SwitchParameter) this.hideComputerName;
      set => this.hideComputerName = (bool) value;
    }

    [Parameter(ParameterSetName = "ComputerName")]
    [Parameter(ParameterSetName = "Session")]
    [Parameter(ParameterSetName = "Uri")]
    [Parameter(ParameterSetName = "FilePathComputerName")]
    [Parameter(ParameterSetName = "FilePathRunspace")]
    [Parameter(ParameterSetName = "FilePathUri")]
    public string JobName
    {
      get => this.name;
      set
      {
        if (string.IsNullOrEmpty(value))
          return;
        this.name = value;
        this.asjob = true;
      }
    }

    [Parameter(Mandatory = true, ParameterSetName = "Uri", Position = 1)]
    [Parameter(Mandatory = true, ParameterSetName = "Session", Position = 1)]
    [Parameter(Mandatory = true, ParameterSetName = "ComputerName", Position = 1)]
    [Parameter(Mandatory = true, ParameterSetName = "InProcess", Position = 0)]
    [ValidateNotNull]
    [Alias(new string[] {"Command"})]
    public override ScriptBlock ScriptBlock
    {
      get => base.ScriptBlock;
      set => base.ScriptBlock = value;
    }

    [Parameter(Mandatory = true, ParameterSetName = "FilePathUri", Position = 1)]
    [Parameter(Mandatory = true, ParameterSetName = "FilePathRunspace", Position = 1)]
    [Parameter(Mandatory = true, ParameterSetName = "FilePathComputerName", Position = 1)]
    [ValidateNotNull]
    [Alias(new string[] {"PSPath"})]
    public override string FilePath
    {
      get => base.FilePath;
      set => base.FilePath = value;
    }

    [Parameter(ParameterSetName = "Uri")]
    [Parameter(ParameterSetName = "FilePathUri")]
    public override SwitchParameter AllowRedirection
    {
      get => base.AllowRedirection;
      set => base.AllowRedirection = value;
    }

    [Parameter(ParameterSetName = "Uri")]
    [Parameter(ParameterSetName = "ComputerName")]
    [Parameter(ParameterSetName = "FilePathComputerName")]
    [Parameter(ParameterSetName = "FilePathUri")]
    public override PSSessionOption SessionOption
    {
      get => base.SessionOption;
      set => base.SessionOption = value;
    }

    [Parameter(ParameterSetName = "Uri")]
    [Parameter(ParameterSetName = "FilePathComputerName")]
    [Parameter(ParameterSetName = "FilePathUri")]
    [Parameter(ParameterSetName = "ComputerName")]
    public override AuthenticationMechanism Authentication
    {
      get => base.Authentication;
      set => base.Authentication = value;
    }

    protected override void BeginProcessing()
    {
      if (this.ParameterSetName.Equals("InProcess"))
      {
        if (this.FilePath != null)
          this.ScriptBlock = this.GetScriptBlockFromFile(this.FilePath);
        if (!this.MyInvocation.ExpectingInput)
          return;
        if (this.ScriptBlock.IsUsingDollarInput())
          return;
        try
        {
          this.steppablePipeline = this.ScriptBlock.GetSteppablePipeline();
          this.steppablePipeline.Begin((InternalCommand) this);
        }
        catch (InvalidOperationException ex)
        {
        }
      }
      else
      {
        base.BeginProcessing();
        foreach (ExecutionCmdletHelper operation in this.Operations)
          this.inputWriters.Add(operation.Pipeline.Input);
        if (this.ParameterSetName.Equals("Session"))
        {
          long instanceId = this.Context.CurrentRunspace.GetCurrentlyRunningPipeline().InstanceId;
          foreach (PSSession psSession in this.Session)
          {
            if (((RemoteRunspace) psSession.Runspace).IsAnotherInvokeCommandExecuting(this, instanceId))
            {
              this.needToCollect = true;
              break;
            }
          }
        }
        this.DetermineThrowStatementBehavior();
      }
    }

    protected override void ProcessRecord()
    {
      if (!this.pipelineinvoked && !this.needToCollect)
      {
        this.pipelineinvoked = true;
        if (this.InputObject == AutomationNull.Value)
        {
          this.CloseAllInputStreams();
          this.inputStreamClosed = true;
        }
        if (!this.ParameterSetName.Equals("InProcess"))
        {
          if (!this.asjob)
          {
            this.CreateAndRunSyncJob();
          }
          else
          {
            switch (this.ParameterSetName)
            {
              case "ComputerName":
              case "FilePathComputerName":
                if (this.ResolvedComputerNames.Length != 0 && this.Operations.Count > 0)
                {
                  PSRemotingJob psRemotingJob = new PSRemotingJob(this.ResolvedComputerNames, this.Operations, this.ScriptBlock.ToString(), this.ThrottleLimit, this.name);
                  psRemotingJob.HideComputerName = this.hideComputerName;
                  this.JobRepository.Add((Job) psRemotingJob);
                  this.WriteObject((object) psRemotingJob);
                  break;
                }
                break;
              case "Session":
              case "FilePathRunspace":
                PSRemotingJob psRemotingJob1 = new PSRemotingJob(this.Session, this.Operations, this.ScriptBlock.ToString(), this.ThrottleLimit, this.name);
                psRemotingJob1.HideComputerName = this.hideComputerName;
                this.JobRepository.Add((Job) psRemotingJob1);
                this.WriteObject((object) psRemotingJob1);
                break;
              case "Uri":
              case "FilePathUri":
                if (this.Operations.Count > 0)
                {
                  string[] computerNames = new string[this.ConnectionUri.Length];
                  for (int index = 0; index < computerNames.Length; ++index)
                    computerNames[index] = this.ConnectionUri[index].ToString();
                  PSRemotingJob psRemotingJob2 = new PSRemotingJob(computerNames, this.Operations, this.ScriptBlock.ToString(), this.ThrottleLimit, this.name);
                  psRemotingJob2.HideComputerName = this.hideComputerName;
                  this.JobRepository.Add((Job) psRemotingJob2);
                  this.WriteObject((object) psRemotingJob2);
                  break;
                }
                break;
            }
          }
        }
      }
      if (this.InputObject == AutomationNull.Value || this.inputStreamClosed)
        return;
      if (this.ParameterSetName.Equals("InProcess") && this.steppablePipeline == null || this.needToCollect)
        this.input.Add((object) this.InputObject);
      else if (this.ParameterSetName.Equals("InProcess") && this.steppablePipeline != null)
      {
        this.steppablePipeline.Process(this.InputObject);
      }
      else
      {
        this.WriteInput((object) this.InputObject);
        if (this.asjob)
          return;
        this.WriteJobResults(true);
      }
    }

    protected override void EndProcessing()
    {
      if (!this.needToCollect)
      {
        this.CloseAllInputStreams();
        this.ClearInvokeCommandOnRunspaces();
      }
      if (this.asjob)
        return;
      if (this.ParameterSetName.Equals("InProcess"))
      {
        if (this.steppablePipeline != null)
        {
          this.steppablePipeline.End();
        }
        else
        {
          object sendToPipeline = this.ScriptBlock.InvokeUsingCmdlet((Cmdlet) this, true, true, (object) AutomationNull.Value, (object) this.input, (object) AutomationNull.Value, this.ArgumentList);
          if (sendToPipeline == AutomationNull.Value)
            return;
          this.WriteObject(sendToPipeline);
        }
      }
      else if (this.job != null)
      {
        this.WriteJobResults(false);
        this.job.Dispose();
        this.ClearInvokeCommandOnRunspaces();
      }
      else
      {
        if (!this.needToCollect || !this.ParameterSetName.Equals("Session"))
          return;
        this.CreateAndRunSyncJob();
        foreach (object inputValue in this.input)
          this.WriteInput(inputValue);
        this.CloseAllInputStreams();
        this.WriteJobResults(false);
        this.job.Dispose();
      }
    }

    protected override void StopProcessing()
    {
      if (this.ParameterSetName.Equals("InProcess") || this.asjob)
        return;
      bool flag = false;
      lock (this.jobSyncObject)
      {
        if (this.job != null)
          flag = true;
        else
          this.nojob = true;
      }
      if (flag)
        this.job.StopJob();
      this.needToCollect = false;
    }

    private void HandleThrottleComplete(object sender, EventArgs eventArgs)
    {
      this.operationsComplete.Set();
      this.throttleManager.ThrottleComplete -= new EventHandler<EventArgs>(this.HandleThrottleComplete);
    }

    private void ClearInvokeCommandOnRunspaces()
    {
      if (!this.ParameterSetName.Equals("Session"))
        return;
      foreach (PSSession psSession in this.Session)
        ((RemoteRunspace) psSession.Runspace).ClearInvokeCommand();
    }

    private void CreateAndRunSyncJob()
    {
      lock (this.jobSyncObject)
      {
        if (this.nojob)
          return;
        this.throttleManager.ThrottleLimit = this.ThrottleLimit;
        this.throttleManager.ThrottleComplete += new EventHandler<EventArgs>(this.HandleThrottleComplete);
        this.operationsComplete.Reset();
        this.job = new PSInvokeExpressionSyncJob(this.Operations, this.throttleManager);
        this.job.HideComputerName = this.hideComputerName;
      }
    }

    private void WriteInput(object inputValue)
    {
      if (this.inputWriters.Count == 0)
      {
        if (!this.asjob)
          this.WriteJobResults(false);
        this.ThrowTerminatingError(new ErrorRecord((Exception) new PSInvalidOperationException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.NoMoreInputWrites)), "NoMoreInputWrite", ErrorCategory.InvalidOperation, (object) null));
      }
      List<PipelineWriter> pipelineWriterList = new List<PipelineWriter>();
      foreach (PipelineWriter inputWriter in this.inputWriters)
      {
        try
        {
          inputWriter.Write(inputValue);
        }
        catch (PipelineClosedException ex)
        {
          pipelineWriterList.Add(inputWriter);
        }
      }
      foreach (PipelineWriter pipelineWriter in pipelineWriterList)
        this.inputWriters.Remove(pipelineWriter);
    }

    private void WriteJobResults(bool nonblocking)
    {
      if (this.job == null)
        return;
      do
      {
        if (!nonblocking)
          this.job.Results.WaitHandle.WaitOne();
        this.WriteStreamObjectsFromCollection(this.job.ReadAll());
      }
      while (!nonblocking && !this.job.IsTerminalState());
      this.WriteStreamObjectsFromCollection(this.job.ReadAll());
    }

    private void WriteStreamObjectsFromCollection(Collection<PSStreamObject> results)
    {
      foreach (PSStreamObject result in results)
      {
        if (this.propagateErrors && result.objectType == PSStreamObjectType.Error)
        {
          ErrorRecord errorRecord = (ErrorRecord) result.value;
          if (errorRecord.Exception is RuntimeException exception && exception is RemoteException)
          {
            PSPropertyInfo property = ((RemoteException) errorRecord.Exception).SerializedRemoteException.Properties["WasThrownFromThrowStatement"];
            if (property != null && (bool) property.Value)
            {
              exception.WasThrownFromThrowStatement = true;
              throw exception;
            }
          }
        }
        this.WriteStreamObject(result);
      }
    }

    private void DetermineThrowStatementBehavior()
    {
      if (this.ParameterSetName.Equals("InProcess") || this.asjob)
        return;
      if (this.ParameterSetName.Equals("ComputerName") || this.ParameterSetName.Equals("FilePathComputerName"))
      {
        if (this.ComputerName.Length != 1)
          return;
        this.propagateErrors = true;
      }
      else if (this.ParameterSetName.Equals("Session") || this.ParameterSetName.Equals("FilePathRunspace"))
      {
        if (this.Session.Length != 1)
          return;
        this.propagateErrors = true;
      }
      else
      {
        if (!this.ParameterSetName.Equals("Uri") && !this.ParameterSetName.Equals("FilePathUri") || this.ConnectionUri.Length != 1)
          return;
        this.propagateErrors = true;
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      this.StopProcessing();
      this.operationsComplete.WaitOne();
      this.operationsComplete.Close();
      if (!this.asjob)
      {
        if (this.job != null)
          this.job.Dispose();
        this.throttleManager.ThrottleComplete -= new EventHandler<EventArgs>(this.HandleThrottleComplete);
        this.throttleManager.Dispose();
        this.throttleManager = (ThrottleManager) null;
        this.ClearInvokeCommandOnRunspaces();
      }
      this.input.Dispose();
    }
  }
}
