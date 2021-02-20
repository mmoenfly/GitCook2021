// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.RunspaceConnectionInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.Management.Automation.Remoting;
using System.Threading;

namespace System.Management.Automation.Runspaces
{
  public abstract class RunspaceConnectionInfo
  {
    internal const int defaultOpenTimeout = 180000;
    internal const int defaultCancelTimeout = 60000;
    internal const int defaultIdleTimeout = 240000;
    [TraceSource("RSCI", "Connection Info for Remote Runspaces")]
    internal static readonly PSTraceSource tracer = PSTraceSource.GetTracer("RSCI", "Connection Info for Remote Runspaces");
    private CultureInfo culture = Thread.CurrentThread.CurrentCulture;
    private CultureInfo uiCulture = Thread.CurrentThread.CurrentUICulture;
    private int openTimeout = 180000;
    private int cancelTimeout = 60000;
    private int operationTimeout = 180000;
    private int idleTimeout = 240000;

    public abstract string ComputerName { get; }

    public abstract PSCredential Credential { get; }

    public abstract AuthenticationMechanism AuthenticationMechanism { get; set; }

    public abstract string CertificateThumbprint { get; }

    public CultureInfo Culture
    {
      get => this.culture;
      set => this.culture = value != null ? value : throw new ArgumentNullException(nameof (value));
    }

    public CultureInfo UICulture
    {
      get => this.uiCulture;
      set => this.uiCulture = value != null ? value : throw new ArgumentNullException(nameof (value));
    }

    public int OpenTimeout
    {
      get => this.openTimeout;
      set => this.openTimeout = value;
    }

    public int CancelTimeout
    {
      get => this.cancelTimeout;
      set => this.cancelTimeout = value;
    }

    public int OperationTimeout
    {
      get => this.operationTimeout;
      set => this.operationTimeout = value;
    }

    public int IdleTimeout
    {
      get => this.idleTimeout;
      set => this.idleTimeout = value;
    }

    internal int TimeSpanToTimeOutMs(TimeSpan t) => t.TotalMilliseconds > (double) int.MaxValue || t == TimeSpan.MaxValue || t.TotalMilliseconds < 0.0 ? int.MaxValue : (int) t.TotalMilliseconds;

    internal virtual void SetSessionOptions(PSSessionOption options)
    {
      if (options.Culture != null)
        this.Culture = options.Culture;
      if (options.UICulture != null)
        this.UICulture = options.UICulture;
      this.openTimeout = this.TimeSpanToTimeOutMs(options.OpenTimeout);
      this.cancelTimeout = this.TimeSpanToTimeOutMs(options.CancelTimeout);
      this.idleTimeout = this.TimeSpanToTimeOutMs(options.IdleTimeout);
      this.operationTimeout = this.TimeSpanToTimeOutMs(options.OperationTimeout);
    }
  }
}
