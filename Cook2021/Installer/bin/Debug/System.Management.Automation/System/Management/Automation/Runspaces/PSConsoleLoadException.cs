// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.PSConsoleLoadException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace System.Management.Automation.Runspaces
{
  [Serializable]
  public class PSConsoleLoadException : SystemException, IContainsErrorRecord
  {
    private ErrorRecord _errorRecord;
    private string _consoleFileName = "";
    private Collection<PSSnapInException> _PSSnapInExceptions = new Collection<PSSnapInException>();
    [TraceSource("PSConsoleLoadException ", "PSConsoleLoadException")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (PSConsoleLoadException), nameof (PSConsoleLoadException));

    internal PSConsoleLoadException(
      MshConsoleInfo consoleInfo,
      Collection<PSSnapInException> exceptions)
    {
      using (PSConsoleLoadException.tracer.TraceConstructor((object) this))
      {
        if (!string.IsNullOrEmpty(consoleInfo.Filename))
          this._consoleFileName = consoleInfo.Filename;
        if (exceptions != null)
          this._PSSnapInExceptions = exceptions;
        this.CreateErrorRecord();
      }
    }

    public PSConsoleLoadException()
    {
      using (PSConsoleLoadException.tracer.TraceConstructor((object) this))
        ;
    }

    public PSConsoleLoadException(string message)
      : base(message)
    {
      using (PSConsoleLoadException.tracer.TraceConstructor((object) this))
        ;
    }

    public PSConsoleLoadException(string message, Exception innerException)
      : base(message, innerException)
    {
      using (PSConsoleLoadException.tracer.TraceConstructor((object) this))
        ;
    }

    private void CreateErrorRecord()
    {
      using (PSConsoleLoadException.tracer.TraceMethod())
      {
        StringBuilder stringBuilder = new StringBuilder();
        if (this.PSSnapInExceptions != null)
        {
          foreach (PSSnapInException psSnapInException in this.PSSnapInExceptions)
          {
            stringBuilder.Append("\n");
            stringBuilder.Append(psSnapInException.Message);
          }
        }
        this._errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), "ConsoleLoadFailure", ErrorCategory.ResourceUnavailable, (object) null);
        this._errorRecord.ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "ConsoleInfoErrorStrings", "ConsoleLoadFailure", new object[2]
        {
          (object) this._consoleFileName,
          (object) stringBuilder.ToString()
        });
      }
    }

    public ErrorRecord ErrorRecord
    {
      get
      {
        using (PSConsoleLoadException.tracer.TraceProperty())
          return this._errorRecord;
      }
    }

    internal Collection<PSSnapInException> PSSnapInExceptions => this._PSSnapInExceptions;

    public override string Message
    {
      get
      {
        using (PSConsoleLoadException.tracer.TraceProperty())
          return this._errorRecord != null ? this._errorRecord.ToString() : base.Message;
      }
    }

    protected PSConsoleLoadException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      using (PSConsoleLoadException.tracer.TraceConstructor((object) this))
      {
        this._consoleFileName = info.GetString("ConsoleFileName");
        this.CreateErrorRecord();
      }
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      using (PSConsoleLoadException.tracer.TraceMethod())
      {
        base.GetObjectData(info, context);
        info.AddValue("ConsoleFileName", (object) this._consoleFileName);
      }
    }
  }
}
