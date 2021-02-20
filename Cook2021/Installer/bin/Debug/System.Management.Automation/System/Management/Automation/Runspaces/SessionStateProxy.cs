// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.SessionStateProxy
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation.Runspaces
{
  public class SessionStateProxy
  {
    private RunspaceBase _runspace;
    [TraceSource("SessionStateProxy", "SessionStateProxy")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (SessionStateProxy), nameof (SessionStateProxy));

    internal SessionStateProxy(RunspaceBase runspace) => this._runspace = runspace;

    public void SetVariable(string name, object value)
    {
      if (name == null)
        throw SessionStateProxy._trace.NewArgumentNullException(nameof (name));
      this._runspace.SetVariable(name, value);
    }

    public object GetVariable(string name) => name != null ? this._runspace.GetVariable(name) : throw SessionStateProxy._trace.NewArgumentNullException(nameof (name));

    public List<string> Applications => this._runspace.Applications;

    public List<string> Scripts => this._runspace.Scripts;

    public DriveManagementIntrinsics Drive => this._runspace.Drive;

    public PSLanguageMode LanguageMode
    {
      get => this._runspace.LanguageMode;
      set => this._runspace.LanguageMode = value;
    }

    public PSModuleInfo Module => this._runspace.Module;

    public PathIntrinsics Path => this._runspace.PathIntrinsics;

    public CmdletProviderManagementIntrinsics Provider => this._runspace.Provider;

    public PSVariableIntrinsics PSVariable => this._runspace.PSVariable;

    public CommandInvocationIntrinsics InvokeCommand => this._runspace.InvokeCommand;

    public ProviderIntrinsics InvokeProvider => this._runspace.InvokeProvider;
  }
}
