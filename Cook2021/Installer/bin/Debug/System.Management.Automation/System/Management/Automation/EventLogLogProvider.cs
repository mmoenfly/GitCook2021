// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.EventLogLogProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;

namespace System.Management.Automation
{
  internal class EventLogLogProvider : LogProvider
  {
    private const int EngineHealthCategoryId = 1;
    private const int CommandHealthCategoryId = 2;
    private const int ProviderHealthCategoryId = 3;
    private const int EngineLifecycleCategoryId = 4;
    private const int CommandLifecycleCategoryId = 5;
    private const int ProviderLifecycleCategoryId = 6;
    private const int SettingsCategoryId = 7;
    private const int PipelineExecutionDetailCategoryId = 8;
    private const int _baseEngineLifecycleEventId = 400;
    private const int _invalidEventId = -1;
    private const int _commandHealthEventId = 200;
    private const int _baseCommandLifecycleEventId = 500;
    private const int _pipelineExecutionDetailEventId = 800;
    private const int MaxLength = 16000;
    private const int _providerHealthEventId = 300;
    private const int _baseProviderLifecycleEventId = 600;
    private const int _settingsEventId = 700;
    private EventLog _eventLog;
    private ResourceManager _resourceManager;
    [TraceSource("EventLogLogProvider", "EventLogLogProvider")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (EventLogLogProvider), nameof (EventLogLogProvider));

    internal EventLogLogProvider(string shellId)
    {
      string str = this.SetupEventSource(shellId);
      this._eventLog = new EventLog();
      this._eventLog.Source = str;
      this._resourceManager = new ResourceManager("Logging", Assembly.GetExecutingAssembly());
    }

    internal string SetupEventSource(string shellId)
    {
      string source;
      if (string.IsNullOrEmpty(shellId))
      {
        source = "Default";
      }
      else
      {
        int num = shellId.LastIndexOf('.');
        source = num >= 0 ? shellId.Substring(num + 1) : shellId;
        if (string.IsNullOrEmpty(source))
          source = "Default";
      }
      return EventLog.SourceExists(source) ? source : throw new InvalidOperationException(string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, "Event source '{0}' is not registered", (object) source));
    }

    private static string GetMessageDllPath(string shellId)
    {
      string path1 = (string) null;
      if (!string.IsNullOrEmpty(shellId))
        path1 = Path.GetDirectoryName(CommandDiscovery.GetShellPathFromRegistry(shellId));
      if (string.IsNullOrEmpty(path1) && Assembly.GetEntryAssembly() != null)
        path1 = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
      return Path.Combine(path1, "pwrshmsg.dll");
    }

    internal override void LogEngineHealthEvent(
      LogContext logContext,
      int eventId,
      Exception exception,
      Dictionary<string, string> additionalInfo)
    {
      using (EventLogLogProvider._trace.TraceMethod())
      {
        Hashtable mapArgs = new Hashtable();
        if (exception is IContainsErrorRecord containsErrorRecord && containsErrorRecord.ErrorRecord != null)
        {
          mapArgs[(object) "ExceptionClass"] = (object) exception.GetType().Name;
          mapArgs[(object) "ErrorCategory"] = (object) containsErrorRecord.ErrorRecord.CategoryInfo.Category;
          mapArgs[(object) "ErrorId"] = (object) containsErrorRecord.ErrorRecord.FullyQualifiedErrorId;
          if (containsErrorRecord.ErrorRecord.ErrorDetails != null)
            mapArgs[(object) "ErrorMessage"] = (object) containsErrorRecord.ErrorRecord.ErrorDetails.Message;
          else
            mapArgs[(object) "ErrorMessage"] = (object) exception.Message;
        }
        else
        {
          mapArgs[(object) "ExceptionClass"] = (object) exception.GetType().Name;
          mapArgs[(object) "ErrorCategory"] = (object) "";
          mapArgs[(object) "ErrorId"] = (object) "";
          mapArgs[(object) "ErrorMessage"] = (object) exception.Message;
        }
        EventLogLogProvider.FillEventArgs(mapArgs, logContext);
        EventLogLogProvider.FillEventArgs(mapArgs, additionalInfo);
        EventInstance entry = new EventInstance((long) eventId, 1);
        entry.EntryType = EventLogLogProvider.GetEventLogEntryType(logContext);
        string eventDetail = this.GetEventDetail("EngineHealthContext", mapArgs);
        this.LogEvent(entry, mapArgs[(object) "ErrorMessage"], (object) eventDetail);
      }
    }

