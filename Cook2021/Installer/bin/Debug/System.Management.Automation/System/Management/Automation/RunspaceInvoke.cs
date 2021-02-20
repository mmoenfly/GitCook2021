// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RunspaceInvoke
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  public class RunspaceInvoke : IDisposable
  {
    private Runspace _runspace;
    private bool _disposed;
    [TraceSource("RunspaceInvoke", "RunspaceInvoke")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer(nameof (RunspaceInvoke), nameof (RunspaceInvoke));

    public RunspaceInvoke()
    {
      using (RunspaceInvoke._trace.TraceConstructor((object) this))
      {
        this._runspace = RunspaceFactory.CreateRunspace(RunspaceConfiguration.Create());
        this._runspace.Open();
        if (Runspace.DefaultRunspace != null)
          return;
        Runspace.DefaultRunspace = this._runspace;
      }
    }

    public RunspaceInvoke(RunspaceConfiguration runspaceConfiguration)
    {
      using (RunspaceInvoke._trace.TraceConstructor((object) this))
      {
        this._runspace = runspaceConfiguration != null ? RunspaceFactory.CreateRunspace(runspaceConfiguration) : throw RunspaceInvoke._trace.NewArgumentNullException(nameof (runspaceConfiguration));
        this._runspace.Open();
        if (Runspace.DefaultRunspace != null)
          return;
        Runspace.DefaultRunspace = this._runspace;
      }
    }

    public RunspaceInvoke(string consoleFilePath)
    {
      using (RunspaceInvoke._trace.TraceConstructor((object) this))
      {
        PSConsoleLoadException warnings;
        RunspaceConfiguration runspaceConfiguration = consoleFilePath != null ? RunspaceConfiguration.Create(consoleFilePath, out warnings) : throw RunspaceInvoke._trace.NewArgumentNullException(nameof (consoleFilePath));
        if (warnings != null)
          throw warnings;
        this._runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
        this._runspace.Open();
        if (Runspace.DefaultRunspace != null)
          return;
        Runspace.DefaultRunspace = this._runspace;
      }
    }

    public RunspaceInvoke(Runspace runspace)
    {
      using (RunspaceInvoke._trace.TraceConstructor((object) this))
      {
        this._runspace = runspace != null ? runspace : throw RunspaceInvoke._trace.NewArgumentNullException(nameof (runspace));
        if (Runspace.DefaultRunspace != null)
          return;
        Runspace.DefaultRunspace = this._runspace;
      }
    }

    public Collection<PSObject> Invoke(string script)
    {
      using (RunspaceInvoke._trace.TraceMethod())
        return this.Invoke(script, (IEnumerable) null);
    }

    public Collection<PSObject> Invoke(string script, IEnumerable input)
    {
      using (RunspaceInvoke._trace.TraceMethod())
      {
        if (this._disposed)
          throw RunspaceInvoke._trace.NewObjectDisposedException("runspace");
        if (script == null)
          throw RunspaceInvoke._trace.NewArgumentNullException(nameof (script));
        return this._runspace.CreatePipeline(script).Invoke(input);
      }
    }

    public Collection<PSObject> Invoke(
      string script,
      IEnumerable input,
      out IList errors)
    {
      using (RunspaceInvoke._trace.TraceMethod())
      {
        if (this._disposed)
          throw RunspaceInvoke._trace.NewObjectDisposedException("runspace");
        Pipeline pipeline = script != null ? this._runspace.CreatePipeline(script) : throw RunspaceInvoke._trace.NewArgumentNullException(nameof (script));
        Collection<PSObject> collection = pipeline.Invoke(input);
        errors = (IList) pipeline.Error.NonBlockingRead();
        return collection;
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!this._disposed && disposing)
      {
        this._runspace.Close();
        this._runspace = (Runspace) null;
      }
      this._disposed = true;
    }
  }
}
