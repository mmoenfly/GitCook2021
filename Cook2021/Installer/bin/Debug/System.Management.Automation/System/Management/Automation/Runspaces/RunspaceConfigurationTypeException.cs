// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.RunspaceConfigurationTypeException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation.Runspaces
{
  [Serializable]
  public class RunspaceConfigurationTypeException : SystemException, IContainsErrorRecord
  {
    private ErrorRecord _errorRecord;
    private string _assemblyName = "";
    private string _typeName = "";
    [TraceSource("RunspaceConfigurationTypeException", "RunspaceConfigurationTypeException")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (RunspaceConfigurationTypeException), nameof (RunspaceConfigurationTypeException));

    internal RunspaceConfigurationTypeException(string assemblyName, string typeName)
    {
      using (RunspaceConfigurationTypeException.tracer.TraceConstructor((object) this))
      {
        this._assemblyName = assemblyName;
        this._typeName = typeName;
        this.CreateErrorRecord();
      }
    }

    public RunspaceConfigurationTypeException()
    {
      using (RunspaceConfigurationTypeException.tracer.TraceConstructor((object) this))
        ;
    }

    internal RunspaceConfigurationTypeException(
      string assemblyName,
      string typeName,
      Exception innerException)
      : base(innerException.Message, innerException)
    {
      using (RunspaceConfigurationTypeException.tracer.TraceConstructor((object) this))
      {
        this._assemblyName = assemblyName;
        this._typeName = typeName;
        this.CreateErrorRecord();
      }
    }

    public RunspaceConfigurationTypeException(string message)
      : base(message)
    {
      using (RunspaceConfigurationTypeException.tracer.TraceConstructor((object) this))
        ;
    }

    public RunspaceConfigurationTypeException(string message, Exception innerException)
      : base(message, innerException)
    {
      using (RunspaceConfigurationTypeException.tracer.TraceConstructor((object) this))
        ;
    }

    private void CreateErrorRecord()
    {
      using (RunspaceConfigurationTypeException.tracer.TraceMethod())
      {
        if (string.IsNullOrEmpty(this._assemblyName) || string.IsNullOrEmpty(this._typeName))
          return;
        this._errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), "UndefinedRunspaceConfigurationType", ErrorCategory.ResourceUnavailable, (object) null);
        this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "MiniShellErrors", "UndefinedRunspaceConfigurationType", new object[2]
        {
          (object) this._assemblyName,
          (object) this._typeName
        });
      }
    }

    public ErrorRecord ErrorRecord
    {
      get
      {
        using (RunspaceConfigurationTypeException.tracer.TraceProperty())
          return this._errorRecord;
      }
    }

    public string AssemblyName
    {
      get
      {
        using (RunspaceConfigurationTypeException.tracer.TraceProperty())
          return this._assemblyName;
      }
    }

    public string TypeName
    {
      get
      {
        using (RunspaceConfigurationTypeException.tracer.TraceProperty())
          return this._typeName;
      }
    }

    public override string Message
    {
      get
      {
        using (RunspaceConfigurationTypeException.tracer.TraceProperty())
          return this._errorRecord != null ? this._errorRecord.ToString() : base.Message;
      }
    }

    protected RunspaceConfigurationTypeException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      using (RunspaceConfigurationTypeException.tracer.TraceConstructor((object) this))
      {
        this._typeName = info.GetString(nameof (TypeName));
        this._assemblyName = info.GetString(nameof (AssemblyName));
      }
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      using (RunspaceConfigurationTypeException.tracer.TraceMethod())
      {
        base.GetObjectData(info, context);
        info.AddValue("TypeName", (object) this._typeName);
        info.AddValue("AssemblyName", (object) this._assemblyName);
      }
    }
  }
}
