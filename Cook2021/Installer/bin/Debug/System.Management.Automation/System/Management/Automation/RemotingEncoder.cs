// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemotingEncoder
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Runspaces.Internal;
using System.Threading;

namespace System.Management.Automation
{
  internal static class RemotingEncoder
  {
    [TraceSource("RemotingEncoder", "RemotingEncoder")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (RemotingEncoder), nameof (RemotingEncoder));
    private static IETWTracer etwTracer = PSETWTracerFactory.GetETWTracer(PSKeyword.Serializer);

    internal static void AddNoteProperty<T>(
      PSObject pso,
      string propertyName,
      RemotingEncoder.ValueGetterDelegate<T> valueGetter)
    {
      T obj = default (T);
      try
      {
        obj = valueGetter();
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        RemotingEncoder.etwTracer.AnalyticChannel.WriteWarning(PSEventId.Serializer_PropertyGetterFailed, PSOpcode.Exception, PSTask.Serialization, (object) propertyName, valueGetter.Target == null ? (object) string.Empty : (object) valueGetter.Target.GetType().FullName, (object) ex.ToString(), ex.InnerException == null ? (object) string.Empty : (object) ex.InnerException.ToString());
      }
      pso.Properties.Add((PSPropertyInfo) new PSNoteProperty(propertyName, (object) obj));
    }

    internal static PSObject CreateEmptyPSObject()
    {
      PSObject psObject = new PSObject();
      psObject.TypeNames.Clear();
      return psObject;
    }

    private static PSNoteProperty CreateHostInfoProperty(HostInfo hostInfo) => new PSNoteProperty("HostInfo", RemoteHostEncoder.EncodeObject((object) hostInfo));

