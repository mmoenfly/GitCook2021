// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.AuthorizationManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Host;

namespace System.Management.Automation
{
  public class AuthorizationManager
  {
    [TraceSource("AuthorizationManager", "tracer for AuthorizationManager")]
    private static readonly PSTraceSource tracer = PSTraceSource.GetTracer(nameof (AuthorizationManager), "tracer for AuthorizationManager");
    private string shellId;

    public AuthorizationManager(string shellId)
    {
      using (AuthorizationManager.tracer.TraceConstructor((object) this))
        this.shellId = shellId;
    }

    internal void ShouldRunInternal(CommandInfo commandInfo, CommandOrigin origin, PSHost host)
    {
      Exception reason = (Exception) null;
      bool flag;
      try
      {
        flag = this.ShouldRun(commandInfo, origin, host, out reason);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        flag = false;
        reason = (Exception) null;
      }
      if (flag)
        return;
      if (reason == null)
        throw new PSSecurityException(ResourceManagerCache.GetResourceString("AuthorizationManagerBase", "AuthorizationManagerDefaultFailureReason"));
      if (reason is PSSecurityException)
      {
        AuthorizationManager.tracer.TraceException(reason);
        throw reason;
      }
      PSSecurityException securityException = new PSSecurityException(reason.Message, reason);
      AuthorizationManager.tracer.TraceException((Exception) securityException);
      throw securityException;
    }

    internal string ShellId => this.shellId;

    protected internal virtual bool ShouldRun(
      CommandInfo commandInfo,
      CommandOrigin origin,
      PSHost host,
      out Exception reason)
    {
      reason = (Exception) null;
      return true;
    }
  }
}