    private static EventLogEntryType GetEventLogEntryType(LogContext logContext)
    {
      switch (logContext.Severity)
      {
        case "Critical":
        case "Error":
          return EventLogEntryType.Error;
        case "Warning":
          return EventLogEntryType.Warning;
        default:
          return EventLogEntryType.Information;
      }
    }

    internal override void LogEngineLifecycleEvent(
      LogContext logContext,
      EngineState newState,
      EngineState previousState)
    {
      using (EventLogLogProvider._trace.TraceMethod())
      {
        int lifecycleEventId = EventLogLogProvider.GetEngineLifecycleEventId(newState);
        if (lifecycleEventId == -1)
          return;
        Hashtable mapArgs = new Hashtable();
        mapArgs[(object) "NewEngineState"] = (object) newState.ToString();
        mapArgs[(object) "PreviousEngineState"] = (object) previousState.ToString();
        EventLogLogProvider.FillEventArgs(mapArgs, logContext);
        EventInstance entry = new EventInstance((long) lifecycleEventId, 4);
        entry.EntryType = EventLogEntryType.Information;
        string eventDetail = this.GetEventDetail("EngineLifecycleContext", mapArgs);
        this.LogEvent(entry, (object) newState, (object) previousState, (object) eventDetail);
      }
    }

    private static int GetEngineLifecycleEventId(EngineState engineState)
    {
      switch (engineState)
      {
        case EngineState.None:
          return -1;
        case EngineState.Available:
          return 400;
        case EngineState.Degraded:
          return 401;
        case EngineState.OutOfService:
          return 402;
        case EngineState.Stopped:
          return 403;
        default:
          return -1;
      }
    }

    internal override void LogCommandHealthEvent(LogContext logContext, Exception exception)
    {
      using (EventLogLogProvider._trace.TraceMethod())
      {
        int num = 200;
        Hashtable mapArgs = new Hashtable();
        if (exception is IContainsErrorRecord containsErrorRecord && containsErrorRecord.ErrorRecord != null)
        {
          mapArgs[(object) "ExceptionClass"] = (object) exception.GetType().Name;
          mapArgs[(object) "ErrorCategory"] = (object) containsErrorRecord.ErrorRecord.CategoryInfo.Category;
          mapArgs[(object) "ErrorId"] = (object) containsErrorRecord.ErrorRecord.FullyQualifiedErrorId;
          if (containsErrorRecord.ErrorRecord.ErrorDetails != null)
            mapArgs[(object) "ErrorMessage"] = (object) containsErrorRecord.ErrorRecord.ErrorDetails.Message;
          else
            mapArgs[(object) "ErrorMessage"] = (object) exception.Message;
        }
        else
        {
          mapArgs[(object) "ExceptionClass"] = (object) exception.GetType().Name;
          mapArgs[(object) "ErrorCategory"] = (object) "";
          mapArgs[(object) "ErrorId"] = (object) "";
          mapArgs[(object) "ErrorMessage"] = (object) exception.Message;
        }
        EventLogLogProvider.FillEventArgs(mapArgs, logContext);
        EventInstance entry = new EventInstance((long) num, 2);
        entry.EntryType = EventLogLogProvider.GetEventLogEntryType(logContext);
        string eventDetail = this.GetEventDetail("CommandHealthContext", mapArgs);
        this.LogEvent(entry, mapArgs[(object) "ErrorMessage"], (object) eventDetail);
      }
    }

