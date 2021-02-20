// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSVariableAttributeCollection
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;

namespace System.Management.Automation
{
  internal class PSVariableAttributeCollection : Collection<Attribute>
  {
    [TraceSource("SessionState", "Traces access to variables in session state.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("SessionState", "Traces access to variables in session state.");
    private PSVariable variable;

    internal PSVariableAttributeCollection(PSVariable variable) => this.variable = variable != null ? variable : throw PSVariableAttributeCollection.tracer.NewArgumentNullException(nameof (variable));

    protected override void InsertItem(int index, Attribute item)
    {
      object newValue = this.VerifyNewAttribute(item);
      base.InsertItem(index, item);
      this.variable.SetValueRaw(newValue, true);
    }

    protected override void SetItem(int index, Attribute item)
    {
      object newValue = this.VerifyNewAttribute(item);
      base.SetItem(index, item);
      this.variable.SetValueRaw(newValue, true);
    }

    internal void AddAttributeNoCheck(Attribute item) => base.InsertItem(this.Count, item);

    private object VerifyNewAttribute(Attribute item)
    {
      object inputData = this.variable.Value;
      if (item is ArgumentTransformationAttribute transformationAttribute)
      {
        ExecutionContext executionContextFromTls = LocalPipeline.GetExecutionContextFromTLS();
        EngineIntrinsics engineIntrinsics = (EngineIntrinsics) null;
        if (executionContextFromTls != null)
          engineIntrinsics = executionContextFromTls.EngineIntrinsics;
        inputData = transformationAttribute.Transform(engineIntrinsics, inputData);
      }
      if (!PSVariable.IsValidValue(inputData, item))
      {
        ValidationMetadataException metadataException = new ValidationMetadataException("ValidateSetFailure", (Exception) null, "Metadata", "InvalidMetadataForCurrentValue", new object[2]
        {
          (object) this.variable.Name,
          this.variable.Value != null ? (object) this.variable.Value.ToString() : (object) ""
        });
        PSVariableAttributeCollection.tracer.TraceException((Exception) metadataException);
        throw metadataException;
      }
      return inputData;
    }
  }
}
