// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.ForEachObjectCommand
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
  [Cmdlet("ForEach", "Object")]
  public sealed class ForEachObjectCommand : PSCmdlet
  {
    [TraceSource("Parser", "Parser")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer("Parser", "Parser");
    private List<ScriptBlock> scripts = new List<ScriptBlock>();
    private PSObject _inputObject = AutomationNull.Value;
    private ScriptBlock endScript;
    private bool setEndScript;
    private int start;
    private int end;

    [Parameter(ValueFromPipeline = true)]
    public PSObject InputObject
    {
      set => this._inputObject = value;
      get => this._inputObject;
    }

    [Parameter]
    public ScriptBlock Begin
    {
      set => this.scripts.Insert(0, value);
      get => (ScriptBlock) null;
    }

    [Parameter(Mandatory = true, Position = 0, ValueFromRemainingArguments = true)]
    [AllowEmptyCollection]
    [AllowNull]
    public ScriptBlock[] Process
    {
      set
      {
        if (value == null)
          this.scripts.Add((ScriptBlock) null);
        else
          this.scripts.AddRange((IEnumerable<ScriptBlock>) value);
      }
      get => (ScriptBlock[]) null;
    }

    [Parameter]
    public ScriptBlock End
    {
      set
      {
        this.endScript = value;
        this.setEndScript = true;
      }
      get => this.endScript;
    }

    protected override void BeginProcessing()
    {
      this.end = this.scripts.Count;
      this.start = this.scripts.Count > 1 ? 1 : 0;
      if (!this.setEndScript && this.scripts.Count > 2)
      {
        this.end = this.scripts.Count - 1;
        this.endScript = this.scripts[this.end];
      }
      if (this.end < 2 || this.scripts[0] == null)
        return;
      object sendToPipeline = this.scripts[0].InvokeUsingCmdlet((Cmdlet) this, false, true, (object) AutomationNull.Value, (object) new object[0], (object) AutomationNull.Value);
      if (sendToPipeline == AutomationNull.Value)
        return;
      this.WriteObject(sendToPipeline);
    }

    protected override void ProcessRecord()
    {
      for (int start = this.start; start < this.end; ++start)
      {
        if (this.scripts[start] != null)
        {
          object sendToPipeline = this.scripts[start].InvokeUsingCmdlet((Cmdlet) this, false, true, (object) this.InputObject, (object) new object[1]
          {
            (object) this.InputObject
          }, (object) AutomationNull.Value);
          if (sendToPipeline != AutomationNull.Value)
            this.WriteObject(sendToPipeline);
        }
      }
    }

    protected override void EndProcessing()
    {
      if (this.endScript == null)
        return;
      object sendToPipeline = this.endScript.InvokeUsingCmdlet((Cmdlet) this, false, true, (object) AutomationNull.Value, (object) new object[0], (object) AutomationNull.Value);
      if (sendToPipeline == AutomationNull.Value)
        return;
      this.WriteObject(sendToPipeline);
    }
  }
}
