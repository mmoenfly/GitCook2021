// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.RunspaceConfigurationAttributeException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation.Runspaces
{
  [Serializable]
  public class RunspaceConfigurationAttributeException : SystemException, IContainsErrorRecord
  {
    private ErrorRecord _errorRecord;
    private string _error = "";
    private string _assemblyName = "";
    [TraceSource("RunspaceConfigurationAttributeException ", "RunspaceConfigurationAttributeException")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (RunspaceConfigurationAttributeException), nameof (RunspaceConfigurationAttributeException));

    internal RunspaceConfigurationAttributeException(string error, string assemblyName)
    {
      using (RunspaceConfigurationAttributeException.tracer.TraceConstructor((object) this))
      {
        this._error = error;
        this._assemblyName = assemblyName;
        this.CreateErrorRecord();
      }
    }

    public RunspaceConfigurationAttributeException()
    {
      using (RunspaceConfigurationAttributeException.tracer.TraceConstructor((object) this))
        ;
    }

    public RunspaceConfigurationAttributeException(string message)
      : base(message)
    {
      using (RunspaceConfigurationAttributeException.tracer.TraceConstructor((object) this))
        ;
    }

    public RunspaceConfigurationAttributeException(string message, Exception innerException)
      : base(message, innerException)
    {
      using (RunspaceConfigurationAttributeException.tracer.TraceConstructor((object) this))
        ;
    }

    internal RunspaceConfigurationAttributeException(
      string error,
      string assemblyName,
      Exception innerException)
      : base(innerException.Message, innerException)
    {
      using (RunspaceConfigurationAttributeException.tracer.TraceConstructor((object) this))
      {
        this._error = error;
        this._assemblyName = assemblyName;
        this.CreateErrorRecord();
      }
    }

    private void CreateErrorRecord()
    {
      using (RunspaceConfigurationAttributeException.tracer.TraceMethod())
      {
        if (string.IsNullOrEmpty(this._error) || string.IsNullOrEmpty(this._assemblyName))
          return;
        this._errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), this._error, ErrorCategory.ResourceUnavailable, (object) null);
        this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "MiniShellErrors", this._error, new object[1]
        {
          (object) this._assemblyName
        });
      }
    }

    public ErrorRecord ErrorRecord
    {
      get
      {
        using (RunspaceConfigurationAttributeException.tracer.TraceProperty())
          return this._errorRecord;
      }
    }

    public string Error
    {
      get
      {
        using (RunspaceConfigurationAttributeException.tracer.TraceProperty())
          return this._error;
      }
    }

    public string AssemblyName
    {
      get
      {
        using (RunspaceConfigurationAttributeException.tracer.TraceProperty())
          return this._assemblyName;
      }
    }

    public override string Message
    {
      get
      {
        using (RunspaceConfigurationAttributeException.tracer.TraceProperty())
          return this._errorRecord != null ? this._errorRecord.ToString() : base.Message;
      }
    }

    protected RunspaceConfigurationAttributeException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    {
      using (RunspaceConfigurationAttributeException.tracer.TraceConstructor((object) this))
      {
        this._error = info.GetString(nameof (Error));
        this._assemblyName = info.GetString(nameof (AssemblyName));
        this.CreateErrorRecord();
      }
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      using (RunspaceConfigurationAttributeException.tracer.TraceMethod())
      {
        base.GetObjectData(info, context);
        info.AddValue("Error", (object) this._error);
        info.AddValue("AssemblyName", (object) this._assemblyName);
      }
    }
  }
}
