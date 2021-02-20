// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CmdletHelpInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class CmdletHelpInfo : HelpInfo
  {
    private PSObject fullHelpObject;

    internal CmdletHelpInfo(CmdletInfo cmdletInfo)
    {
      this.fullHelpObject = new PSObject();
      this.fullHelpObject.Properties.Add((PSPropertyInfo) new PSNoteProperty(nameof (Name), (object) cmdletInfo.Name));
      this.fullHelpObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("ModuleName", (object) cmdletInfo.ModuleName));
      this.fullHelpObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("Syntax", (object) cmdletInfo.Definition));
      this.fullHelpObject.TypeNames.Clear();
      this.fullHelpObject.TypeNames.Add(nameof (CmdletHelpInfo));
      this.fullHelpObject.TypeNames.Add("HelpInfo");
      this.AddCommonHelpProperties();
    }

    internal override string Name => this.fullHelpObject.Properties[nameof (Name)].Value.ToString();

    internal override string Synopsis => this.fullHelpObject.Properties["Syntax"].Value.ToString();

    internal override HelpCategory HelpCategory => HelpCategory.Cmdlet;

    internal override PSObject FullHelp => this.fullHelpObject;
  }
}
