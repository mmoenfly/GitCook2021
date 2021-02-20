// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CustomShellProviderNames
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class CustomShellProviderNames : ProviderNames
  {
    internal override string Environment => nameof (Environment);

    internal override string Certificate => nameof (Certificate);

    internal override string Variable => nameof (Variable);

    internal override string Alias => nameof (Alias);

    internal override string Function => nameof (Function);

    internal override string FileSystem => nameof (FileSystem);

    internal override string Registry => nameof (Registry);
  }
}
