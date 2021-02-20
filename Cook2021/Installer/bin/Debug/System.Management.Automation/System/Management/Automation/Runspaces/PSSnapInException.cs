// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.PSSnapInException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation.Runspaces
{
  [Serializable]
  public class PSSnapInException : RuntimeException
  {
    private bool _warning;
    private ErrorRecord _errorRecord;
    private string _PSSnapin = "";
    private string _reason = "";
    [TraceSource("PSSnapInException", "PSSnapInException")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (PSSnapInException), nameof (PSSnapInException));

    internal PSSnapInException(string PSSnapin, string message)
    {
      using (PSSnapInException.tracer.TraceConstructor((object) this))
      {
        this._PSSnapin = PSSnapin;
        this._reason = message;
        this.CreateErrorRecord();
      }
    }

    internal PSSnapInException(string PSSnapin, string message, bool warning)
    {
      using (PSSnapInException.tracer.TraceConstructor((object) this))
      {
        this._PSSnapin = PSSnapin;
        this._reason = message;
        this._warning = warning;
        this.CreateErrorRecord();
      }
    }

    internal PSSnapInException(string PSSnapin, string message, Exception exception)
      : base(message, exception)
    {
      using (PSSnapInException.tracer.TraceConstructor((object) this))
      {
        this._PSSnapin = PSSnapin;
        this._reason = message;
        this.CreateErrorRecord();
      }
    }

    public PSSnapInException()
    {
      using (PSSnapInException.tracer.TraceConstructor((object) this))
        ;
    }

    public PSSnapInException(string message)
      : base(message)
    {
      using (PSSnapInException.tracer.TraceConstructor((object) this))
        ;
    }

    public PSSnapInException(string message, Exception innerException)
      : base(message, innerException)
    {
      using (PSSnapInException.tracer.TraceConstructor((object) this))
        ;
    }

    private void CreateErrorRecord()
    {
      using (PSSnapInException.tracer.TraceMethod())
      {
        if (string.IsNullOrEmpty(this._PSSnapin) || string.IsNullOrEmpty(this._reason))
          return;
        if (this._warning)
        {
          this._errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), "PSSnapInLoadWarning", ErrorCategory.ResourceUnavailable, (object) null);
          this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "ConsoleInfoErrorStrings", "PSSnapInLoadWarning", new object[2]
          {
            (object) this._PSSnapin,
            (object) this._reason
          });
        }
        else
        {
          this._errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), "PSSnapInLoadFailure", ErrorCategory.ResourceUnavailable, (object) null);
          this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "ConsoleInfoErrorStrings", "PSSnapInLoadFailure", new object[2]
          {
            (object) this._PSSnapin,
            (object) this._reason
          });
        }
      }
    }

    public override ErrorRecord ErrorRecord
    {
      get
      {
        using (PSSnapInException.tracer.TraceProperty())
          return this._errorRecord;
      }
    }

    public override string Message
    {
      get
      {
        using (PSSnapInException.tracer.TraceProperty())
          return this._errorRecord != null ? this._errorRecord.ToString() : base.Message;
      }
    }

    protected PSSnapInException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      using (PSSnapInException.tracer.TraceConstructor((object) this))
      {
        this._PSSnapin = info.GetString("PSSnapIn");
        this._reason = info.GetString("Reason");
        this.CreateErrorRecord();
      }
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      using (PSSnapInException.tracer.TraceMethod())
      {
        base.GetObjectData(info, context);
        info.AddValue("PSSnapIn", (object) this._PSSnapin);
        info.AddValue("Reason", (object) this._reason);
      }
    }
  }
}
