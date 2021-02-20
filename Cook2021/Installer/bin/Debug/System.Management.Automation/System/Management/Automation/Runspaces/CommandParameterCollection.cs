// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.CommandParameterCollection
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation.Runspaces
{
  public sealed class CommandParameterCollection : Collection<CommandParameter>
  {
    [TraceSource("ParameterCollection", "ParameterCollection")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("ParameterCollection", "ParameterCollection");

    internal CommandParameterCollection()
    {
      using (CommandParameterCollection._trace.TraceConstructor((object) this))
        ;
    }

    public void Add(string name)
    {
      using (CommandParameterCollection._trace.TraceMethod())
        this.Add(new CommandParameter(name));
    }

    public void Add(string name, object value)
    {
      using (CommandParameterCollection._trace.TraceMethod())
        this.Add(new CommandParameter(name, value));
    }
  }
}
