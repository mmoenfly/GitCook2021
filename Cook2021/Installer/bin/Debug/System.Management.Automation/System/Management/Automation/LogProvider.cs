// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.LogProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  internal abstract class LogProvider
  {
    internal LogProvider()
    {
    }

    internal abstract void LogEngineHealthEvent(
      LogContext logContext,
      int eventId,
      Exception exception,
      Dictionary<string, string> additionalInfo);

    internal abstract void LogEngineLifecycleEvent(
      LogContext logContext,
      EngineState newState,
      EngineState previousState);

    internal abstract void LogCommandHealthEvent(LogContext logContext, Exception exception);

    internal abstract void LogCommandLifecycleEvent(LogContext logContext, CommandState newState);

    internal abstract void LogPipelineExecutionDetailEvent(
      LogContext logContext,
      List<string> pipelineExecutionDetail);

    internal abstract void LogProviderHealthEvent(
      LogContext logContext,
      string providerName,
      Exception exception);

    internal abstract void LogProviderLifecycleEvent(
      LogContext logContext,
      string providerName,
      ProviderState newState);

    internal abstract void LogSettingsEvent(
      LogContext logContext,
      string variableName,
      string value,
      string previousValue);
  }
}
