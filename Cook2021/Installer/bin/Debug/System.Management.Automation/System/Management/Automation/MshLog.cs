// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.MshLog
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Threading;

namespace System.Management.Automation
{
  internal static class MshLog
  {
    private const string _crimsonLogProviderAssemblyName = "MshCrimsonLog";
    private const string _crimsonLogProviderTypeName = "System.Management.Automation.Logging.CrimsonLogProvider";
    internal const int EVENT_ID_GENERAL_HEALTH_ISSUE = 100;
    internal const int EVENT_ID_RESOURCE_NOT_AVAILABLE = 101;
    internal const int EVENT_ID_NETWORK_CONNECTIVITY_ISSUE = 102;
    internal const int EVENT_ID_CONFIGURATION_FAILURE = 103;
    internal const int EVENT_ID_PERFORMANCE_ISSUE = 104;
    internal const int EVENT_ID_SECURITY_ISSUE = 105;
    internal const int EVENT_ID_SYSTEM_OVERLOADED = 106;
    internal const int EVENT_ID_UNEXPECTED_EXCEPTION = 195;
    private static Hashtable _logProviders = new Hashtable();
    private static int _nextSequenceNumber = 0;
    [TraceSource("MshLog", "MshLog")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (MshLog), nameof (MshLog));

    private static LogProvider GetLogProvider(string shellId)
    {
      using (MshLog._trace.TraceMethod())
      {
        Hashtable hashtable = Hashtable.Synchronized(MshLog._logProviders);
        if (hashtable[(object) shellId] == null)
          hashtable[(object) shellId] = (object) MshLog.CreateLogProvider(shellId);
        return (LogProvider) hashtable[(object) shellId];
      }
    }

    private static LogProvider GetLogProvider(ExecutionContext executionContext)
    {
      using (MshLog._trace.TraceMethod())
        return executionContext != null ? MshLog.GetLogProvider(executionContext.ShellID) : throw MshLog._trace.NewArgumentNullException(nameof (executionContext));
    }

    private static LogProvider GetLogProvider(LogContext logContext)
    {
      using (MshLog._trace.TraceMethod())
        return MshLog.GetLogProvider(logContext.ShellId);
    }

    private static LogProvider CreateLogProvider(string shellId)
    {
      using (MshLog._trace.TraceMethod())
      {
        try
        {
          return (LogProvider) new EventLogLogProvider(shellId);
        }
        catch (ArgumentException ex)
        {
          MshLog._trace.TraceException((Exception) ex);
        }
        catch (InvalidOperationException ex)
        {
          MshLog._trace.TraceException((Exception) ex);
        }
        catch (SecurityException ex)
        {
          MshLog._trace.TraceException((Exception) ex);
        }
        return (LogProvider) new DummyLogProvider();
      }
    }

    internal static void SetDummyLog(string shellId) => Hashtable.Synchronized(MshLog._logProviders)[(object) shellId] = (object) new DummyLogProvider();

    internal static void LogEngineHealthEvent(
      ExecutionContext executionContext,
      int eventId,
      Exception exception,
      Severity severity,
      Dictionary<string, string> additionalInfo,
      EngineState newEngineState)
    {
      using (MshLog._trace.TraceMethod())
      {
        if (executionContext == null)
          MshLog._trace.NewArgumentNullException(nameof (executionContext));
        else if (exception == null)
        {
          MshLog._trace.NewArgumentNullException(nameof (exception));
        }
        else
        {
          if (!MshLog.NeedToLogEngineHealthEvent(executionContext))
            return;
          InvocationInfo invocationInfo = (InvocationInfo) null;
          if (exception is IContainsErrorRecord containsErrorRecord && containsErrorRecord.ErrorRecord != null)
            invocationInfo = containsErrorRecord.ErrorRecord.InvocationInfo;
          MshLog.GetLogProvider(executionContext).LogEngineHealthEvent(MshLog.GetLogContext(executionContext, invocationInfo, severity), eventId, exception, additionalInfo);
          if (newEngineState == EngineState.None)
            return;
          MshLog.LogEngineLifecycleEvent(executionContext, newEngineState, invocationInfo);
        }
      }
    }

    internal static void LogEngineHealthEvent(
      ExecutionContext executionContext,
      int eventId,
      Exception exception,
      Severity severity)
    {
      using (MshLog._trace.TraceMethod())
        MshLog.LogEngineHealthEvent(executionContext, eventId, exception, severity, (Dictionary<string, string>) null);
    }

