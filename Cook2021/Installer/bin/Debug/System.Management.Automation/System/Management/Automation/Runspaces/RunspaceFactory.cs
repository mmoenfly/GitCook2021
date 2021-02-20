// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.RunspaceFactory
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell;
using System.Management.Automation.Host;
using System.Threading;

namespace System.Management.Automation.Runspaces
{
  public static class RunspaceFactory
  {
    [TraceSource("RunspaceFactory", "Runspace Tracer")]
    private static PSTraceSource _tracer = PSTraceSource.GetTracer(nameof (RunspaceFactory), "Runspace Tracer");

    public static Runspace CreateRunspace()
    {
      using (RunspaceFactory._tracer.TraceMethod())
        return RunspaceFactory.CreateRunspace((PSHost) new DefaultHost(Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture));
    }

    public static Runspace CreateRunspace(PSHost host)
    {
      using (RunspaceFactory._tracer.TraceMethod())
        return host != null ? RunspaceFactory.CreateRunspace(host, RunspaceConfiguration.Create()) : throw RunspaceFactory._tracer.NewArgumentNullException(nameof (host));
    }

    public static Runspace CreateRunspace(RunspaceConfiguration runspaceConfiguration)
    {
      if (runspaceConfiguration == null)
        throw RunspaceFactory._tracer.NewArgumentNullException(nameof (runspaceConfiguration));
      return RunspaceFactory.CreateRunspace((PSHost) new DefaultHost(Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture), runspaceConfiguration);
    }

    public static Runspace CreateRunspace(
      PSHost host,
      RunspaceConfiguration runspaceConfiguration)
    {
      if (host == null)
        throw RunspaceFactory._tracer.NewArgumentNullException(nameof (host));
      return runspaceConfiguration != null ? (Runspace) new LocalRunspace(host, runspaceConfiguration) : throw RunspaceFactory._tracer.NewArgumentNullException(nameof (runspaceConfiguration));
    }

    public static Runspace CreateRunspace(InitialSessionState initialSessionState)
    {
      if (initialSessionState == null)
        throw RunspaceFactory._tracer.NewArgumentNullException(nameof (initialSessionState));
      return RunspaceFactory.CreateRunspace((PSHost) new DefaultHost(Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture), initialSessionState);
    }

    public static Runspace CreateRunspace(
      PSHost host,
      InitialSessionState initialSessionState)
    {
      if (host == null)
        throw RunspaceFactory._tracer.NewArgumentNullException(nameof (host));
      return initialSessionState != null ? (Runspace) new LocalRunspace(host, initialSessionState) : throw RunspaceFactory._tracer.NewArgumentNullException(nameof (initialSessionState));
    }

    internal static Runspace CreateRunspaceFromSessionStateNoClone(
      PSHost host,
      InitialSessionState initialSessionState)
    {
      if (host == null)
        throw RunspaceFactory._tracer.NewArgumentNullException(nameof (host));
      return initialSessionState != null ? (Runspace) new LocalRunspace(host, initialSessionState, true) : throw RunspaceFactory._tracer.NewArgumentNullException(nameof (initialSessionState));
    }

    public static RunspacePool CreateRunspacePool()
    {
      using (RunspaceFactory._tracer.TraceMethod())
        return RunspaceFactory.CreateRunspacePool(1, 1, RunspaceConfiguration.Create(), (PSHost) new DefaultHost(Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture));
    }

    public static RunspacePool CreateRunspacePool(int minRunspaces, int maxRunspaces)
    {
      using (RunspaceFactory._tracer.TraceMethod())
        return RunspaceFactory.CreateRunspacePool(minRunspaces, maxRunspaces, RunspaceConfiguration.Create(), (PSHost) new DefaultHost(Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture));
    }

    public static RunspacePool CreateRunspacePool(
      InitialSessionState initialSessionState)
    {
      using (RunspaceFactory._tracer.TraceMethod())
        return RunspaceFactory.CreateRunspacePool(1, 1, initialSessionState, (PSHost) new DefaultHost(Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture));
    }

