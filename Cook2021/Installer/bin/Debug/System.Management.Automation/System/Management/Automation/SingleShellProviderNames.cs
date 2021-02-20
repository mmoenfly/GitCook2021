// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.SingleShellProviderNames
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class SingleShellProviderNames : ProviderNames
  {
    internal override string Environment => "Microsoft.PowerShell.Core\\Environment";

    internal override string Certificate => "Microsoft.PowerShell.Security\\Certificate";

    internal override string Variable => "Microsoft.PowerShell.Core\\Variable";

    internal override string Alias => "Microsoft.PowerShell.Core\\Alias";

    internal override string Function => "Microsoft.PowerShell.Core\\Function";

    internal override string FileSystem => "Microsoft.PowerShell.Core\\FileSystem";

    internal override string Registry => "Microsoft.PowerShell.Core\\Registry";
  }
}