    internal static void LogEngineHealthEvent(
      ExecutionContext executionContext,
      Exception exception,
      Severity severity)
    {
      using (MshLog._trace.TraceMethod())
        MshLog.LogEngineHealthEvent(executionContext, 100, exception, severity, (Dictionary<string, string>) null);
    }

    internal static void LogEngineHealthEvent(
      ExecutionContext executionContext,
      int eventId,
      Exception exception,
      Severity severity,
      Dictionary<string, string> additionalInfo)
    {
      using (MshLog._trace.TraceMethod())
        MshLog.LogEngineHealthEvent(executionContext, eventId, exception, severity, additionalInfo, EngineState.None);
    }

    internal static void LogEngineHealthEvent(
      ExecutionContext executionContext,
      int eventId,
      Exception exception,
      Severity severity,
      EngineState newEngineState)
    {
      using (MshLog._trace.TraceMethod())
        MshLog.LogEngineHealthEvent(executionContext, eventId, exception, severity, (Dictionary<string, string>) null, newEngineState);
    }

    internal static void LogEngineHealthEvent(
      LogContext logContext,
      int eventId,
      Exception exception,
      Dictionary<string, string> additionalInfo)
    {
      using (MshLog._trace.TraceMethod())
      {
        if (logContext == null)
          MshLog._trace.NewArgumentNullException(nameof (logContext));
        else if (exception == null)
          MshLog._trace.NewArgumentNullException(nameof (exception));
        else
          MshLog.GetLogProvider(logContext).LogEngineHealthEvent(logContext, eventId, exception, additionalInfo);
      }
    }

    internal static void LogEngineLifecycleEvent(
      ExecutionContext executionContext,
      EngineState engineState,
      InvocationInfo invocationInfo)
    {
      using (MshLog._trace.TraceMethod())
      {
        if (executionContext == null)
        {
          MshLog._trace.NewArgumentNullException(nameof (executionContext));
        }
        else
        {
          if (!MshLog.NeedToLogEngineLifecycleEvent(executionContext))
            return;
          EngineState engineState1 = MshLog.GetEngineState(executionContext);
          if (engineState == engineState1)
            return;
          MshLog.GetLogProvider(executionContext).LogEngineLifecycleEvent(MshLog.GetLogContext(executionContext, invocationInfo), engineState, engineState1);
          MshLog.SetEngineState(executionContext, engineState);
        }
      }
    }

    internal static void LogEngineLifecycleEvent(
      ExecutionContext executionContext,
      EngineState engineState)
    {
      using (MshLog._trace.TraceMethod())
        MshLog.LogEngineLifecycleEvent(executionContext, engineState, (InvocationInfo) null);
    }

    internal static void LogCommandHealthEvent(
      ExecutionContext executionContext,
      Exception exception,
      Severity severity)
    {
      using (MshLog._trace.TraceMethod())
      {
        if (executionContext == null)
          MshLog._trace.NewArgumentNullException(nameof (executionContext));
        else if (exception == null)
        {
          MshLog._trace.NewArgumentNullException(nameof (exception));
        }
        else
        {
          if (!MshLog.NeedToLogCommandHealthEvent(executionContext))
            return;
          InvocationInfo invocationInfo = (InvocationInfo) null;
          if (exception is IContainsErrorRecord containsErrorRecord && containsErrorRecord.ErrorRecord != null)
            invocationInfo = containsErrorRecord.ErrorRecord.InvocationInfo;
          MshLog.GetLogProvider(executionContext).LogCommandHealthEvent(MshLog.GetLogContext(executionContext, invocationInfo, severity), exception);
        }
      }
    }

    internal static void LogCommandLifecycleEvent(
      ExecutionContext executionContext,
      CommandState commandState,
      InvocationInfo invocationInfo)
    {
      using (MshLog._trace.TraceMethod())
      {
        if (executionContext == null)
          MshLog._trace.NewArgumentNullException(nameof (executionContext));
        else if (invocationInfo == null)
        {
          MshLog._trace.NewArgumentNullException(nameof (invocationInfo));
        }
        else
        {
          if (!MshLog.NeedToLogCommandLifecycleEvent(executionContext))
            return;
          MshLog.GetLogProvider(executionContext).LogCommandLifecycleEvent(MshLog.GetLogContext(executionContext, invocationInfo), commandState);
        }
      }
    }

