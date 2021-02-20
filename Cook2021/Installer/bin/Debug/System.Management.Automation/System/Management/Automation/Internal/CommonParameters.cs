// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.CommonParameters
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Internal
{
  public sealed class CommonParameters
  {
    [TraceSource("CommonCommandParameters", "This class is used to expose the ubiquitous parameters to the command line")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("CommonCommandParameters", "This class is used to expose the ubiquitous parameters to the command line");
    private MshCommandRuntime commandRuntime;

    internal CommonParameters(MshCommandRuntime commandRuntime)
    {
      using (CommonParameters.tracer.TraceConstructor((object) this))
        this.commandRuntime = commandRuntime != null ? commandRuntime : throw CommonParameters.tracer.NewArgumentNullException(nameof (commandRuntime));
    }

    [Parameter]
    [Alias(new string[] {"vb"})]
    public SwitchParameter Verbose
    {
      get => (SwitchParameter) this.commandRuntime.Verbose;
      set => this.commandRuntime.Verbose = (bool) value;
    }

    [Alias(new string[] {"db"})]
    [Parameter]
    public SwitchParameter Debug
    {
      get => (SwitchParameter) this.commandRuntime.Debug;
      set => this.commandRuntime.Debug = (bool) value;
    }

    [Parameter]
    [Alias(new string[] {"ea"})]
    public ActionPreference ErrorAction
    {
      get => this.commandRuntime.ErrorAction;
      set => this.commandRuntime.ErrorAction = value;
    }

    [Parameter]
    [Alias(new string[] {"wa"})]
    public ActionPreference WarningAction
    {
      get => this.commandRuntime.WarningPreference;
      set => this.commandRuntime.WarningPreference = value;
    }

    [Alias(new string[] {"ev"})]
    [Parameter]
    [CommonParameters.ValidateVariableName]
    public string ErrorVariable
    {
      get => this.commandRuntime.ErrorVariable;
      set => this.commandRuntime.ErrorVariable = value;
    }

    [Alias(new string[] {"wv"})]
    [Parameter]
    [CommonParameters.ValidateVariableName]
    public string WarningVariable
    {
      get => this.commandRuntime.WarningVariable;
      set => this.commandRuntime.WarningVariable = value;
    }

    [CommonParameters.ValidateVariableName]
    [Alias(new string[] {"ov"})]
    [Parameter]
    public string OutVariable
    {
      get => this.commandRuntime.OutVariable;
      set => this.commandRuntime.OutVariable = value;
    }

    [Alias(new string[] {"ob"})]
    [Parameter]
    [ValidateRange(0, 2147483647)]
    public int OutBuffer
    {
      get => this.commandRuntime.OutBuffer;
      set => this.commandRuntime.OutBuffer = value;
    }

    internal class ValidateVariableName : ValidateArgumentsAttribute
    {
      protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
      {
        if (!(arguments is string path))
          return;
        if (path.StartsWith("+", StringComparison.Ordinal))
          path = path.Substring(1);
        if (!new ScopedItemLookupPath(path).IsScopedItem)
          throw new ValidationMetadataException("ArgumentNotValidVariableName", (Exception) null, "Metadata", nameof (ValidateVariableName), new object[1]
          {
            (object) path
          });
      }
    }
  }
}