    public static RunspacePool CreateRunspacePool(
      int minRunspaces,
      int maxRunspaces,
      PSHost host)
    {
      using (RunspaceFactory._tracer.TraceMethod())
        return RunspaceFactory.CreateRunspacePool(minRunspaces, maxRunspaces, RunspaceConfiguration.Create(), host);
    }

    private static RunspacePool CreateRunspacePool(
      int minRunspaces,
      int maxRunspaces,
      RunspaceConfiguration runspaceConfiguration,
      PSHost host)
    {
      using (RunspaceFactory._tracer.TraceMethod())
        return new RunspacePool(minRunspaces, maxRunspaces, runspaceConfiguration, host);
    }

    public static RunspacePool CreateRunspacePool(
      int minRunspaces,
      int maxRunspaces,
      InitialSessionState initialSessionState,
      PSHost host)
    {
      using (RunspaceFactory._tracer.TraceMethod())
        return new RunspacePool(minRunspaces, maxRunspaces, initialSessionState, host);
    }

    public static RunspacePool CreateRunspacePool(
      int minRunspaces,
      int maxRunspaces,
      RunspaceConnectionInfo connectionInfo)
    {
      using (RunspaceFactory._tracer.TraceMethod())
        return RunspaceFactory.CreateRunspacePool(minRunspaces, maxRunspaces, connectionInfo, (PSHost) null);
    }

    public static RunspacePool CreateRunspacePool(
      int minRunspaces,
      int maxRunspaces,
      RunspaceConnectionInfo connectionInfo,
      PSHost host)
    {
      return RunspaceFactory.CreateRunspacePool(minRunspaces, maxRunspaces, connectionInfo, host, (TypeTable) null);
    }

    public static RunspacePool CreateRunspacePool(
      int minRunspaces,
      int maxRunspaces,
      RunspaceConnectionInfo connectionInfo,
      PSHost host,
      TypeTable typeTable)
    {
      return RunspaceFactory.CreateRunspacePool(minRunspaces, maxRunspaces, connectionInfo, host, typeTable, (PSPrimitiveDictionary) null);
    }

    public static RunspacePool CreateRunspacePool(
      int minRunspaces,
      int maxRunspaces,
      RunspaceConnectionInfo connectionInfo,
      PSHost host,
      TypeTable typeTable,
      PSPrimitiveDictionary applicationArguments)
    {
      switch (connectionInfo)
      {
        case WSManConnectionInfo _:
        case NewProcessConnectionInfo _:
          return new RunspacePool(minRunspaces, maxRunspaces, typeTable, host, applicationArguments, connectionInfo);
        default:
          throw new NotSupportedException();
      }
    }

    public static Runspace CreateRunspace(
      RunspaceConnectionInfo connectionInfo,
      PSHost host,
      TypeTable typeTable)
    {
      return RunspaceFactory.CreateRunspace(connectionInfo, host, typeTable, (PSPrimitiveDictionary) null);
    }

    public static Runspace CreateRunspace(
      RunspaceConnectionInfo connectionInfo,
      PSHost host,
      TypeTable typeTable,
      PSPrimitiveDictionary applicationArguments)
    {
      switch (connectionInfo)
      {
        case WSManConnectionInfo _:
        case NewProcessConnectionInfo _:
          return (Runspace) new RemoteRunspace(typeTable, connectionInfo, host, applicationArguments);
        default:
          throw new NotSupportedException();
      }
    }

    public static Runspace CreateRunspace(
      PSHost host,
      RunspaceConnectionInfo connectionInfo)
    {
      return RunspaceFactory.CreateRunspace(connectionInfo, host, (TypeTable) null);
    }

    public static Runspace CreateRunspace(RunspaceConnectionInfo connectionInfo) => RunspaceFactory.CreateRunspace((PSHost) null, connectionInfo);
  }
}