    internal static RemoteDataObject GenerateCreateRunspacePool(
      Guid clientRunspacePoolId,
      int minRunspaces,
      int maxRunspaces,
      RemoteRunspacePoolInternal runspacePool,
      PSHost host,
      PSPrimitiveDictionary applicationArguments)
    {
      using (RemotingEncoder._trace.TraceMethod())
      {
        PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("MinRunspaces", (object) minRunspaces));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("MaxRunspaces", (object) maxRunspaces));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("PSThreadOptions", (object) runspacePool.ThreadOptions));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("ApartmentState", (object) runspacePool.ApartmentState));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("ApplicationArguments", (object) applicationArguments));
        emptyPsObject.Properties.Add((PSPropertyInfo) RemotingEncoder.CreateHostInfoProperty(new HostInfo(host)));
        return RemoteDataObject.CreateFrom(RemotingDestination.Server, RemotingDataType.CreateRunspacePool, clientRunspacePoolId, Guid.Empty, (object) emptyPsObject);
      }
    }

    internal static RemoteDataObject GenerateSetMaxRunspaces(
      Guid clientRunspacePoolId,
      int maxRunspaces,
      long callId)
    {
      using (RemotingEncoder._trace.TraceMethod())
      {
        PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("MaxRunspaces", (object) maxRunspaces));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("ci", (object) callId));
        return RemoteDataObject.CreateFrom(RemotingDestination.Server, RemotingDataType.SetMaxRunspaces, clientRunspacePoolId, Guid.Empty, (object) emptyPsObject);
      }
    }

    internal static RemoteDataObject GenerateSetMinRunspaces(
      Guid clientRunspacePoolId,
      int minRunspaces,
      long callId)
    {
      using (RemotingEncoder._trace.TraceMethod())
      {
        PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("MinRunspaces", (object) minRunspaces));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("ci", (object) callId));
        return RemoteDataObject.CreateFrom(RemotingDestination.Server, RemotingDataType.SetMinRunspaces, clientRunspacePoolId, Guid.Empty, (object) emptyPsObject);
      }
    }

    internal static RemoteDataObject GenerateRunspacePoolOperationResponse(
      Guid clientRunspacePoolId,
      object response,
      long callId)
    {
      using (RemotingEncoder._trace.TraceMethod())
      {
        PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("SetMinMaxRunspacesResponse", response));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("ci", (object) callId));
        return RemoteDataObject.CreateFrom(RemotingDestination.Client, RemotingDataType.RunspacePoolOperationResponse, clientRunspacePoolId, Guid.Empty, (object) emptyPsObject);
      }
    }

    internal static RemoteDataObject GenerateGetAvailableRunspaces(
      Guid clientRunspacePoolId,
      long callId)
    {
      PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("ci", (object) callId));
      return RemoteDataObject.CreateFrom(RemotingDestination.Server, RemotingDataType.AvailableRunspaces, clientRunspacePoolId, Guid.Empty, (object) emptyPsObject);
    }

    internal static RemoteDataObject GenerateMyPublicKey(
      Guid runspacePoolId,
      string publicKey,
      RemotingDestination destination)
    {
      PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("PublicKey", (object) publicKey));
      return RemoteDataObject.CreateFrom(destination, RemotingDataType.PublicKey, runspacePoolId, Guid.Empty, (object) emptyPsObject);
    }

    internal static RemoteDataObject GeneratePublicKeyRequest(Guid runspacePoolId) => RemoteDataObject.CreateFrom(RemotingDestination.Client, RemotingDataType.PublicKeyRequest, runspacePoolId, Guid.Empty, (object) string.Empty);

    internal static RemoteDataObject GenerateEncryptedSessionKeyResponse(
      Guid runspacePoolId,
      string encryptedSessionKey)
    {
      PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("EncryptedSessionKey", (object) encryptedSessionKey));
      return RemoteDataObject.CreateFrom(RemotingDestination.Client, RemotingDataType.EncryptedSessionKey, runspacePoolId, Guid.Empty, (object) emptyPsObject);
    }

    internal static RemoteDataObject GenerateGetCommandMetadata(
      ClientRemotePowerShell shell)
    {
      Command command1 = (Command) null;
      foreach (Command command2 in (Collection<Command>) shell.PowerShell.Commands.Commands)
      {
        if (command2.CommandText.Equals("Get-Command", StringComparison.OrdinalIgnoreCase))
        {
          command1 = command2;
          break;
        }
      }
      string[] strArray1 = (string[]) null;
      CommandTypes commandTypes = CommandTypes.Alias | CommandTypes.Function | CommandTypes.Filter | CommandTypes.Cmdlet;
      string[] strArray2 = (string[]) null;
      object[] objArray = (object[]) null;
      foreach (CommandParameter parameter in (Collection<CommandParameter>) command1.Parameters)
      {
        if (parameter.Name.Equals("Name", StringComparison.OrdinalIgnoreCase))
          strArray1 = (string[]) LanguagePrimitives.ConvertTo(parameter.Value, typeof (string[]), (IFormatProvider) CultureInfo.InvariantCulture);
        else if (parameter.Name.Equals("CommandType", StringComparison.OrdinalIgnoreCase))
          commandTypes = (CommandTypes) LanguagePrimitives.ConvertTo(parameter.Value, typeof (CommandTypes), (IFormatProvider) CultureInfo.InvariantCulture);
        else if (parameter.Name.Equals("Module", StringComparison.OrdinalIgnoreCase))
          strArray2 = (string[]) LanguagePrimitives.ConvertTo(parameter.Value, typeof (string[]), (IFormatProvider) CultureInfo.InvariantCulture);
        else if (parameter.Name.Equals("ArgumentList", StringComparison.OrdinalIgnoreCase))
          objArray = (object[]) LanguagePrimitives.ConvertTo(parameter.Value, typeof (object[]), (IFormatProvider) CultureInfo.InvariantCulture);
      }
      Guid instanceId = (shell.PowerShell.GetRunspaceConnection() as RunspacePool).InstanceId;
      PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("Name", (object) strArray1));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("CommandType", (object) commandTypes));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("Namespace", (object) strArray2));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("ArgumentList", (object) objArray));
      return RemoteDataObject.CreateFrom(RemotingDestination.Server, RemotingDataType.GetCommandMetadata, instanceId, shell.InstanceId, (object) emptyPsObject);
    }

    internal static RemoteDataObject GenerateCreatePowerShell(
      ClientRemotePowerShell shell)
    {
      PowerShell powerShell = shell.PowerShell;
      PSInvocationSettings settings = shell.Settings;
      PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
      Guid empty = Guid.Empty;
      RunspacePool runspaceConnection = powerShell.GetRunspaceConnection() as RunspacePool;
      Guid instanceId = runspaceConnection.InstanceId;
      ApartmentState apartmentState = runspaceConnection.ApartmentState;
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("PowerShell", (object) powerShell.ToPSObjectForRemoting()));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("NoInput", (object) shell.NoInput));
      HostInfo hostInfo;
      if (settings == null)
      {
        hostInfo = new HostInfo((PSHost) null);
        hostInfo.UseRunspaceHost = true;
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("ApartmentState", (object) apartmentState));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("RemoteStreamOptions", (object) RemoteStreamOptions.AddInvocationInfo));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("AddToHistory", (object) false));
      }
      else
      {
        hostInfo = new HostInfo(settings.Host);
        if (settings.Host == null)
          hostInfo.UseRunspaceHost = true;
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("ApartmentState", (object) settings.ApartmentState));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("RemoteStreamOptions", (object) settings.RemoteStreamOptions));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("AddToHistory", (object) settings.AddToHistory));
      }
      PSNoteProperty hostInfoProperty = RemotingEncoder.CreateHostInfoProperty(hostInfo);
      emptyPsObject.Properties.Add((PSPropertyInfo) hostInfoProperty);
      return RemoteDataObject.CreateFrom(RemotingDestination.Server, RemotingDataType.CreatePowerShell, instanceId, shell.InstanceId, (object) emptyPsObject);
    }

    internal static RemoteDataObject GenerateApplicationPrivateData(
      Guid clientRunspacePoolId,
      PSPrimitiveDictionary applicationPrivateData)
    {
      using (RemotingEncoder._trace.TraceMethod())
      {
        PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("ApplicationPrivateData", (object) applicationPrivateData));
        return RemoteDataObject.CreateFrom(RemotingDestination.Client, RemotingDataType.ApplicationPrivateData, clientRunspacePoolId, Guid.Empty, (object) emptyPsObject);
      }
    }

    internal static RemoteDataObject GenerateRunspacePoolStateInfo(
      Guid clientRunspacePoolId,
      RunspacePoolStateInfo stateInfo)
    {
      using (RemotingEncoder._trace.TraceMethod())
      {
        PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
        PSNoteProperty psNoteProperty = new PSNoteProperty("RunspaceState", (object) (int) stateInfo.State);
        emptyPsObject.Properties.Add((PSPropertyInfo) psNoteProperty);
        if (stateInfo.Reason != null)
        {
          string errorId = "RemoteRunspaceStateInfoReason";
          PSNoteProperty exceptionProperty = RemotingEncoder.GetExceptionProperty(stateInfo.Reason, errorId, ErrorCategory.NotSpecified);
          emptyPsObject.Properties.Add((PSPropertyInfo) exceptionProperty);
        }
        return RemoteDataObject.CreateFrom(RemotingDestination.Client, RemotingDataType.RunspacePoolStateInfo, clientRunspacePoolId, Guid.Empty, (object) emptyPsObject);
      }
    }

    internal static RemoteDataObject GeneratePSEventArgs(
      Guid clientRunspacePoolId,
      PSEventArgs e)
    {
      using (RemotingEncoder._trace.TraceMethod())
      {
        PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("PSEventArgs.EventIdentifier", (object) e.EventIdentifier));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("PSEventArgs.SourceIdentifier", (object) e.SourceIdentifier));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("PSEventArgs.TimeGenerated", (object) e.TimeGenerated));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("PSEventArgs.Sender", e.Sender));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("PSEventArgs.SourceArgs", (object) e.SourceArgs));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("PSEventArgs.MessageData", (object) e.MessageData));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("PSEventArgs.ComputerName", (object) e.ComputerName));
        emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("PSEventArgs.RunspaceId", (object) e.RunspaceId));
        return RemoteDataObject.CreateFrom(RemotingDestination.Client, RemotingDataType.PSEventArgs, clientRunspacePoolId, Guid.Empty, (object) emptyPsObject);
      }
    }

    internal static RemoteDataObject GeneratePowerShellInput(
      object data,
      Guid clientRemoteRunspacePoolId,
      Guid clientPowerShellId)
    {
      using (RemotingEncoder._trace.TraceMethod())
        return RemoteDataObject.CreateFrom(RemotingDestination.Server, RemotingDataType.PowerShellInput, clientRemoteRunspacePoolId, clientPowerShellId, data);
    }

    internal static RemoteDataObject GeneratePowerShellInputEnd(
      Guid clientRemoteRunspacePoolId,
      Guid clientPowerShellId)
    {
      using (RemotingEncoder._trace.TraceMethod())
        return RemoteDataObject.CreateFrom(RemotingDestination.Server, RemotingDataType.PowerShellInputEnd, clientRemoteRunspacePoolId, clientPowerShellId, (object) null);
    }

    internal static RemoteDataObject GeneratePowerShellOutput(
      PSObject data,
      Guid clientPowerShellId,
      Guid clientRunspacePoolId)
    {
      using (RemotingEncoder._trace.TraceMethod())
        return RemoteDataObject.CreateFrom(RemotingDestination.Client, RemotingDataType.PowerShellOutput, clientRunspacePoolId, clientPowerShellId, (object) data);
    }

    internal static RemoteDataObject GeneratePowerShellInformational(
      object data,
      Guid clientRunspacePoolId,
      Guid clientPowerShellId,
      RemotingDataType dataType)
    {
      using (RemotingEncoder._trace.TraceMethod())
        return RemoteDataObject.CreateFrom(RemotingDestination.Client, dataType, clientRunspacePoolId, clientPowerShellId, (object) PSObject.AsPSObject(data));
    }

    internal static RemoteDataObject GeneratePowerShellInformational(
      ProgressRecord progressRecord,
      Guid clientRunspacePoolId,
      Guid clientPowerShellId)
    {
      using (RemotingEncoder._trace.TraceMethod())
      {
        if (progressRecord == null)
          throw RemotingEncoder._trace.NewArgumentNullException(nameof (progressRecord));
        return RemoteDataObject.CreateFrom(RemotingDestination.Client, RemotingDataType.PowerShellProgress, clientRunspacePoolId, clientPowerShellId, (object) progressRecord.ToPSObjectForRemoting());
      }
    }

    internal static RemoteDataObject GeneratePowerShellError(
      object errorRecord,
      Guid clientRunspacePoolId,
      Guid clientPowerShellId)
    {
      using (RemotingEncoder._trace.TraceMethod())
        return RemoteDataObject.CreateFrom(RemotingDestination.Client, RemotingDataType.PowerShellErrorRecord, clientRunspacePoolId, clientPowerShellId, (object) PSObject.AsPSObject(errorRecord));
    }

    internal static RemoteDataObject GeneratePowerShellStateInfo(
      PSInvocationStateInfo stateInfo,
      Guid clientPowerShellId,
      Guid clientRunspacePoolId)
    {
      using (RemotingEncoder._trace.TraceMethod())
      {
        PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
        PSNoteProperty psNoteProperty = new PSNoteProperty("PipelineState", (object) (int) stateInfo.State);
        emptyPsObject.Properties.Add((PSPropertyInfo) psNoteProperty);
        if (stateInfo.Reason != null)
        {
          string errorId = "RemotePSInvocationStateInfoReason";
          PSNoteProperty exceptionProperty = RemotingEncoder.GetExceptionProperty(stateInfo.Reason, errorId, ErrorCategory.NotSpecified);
          emptyPsObject.Properties.Add((PSPropertyInfo) exceptionProperty);
        }
        return RemoteDataObject.CreateFrom(RemotingDestination.Client, RemotingDataType.PowerShellStateInfo, clientRunspacePoolId, clientPowerShellId, (object) emptyPsObject);
      }
    }

    internal static ErrorRecord GetErrorRecordFromException(Exception exception)
    {
      using (RemotingEncoder._trace.TraceMethod())
      {
        ErrorRecord errorRecord = (ErrorRecord) null;
        if (exception is IContainsErrorRecord containsErrorRecord)
          errorRecord = new ErrorRecord(containsErrorRecord.ErrorRecord, exception);
        return errorRecord;
      }
    }

    private static PSNoteProperty GetExceptionProperty(
      Exception exception,
      string errorId,
      ErrorCategory category)
    {
      using (RemotingEncoder._trace.TraceMethod())
        return new PSNoteProperty("ExceptionAsErrorRecord", (object) (RemotingEncoder.GetErrorRecordFromException(exception) ?? new ErrorRecord(exception, errorId, category, (object) null)));
    }

    internal static RemoteDataObject GenerateClientSessionCapability(
      RemoteSessionCapability capability,
      Guid runspacePoolId)
    {
      PSObject sessionCapability = RemotingEncoder.GenerateSessionCapability(capability);
      sessionCapability.Properties.Add((PSPropertyInfo) new PSNoteProperty("TimeZone", (object) RemoteSessionCapability.GetCurrentTimeZoneInByteFormat()));
      return RemoteDataObject.CreateFrom(capability.RemotingDestination, RemotingDataType.SessionCapability, runspacePoolId, Guid.Empty, (object) sessionCapability);
    }

    internal static RemoteDataObject GenerateServerSessionCapability(
      RemoteSessionCapability capability,
      Guid runspacePoolId)
    {
      PSObject sessionCapability = RemotingEncoder.GenerateSessionCapability(capability);
      return RemoteDataObject.CreateFrom(capability.RemotingDestination, RemotingDataType.SessionCapability, runspacePoolId, Guid.Empty, (object) sessionCapability);
    }

    private static PSObject GenerateSessionCapability(RemoteSessionCapability capability)
    {
      PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("protocolversion", (object) capability.ProtocolVersion));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("PSVersion", (object) capability.PSVersion));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("SerializationVersion", (object) capability.SerializationVersion));
      return emptyPsObject;
    }

    internal delegate T ValueGetterDelegate<T>();
  }
}
