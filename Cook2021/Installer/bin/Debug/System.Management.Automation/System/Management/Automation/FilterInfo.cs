// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.FilterInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  public class FilterInfo : FunctionInfo
  {
    [TraceSource("FilterInfo", "The command information for MSH filters.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (FilterInfo), "The command information for MSH filters.");

    internal FilterInfo(string name, ScriptBlock filter, ExecutionContext context)
      : base(name, filter, context)
      => this.SetCommandType(CommandTypes.Filter);

    internal FilterInfo(
      string name,
      ScriptBlock filter,
      ScopedItemOptions options,
      ExecutionContext context)
      : base(name, filter, options, context)
    {
      this.SetCommandType(CommandTypes.Filter);
    }

    internal FilterInfo(FilterInfo other)
      : base((FunctionInfo) other)
    {
    }

    internal override CommandInfo CreateGetCommandCopy(object[] arguments)
    {
      FilterInfo filterInfo = new FilterInfo(this);
      filterInfo.IsGetCommandCopy = true;
      filterInfo.Arguments = arguments;
      return (CommandInfo) filterInfo;
    }

    internal override HelpCategory HelpCategory => HelpCategory.Filter;
  }
}
