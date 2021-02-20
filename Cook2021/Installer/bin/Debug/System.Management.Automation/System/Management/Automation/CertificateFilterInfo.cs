// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CertificateFilterInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands;

namespace System.Management.Automation
{
  internal sealed class CertificateFilterInfo
  {
    internal const string CodeSigningOid = "1.3.6.1.5.5.7.3.3";
    [TraceSource("CertificateFilterInfo", "certificate provider filter info struct")]
    private static readonly PSTraceSource tracer = PSTraceSource.GetTracer(nameof (CertificateFilterInfo), "certificate provider filter info struct");
    private CertificatePurpose purpose;

    internal CertificatePurpose Purpose
    {
      get
      {
        using (CertificateFilterInfo.tracer.TraceProperty())
          return this.purpose;
      }
    }

    internal CertificateFilterInfo(CertificatePurpose p) => this.purpose = p;
  }
}
