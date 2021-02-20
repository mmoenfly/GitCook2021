// Decompiled with JetBrains decompiler
// Type: Partial.Properties.Settings
// Assembly: PartialBeta, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9CF783A5-38FA-472F-8C51-9A2203433095
// Assembly location: E:\git\partialbits\PartialBeta.exe

using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Partial.Properties
{
  [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
  [CompilerGenerated]
  internal sealed class Settings : ApplicationSettingsBase
  {
    private static Settings defaultInstance = (Settings) SettingsBase.Synchronized((SettingsBase) new Settings());

    public static Settings Default => Settings.defaultInstance;

    [SpecialSetting(SpecialSetting.WebServiceUrl)]
    [DefaultSettingValue("http://cci.cookconsulting.net:80/cciweb.nsf/GetPartialCustomers?OpenWebService")]
    [ApplicationScopedSetting]
    [DebuggerNonUserCode]
    public string Partial_WSCustomer_GetCustomersPartialService => (string) this[nameof (Partial_WSCustomer_GetCustomersPartialService)];

    [DebuggerNonUserCode]
    [SpecialSetting(SpecialSetting.WebServiceUrl)]
    [DefaultSettingValue("http://cci.cookconsulting.net:80/cciweb.nsf/RemovePartialFlags?OpenWebService")]
    [ApplicationScopedSetting]
    public string Partial_WSPartials_RemovePartialFlagsService => (string) this[nameof (Partial_WSPartials_RemovePartialFlagsService)];

    [DebuggerNonUserCode]
    [DefaultSettingValue("http://installs.ccisupportsite.com/ap/wsl/ws_logger.asmx")]
    [ApplicationScopedSetting]
    [SpecialSetting(SpecialSetting.WebServiceUrl)]
    public string Partial_wslogger_WS_Logger => (string) this[nameof (Partial_wslogger_WS_Logger)];
  }
}
