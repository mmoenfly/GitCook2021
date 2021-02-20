// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ValidateScriptAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Management.Automation.Internal;

namespace System.Management.Automation
{
  public sealed class ValidateScriptAttribute : ValidateEnumeratedArgumentsAttribute
  {
    private ScriptBlock _scriptBlock;

    public ScriptBlock ScriptBlock => this._scriptBlock;

    protected override void ValidateElement(object element)
    {
      if (element == null)
        throw new ValidationMetadataException("ArgumentIsEmpty", (Exception) null, "Metadata", "ValidateNotNullFailure", new object[0]);
      if (!LanguagePrimitives.IsTrue(this._scriptBlock.DoInvokeReturnAsIs((object) LanguagePrimitives.AsPSObjectOrNull(element), (object) AutomationNull.Value)))
        throw new ValidationMetadataException("ValidateScriptFailure", (Exception) null, "Metadata", "ValidateScriptFailure", new object[2]
        {
          element,
          (object) this._scriptBlock
        });
    }

    public ValidateScriptAttribute(ScriptBlock scriptBlock) => this._scriptBlock = scriptBlock != null ? scriptBlock : throw CmdletMetadataAttribute.tracer.NewArgumentException(nameof (scriptBlock));
  }
}
