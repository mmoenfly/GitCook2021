// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.TypeInfoDataBase
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal sealed class TypeInfoDataBase
  {
    internal DefaultSettingsSection defaultSettingsSection = new DefaultSettingsSection();
    internal TypeGroupsSection typeGroupSection = new TypeGroupsSection();
    internal ViewDefinitionsSection viewDefinitionsSection = new ViewDefinitionsSection();
    internal FormatControlDefinitionHolder formatControlDefinitionHolder = new FormatControlDefinitionHolder();
    internal DisplayResourceManagerCache displayResourceManagerCache = new DisplayResourceManagerCache();
  }
}
