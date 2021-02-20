// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ScriptTrace
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal.Host;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  internal static class ScriptTrace
  {
    internal static void Trace(int level, string messageId, params object[] args)
    {
      ExecutionContext executionContextFromTls = LocalPipeline.GetExecutionContextFromTLS();
      if (executionContextFromTls == null)
        return;
      ScriptTrace.Trace(executionContextFromTls, level, messageId, args);
    }

    internal static void Trace(
      ExecutionContext context,
      int level,
      string messageId,
      params object[] args)
    {
      ActionPreference preference = ActionPreference.Continue;
      if (context.PSDebug <= level)
        return;
      string message = args == null || args.Length == 0 ? ResourceManagerCache.GetResourceString("Parser", messageId) : ResourceManagerCache.FormatResourceString("Parser", messageId, args);
      if (string.IsNullOrEmpty(message))
        message = "Could not load text for msh script tracing message id '" + messageId + "'";
      ((InternalHostUserInterface) context.EngineHostInterface.UI).WriteDebugLine(message, ref preference);
    }

    internal static void TraceLine(ExecutionContext context, ParseTreeNode statement)
    {
      if (statement == null || !context.ShouldTraceStatement || statement.NodeToken == null)
        return;
      string message = statement.NodeToken.Position(false);
      InternalHostUserInterface ui = (InternalHostUserInterface) context.EngineHostInterface.UI;
      ActionPreference preference = context.StepScript ? ActionPreference.Inquire : ActionPreference.Continue;
      ui.WriteDebugLine(message, ref preference);
      if (preference != ActionPreference.Continue)
        return;
      context.StepScript = false;
    }
  }
}
