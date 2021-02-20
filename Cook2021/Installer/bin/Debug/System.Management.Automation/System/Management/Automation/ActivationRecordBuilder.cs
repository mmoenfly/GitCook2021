// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ActivationRecordBuilder
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;

namespace System.Management.Automation
{
  internal class ActivationRecordBuilder : DoNothingParseTreeNodeVisitor
  {
    private Dictionary<string, int> variableSlots = new Dictionary<string, int>((IEqualityComparer<string>) StringComparer.CurrentCultureIgnoreCase);
    private int maxVariableSlot;
    private int maxPipelineSlot;

    internal ActivationRecordBuilder()
      : base(ParseTreeVisitorOptions.SkipInvokableScriptBlocks)
    {
      this.variableSlots.Add("_", 0);
      this.variableSlots.Add("this", 1);
      this.variableSlots.Add("input", 2);
      this.variableSlots.Add("args", 3);
      this.variableSlots.Add("PSCmdlet", 4);
      this.maxVariableSlot = this.variableSlots.Count;
    }

    internal override void Visit(VariableDereferenceNode node)
    {
      if (!node.IsScopedItem)
        return;
      int slot;
      if (!this.variableSlots.TryGetValue(node.VariableName, out slot))
      {
        slot = this.maxVariableSlot++;
        this.variableSlots.Add(node.VariableName, slot);
      }
      node.SetActivationRecordSlot(slot);
    }

    internal override void Visit(PipelineNode node) => node.SetActivationRecordSlot(this.maxPipelineSlot++);

    internal int PipelineSlots => this.maxPipelineSlot;

    internal int VariableSlots => this.maxVariableSlot;
  }
}
