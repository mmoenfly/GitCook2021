// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.PSCorePSSnapIn
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.ComponentModel;
using System.Management.Automation;

namespace Microsoft.PowerShell
{
  [RunInstaller(true)]
  public sealed class PSCorePSSnapIn : PSSnapIn
  {
    private string[] _types = new string[1]
    {
      "types.ps1xml"
    };
    private string[] _formats = new string[7]
    {
      "Certificate.format.ps1xml",
      "DotNetTypes.format.ps1xml",
      "FileSystem.format.ps1xml",
      "Help.format.ps1xml",
      "PowerShellCore.format.ps1xml",
      "PowerShellTrace.format.ps1xml",
      "Registry.format.ps1xml"
    };

    public override string Name => "Microsoft.PowerShell.Core";

    public override string Vendor => "Microsoft";

    public override string VendorResource => "CoreMshSnapInResources,Vendor";

    public override string Description => "This PSSnapIn contains MSH management cmdlets used to manage components affecting the MSH engine.";

    public override string DescriptionResource => "CoreMshSnapInResources,Description";

    public override string[] Types => this._types;

    public override string[] Formats => this._formats;
  }
}