    internal static void LogCommandLifecycleEvent(
      ExecutionContext executionContext,
      CommandState commandState,
      string commandName)
    {
      using (MshLog._trace.TraceMethod())
      {
        if (executionContext == null)
        {
          MshLog._trace.NewArgumentNullException(nameof (executionContext));
        }
        else
        {
          if (!MshLog.NeedToLogCommandLifecycleEvent(executionContext))
            return;
          LogContext logContext = MshLog.GetLogContext(executionContext, (InvocationInfo) null);
          logContext.CommandName = commandName;
          MshLog.GetLogProvider(executionContext).LogCommandLifecycleEvent(logContext, commandState);
        }
      }
    }

    internal static void LogPipelineExecutionDetailEvent(
      ExecutionContext executionContext,
      List<string> detail,
      InvocationInfo invocationInfo)
    {
      using (MshLog._trace.TraceMethod())
      {
        if (executionContext == null)
        {
          MshLog._trace.NewArgumentNullException(nameof (executionContext));
        }
        else
        {
          if (!MshLog.NeedToLogPipelineExecutionDetailEvent(executionContext))
            return;
          MshLog.GetLogProvider(executionContext).LogPipelineExecutionDetailEvent(MshLog.GetLogContext(executionContext, invocationInfo), detail);
        }
      }
    }

    internal static void LogPipelineExecutionDetailEvent(
      ExecutionContext executionContext,
      List<string> detail,
      string scriptName,
      string commandLine)
    {
      using (MshLog._trace.TraceMethod())
      {
        if (executionContext == null)
        {
          MshLog._trace.NewArgumentNullException(nameof (executionContext));
        }
        else
        {
          if (!MshLog.NeedToLogPipelineExecutionDetailEvent(executionContext))
            return;
          LogContext logContext = MshLog.GetLogContext(executionContext, (InvocationInfo) null);
          logContext.CommandLine = commandLine;
          logContext.ScriptName = scriptName;
          MshLog.GetLogProvider(executionContext).LogPipelineExecutionDetailEvent(logContext, detail);
        }
      }
    }

    internal static void LogProviderHealthEvent(
      ExecutionContext executionContext,
      string providerName,
      Exception exception,
      Severity severity)
    {
      using (MshLog._trace.TraceMethod())
      {
        if (executionContext == null)
          MshLog._trace.NewArgumentNullException(nameof (executionContext));
        else if (exception == null)
        {
          MshLog._trace.NewArgumentNullException(nameof (exception));
        }
        else
        {
          if (!MshLog.NeedToLogProviderHealthEvent(executionContext))
            return;
          InvocationInfo invocationInfo = (InvocationInfo) null;
          if (exception is IContainsErrorRecord containsErrorRecord && containsErrorRecord.ErrorRecord != null)
            invocationInfo = containsErrorRecord.ErrorRecord.InvocationInfo;
          MshLog.GetLogProvider(executionContext).LogProviderHealthEvent(MshLog.GetLogContext(executionContext, invocationInfo, severity), providerName, exception);
        }
      }
    }

    internal static void LogProviderLifecycleEvent(
      ExecutionContext executionContext,
      string providerName,
      ProviderState providerState)
    {
      using (MshLog._trace.TraceMethod())
      {
        if (executionContext == null)
        {
          MshLog._trace.NewArgumentNullException(nameof (executionContext));
        }
        else
        {
          if (!MshLog.NeedToLogProviderLifecycleEvent(executionContext))
            return;
          MshLog.GetLogProvider(executionContext).LogProviderLifecycleEvent(MshLog.GetLogContext(executionContext, (InvocationInfo) null), providerName, providerState);
        }
      }
    }

    internal static void LogSettingsEvent(
      ExecutionContext executionContext,
      string variableName,
      string newValue,
      string previousValue,
      InvocationInfo invocationInfo)
    {
      using (MshLog._trace.TraceMethod())
      {
        if (executionContext == null)
        {
          MshLog._trace.NewArgumentNullException(nameof (executionContext));
        }
        else
        {
          if (!MshLog.NeedToLogSettingsEvent(executionContext))
            return;
          MshLog.GetLogProvider(executionContext).LogSettingsEvent(MshLog.GetLogContext(executionContext, invocationInfo), variableName, newValue, previousValue);
        }
      }
    }

    internal static void LogSettingsEvent(
      ExecutionContext executionContext,
      string variableName,
      string newValue,
      string previousValue)
    {
      using (MshLog._trace.TraceMethod())
        MshLog.LogSettingsEvent(executionContext, variableName, newValue, previousValue, (InvocationInfo) null);
    }