    internal override void LogCommandLifecycleEvent(LogContext logContext, CommandState newState)
    {
      using (EventLogLogProvider._trace.TraceMethod())
      {
        int lifecycleEventId = EventLogLogProvider.GetCommandLifecycleEventId(newState);
        if (lifecycleEventId == -1)
          return;
        Hashtable mapArgs = new Hashtable();
        mapArgs[(object) "NewCommandState"] = (object) newState.ToString();
        EventLogLogProvider.FillEventArgs(mapArgs, logContext);
        EventInstance entry = new EventInstance((long) lifecycleEventId, 5);
        entry.EntryType = EventLogEntryType.Information;
        string eventDetail = this.GetEventDetail("CommandLifecycleContext", mapArgs);
        this.LogEvent(entry, (object) logContext.CommandName, (object) newState, (object) eventDetail);
      }
    }

    private static int GetCommandLifecycleEventId(CommandState commandState)
    {
      switch (commandState)
      {
        case CommandState.Started:
          return 500;
        case CommandState.Stopped:
          return 501;
        case CommandState.Terminated:
          return 502;
        default:
          return -1;
      }
    }

    internal override void LogPipelineExecutionDetailEvent(
      LogContext logContext,
      List<string> pipelineExecutionDetail)
    {
      using (EventLogLogProvider._trace.TraceMethod())
      {
        List<string> stringList = this.GroupMessages(pipelineExecutionDetail);
        for (int index = 0; index < stringList.Count; ++index)
          this.LogPipelineExecutionDetailEvent(logContext, stringList[index], index + 1, stringList.Count);
      }
    }

