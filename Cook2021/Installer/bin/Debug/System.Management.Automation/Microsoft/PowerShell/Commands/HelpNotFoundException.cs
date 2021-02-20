// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.HelpNotFoundException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.PowerShell.Commands
{
  [Serializable]
  public class HelpNotFoundException : SystemException, IContainsErrorRecord
  {
    private ErrorRecord _errorRecord;
    private string _helpTopic = "";
    [TraceSource("HelpExceptions", "HelpExceptions")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("HelpExceptions", "HelpExceptions");

    public HelpNotFoundException(string helpTopic)
    {
      using (HelpNotFoundException.tracer.TraceConstructor((object) this))
      {
        this._helpTopic = helpTopic;
        this.CreateErrorRecord();
      }
    }

    public HelpNotFoundException()
    {
      using (HelpNotFoundException.tracer.TraceConstructor((object) this))
        this.CreateErrorRecord();
    }

    public HelpNotFoundException(string helpTopic, Exception innerException)
      : base(innerException != null ? innerException.Message : string.Empty, innerException)
    {
      using (HelpNotFoundException.tracer.TraceConstructor((object) this))
      {
        this._helpTopic = helpTopic;
        this.CreateErrorRecord();
      }
    }

    private void CreateErrorRecord()
    {
      using (HelpNotFoundException.tracer.TraceMethod())
      {
        this._errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), "HelpNotFound", ErrorCategory.ResourceUnavailable, (object) null);
        this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "HelpNotFound", new object[1]
        {
          (object) this._helpTopic
        });
      }
    }

    public ErrorRecord ErrorRecord
    {
      get
      {
        using (HelpNotFoundException.tracer.TraceProperty())
          return this._errorRecord;
      }
    }

    public string HelpTopic
    {
      get
      {
        using (HelpNotFoundException.tracer.TraceProperty())
          return this._helpTopic;
      }
    }

    public override string Message
    {
      get
      {
        using (HelpNotFoundException.tracer.TraceProperty())
          return this._errorRecord != null ? this._errorRecord.ToString() : base.Message;
      }
    }

    protected HelpNotFoundException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      using (HelpNotFoundException.tracer.TraceConstructor((object) this))
      {
        this._helpTopic = info.GetString(nameof (HelpTopic));
        this.CreateErrorRecord();
      }
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      using (HelpNotFoundException.tracer.TraceMethod())
      {
        base.GetObjectData(info, context);
        info.AddValue("HelpTopic", (object) this._helpTopic);
      }
    }
  }
}
