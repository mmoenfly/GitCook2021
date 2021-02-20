// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ActivationRecord
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal class ActivationRecord
  {
    public const int DollarUnderSlot = 0;
    public const int ThisSlot = 1;
    public const int InputSlot = 2;
    public const int ArgsSlot = 3;
    public const int PSCmdletSlot = 4;
    public const int MaxPredefinedVariableSlots = 5;
    private bool[] pipelineFailed;
    private PSVariable[] variables;
    private SessionStateScope _scope;

    internal ActivationRecord()
      : this(0, 5, (SessionStateScope) null)
    {
    }

    internal ActivationRecord(int pipelineSlots, int variableSlots, SessionStateScope scope)
    {
      if (pipelineSlots < 0)
        throw new ArgumentOutOfRangeException(nameof (pipelineSlots));
      if (variableSlots < 0)
        throw new ArgumentOutOfRangeException(nameof (variableSlots));
      this.pipelineFailed = new bool[pipelineSlots];
      this.variables = new PSVariable[variableSlots];
      this._scope = scope;
    }

    internal bool GetExecutionFailed(int slot) => this.pipelineFailed[slot];

    internal void SetExecutionFailed(int slot, bool value) => this.pipelineFailed[slot] = value;

    internal PSVariable GetVariable(int slot, CommandOrigin origin)
    {
      PSVariable psVariable = this.variables[slot];
      if (psVariable != null)
      {
        if (psVariable.WasRemoved)
        {
          this.variables[slot] = (PSVariable) null;
          psVariable = (PSVariable) null;
        }
        SessionState.ThrowIfNotVisible(origin, (object) psVariable);
      }
      return psVariable;
    }

    internal void SetVariable(PSVariable variable, SessionStateScope varScope, int slot)
    {
      if (varScope != this._scope)
        return;
      this.variables[slot] = variable;
    }
  }
}
