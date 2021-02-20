// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ParameterBindingException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Resources;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
  [Serializable]
  public class ParameterBindingException : RuntimeException
  {
    private string message;
    private string parameterName = string.Empty;
    private Type parameterType;
    private Type typeSpecified;
    private long line = long.MinValue;
    private long offset = long.MinValue;
    private InvocationInfo invocationInfo;
    private string resourceBaseName;
    private string resourceId;
    private object[] args = new object[0];
    private string commandName;
    [TraceSource("ParameterBindingException", "Exception thrown when a parameter binding error occurs")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ParameterBindingException), "Exception thrown when a parameter binding error occurs");

    internal ParameterBindingException(
      ErrorCategory errorCategory,
      InvocationInfo invocationInfo,
      Token token,
      string parameterName,
      Type parameterType,
      Type typeSpecified,
      string resourceBaseName,
      string errorIdAndResourceId,
      params object[] args)
    {
      if (invocationInfo == null)
        throw ParameterBindingException.tracer.NewArgumentNullException(nameof (invocationInfo));
      if (string.IsNullOrEmpty(resourceBaseName))
        throw ParameterBindingException.tracer.NewArgumentException(nameof (resourceBaseName));
      if (string.IsNullOrEmpty(errorIdAndResourceId))
        throw ParameterBindingException.tracer.NewArgumentException(nameof (errorIdAndResourceId));
      if (token == null)
        token = invocationInfo.ScriptToken;
      this.invocationInfo = invocationInfo;
      this.commandName = invocationInfo.MyCommand.Name;
      this.parameterName = parameterName;
      this.parameterType = parameterType;
      this.typeSpecified = typeSpecified;
      if (token != null)
      {
        this.line = (long) token.LineNumber;
        this.offset = (long) token.OffsetInLine;
      }
      this.resourceBaseName = resourceBaseName;
      this.resourceId = errorIdAndResourceId;
      if (args != null)
        this.args = args;
      this.SetErrorCategory(errorCategory);
      this.SetErrorId(errorIdAndResourceId);
      this.ErrorRecord.SetInvocationInfo(new InvocationInfo(invocationInfo.MyCommand, invocationInfo.ScriptToken, token));
    }

    internal ParameterBindingException(
      Exception innerException,
      ErrorCategory errorCategory,
      InvocationInfo invocationInfo,
      Token token,
      string parameterName,
      Type parameterType,
      Type typeSpecified,
      string resourceBaseName,
      string errorIdAndResourceId,
      params object[] args)
      : base(string.Empty, innerException)
    {
      if (invocationInfo == null)
        throw ParameterBindingException.tracer.NewArgumentNullException(nameof (invocationInfo));
      if (string.IsNullOrEmpty(resourceBaseName))
        throw ParameterBindingException.tracer.NewArgumentException(nameof (resourceBaseName));
      if (string.IsNullOrEmpty(errorIdAndResourceId))
        throw ParameterBindingException.tracer.NewArgumentException(nameof (errorIdAndResourceId));
      if (token == null)
        token = invocationInfo.ScriptToken;
      this.invocationInfo = invocationInfo;
      this.commandName = invocationInfo.MyCommand.Name;
      this.parameterName = parameterName;
      this.parameterType = parameterType;
      this.typeSpecified = typeSpecified;
      if (token != null)
      {
        this.line = (long) token.LineNumber;
        this.offset = (long) token.OffsetInLine;
      }
      this.resourceBaseName = resourceBaseName;
      this.resourceId = errorIdAndResourceId;
      if (args != null)
        this.args = args;
      this.SetErrorCategory(errorCategory);
      this.SetErrorId(errorIdAndResourceId);
      this.ErrorRecord.SetInvocationInfo(new InvocationInfo(invocationInfo.MyCommand, invocationInfo.ScriptToken, token));
    }

    protected ParameterBindingException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this.parameterName = info.GetString(nameof (ParameterName));
      this.line = info.GetInt64(nameof (Line));
      this.offset = info.GetInt64(nameof (Offset));
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new PSArgumentNullException(nameof (info));
      base.GetObjectData(info, context);
      info.AddValue("ParameterName", (object) this.parameterName);
      info.AddValue("Line", this.line);
      info.AddValue("Offset", this.offset);
    }

    public ParameterBindingException()
    {
    }

    public ParameterBindingException(string message)
      : base(message)
      => this.message = message;

    public ParameterBindingException(string message, Exception innerException)
      : base(message, innerException)
      => this.message = message;

    public override string Message
    {
      get
      {
        if (this.message == null)
          this.message = this.BuildMessage();
        return this.message;
      }
    }

    public string ParameterName => this.parameterName;

    public Type ParameterType => this.parameterType;

    public Type TypeSpecified => this.typeSpecified;

    public long Line => this.line;

    public long Offset => this.offset;

    public InvocationInfo CommandInvocation => this.invocationInfo;

    private string BuildMessage()
    {
      try
      {
        object[] objArray = new object[0];
        if (this.args != null)
        {
          objArray = new object[this.args.Length + 6];
          objArray[0] = (object) this.commandName;
          objArray[1] = (object) this.parameterName;
          objArray[2] = (object) this.parameterType;
          objArray[3] = (object) this.typeSpecified;
          objArray[4] = (object) this.line;
          objArray[5] = (object) this.offset;
          this.args.CopyTo((Array) objArray, 6);
        }
        string str = string.Empty;
        if (!string.IsNullOrEmpty(this.resourceBaseName) && !string.IsNullOrEmpty(this.resourceId))
          str = ResourceManagerCache.FormatResourceString(this.resourceBaseName, this.resourceId, objArray);
        return str;
      }
      catch (MissingManifestResourceException ex)
      {
        ParameterBindingException.tracer.TraceException((Exception) ex);
        return ResourceManagerCache.FormatResourceString("ParameterBinderStrings", "ResourceStringLoadError", this.args[0], (object) this.resourceBaseName, (object) this.resourceId, (object) ex.Message);
      }
      catch (FormatException ex)
      {
        ParameterBindingException.tracer.TraceException((Exception) ex);
        return ResourceManagerCache.FormatResourceString("ParameterBinderStrings", "ResourceStringFormatError", this.args[0], (object) this.resourceBaseName, (object) this.resourceId, (object) ex.Message);
      }
    }
  }
}