    private List<string> GroupMessages(List<string> messages)
    {
      List<string> stringList = new List<string>();
      if (messages == null || messages.Count == 0)
        return stringList;
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < messages.Count; ++index)
      {
        if (stringBuilder.Length + messages[index].Length < 16000)
        {
          stringBuilder.AppendLine(messages[index]);
        }
        else
        {
          stringList.Add(stringBuilder.ToString());
          stringBuilder = new StringBuilder();
          stringBuilder.AppendLine(messages[index]);
        }
      }
      stringList.Add(stringBuilder.ToString());
      return stringList;
    }

    private void LogPipelineExecutionDetailEvent(
      LogContext logContext,
      string pipelineExecutionDetail,
      int detailSequence,
      int detailTotal)
    {
      using (EventLogLogProvider._trace.TraceMethod())
      {
        int num = 800;
        Hashtable mapArgs = new Hashtable();
        mapArgs[(object) "PipelineExecutionDetail"] = (object) pipelineExecutionDetail;
        mapArgs[(object) "DetailSequence"] = (object) detailSequence;
        mapArgs[(object) "DetailTotal"] = (object) detailTotal;
        EventLogLogProvider.FillEventArgs(mapArgs, logContext);
        EventInstance entry = new EventInstance((long) num, 8);
        entry.EntryType = EventLogEntryType.Information;
        string eventDetail = this.GetEventDetail("PipelineExecutionDetailContext", mapArgs);
        this.LogEvent(entry, (object) logContext.CommandLine, (object) eventDetail, (object) pipelineExecutionDetail);
      }
    }

    internal override void LogProviderHealthEvent(
      LogContext logContext,
      string providerName,
      Exception exception)
    {
      using (EventLogLogProvider._trace.TraceMethod())
      {
        int num = 300;
        Hashtable mapArgs = new Hashtable();
        mapArgs[(object) "ProviderName"] = (object) providerName;
        if (exception is IContainsErrorRecord containsErrorRecord && containsErrorRecord.ErrorRecord != null)
        {
          mapArgs[(object) "ExceptionClass"] = (object) exception.GetType().Name;
          mapArgs[(object) "ErrorCategory"] = (object) containsErrorRecord.ErrorRecord.CategoryInfo.Category;
          mapArgs[(object) "ErrorId"] = (object) containsErrorRecord.ErrorRecord.FullyQualifiedErrorId;
          if (containsErrorRecord.ErrorRecord.ErrorDetails != null && !string.IsNullOrEmpty(containsErrorRecord.ErrorRecord.ErrorDetails.Message))
            mapArgs[(object) "ErrorMessage"] = (object) containsErrorRecord.ErrorRecord.ErrorDetails.Message;
          else
            mapArgs[(object) "ErrorMessage"] = (object) exception.Message;
        }
        else
        {
          mapArgs[(object) "ExceptionClass"] = (object) exception.GetType().Name;
          mapArgs[(object) "ErrorCategory"] = (object) "";
          mapArgs[(object) "ErrorId"] = (object) "";
          mapArgs[(object) "ErrorMessage"] = (object) exception.Message;
        }
        EventLogLogProvider.FillEventArgs(mapArgs, logContext);
        EventInstance entry = new EventInstance((long) num, 3);
        entry.EntryType = EventLogLogProvider.GetEventLogEntryType(logContext);
        string eventDetail = this.GetEventDetail("ProviderHealthContext", mapArgs);
        this.LogEvent(entry, mapArgs[(object) "ErrorMessage"], (object) eventDetail);
      }
    }

    internal override void LogProviderLifecycleEvent(
      LogContext logContext,
      string providerName,
      ProviderState newState)
    {
      using (EventLogLogProvider._trace.TraceMethod())
      {
        int lifecycleEventId = EventLogLogProvider.GetProviderLifecycleEventId(newState);
        if (lifecycleEventId == -1)
          return;
        Hashtable mapArgs = new Hashtable();
        mapArgs[(object) "ProviderName"] = (object) providerName;
        mapArgs[(object) "NewProviderState"] = (object) newState.ToString();
        EventLogLogProvider.FillEventArgs(mapArgs, logContext);
        EventInstance entry = new EventInstance((long) lifecycleEventId, 6);
        entry.EntryType = EventLogEntryType.Information;
        string eventDetail = this.GetEventDetail("ProviderLifecycleContext", mapArgs);
        this.LogEvent(entry, (object) providerName, (object) newState, (object) eventDetail);
      }
    }

    private static int GetProviderLifecycleEventId(ProviderState providerState)
    {
      switch (providerState)
      {
        case ProviderState.Started:
          return 600;
        case ProviderState.Stopped:
          return 601;
        default:
          return -1;
      }
    }

    internal override void LogSettingsEvent(
      LogContext logContext,
      string variableName,
      string value,
      string previousValue)
    {
      using (EventLogLogProvider._trace.TraceMethod())
      {
        int num = 700;
        Hashtable mapArgs = new Hashtable();
        mapArgs[(object) "VariableName"] = (object) variableName;
        mapArgs[(object) "NewValue"] = (object) value;
        mapArgs[(object) "PreviousValue"] = (object) previousValue;
        EventLogLogProvider.FillEventArgs(mapArgs, logContext);
        EventInstance entry = new EventInstance((long) num, 7);
        entry.EntryType = EventLogEntryType.Information;
        string eventDetail = this.GetEventDetail("SettingsContext", mapArgs);
        this.LogEvent(entry, (object) variableName, (object) value, (object) previousValue, (object) eventDetail);
      }
    }

    private void LogEvent(EventInstance entry, params object[] args)
    {
      using (EventLogLogProvider._trace.TraceMethod())
      {
        try
        {
          this._eventLog.WriteEvent(entry, args);
        }
        catch (ArgumentException ex)
        {
          EventLogLogProvider._trace.TraceException((Exception) ex);
        }
        catch (InvalidOperationException ex)
        {
          EventLogLogProvider._trace.TraceException((Exception) ex);
        }
        catch (Win32Exception ex)
        {
          EventLogLogProvider._trace.TraceException((Exception) ex);
        }
      }
    }

    private static void FillEventArgs(Hashtable mapArgs, LogContext logContext)
    {
      mapArgs[(object) "Severity"] = (object) logContext.Severity;
      mapArgs[(object) "SequenceNumber"] = (object) logContext.SequenceNumber;
      mapArgs[(object) "HostName"] = (object) logContext.HostName;
      mapArgs[(object) "HostVersion"] = (object) logContext.HostVersion;
      mapArgs[(object) "HostId"] = (object) logContext.HostId;
      mapArgs[(object) "EngineVersion"] = (object) logContext.EngineVersion;
      mapArgs[(object) "RunspaceId"] = (object) logContext.RunspaceId;
      mapArgs[(object) "PipelineId"] = (object) logContext.PipelineId;
      mapArgs[(object) "CommandName"] = (object) logContext.CommandName;
      mapArgs[(object) "CommandType"] = (object) logContext.CommandType;
      mapArgs[(object) "ScriptName"] = (object) logContext.ScriptName;
      mapArgs[(object) "CommandPath"] = (object) logContext.CommandPath;
      mapArgs[(object) "CommandLine"] = (object) logContext.CommandLine;
      mapArgs[(object) "User"] = (object) logContext.User;
      mapArgs[(object) "Time"] = (object) logContext.Time;
    }

    private static void FillEventArgs(Hashtable mapArgs, Dictionary<string, string> additionalInfo)
    {
      if (additionalInfo == null)
      {
        for (int index = 0; index < 3; ++index)
        {
          string str = (index + 1).ToString("d1", (IFormatProvider) CultureInfo.CurrentCulture);
          mapArgs[(object) ("AdditionalInfo_Name" + str)] = (object) "";
          mapArgs[(object) ("AdditionalInfo_Value" + str)] = (object) "";
        }
      }
      else
      {
        string[] array1 = new string[additionalInfo.Count];
        string[] array2 = new string[additionalInfo.Count];
        additionalInfo.Keys.CopyTo(array1, 0);
        additionalInfo.Values.CopyTo(array2, 0);
        for (int index = 0; index < 3; ++index)
        {
          string str = (index + 1).ToString("d1", (IFormatProvider) CultureInfo.CurrentCulture);
          if (index < array1.Length)
          {
            mapArgs[(object) ("AdditionalInfo_Name" + str)] = (object) array1[index];
            mapArgs[(object) ("AdditionalInfo_Value" + str)] = (object) array2[index];
          }
          else
          {
            mapArgs[(object) ("AdditionalInfo_Name" + str)] = (object) "";
            mapArgs[(object) ("AdditionalInfo_Value" + str)] = (object) "";
          }
        }
      }
    }

    private string GetEventDetail(string contextId, Hashtable mapArgs) => this.GetMessage(contextId, mapArgs);

    private string GetMessage(string messageId, Hashtable mapArgs)
    {
      if (this._resourceManager == null)
        return "";
      string messageTemplate = this._resourceManager.GetString(messageId);
      return string.IsNullOrEmpty(messageTemplate) ? "" : EventLogLogProvider.FillMessageTemplate(messageTemplate, mapArgs);
    }

    private static string FillMessageTemplate(string messageTemplate, Hashtable mapArgs)
    {
      StringBuilder stringBuilder = new StringBuilder();
      int startIndex = 0;
      while (true)
      {
        int num1 = messageTemplate.IndexOf('[', startIndex);
        if (num1 >= 0)
        {
          int num2 = messageTemplate.IndexOf(']', num1 + 1);
          if (num2 >= 0)
          {
            stringBuilder.Append(messageTemplate.Substring(startIndex, num1 - startIndex));
            int num3 = num1;
            string str = messageTemplate.Substring(num1 + 1, num2 - num1 - 1);
            if (mapArgs.Contains((object) str))
            {
              stringBuilder.Append(mapArgs[(object) str]);
              startIndex = num2 + 1;
            }
            else
            {
              stringBuilder.Append("[");
              startIndex = num3 + 1;
            }
          }
          else
            goto label_4;
        }
        else
          break;
      }
      stringBuilder.Append(messageTemplate.Substring(startIndex));
      return stringBuilder.ToString();
label_4:
      stringBuilder.Append(messageTemplate.Substring(startIndex));
      return stringBuilder.ToString();
    }
  }
}
