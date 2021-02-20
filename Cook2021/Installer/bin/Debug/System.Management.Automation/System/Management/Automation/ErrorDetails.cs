// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ErrorDetails
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace System.Management.Automation
{
  [Serializable]
  public class ErrorDetails : ISerializable
  {
    [TraceSource("ErrorDetails", "ErrorDetails")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ErrorDetails), nameof (ErrorDetails));
    private string _message = "";
    private string _recommendedAction = "";
    private Exception _textLookupError;

    public ErrorDetails(string message)
    {
      using (ErrorDetails.tracer.TraceConstructor((object) this, "message = {0}", (object) message))
        this._message = message;
    }

    public ErrorDetails(Cmdlet cmdlet, string baseName, string resourceId, params object[] args)
    {
      using (ErrorDetails.tracer.TraceConstructor((object) this, "cmdlet = {0} baseName = {1} resourceId = {2} args = {3}", (object) cmdlet, (object) baseName, (object) resourceId, (object) args))
        this._message = this.BuildMessage(cmdlet, baseName, resourceId, args);
    }

    public ErrorDetails(
      IResourceSupplier resourceSupplier,
      string baseName,
      string resourceId,
      params object[] args)
    {
      using (ErrorDetails.tracer.TraceConstructor((object) this, "resourceSupplier = {0} baseName = {1} resourceId = {2} args = {3}", (object) resourceSupplier, (object) baseName, (object) resourceId, (object) args))
        this._message = this.BuildMessage(resourceSupplier, baseName, resourceId, args);
    }

    public ErrorDetails(
      Assembly assembly,
      string baseName,
      string resourceId,
      params object[] args)
    {
      using (ErrorDetails.tracer.TraceConstructor((object) this, "assembly = {0} baseName = {1} resourceId = {2} args = {3}", (object) assembly, (object) baseName, (object) resourceId, (object) args))
        this._message = this.BuildMessage(assembly, baseName, resourceId, args);
    }

    internal ErrorDetails(ErrorDetails errorDetails)
    {
      this._message = errorDetails._message;
      this._recommendedAction = errorDetails._recommendedAction;
    }

    protected ErrorDetails(SerializationInfo info, StreamingContext context)
    {
      this._message = info.GetString("ErrorDetails_Message");
      this._recommendedAction = info.GetString("ErrorDetails_RecommendedAction");
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        return;
      info.AddValue("ErrorDetails_Message", (object) this._message);
      info.AddValue("ErrorDetails_RecommendedAction", (object) this._recommendedAction);
    }

    public string Message => ErrorRecord.NotNull(this._message);

    public string RecommendedAction
    {
      get => ErrorRecord.NotNull(this._recommendedAction);
      set => this._recommendedAction = value;
    }

    internal Exception TextLookupError
    {
      get => this._textLookupError;
      set => this._textLookupError = value;
    }

    public override string ToString() => this.Message;

    private string BuildMessage(
      Cmdlet cmdlet,
      string baseName,
      string resourceId,
      params object[] args)
    {
      if (cmdlet == null)
        throw ErrorDetails.tracer.NewArgumentNullException(nameof (cmdlet));
      if (string.IsNullOrEmpty(baseName))
        throw ErrorDetails.tracer.NewArgumentNullException(nameof (baseName));
      if (string.IsNullOrEmpty(resourceId))
        throw ErrorDetails.tracer.NewArgumentNullException(nameof (resourceId));
      string resourceString;
      try
      {
        resourceString = cmdlet.GetResourceString(baseName, resourceId);
      }
      catch (MissingManifestResourceException ex)
      {
        this._textLookupError = (Exception) ex;
        return "";
      }
      catch (ArgumentException ex)
      {
        this._textLookupError = (Exception) ex;
        return "";
      }
      return this.BuildMessage(resourceString, baseName, resourceId, args);
    }

    private string BuildMessage(
      IResourceSupplier resourceSupplier,
      string baseName,
      string resourceId,
      params object[] args)
    {
      if (resourceSupplier == null)
        throw ErrorDetails.tracer.NewArgumentNullException(nameof (resourceSupplier));
      if (string.IsNullOrEmpty(baseName))
        throw ErrorDetails.tracer.NewArgumentNullException(nameof (baseName));
      if (string.IsNullOrEmpty(resourceId))
        throw ErrorDetails.tracer.NewArgumentNullException(nameof (resourceId));
      string resourceString;
      try
      {
        resourceString = resourceSupplier.GetResourceString(baseName, resourceId);
      }
      catch (MissingManifestResourceException ex)
      {
        this._textLookupError = (Exception) ex;
        return "";
      }
      catch (ArgumentException ex)
      {
        this._textLookupError = (Exception) ex;
        return "";
      }
      return this.BuildMessage(resourceString, baseName, resourceId, args);
    }

    private string BuildMessage(
      Assembly assembly,
      string baseName,
      string resourceId,
      params object[] args)
    {
      if (assembly == null)
        throw ErrorDetails.tracer.NewArgumentNullException(nameof (assembly));
      if (string.IsNullOrEmpty(baseName))
        throw ErrorDetails.tracer.NewArgumentNullException(nameof (baseName));
      if (string.IsNullOrEmpty(resourceId))
        throw ErrorDetails.tracer.NewArgumentNullException(nameof (resourceId));
      ResourceManager resourceManager = ResourceManagerCache.GetResourceManager(assembly, baseName);
      string template;
      try
      {
        template = resourceManager.GetString(resourceId, Thread.CurrentThread.CurrentUICulture);
      }
      catch (MissingManifestResourceException ex)
      {
        this._textLookupError = (Exception) ex;
        return "";
      }
      return this.BuildMessage(template, baseName, resourceId, args);
    }

    private string BuildMessage(
      string template,
      string baseName,
      string resourceId,
      params object[] args)
    {
      if (!string.IsNullOrEmpty(template))
      {
        if (1 < template.Trim().Length)
        {
          try
          {
            return string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, template, args);
          }
          catch (FormatException ex)
          {
            this._textLookupError = (Exception) ex;
            return "";
          }
        }
      }
      this._textLookupError = (Exception) ErrorDetails.tracer.NewInvalidOperationException("ErrorPackage", "ErrorDetailsEmptyTemplate", (object) baseName, (object) resourceId);
      return "";
    }
  }
}
