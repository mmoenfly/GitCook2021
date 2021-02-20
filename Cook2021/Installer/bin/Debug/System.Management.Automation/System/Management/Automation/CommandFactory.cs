// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandFactory
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class CommandFactory
  {
    [TraceSource("CommandFactory", "CommandFactory")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (CommandFactory), nameof (CommandFactory));
    private ExecutionContext context;

    internal ExecutionContext Context
    {
      get => this.context;
      set => this.context = value;
    }

    internal CommandFactory() => CommandFactory.tracer.TraceConstructor((object) this);

    internal CommandFactory(ExecutionContext context)
    {
      using (CommandFactory.tracer.TraceConstructor((object) this))
        this.Context = context;
    }

    internal CommandProcessorBase CreateCommand(
      string commandName,
      CommandOrigin commandOrigin)
    {
      return this._CreateCommand(commandName, commandOrigin, new bool?(false));
    }

    internal CommandProcessorBase CreateCommand(
      string commandName,
      CommandOrigin commandOrigin,
      bool? useLocalScope)
    {
      return this._CreateCommand(commandName, commandOrigin, useLocalScope);
    }

    internal CommandProcessorBase CreateCommand(
      string commandName,
      ExecutionContext executionContext,
      CommandOrigin commandOrigin)
    {
      this.Context = executionContext;
      return this._CreateCommand(commandName, commandOrigin, new bool?(false));
    }

    private CommandProcessorBase _CreateCommand(
      string commandName,
      CommandOrigin commandOrigin,
      bool? useLocalScope)
    {
      CommandFactory.tracer.WriteLine("Creating command from name: {0}", (object) commandName);
      return ((this.context != null ? this.context.CommandDiscovery : throw CommandFactory.tracer.NewInvalidOperationException("DiscoveryExceptions", "ExecutionContextNotSet")) ?? throw CommandFactory.tracer.NewInvalidOperationException("DiscoveryExceptions", "CommandDiscoveryMissing")).LookupCommandProcessor(commandName, commandOrigin, useLocalScope);
    }
  }
}