    private static EngineState GetEngineState(ExecutionContext executionContext) => executionContext.EngineState;

    private static void SetEngineState(ExecutionContext executionContext, EngineState engineState) => executionContext.EngineState = engineState;

    private static LogContext GetLogContext(
      ExecutionContext executionContext,
      InvocationInfo invocationInfo)
    {
      return MshLog.GetLogContext(executionContext, invocationInfo, Severity.Informational);
    }

    private static LogContext GetLogContext(
      ExecutionContext executionContext,
      InvocationInfo invocationInfo,
      Severity severity)
    {
      if (executionContext == null)
        return (LogContext) null;
      LogContext logContext = new LogContext();
      logContext.ShellId = executionContext.ShellID;
      logContext.Severity = severity.ToString();
      if (executionContext.EngineHostInterface != null)
      {
        logContext.HostName = executionContext.EngineHostInterface.Name;
        logContext.HostVersion = executionContext.EngineHostInterface.Version.ToString();
        logContext.HostId = executionContext.EngineHostInterface.InstanceId.ToString();
      }
      if (executionContext.CurrentRunspace != null)
      {
        logContext.EngineVersion = executionContext.CurrentRunspace.Version.ToString();
        logContext.RunspaceId = executionContext.CurrentRunspace.InstanceId.ToString();
        Pipeline currentlyRunningPipeline = executionContext.CurrentRunspace.GetCurrentlyRunningPipeline();
        if (currentlyRunningPipeline != null)
          logContext.PipelineId = currentlyRunningPipeline.InstanceId.ToString((IFormatProvider) CultureInfo.CurrentCulture);
      }
      logContext.SequenceNumber = MshLog.NextSequenceNumber;
      logContext.User = Environment.UserDomainName + "\\" + Environment.UserName;
      logContext.Time = DateTime.Now.ToString((IFormatProvider) CultureInfo.CurrentCulture);
      if (invocationInfo == null)
        return logContext;
      logContext.ScriptName = invocationInfo.ScriptName;
      logContext.CommandLine = invocationInfo.Line;
      if (invocationInfo.MyCommand != null)
      {
        logContext.CommandName = invocationInfo.MyCommand.Name;
        logContext.CommandType = invocationInfo.MyCommand.CommandType.ToString();
        switch (invocationInfo.MyCommand.CommandType)
        {
          case CommandTypes.ExternalScript:
            logContext.CommandPath = ((ExternalScriptInfo) invocationInfo.MyCommand).Path;
            break;
          case CommandTypes.Application:
            logContext.CommandPath = ((ApplicationInfo) invocationInfo.MyCommand).Path;
            break;
        }
      }
      return logContext;
    }

    private static bool NeedToLogEngineHealthEvent(ExecutionContext executionContext) => LanguagePrimitives.IsTrue(executionContext.GetVariable("LogEngineHealthEvent", (object) true));

    private static bool NeedToLogEngineLifecycleEvent(ExecutionContext executionContext) => LanguagePrimitives.IsTrue(executionContext.GetVariable("LogEngineLifecycleEvent", (object) true));

    private static bool NeedToLogCommandHealthEvent(ExecutionContext executionContext) => LanguagePrimitives.IsTrue(executionContext.GetVariable("LogCommandHealthEvent", (object) false));

    private static bool NeedToLogCommandLifecycleEvent(ExecutionContext executionContext) => LanguagePrimitives.IsTrue(executionContext.GetVariable("LogCommandLifecycleEvent", (object) false));

    private static bool NeedToLogPipelineExecutionDetailEvent(ExecutionContext executionContext) => true;

    private static bool NeedToLogProviderHealthEvent(ExecutionContext executionContext) => LanguagePrimitives.IsTrue(executionContext.GetVariable("LogProviderHealthEvent", (object) true));

    private static bool NeedToLogProviderLifecycleEvent(ExecutionContext executionContext) => LanguagePrimitives.IsTrue(executionContext.GetVariable("LogProviderLifecycleEvent", (object) true));

    private static bool NeedToLogSettingsEvent(ExecutionContext executionContext) => LanguagePrimitives.IsTrue(executionContext.GetVariable("LogSettingsEvent", (object) true));

    private static string NextSequenceNumber
    {
      get
      {
        using (MshLog._trace.TraceProperty())
          return Convert.ToString(Interlocked.Increment(ref MshLog._nextSequenceNumber), (IFormatProvider) CultureInfo.CurrentCulture);
      }
    }
  }
}
