// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemoteHelpInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.PowerShell.Commands;
using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  internal class RemoteHelpInfo : BaseCommandHelpInfo
  {
    private PSObject deserializedRemoteHelp;
    [TraceSource("RemoteHelpInfo", "RemoteHelpInfo")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (RemoteHelpInfo), nameof (RemoteHelpInfo));

    internal RemoteHelpInfo(
      ExecutionContext context,
      RemoteRunspace remoteRunspace,
      string remoteHelpTopic,
      string remoteHelpCategory,
      HelpCategory localHelpCategory)
      : base(localHelpCategory)
    {
      using (System.Management.Automation.PowerShell powerShell = System.Management.Automation.PowerShell.Create())
      {
        powerShell.AddCommand("Get-Help");
        powerShell.AddParameter("Name", (object) remoteHelpTopic);
        if (!string.IsNullOrEmpty(remoteHelpCategory))
          powerShell.AddParameter("Category", (object) remoteHelpCategory);
        powerShell.Runspace = (Runspace) remoteRunspace;
        Collection<PSObject> collection;
        using (new PowerShellStopper(context, powerShell))
        {
          using (RemoteHelpInfo.tracer.TraceScope("Downloading remote help for {0}", (object) remoteHelpTopic))
            collection = powerShell.Invoke();
        }
        this.deserializedRemoteHelp = collection != null && collection.Count != 0 ? collection[0] : throw new HelpNotFoundException(remoteHelpTopic);
        this.deserializedRemoteHelp.TypeNames.Clear();
        this.deserializedRemoteHelp.TypeNames.Add("MamlCommandHelpInfo");
        this.deserializedRemoteHelp.TypeNames.Add("HelpInfo");
        this.deserializedRemoteHelp.Methods.Remove("ToString");
      }
    }

    internal override PSObject FullHelp => this.deserializedRemoteHelp;

    private string GetHelpProperty(string propertyName)
    {
      PSPropertyInfo property = this.deserializedRemoteHelp.Properties[propertyName];
      return property == null ? (string) null : property.Value as string;
    }

    internal override string Component => this.GetHelpProperty(nameof (Component));

    internal override string Functionality => this.GetHelpProperty(nameof (Functionality));

    internal override string Role => this.GetHelpProperty(nameof (Role));
  }
}
