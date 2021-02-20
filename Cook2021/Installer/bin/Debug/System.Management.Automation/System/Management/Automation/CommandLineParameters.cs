// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandLineParameters
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  internal sealed class CommandLineParameters
  {
    private Dictionary<string, object> dictionary = new Dictionary<string, object>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private List<string> boundPositionally = new List<string>();

    internal bool ContainsKey(string name) => this.dictionary.ContainsKey(name);

    internal void Add(string name, object value) => this.dictionary[name] = value;

    internal void MarkAsBoundPositionally(string name) => this.boundPositionally.Add(name);

    internal void SetPSBoundParametersVariable(ExecutionContext context)
    {
      PSObject psObject = PSObject.AsPSObject((object) this.dictionary);
      psObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("BoundPositionally", (object) this.boundPositionally));
      context.SetVariable("PSBoundParameters", (object) psObject);
    }

    internal void UpdateInvocationInfo(InvocationInfo invocationInfo) => invocationInfo.BoundParameters = this.dictionary;
  }
}
