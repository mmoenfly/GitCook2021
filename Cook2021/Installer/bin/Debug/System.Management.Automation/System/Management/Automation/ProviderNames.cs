// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ProviderNames
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal abstract class ProviderNames
  {
    internal abstract string Environment { get; }

    internal abstract string Certificate { get; }

    internal abstract string Variable { get; }

    internal abstract string Alias { get; }

    internal abstract string Function { get; }

    internal abstract string FileSystem { get; }

    internal abstract string Registry { get; }
  }
}
