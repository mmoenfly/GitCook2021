// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.HelpCategoryInvalidException
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
  public class HelpCategoryInvalidException : ArgumentException, IContainsErrorRecord
  {
    private ErrorRecord _errorRecord;
    private string _helpCategory = System.Management.Automation.HelpCategory.None.ToString();
    [TraceSource("HelpExceptions", "HelpExceptions")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("HelpExceptions", "HelpExceptions");

    public HelpCategoryInvalidException(string helpCategory)
    {
      using (HelpCategoryInvalidException.tracer.TraceConstructor((object) this))
      {
        this._helpCategory = helpCategory;
        this.CreateErrorRecord();
      }
    }

    public HelpCategoryInvalidException()
    {
      using (HelpCategoryInvalidException.tracer.TraceConstructor((object) this))
        this.CreateErrorRecord();
    }

    public HelpCategoryInvalidException(string helpCategory, Exception innerException)
      : base(innerException != null ? innerException.Message : string.Empty, innerException)
    {
      using (HelpCategoryInvalidException.tracer.TraceConstructor((object) this))
      {
        this._helpCategory = helpCategory;
        this.CreateErrorRecord();
      }
    }

    private void CreateErrorRecord()
    {
      using (HelpCategoryInvalidException.tracer.TraceMethod())
      {
        this._errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), "HelpCategoryInvalid", ErrorCategory.InvalidArgument, (object) null);
        this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "HelpCategoryInvalid", new object[1]
        {
          (object) this._helpCategory
        });
      }
    }

    public ErrorRecord ErrorRecord
    {
      get
      {
        using (HelpCategoryInvalidException.tracer.TraceProperty())
          return this._errorRecord;
      }
    }

    public string HelpCategory
    {
      get
      {
        using (HelpCategoryInvalidException.tracer.TraceProperty())
          return this._helpCategory;
      }
    }

    public override string Message
    {
      get
      {
        using (HelpCategoryInvalidException.tracer.TraceProperty())
          return this._errorRecord != null ? this._errorRecord.ToString() : base.Message;
      }
    }

    protected HelpCategoryInvalidException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      using (HelpCategoryInvalidException.tracer.TraceConstructor((object) this))
      {
        this._helpCategory = info.GetString(nameof (HelpCategory));
        this.CreateErrorRecord();
      }
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      using (HelpCategoryInvalidException.tracer.TraceMethod())
      {
        base.GetObjectData(info, context);
        info.AddValue("HelpCategory", (object) this._helpCategory);
      }
    }
  }
}
