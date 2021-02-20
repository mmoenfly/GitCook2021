// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.EnterPSSessionCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Internal.Host;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Enter", "PSSession", DefaultParameterSetName = "ComputerName")]
  public class EnterPSSessionCommand : PSRemotingBaseCmdlet
  {
    private const string InstanceIdParameterSet = "InstanceId";
    private const string IdParameterSet = "Id";
    private const string NameParameterSet = "Name";
    private ObjectStream stream;
    private string computerName;
    private PSSession remoteRunspaceInfo;
    private Uri connectionUri;
    private Guid remoteRunspaceId;
    private int sessionId;
    private string name;

    public new int ThrottleLimit
    {
      set
      {
      }
      get => 0;
    }

    [Parameter(Mandatory = true, ParameterSetName = "ComputerName", Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    [Alias(new string[] {"Cn"})]
    public string ComputerName
    {
      get => this.computerName;
      set => this.computerName = value;
    }

    [Parameter(ParameterSetName = "Session", Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public PSSession Session
    {
      get => this.remoteRunspaceInfo;
      set => this.remoteRunspaceInfo = value;
    }

    [Alias(new string[] {"URI", "CU"})]
    [Parameter(ParameterSetName = "Uri", Position = 1, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public Uri ConnectionUri
    {
      get => this.connectionUri;
      set => this.connectionUri = value;
    }

    [ValidateNotNull]
    [Parameter(ParameterSetName = "InstanceId", ValueFromPipelineByPropertyName = true)]
    public Guid InstanceId
    {
      get => this.remoteRunspaceId;
      set => this.remoteRunspaceId = value;
    }

    [ValidateNotNull]
    [Parameter(ParameterSetName = "Id", Position = 0, ValueFromPipelineByPropertyName = true)]
    public int Id
    {
      get => this.sessionId;
      set => this.sessionId = value;
    }

    [Parameter(ParameterSetName = "Name", ValueFromPipelineByPropertyName = true)]
    public string Name
    {
      get => this.name;
      set => this.name = value;
    }

    protected override void ProcessRecord()
    {
      if (!(this.Host is IHostSupportsInteractiveSession host))
      {
        this.WriteError(new ErrorRecord((Exception) new ArgumentException(this.GetMessage(PSRemotingErrorId.HostDoesNotSupportPushRunspace)), PSRemotingErrorId.HostDoesNotSupportPushRunspace.ToString(), ErrorCategory.InvalidArgument, (object) null));
      }
      else
      {
        if (this.Host is InternalHost host && host.HostInNestedPrompt())
          this.ThrowTerminatingError(new ErrorRecord((Exception) new InvalidOperationException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.HostInNestedPrompt)), "HostInNestedPrompt", ErrorCategory.InvalidOperation, (object) host));
        RemoteRunspace remoteRunspace = (RemoteRunspace) null;
        switch (this.ParameterSetName)
        {
          case "ComputerName":
            remoteRunspace = this.CreateRunspaceWhenComputerNameParameterSpecified();
            break;
          case "Uri":
            remoteRunspace = this.CreateRunspaceWhenUriParameterSpecified();
            break;
          case "Session":
            remoteRunspace = (RemoteRunspace) this.remoteRunspaceInfo.Runspace;
            break;
          case "InstanceId":
            remoteRunspace = this.GetRunspaceMatchingRunspaceId(this.InstanceId);
            break;
          case "Id":
            remoteRunspace = this.GetRunspaceMatchingSessionId(this.Id);
            break;
          case "Name":
            remoteRunspace = this.GetRunspaceMatchingName(this.Name);
            break;
        }
        if (remoteRunspace == null)
          return;
        if (remoteRunspace.RunspaceStateInfo.State != RunspaceState.Opened)
        {
          this.WriteError(new ErrorRecord((Exception) new ArgumentException(this.GetMessage(PSRemotingErrorId.PushedRunspaceMustBeOpen)), PSRemotingErrorId.PushedRunspaceMustBeOpen.ToString(), ErrorCategory.InvalidArgument, (object) null));
        }
        else
        {
          try
          {
            host.PushRunspace((Runspace) remoteRunspace);
          }
          catch (Exception ex)
          {
            if (remoteRunspace != null && remoteRunspace.ShouldCloseOnPop)
              remoteRunspace.Close();
            throw;
          }
        }
      }
    }

    protected override void EndProcessing()
    {
      if (this.stream == null)
        return;
      while (true)
      {
        this.stream.ObjectReader.WaitHandle.WaitOne();
        if (!this.stream.ObjectReader.EndOfPipeline)
          this.WriteStreamObject((PSStreamObject) this.stream.ObjectReader.Read());
        else
          break;
      }
    }

    private RemoteRunspace CreateTemporaryRemoteRunspace(
      PSHost host,
      WSManConnectionInfo connectionInfo)
    {
      RemoteRunspace remoteRunspace = (RemoteRunspace) RunspaceFactory.CreateRunspace((RunspaceConnectionInfo) connectionInfo, host, Utils.GetTypeTableFromExecutionContextTLS(), this.SessionOption.ApplicationArguments);
      remoteRunspace.URIRedirectionReported += new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
      this.stream = new ObjectStream();
      try
      {
        remoteRunspace.Open();
        remoteRunspace.ShouldCloseOnPop = true;
      }
      finally
      {
        remoteRunspace.URIRedirectionReported -= new EventHandler<RemoteDataEventArgs<Uri>>(this.HandleURIDirectionReported);
        this.stream.ObjectWriter.Close();
        if (remoteRunspace.RunspaceStateInfo.State != RunspaceState.Opened)
        {
          remoteRunspace.Dispose();
          remoteRunspace = (RemoteRunspace) null;
        }
      }
      return remoteRunspace;
    }

    private void WriteErrorCreateRemoteRunspaceFailed(Exception exception, object argument)
    {
      PSRemotingTransportException transportException = exception as PSRemotingTransportException;
      string errorDetails_Message = (string) null;
      if (transportException != null && transportException.ErrorCode == -2144108135)
        errorDetails_Message = PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.URIRedirectionReported, (object) transportException.Message, (object) "MaximumConnectionRedirectionCount", (object) "PSSessionOption", (object) "AllowRedirection");
      this.WriteError(new ErrorRecord(exception, argument, "CreateRemoteRunspaceFailed", ErrorCategory.InvalidArgument, (string) null, (string) null, (string) null, (string) null, (string) null, errorDetails_Message, (string) null));
    }

    private void WriteInvalidArgumentError(PSRemotingErrorId errorId, object errorArgument) => this.WriteError(new ErrorRecord((Exception) new ArgumentException(this.GetMessage(errorId, errorArgument)), errorId.ToString(), ErrorCategory.InvalidArgument, errorArgument));

    private void HandleURIDirectionReported(object sender, RemoteDataEventArgs<Uri> eventArgs) => this.stream.Write((object) new PSStreamObject(PSStreamObjectType.Warning, (object) ResourceManagerCache.FormatResourceString("RemotingErrorIdStrings", "URIRedirectWarningToHost", (object) eventArgs.Data.OriginalString)));

    private RemoteRunspace CreateRunspaceWhenComputerNameParameterSpecified()
    {
      RemoteRunspace remoteRunspace = (RemoteRunspace) null;
      string computerName = this.ResolveComputerName(this.computerName);
      try
      {
        WSManConnectionInfo connectionInfo = new WSManConnectionInfo(this.UseSSL.IsPresent, computerName, this.Port, this.ApplicationName, this.ConfigurationName, this.Credential);
        connectionInfo.AuthenticationMechanism = this.Authentication;
        this.UpdateConnectionInfo(connectionInfo);
        remoteRunspace = this.CreateTemporaryRemoteRunspace(this.Host, connectionInfo);
      }
      catch (InvalidOperationException ex)
      {
        this.WriteErrorCreateRemoteRunspaceFailed((Exception) ex, (object) computerName);
      }
      catch (ArgumentException ex)
      {
        this.WriteErrorCreateRemoteRunspaceFailed((Exception) ex, (object) computerName);
      }
      catch (PSRemotingTransportException ex)
      {
        this.WriteErrorCreateRemoteRunspaceFailed((Exception) ex, (object) computerName);
      }
      return remoteRunspace;
    }

    private RemoteRunspace CreateRunspaceWhenUriParameterSpecified()
    {
      RemoteRunspace remoteRunspace = (RemoteRunspace) null;
      try
      {
        WSManConnectionInfo connectionInfo = new WSManConnectionInfo(this.ConnectionUri, this.ConfigurationName, this.Credential);
        connectionInfo.AuthenticationMechanism = this.Authentication;
        this.UpdateConnectionInfo(connectionInfo);
        remoteRunspace = this.CreateTemporaryRemoteRunspace(this.Host, connectionInfo);
      }
      catch (UriFormatException ex)
      {
        this.WriteErrorCreateRemoteRunspaceFailed((Exception) ex, (object) this.ConnectionUri);
      }
      catch (InvalidOperationException ex)
      {
        this.WriteErrorCreateRemoteRunspaceFailed((Exception) ex, (object) this.ConnectionUri);
      }
      catch (ArgumentException ex)
      {
        this.WriteErrorCreateRemoteRunspaceFailed((Exception) ex, (object) this.ConnectionUri);
      }
      catch (PSRemotingTransportException ex)
      {
        this.WriteErrorCreateRemoteRunspaceFailed((Exception) ex, (object) this.ConnectionUri);
      }
      catch (NotSupportedException ex)
      {
        this.WriteErrorCreateRemoteRunspaceFailed((Exception) ex, (object) this.ConnectionUri);
      }
      return remoteRunspace;
    }

    private RemoteRunspace GetRunspaceMatchingCondition(
      Predicate<PSSession> condition,
      PSRemotingErrorId tooFew,
      PSRemotingErrorId tooMany,
      object errorArgument)
    {
      List<PSSession> all = this.RunspaceRepository.Runspaces.FindAll(condition);
      RemoteRunspace remoteRunspace = (RemoteRunspace) null;
      if (all.Count == 0)
        this.WriteInvalidArgumentError(tooFew, errorArgument);
      else if (all.Count > 1)
        this.WriteInvalidArgumentError(tooMany, errorArgument);
      else
        remoteRunspace = (RemoteRunspace) all[0].Runspace;
      return remoteRunspace;
    }

    private RemoteRunspace GetRunspaceMatchingRunspaceId(Guid remoteRunspaceId) => this.GetRunspaceMatchingCondition((Predicate<PSSession>) (info => info.InstanceId == remoteRunspaceId), PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedRunspaceId, PSRemotingErrorId.RemoteRunspaceHasMultipleMatchesForSpecifiedRunspaceId, (object) remoteRunspaceId);

    private RemoteRunspace GetRunspaceMatchingSessionId(int sessionId) => this.GetRunspaceMatchingCondition((Predicate<PSSession>) (info => info.Id == sessionId), PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedSessionId, PSRemotingErrorId.RemoteRunspaceHasMultipleMatchesForSpecifiedSessionId, (object) sessionId);

    private RemoteRunspace GetRunspaceMatchingName(string name) => this.GetRunspaceMatchingCondition((Predicate<PSSession>) (info => info.Name.Equals(name, StringComparison.OrdinalIgnoreCase)), PSRemotingErrorId.RemoteRunspaceNotAvailableForSpecifiedName, PSRemotingErrorId.RemoteRunspaceHasMultipleMatchesForSpecifiedName, (object) name);
  }
}
