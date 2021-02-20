// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.TransportErrorOccuredEventArgs
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Remoting
{
  internal class TransportErrorOccuredEventArgs : EventArgs
  {
    private PSRemotingTransportException exception;
    private TransportMethodEnum method;

    internal TransportErrorOccuredEventArgs(PSRemotingTransportException e, TransportMethodEnum m)
    {
      this.exception = e;
      this.method = m;
    }

    internal PSRemotingTransportException Exception
    {
      get => this.exception;
      set => this.exception = value;
    }

    internal TransportMethodEnum ReportingTransportMethod => this.method;
  }
}
