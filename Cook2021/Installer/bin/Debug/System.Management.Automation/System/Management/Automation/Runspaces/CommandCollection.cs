// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.CommandCollection
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation.Runspaces
{
  public sealed class CommandCollection : Collection<Command>
  {
    internal CommandCollection()
    {
    }

    public void Add(string command)
    {
      if (string.Equals(command, "out-default", StringComparison.OrdinalIgnoreCase))
        this.Add(command, true);
      else
        this.Add(new Command(command));
    }

    internal void Add(string command, bool mergeUnclaimedPreviousCommandError) => this.Add(new Command(command, false, new bool?(false), mergeUnclaimedPreviousCommandError));

    public void AddScript(string scriptContents) => this.Add(new Command(scriptContents, true));

    public void AddScript(string scriptContents, bool useLocalScope) => this.Add(new Command(scriptContents, true, useLocalScope));

    internal string GetCommandStringForHistory() => this[0].CommandText;
  }
}
