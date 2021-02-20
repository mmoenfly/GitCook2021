// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.EngineIntrinsics
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Host;

namespace System.Management.Automation
{
  public class EngineIntrinsics
  {
    private ExecutionContext _context;
    private PSHost _host;
    private CommandInvocationIntrinsics _invokeCommand;

    private EngineIntrinsics()
    {
    }

    internal EngineIntrinsics(ExecutionContext context)
    {
      this._context = context != null ? context : throw new ArgumentNullException(nameof (context));
      this._host = (PSHost) context.EngineHostInterface;
    }

    public PSHost Host => this._host;

    public PSEventManager Events => (PSEventManager) this._context.Events;

    public ProviderIntrinsics InvokeProvider => this._context.EngineSessionState.InvokeProvider;

    public SessionState SessionState => this._context.EngineSessionState.PublicSessionState;

    public CommandInvocationIntrinsics InvokeCommand
    {
      get
      {
        if (this._invokeCommand == null)
          this._invokeCommand = new CommandInvocationIntrinsics(this._context);
        return this._invokeCommand;
      }
    }
  }
}
