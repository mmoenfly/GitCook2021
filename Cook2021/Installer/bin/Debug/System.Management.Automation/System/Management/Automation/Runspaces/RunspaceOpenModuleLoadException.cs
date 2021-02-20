// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.RunspaceOpenModuleLoadException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;

namespace System.Management.Automation.Runspaces
{
  [Serializable]
  public class RunspaceOpenModuleLoadException : RuntimeException
  {
    private PSDataCollection<ErrorRecord> _errors;

    public RunspaceOpenModuleLoadException()
      : base(typeof (ScriptBlockToPowerShellNotSupportedException).FullName)
    {
    }

    public RunspaceOpenModuleLoadException(string message)
      : base(message)
    {
    }

    public RunspaceOpenModuleLoadException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    internal RunspaceOpenModuleLoadException(
      string moduleName,
      PSDataCollection<ErrorRecord> errors)
      : base(ResourceManagerCache.FormatResourceString("runspace", "ErrorLoadingModulesOnRunspaceOpen", (object) moduleName, errors == null || errors.Count <= 0 || errors[0] == null ? (object) string.Empty : (object) errors[0].ToString()), (Exception) null)
    {
      this._errors = errors;
      this.SetErrorId("ErrorLoadingModulesOnRunspaceOpen");
      this.SetErrorCategory(ErrorCategory.OpenError);
    }

    public PSDataCollection<ErrorRecord> ErrorRecords => this._errors;

    protected RunspaceOpenModuleLoadException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new PSArgumentNullException(nameof (info));
      base.GetObjectData(info, context);
    }
  }
}
