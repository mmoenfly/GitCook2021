// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.AliasHelpInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;

namespace System.Management.Automation
{
  internal class AliasHelpInfo : HelpInfo
  {
    private string _name = "";
    private string _synopsis = "";
    private PSObject _fullHelpObject;
    [TraceSource("AliasHelpInfo", "AliasHelpInfo")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (AliasHelpInfo), nameof (AliasHelpInfo));

    private AliasHelpInfo(AliasInfo aliasInfo)
    {
      this._fullHelpObject = new PSObject();
      this.ForwardTarget = aliasInfo.ResolvedCommand.Name;
      this.ForwardHelpCategory = HelpCategory.Cmdlet;
      if (!string.IsNullOrEmpty(aliasInfo.Name))
        this._name = aliasInfo.Name.Trim();
      if (!string.IsNullOrEmpty(aliasInfo.ResolvedCommand.Name))
        this._synopsis = aliasInfo.ResolvedCommand.Name.Trim();
      this._fullHelpObject.TypeNames.Clear();
      this._fullHelpObject.TypeNames.Add(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "AliasHelpInfo#{0}", (object) this.Name));
      this._fullHelpObject.TypeNames.Add(nameof (AliasHelpInfo));
      this._fullHelpObject.TypeNames.Add("HelpInfo");
    }

    internal override string Name => this._name;

    internal override string Synopsis => this._synopsis;

    internal override HelpCategory HelpCategory => HelpCategory.Alias;

    internal override PSObject FullHelp => this._fullHelpObject;

    internal static AliasHelpInfo GetHelpInfo(AliasInfo aliasInfo)
    {
      if (aliasInfo == null)
        return (AliasHelpInfo) null;
      if (aliasInfo.ResolvedCommand == null)
        return (AliasHelpInfo) null;
      AliasHelpInfo aliasHelpInfo = new AliasHelpInfo(aliasInfo);
      if (string.IsNullOrEmpty(aliasHelpInfo.Name))
        return (AliasHelpInfo) null;
      aliasHelpInfo.AddCommonHelpProperties();
      return aliasHelpInfo;
    }
  }
}
