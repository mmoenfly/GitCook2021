// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ContentCmdletProviderIntrinsics
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Provider;

namespace System.Management.Automation
{
  public sealed class ContentCmdletProviderIntrinsics
  {
    [TraceSource("ProviderIntrinsics", "The APIs that are exposed to the Cmdlet base class for manipulating providers")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("ProviderIntrinsics", "The APIs that are exposed to the Cmdlet base class for manipulating providers");
    private Cmdlet cmdlet;
    private SessionStateInternal sessionState;

    private ContentCmdletProviderIntrinsics()
    {
    }

    internal ContentCmdletProviderIntrinsics(Cmdlet cmdlet)
    {
      this.cmdlet = cmdlet != null ? cmdlet : throw ContentCmdletProviderIntrinsics.tracer.NewArgumentNullException(nameof (cmdlet));
      this.sessionState = cmdlet.Context.EngineSessionState;
    }

    internal ContentCmdletProviderIntrinsics(SessionStateInternal sessionState) => this.sessionState = sessionState != null ? sessionState : throw ContentCmdletProviderIntrinsics.tracer.NewArgumentNullException(nameof (sessionState));

    public Collection<IContentReader> GetReader(string path) => this.sessionState.GetContentReader(new string[1]
    {
      path
    }, false, false);

    public Collection<IContentReader> GetReader(
      string[] path,
      bool force,
      bool literalPath)
    {
      return this.sessionState.GetContentReader(path, force, literalPath);
    }

    internal Collection<IContentReader> GetReader(
      string path,
      CmdletProviderContext context)
    {
      return this.sessionState.GetContentReader(new string[1]
      {
        path
      }, context);
    }

    internal object GetContentReaderDynamicParameters(string path, CmdletProviderContext context) => this.sessionState.GetContentReaderDynamicParameters(path, context);

    public Collection<IContentWriter> GetWriter(string path) => this.sessionState.GetContentWriter(new string[1]
    {
      path
    }, false, false);

    public Collection<IContentWriter> GetWriter(
      string[] path,
      bool force,
      bool literalPath)
    {
      return this.sessionState.GetContentWriter(path, force, literalPath);
    }

    internal Collection<IContentWriter> GetWriter(
      string path,
      CmdletProviderContext context)
    {
      return this.sessionState.GetContentWriter(new string[1]
      {
        path
      }, context);
    }

    internal object GetContentWriterDynamicParameters(string path, CmdletProviderContext context) => this.sessionState.GetContentWriterDynamicParameters(path, context);

    public void Clear(string path) => this.sessionState.ClearContent(new string[1]
    {
      path
    }, false, false);

    public void Clear(string[] path, bool force, bool literalPath) => this.sessionState.ClearContent(path, force, literalPath);

    internal void Clear(string path, CmdletProviderContext context) => this.sessionState.ClearContent(new string[1]
    {
      path
    }, context);

    internal object ClearContentDynamicParameters(string path, CmdletProviderContext context) => this.sessionState.ClearContentDynamicParameters(path, context);
  }
}
