// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandNotFoundException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Resources;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management.Automation
{
  [Serializable]
  public class CommandNotFoundException : RuntimeException
  {
    private ErrorRecord _errorRecord;
    private string commandName = string.Empty;
    private string _errorId = nameof (CommandNotFoundException);
    private ErrorCategory _errorCategory = ErrorCategory.ObjectNotFound;
    [TraceSource("CommandNotFoundException", "Exception thrown when a command could not be found.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (CommandNotFoundException), "Exception thrown when a command could not be found.");

    internal CommandNotFoundException(
      string commandName,
      Exception innerException,
      string errorIdAndResourceId,
      params object[] messageArgs)
      : base(CommandNotFoundException.BuildMessage(commandName, errorIdAndResourceId, messageArgs), innerException)
    {
      this.commandName = commandName;
      this._errorId = errorIdAndResourceId;
    }

    public CommandNotFoundException()
    {
    }

    public CommandNotFoundException(string message)
      : base(message)
    {
    }

    public CommandNotFoundException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected CommandNotFoundException(SerializationInfo info, StreamingContext context)
      : base(info, context)
      => this.commandName = info != null ? info.GetString(nameof (CommandName)) : throw new PSArgumentNullException(nameof (info));

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new PSArgumentNullException(nameof (info));
      base.GetObjectData(info, context);
      info.AddValue("CommandName", (object) this.commandName);
    }

    public override ErrorRecord ErrorRecord
    {
      get
      {
        if (this._errorRecord == null)
          this._errorRecord = new ErrorRecord((Exception) new ParentContainsErrorRecordException((Exception) this), this._errorId, this._errorCategory, (object) this.commandName);
        return this._errorRecord;
      }
    }

    public string CommandName
    {
      get => this.commandName;
      set => this.commandName = value;
    }

    private static string BuildMessage(
      string commandName,
      string resourceId,
      params object[] messageArgs)
    {
      try
      {
        object[] objArray;
        if (messageArgs != null && 0 < messageArgs.Length)
        {
          objArray = new object[messageArgs.Length + 1];
          objArray[0] = (object) commandName;
          messageArgs.CopyTo((Array) objArray, 1);
        }
        else
          objArray = new object[1]{ (object) commandName };
        return ResourceManagerCache.FormatResourceString("DiscoveryExceptions", resourceId, objArray);
      }
      catch (MissingManifestResourceException ex)
      {
        CommandNotFoundException.tracer.TraceException((Exception) ex);
        return ResourceManagerCache.FormatResourceString("SessionStateStrings", "ResourceStringLoadError", (object) commandName, (object) "DiscoveryExceptions", (object) resourceId, (object) ex.Message);
      }
      catch (FormatException ex)
      {
        CommandNotFoundException.tracer.TraceException((Exception) ex);
        return ResourceManagerCache.FormatResourceString("SessionStateStrings", "ResourceStringFormatError", (object) commandName, (object) "DiscoveryExceptions", (object) resourceId, (object) ex.Message);
      }
    }
  }
}
