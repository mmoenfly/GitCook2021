// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.PSSession
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.Threading;

namespace System.Management.Automation.Runspaces
{
  public sealed class PSSession
  {
    [TraceSource("RunspaceInfo", "PSSession")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("RunspaceInfo", nameof (PSSession));
    private RemoteRunspace remoteRunspace;
    private string shell;
    private static int seed = 0;
    private int sessionid;
    private string name;

    public string ComputerName => this.remoteRunspace.ConnectionInfo.ComputerName;

    public string ConfigurationName => this.shell;

    public Guid InstanceId => this.remoteRunspace.InstanceId;

    public int Id => this.sessionid;

    public string Name
    {
      get => this.name;
      set => this.name = value;
    }

    public RunspaceAvailability Availability => this.Runspace.RunspaceAvailability;

    public PSPrimitiveDictionary ApplicationPrivateData => this.Runspace.GetApplicationPrivateData();

    public Runspace Runspace => (Runspace) this.remoteRunspace;

    internal PSSession(RemoteRunspace remoteRunspace)
    {
      this.remoteRunspace = remoteRunspace;
      this.sessionid = Interlocked.Increment(ref PSSession.seed);
      this.shell = WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<string>(remoteRunspace.ConnectionInfo, "ShellUri", string.Empty);
      this.name = this.AutoGenerateRunspaceName();
    }

    internal PSSession(RemoteRunspace remoteRunspace, string name, string shell)
      : this(remoteRunspace)
    {
      if (!string.IsNullOrEmpty(name))
        this.name = name;
      this.shell = shell;
    }

    private string AutoGenerateRunspaceName() => "Session" + this.sessionid.ToString((IFormatProvider) NumberFormatInfo.InvariantInfo);
  }
}
