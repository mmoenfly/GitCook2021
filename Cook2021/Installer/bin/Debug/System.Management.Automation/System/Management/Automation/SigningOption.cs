// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SigningOption
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public enum SigningOption
  {
    AddOnlyCertificate = 0,
    AddFullCertificateChain = 1,
    AddFullCertificateChainExceptRoot = 2,
    Default = 2,
  }
}
