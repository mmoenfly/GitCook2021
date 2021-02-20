// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.WhereObjectCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("Where", "Object")]
  public sealed class WhereObjectCommand : PSCmdlet
  {
    [TraceSource("Parser", "Parser")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer("Parser", "Parser");
    private PSObject _inputObject = AutomationNull.Value;
    private ScriptBlock script;

    [Parameter(ValueFromPipeline = true)]
    public PSObject InputObject
    {
      set => this._inputObject = value;
      get => this._inputObject;
    }

    [Parameter(Mandatory = true, Position = 0)]
    public ScriptBlock FilterScript
    {
      set => this.script = value;
      get => this.script;
    }

    protected override void ProcessRecord()
    {
      if (this._inputObject == AutomationNull.Value)
        return;
      if (!LanguagePrimitives.IsTrue(this.script.InvokeUsingCmdlet((Cmdlet) null, false, true, (object) this.InputObject, (object) new object[1]
      {
        (object) this._inputObject
      }, (object) AutomationNull.Value)))
        return;
      this.WriteObject((object) this.InputObject);
    }
  }
}
