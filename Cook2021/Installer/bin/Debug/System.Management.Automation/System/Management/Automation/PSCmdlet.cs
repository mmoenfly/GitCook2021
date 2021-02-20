// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSCmdlet
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  public abstract class PSCmdlet : Cmdlet
  {
    [TraceSource("Cmdlet", "Cmdlet")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("Cmdlet", "Cmdlet");
    private CommandInvocationIntrinsics _invokeCommand;
    private ProviderIntrinsics invokeProvider;

    internal bool HasDynamicParameters => this is IDynamicParameters;

    public string ParameterSetName => this._ParameterSetName;

    public new InvocationInfo MyInvocation => base.MyInvocation;

    public CommandInvocationIntrinsics InvokeCommand
    {
      get
      {
        if (this._invokeCommand == null)
          this._invokeCommand = new CommandInvocationIntrinsics(this.Context, this);
        return this._invokeCommand;
      }
    }

    public PSHost Host => this.PSHostInternal;

    public SessionState SessionState => this.State;

    public PSEventManager Events => (PSEventManager) this.Context.Events;

    public JobRepository JobRepository => ((LocalRunspace) this.Context.CurrentRunspace).JobRepository;

    internal RunspaceRepository RunspaceRepository => ((LocalRunspace) this.Context.CurrentRunspace).RunspaceRepository;

    public ProviderIntrinsics InvokeProvider
    {
      get
      {
        if (this.invokeProvider == null)
          this.invokeProvider = new ProviderIntrinsics((Cmdlet) this);
        return this.invokeProvider;
      }
    }

    public PathInfo CurrentProviderLocation(string providerId)
    {
      using (PSCmdlet.tracer.TraceMethod(providerId, new object[0]))
      {
        if (providerId == null)
          throw PSCmdlet.tracer.NewArgumentNullException(nameof (providerId));
        PathInfo pathInfo = this.SessionState.Path.CurrentProviderLocation(providerId);
        PSCmdlet.tracer.WriteLine("result = {0}", (object) pathInfo);
        return pathInfo;
      }
    }

    public string GetUnresolvedProviderPathFromPSPath(string path)
    {
      using (PSCmdlet.tracer.TraceMethod(path, new object[0]))
        return this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path);
    }

    public Collection<string> GetResolvedProviderPathFromPSPath(
      string path,
      out ProviderInfo provider)
    {
      using (PSCmdlet.tracer.TraceMethod(path, new object[0]))
        return this.SessionState.Path.GetResolvedProviderPathFromPSPath(path, out provider);
    }

    protected PSCmdlet()
    {
      using (PSCmdlet.tracer.TraceConstructor((object) this))
        ;
    }

    public object GetVariableValue(string name)
    {
      using (PSCmdlet.tracer.TraceMethod(name, new object[0]))
        return this.SessionState.PSVariable.GetValue(name);
    }

    public object GetVariableValue(string name, object defaultValue)
    {
      using (PSCmdlet.tracer.TraceMethod(name, new object[0]))
        return this.SessionState.PSVariable.GetValue(name, defaultValue);
    }
  }
}
