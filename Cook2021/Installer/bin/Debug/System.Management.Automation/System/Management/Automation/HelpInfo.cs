// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.HelpInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  internal abstract class HelpInfo
  {
    private HelpCategory _forwardHelpCategory;
    private string _forwardTarget = "";
    private Collection<ErrorRecord> _errors;
    [TraceSource("HelpInfo", "HelpInfo")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (HelpInfo), nameof (HelpInfo));

    internal HelpInfo()
    {
      using (HelpInfo.tracer.TraceConstructor((object) this))
        ;
    }

    internal abstract string Name { get; }

    internal abstract string Synopsis { get; }

    internal virtual string Component => string.Empty;

    internal virtual string Role => string.Empty;

    internal virtual string Functionality => string.Empty;

    internal abstract HelpCategory HelpCategory { get; }

    internal HelpCategory ForwardHelpCategory
    {
      get
      {
        using (HelpInfo.tracer.TraceProperty())
          return this._forwardHelpCategory;
      }
      set
      {
        using (HelpInfo.tracer.TraceProperty())
          this._forwardHelpCategory = value;
      }
    }

    internal string ForwardTarget
    {
      get
      {
        using (HelpInfo.tracer.TraceProperty())
          return this._forwardTarget;
      }
      set
      {
        using (HelpInfo.tracer.TraceProperty())
          this._forwardTarget = value;
      }
    }

    internal abstract PSObject FullHelp { get; }

    internal PSObject ShortHelp
    {
      get
      {
        using (HelpInfo.tracer.TraceProperty())
        {
          if (this.FullHelp == null)
            return (PSObject) null;
          PSObject psObject = this.FullHelp.Copy();
          psObject.TypeNames.Clear();
          psObject.TypeNames.Add("HelpInfoShort");
          return psObject;
        }
      }
    }

    internal virtual PSObject[] GetParameter(string pattern)
    {
      using (HelpInfo.tracer.TraceMethod("HelpInfo.GetParameter", new object[0]))
        return new PSObject[0];
    }

    internal virtual Uri GetUriForOnlineHelp()
    {
      using (HelpInfo.tracer.TraceMethod("HelpInfo.GetUriForOnlineHelp", new object[0]))
        return (Uri) null;
    }

    internal virtual bool MatchPatternInContent(WildcardPattern pattern) => false;

    protected void AddCommonHelpProperties()
    {
      if (this.FullHelp == null)
        return;
      if (this.FullHelp.Properties["Name"] == null)
        this.FullHelp.Properties.Add((PSPropertyInfo) new PSNoteProperty("Name", (object) this.Name.ToString()));
      if (this.FullHelp.Properties["Category"] == null)
        this.FullHelp.Properties.Add((PSPropertyInfo) new PSNoteProperty("Category", (object) this.HelpCategory.ToString()));
      if (this.FullHelp.Properties["Synopsis"] == null)
        this.FullHelp.Properties.Add((PSPropertyInfo) new PSNoteProperty("Synopsis", (object) this.Synopsis.ToString()));
      if (this.FullHelp.Properties["Component"] == null)
        this.FullHelp.Properties.Add((PSPropertyInfo) new PSNoteProperty("Component", (object) this.Component));
      if (this.FullHelp.Properties["Role"] == null)
        this.FullHelp.Properties.Add((PSPropertyInfo) new PSNoteProperty("Role", (object) this.Role));
      if (this.FullHelp.Properties["Functionality"] != null)
        return;
      this.FullHelp.Properties.Add((PSPropertyInfo) new PSNoteProperty("Functionality", (object) this.Functionality));
    }

    protected void UpdateUserDefinedDataProperties()
    {
      if (this.FullHelp == null)
        return;
      this.FullHelp.Properties.Remove("Component");
      this.FullHelp.Properties.Add((PSPropertyInfo) new PSNoteProperty("Component", (object) this.Component));
      this.FullHelp.Properties.Remove("Role");
      this.FullHelp.Properties.Add((PSPropertyInfo) new PSNoteProperty("Role", (object) this.Role));
      this.FullHelp.Properties.Remove("Functionality");
      this.FullHelp.Properties.Add((PSPropertyInfo) new PSNoteProperty("Functionality", (object) this.Functionality));
    }

    internal Collection<ErrorRecord> Errors
    {
      get
      {
        using (HelpInfo.tracer.TraceProperty())
          return this._errors;
      }
      set
      {
        using (HelpInfo.tracer.TraceProperty())
          this._errors = value;
      }
    }
  }
}
