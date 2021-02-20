// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  public sealed class PSCommand
  {
    [TraceSource("PowerShellHosting", "Powershell hosting interfaces")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("PowerShellHosting", "Powershell hosting interfaces");
    private PowerShell owner;
    private CommandCollection commands;
    private Command currentCommand;
    private static string resBaseName = "PSCommandStrings";

    public PSCommand()
    {
      using (PSCommand.tracer.TraceConstructor((object) this))
        this.Initialize((string) null, false, new bool?());
    }

    internal PSCommand(PSCommand commandToClone)
    {
      this.commands = new CommandCollection();
      foreach (Command command1 in (Collection<Command>) commandToClone.Commands)
      {
        Command command2 = command1.Clone();
        this.commands.Add(command2);
        this.currentCommand = command2;
      }
    }

    internal PSCommand(Command command)
    {
      this.currentCommand = command;
      this.commands = new CommandCollection();
      this.commands.Add(this.currentCommand);
    }

    public PSCommand AddCommand(string command)
    {
      using (PSCommand.tracer.TraceMethod())
      {
        if (command == null)
          throw PSCommand.tracer.NewArgumentNullException("cmdlet");
        if (this.owner != null)
          this.owner.AssertChangesAreAccepted();
        this.currentCommand = new Command(command, false);
        this.commands.Add(this.currentCommand);
        return this;
      }
    }

    public PSCommand AddCommand(string cmdlet, bool useLocalScope)
    {
      using (PSCommand.tracer.TraceMethod())
      {
        if (cmdlet == null)
          throw PSCommand.tracer.NewArgumentNullException(nameof (cmdlet));
        if (this.owner != null)
          this.owner.AssertChangesAreAccepted();
        this.currentCommand = new Command(cmdlet, false, useLocalScope);
        this.commands.Add(this.currentCommand);
        return this;
      }
    }

    public PSCommand AddScript(string script)
    {
      using (PSCommand.tracer.TraceMethod())
      {
        if (script == null)
          throw PSCommand.tracer.NewArgumentNullException(nameof (script));
        if (this.owner != null)
          this.owner.AssertChangesAreAccepted();
        this.currentCommand = new Command(script, true);
        this.commands.Add(this.currentCommand);
        return this;
      }
    }

    public PSCommand AddScript(string script, bool useLocalScope)
    {
      using (PSCommand.tracer.TraceMethod())
      {
        if (script == null)
          throw PSCommand.tracer.NewArgumentNullException(nameof (script));
        if (this.owner != null)
          this.owner.AssertChangesAreAccepted();
        this.currentCommand = new Command(script, true, useLocalScope);
        this.commands.Add(this.currentCommand);
        return this;
      }
    }

    public PSCommand AddCommand(Command command)
    {
      using (PSCommand.tracer.TraceMethod())
      {
        if (command == null)
          throw PSCommand.tracer.NewArgumentNullException(nameof (command));
        if (this.owner != null)
          this.owner.AssertChangesAreAccepted();
        this.currentCommand = command;
        this.commands.Add(this.currentCommand);
        return this;
      }
    }

    public PSCommand AddParameter(string parameterName, object value)
    {
      using (PSCommand.tracer.TraceMethod())
      {
        if (this.currentCommand == null)
          throw PSCommand.tracer.NewInvalidOperationException(PSCommand.resBaseName, "ParameterRequiresCommand", (object) nameof (PSCommand));
        if (this.owner != null)
          this.owner.AssertChangesAreAccepted();
        this.currentCommand.Parameters.Add(parameterName, value);
        return this;
      }
    }

    public PSCommand AddParameter(string parameterName)
    {
      using (PSCommand.tracer.TraceMethod())
      {
        if (this.currentCommand == null)
          throw PSCommand.tracer.NewInvalidOperationException(PSCommand.resBaseName, "ParameterRequiresCommand", (object) nameof (PSCommand));
        if (this.owner != null)
          this.owner.AssertChangesAreAccepted();
        this.currentCommand.Parameters.Add(parameterName, (object) true);
        return this;
      }
    }

    public PSCommand AddArgument(object value)
    {
      using (PSCommand.tracer.TraceMethod())
      {
        if (this.currentCommand == null)
          throw PSCommand.tracer.NewInvalidOperationException(PSCommand.resBaseName, "ParameterRequiresCommand", (object) nameof (PSCommand));
        if (this.owner != null)
          this.owner.AssertChangesAreAccepted();
        this.currentCommand.Parameters.Add((string) null, value);
        return this;
      }
    }

    public CommandCollection Commands => this.commands;

    internal PowerShell Owner
    {
      get => this.owner;
      set => this.owner = value;
    }

    public void Clear()
    {
      using (PSCommand.tracer.TraceMethod())
      {
        this.commands.Clear();
        this.currentCommand = (Command) null;
      }
    }

    public PSCommand Clone()
    {
      using (PSCommand.tracer.TraceMethod())
        return new PSCommand(this);
    }

    private void Initialize(string command, bool isScript, bool? useLocalScope)
    {
      this.commands = new CommandCollection();
      if (command == null)
        return;
      this.currentCommand = new Command(command, isScript, useLocalScope);
      this.commands.Add(this.currentCommand);
    }
  }
}
