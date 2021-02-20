// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PipelineDepthException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
  [Serializable]
  public class PipelineDepthException : SystemException, IContainsErrorRecord
  {
    private ErrorRecord errorRecord;
    private int _callDepth;

    internal PipelineDepthException(int callDepth, int maxCallDepth)
      : base(ResourceManagerCache.FormatResourceString("GetErrorText", nameof (PipelineDepthException), (object) callDepth, (object) maxCallDepth))
      => this._callDepth = callDepth;

    public PipelineDepthException()
    {
    }

    public PipelineDepthException(string message)
      : base(message)
    {
    }

    public PipelineDepthException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected PipelineDepthException(SerializationInfo info, StreamingContext context)
      : base(info, context)
      => this._callDepth = info.GetInt32(nameof (CallDepth));

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info?.AddValue("CallDepth", this._callDepth);
    }

    public ErrorRecord ErrorRecord
    {
      get
      {
        if (this.errorRecord == null)
          this.errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), "CallDepthOverflow", ErrorCategory.InvalidOperation, (object) this._callDepth);
        return this.errorRecord;
      }
    }

    public int CallDepth => this._callDepth;
  }
}
