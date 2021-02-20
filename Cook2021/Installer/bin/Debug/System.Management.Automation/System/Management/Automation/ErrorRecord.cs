// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ErrorRecord
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace System.Management.Automation
{
  [Serializable]
  public class ErrorRecord : ISerializable
  {
    [TraceSource("ErrorRecord", "ErrorRecord")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ErrorRecord), nameof (ErrorRecord));
    private bool _isSerialized;
    private string _serializedFullyQualifiedErrorId;
    internal string _serializedErrorCategoryMessageOverride;
    private Exception _error;
    private object _target;
    private ErrorCategoryInfo _categoryInfo;
    private ErrorDetails _errorDetails;
    private InvocationInfo _invocationInfo;
    private bool preserveInvocationInfoOnce;
    private ReadOnlyCollection<int> pipelineIterationInfo = new ReadOnlyCollection<int>((IList<int>) new int[0]);
    private bool serializeExtendedInfo;
    private string _errorId;
    internal ErrorCategory _category;
    internal string _activityOverride;
    internal string _reasonOverride;
    internal string _targetNameOverride;
    internal string _targetTypeOverride;

    public ErrorRecord(
      Exception exception,
      string errorId,
      ErrorCategory errorCategory,
      object targetObject)
    {
      using (ErrorRecord.tracer.TraceConstructor((object) this, "exception = {0} errorId = {1} errorCategory = {2} targetObject = {3}", (object) exception, (object) errorId, (object) errorCategory, targetObject))
      {
        if (exception == null)
          throw ErrorRecord.tracer.NewArgumentNullException(nameof (exception));
        if (errorId == null)
          errorId = "";
        this._error = exception;
        this._errorId = errorId;
        this._category = errorCategory;
        this._target = targetObject;
      }
    }

    protected ErrorRecord(SerializationInfo info, StreamingContext context)
    {
      this._category = (ErrorCategory) info.GetInt32("ErrorRecord_Category");
      this._activityOverride = info.GetString("ErrorRecord_ActivityOverride");
      this._reasonOverride = info.GetString("ErrorRecord_ReasonOverride");
      this._targetNameOverride = info.GetString("ErrorRecord_TargetNameOverride");
      this._targetTypeOverride = info.GetString("ErrorRecord_TargetTypeOverride");
      if (info.GetBoolean("ErrorRecord_HasErrorDetails"))
        this._errorDetails = (ErrorDetails) info.GetValue("ErrorRecord_ErrorDetails", typeof (ErrorDetails));
      try
      {
        this._error = (Exception) info.GetValue("ErrorRecord_Exception", typeof (Exception));
      }
      catch (Exception ex)
      {
        ErrorRecord.tracer.TraceException(ex, true);
        throw;
      }
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        return;
      info.AddValue("ErrorRecord_Category", (int) this._category);
      info.AddValue("ErrorRecord_ActivityOverride", (object) this._activityOverride);
      info.AddValue("ErrorRecord_ReasonOverride", (object) this._reasonOverride);
      info.AddValue("ErrorRecord_TargetNameOverride", (object) this._targetNameOverride);
      info.AddValue("ErrorRecord_TargetTypeOverride", (object) this._targetTypeOverride);
      bool flag = null != this._errorDetails;
      info.AddValue("ErrorRecord_HasErrorDetails", flag);
      if (flag)
        info.AddValue("ErrorRecord_ErrorDetails", (object) this._errorDetails);
      try
      {
        info.AddValue("ErrorRecord_Exception", (object) this._error, typeof (Exception));
      }
      catch (Exception ex)
      {
        ErrorRecord.tracer.TraceException(ex, true);
        throw;
      }
    }

    internal bool IsSerialized => this._isSerialized;

    internal ErrorRecord(
      Exception exception,
      object targetObject,
      string fullyQualifiedErrorId,
      ErrorCategory errorCategory,
      string errorCategory_Activity,
      string errorCategory_Reason,
      string errorCategory_TargetName,
      string errorCategory_TargetType,
      string errorCategory_Message,
      string errorDetails_Message,
      string errorDetails_RecommendedAction)
    {
      using (ErrorRecord.tracer.TraceConstructor((object) this))
      {
        if (exception == null)
          throw ErrorRecord.tracer.NewArgumentNullException(nameof (exception));
        if (fullyQualifiedErrorId == null)
          throw ErrorRecord.tracer.NewArgumentNullException(nameof (fullyQualifiedErrorId));
        this._isSerialized = true;
        this._error = exception;
        this._target = targetObject;
        this._serializedFullyQualifiedErrorId = fullyQualifiedErrorId;
        this._category = errorCategory;
        this._activityOverride = errorCategory_Activity;
        this._reasonOverride = errorCategory_Reason;
        this._targetNameOverride = errorCategory_TargetName;
        this._targetTypeOverride = errorCategory_TargetType;
        this._serializedErrorCategoryMessageOverride = errorCategory_Message;
        if (errorDetails_Message == null)
          return;
        this._errorDetails = new ErrorDetails(errorDetails_Message);
        if (errorDetails_RecommendedAction == null)
          return;
        this._errorDetails.RecommendedAction = errorDetails_RecommendedAction;
      }
    }

    internal void ToPSObjectForRemoting(PSObject dest)
    {
      RemotingEncoder.AddNoteProperty<Exception>(dest, "Exception", (RemotingEncoder.ValueGetterDelegate<Exception>) (() => this.Exception));
      RemotingEncoder.AddNoteProperty<object>(dest, "TargetObject", (RemotingEncoder.ValueGetterDelegate<object>) (() => this.TargetObject));
      RemotingEncoder.AddNoteProperty<string>(dest, "FullyQualifiedErrorId", (RemotingEncoder.ValueGetterDelegate<string>) (() => this.FullyQualifiedErrorId));
      RemotingEncoder.AddNoteProperty<InvocationInfo>(dest, "InvocationInfo", (RemotingEncoder.ValueGetterDelegate<InvocationInfo>) (() => this.InvocationInfo));
      RemotingEncoder.AddNoteProperty<int>(dest, "ErrorCategory_Category", (RemotingEncoder.ValueGetterDelegate<int>) (() => (int) this.CategoryInfo.Category));
      RemotingEncoder.AddNoteProperty<string>(dest, "ErrorCategory_Activity", (RemotingEncoder.ValueGetterDelegate<string>) (() => this.CategoryInfo.Activity));
      RemotingEncoder.AddNoteProperty<string>(dest, "ErrorCategory_Reason", (RemotingEncoder.ValueGetterDelegate<string>) (() => this.CategoryInfo.Reason));
      RemotingEncoder.AddNoteProperty<string>(dest, "ErrorCategory_TargetName", (RemotingEncoder.ValueGetterDelegate<string>) (() => this.CategoryInfo.TargetName));
      RemotingEncoder.AddNoteProperty<string>(dest, "ErrorCategory_TargetType", (RemotingEncoder.ValueGetterDelegate<string>) (() => this.CategoryInfo.TargetType));
      RemotingEncoder.AddNoteProperty<string>(dest, "ErrorCategory_Message", (RemotingEncoder.ValueGetterDelegate<string>) (() => this.CategoryInfo.GetMessage(CultureInfo.CurrentCulture)));
      if (this.ErrorDetails != null)
      {
        RemotingEncoder.AddNoteProperty<string>(dest, "ErrorDetails_Message", (RemotingEncoder.ValueGetterDelegate<string>) (() => this.ErrorDetails.Message));
        RemotingEncoder.AddNoteProperty<string>(dest, "ErrorDetails_RecommendedAction", (RemotingEncoder.ValueGetterDelegate<string>) (() => this.ErrorDetails.RecommendedAction));
      }
      if (!this.SerializeExtendedInfo || this.InvocationInfo == null)
      {
        SerializationUtilities.AddProperty(dest, "SerializeExtendedInfo", (object) false);
      }
      else
      {
        SerializationUtilities.AddProperty(dest, "SerializeExtendedInfo", (object) true);
        this.InvocationInfo.ToPSObjectForRemoting(dest);
        RemotingEncoder.AddNoteProperty<object>(dest, "PipelineIterationInfo", (RemotingEncoder.ValueGetterDelegate<object>) (() => (object) this.PipelineIterationInfo));
      }
    }

    private static object GetNoteValue(PSObject mshObject, string note) => mshObject.Properties[note] is PSNoteProperty property ? property.Value : (object) null;

    internal static ErrorRecord FromPSObjectForRemoting(PSObject serializedErrorRecord)
    {
      PSObject serializedRemoteException = serializedErrorRecord != null ? RemotingDecoder.GetPropertyValue<PSObject>(serializedErrorRecord, "Exception") : throw ErrorRecord.tracer.NewArgumentNullException(nameof (serializedErrorRecord));
      object propertyValue1 = RemotingDecoder.GetPropertyValue<object>(serializedErrorRecord, "TargetObject");
      PSObject propertyValue2 = RemotingDecoder.GetPropertyValue<PSObject>(serializedErrorRecord, "InvocationInfo");
      string str = (string) null;
      if (serializedRemoteException != null)
      {
        PSPropertyInfo property = serializedRemoteException.Properties["Message"];
        if (property != null)
          str = property.Value as string;
      }
      string fullyQualifiedErrorId = RemotingDecoder.GetPropertyValue<string>(serializedErrorRecord, "FullyQualifiedErrorId") ?? "fullyQualifiedErrorId";
      ErrorCategory propertyValue3 = RemotingDecoder.GetPropertyValue<ErrorCategory>(serializedErrorRecord, "errorCategory_Category");
      string propertyValue4 = RemotingDecoder.GetPropertyValue<string>(serializedErrorRecord, "ErrorCategory_Activity");
      string propertyValue5 = RemotingDecoder.GetPropertyValue<string>(serializedErrorRecord, "ErrorCategory_Reason");
      string propertyValue6 = RemotingDecoder.GetPropertyValue<string>(serializedErrorRecord, "ErrorCategory_TargetName");
      string propertyValue7 = RemotingDecoder.GetPropertyValue<string>(serializedErrorRecord, "ErrorCategory_TargetType");
      string propertyValue8 = RemotingDecoder.GetPropertyValue<string>(serializedErrorRecord, "ErrorCategory_Message");
      string noteValue1 = ErrorRecord.GetNoteValue(serializedErrorRecord, "ErrorDetails_Message") as string;
      string noteValue2 = ErrorRecord.GetNoteValue(serializedErrorRecord, "ErrorDetails_RecommendedAction") as string;
      RemoteException remoteException = new RemoteException(str ?? propertyValue8, serializedRemoteException, propertyValue2);
      ErrorRecord remoteError = new ErrorRecord((Exception) remoteException, propertyValue1, fullyQualifiedErrorId, propertyValue3, propertyValue4, propertyValue5, propertyValue6, propertyValue7, propertyValue8, noteValue1, noteValue2);
      remoteException.SetRemoteErrorRecord(remoteError);
      remoteError.serializeExtendedInfo = RemotingDecoder.GetPropertyValue<bool>(serializedErrorRecord, "SerializeExtendedInfo");
      if (remoteError.serializeExtendedInfo)
      {
        remoteError._invocationInfo = new InvocationInfo(serializedErrorRecord);
        ArrayList propertyValue9 = RemotingDecoder.GetPropertyValue<ArrayList>(serializedErrorRecord, "PipelineIterationInfo");
        if (propertyValue9 != null)
          remoteError.pipelineIterationInfo = new ReadOnlyCollection<int>((IList<int>) propertyValue9.ToArray(typeof (int)));
      }
      else
        remoteError._invocationInfo = (InvocationInfo) null;
      return remoteError;
    }

    internal ErrorRecord(ErrorRecord errorRecord, Exception replaceMshSystemException)
    {
      this._error = replaceMshSystemException == null || !(errorRecord.Exception is ParentContainsErrorRecordException) ? errorRecord.Exception : replaceMshSystemException;
      this._target = errorRecord.TargetObject;
      this._errorId = errorRecord._errorId;
      this._category = errorRecord._category;
      this._activityOverride = errorRecord._activityOverride;
      this._reasonOverride = errorRecord._reasonOverride;
      this._targetNameOverride = errorRecord._targetNameOverride;
      this._targetTypeOverride = errorRecord._targetTypeOverride;
      if (errorRecord.ErrorDetails != null)
        this._errorDetails = new ErrorDetails(errorRecord.ErrorDetails);
      this.SetInvocationInfo(errorRecord._invocationInfo);
      this._serializedFullyQualifiedErrorId = errorRecord._serializedFullyQualifiedErrorId;
    }

    public Exception Exception => this._error;

    public object TargetObject => this._target;

    internal void SetTargetObject(object target) => this._target = target;

    public ErrorCategoryInfo CategoryInfo
    {
      get
      {
        if (this._categoryInfo == null)
          this._categoryInfo = new ErrorCategoryInfo(this);
        return this._categoryInfo;
      }
    }

    public string FullyQualifiedErrorId
    {
      get
      {
        if (this._serializedFullyQualifiedErrorId != null)
          return this._serializedFullyQualifiedErrorId;
        string invocationTypeName = this.GetInvocationTypeName();
        string str = string.IsNullOrEmpty(invocationTypeName) || string.IsNullOrEmpty(this._errorId) ? "" : ",";
        return ErrorRecord.NotNull(this._errorId) + str + ErrorRecord.NotNull(invocationTypeName);
      }
    }

    public ErrorDetails ErrorDetails
    {
      get => this._errorDetails;
      set => this._errorDetails = value;
    }

    public InvocationInfo InvocationInfo => this._invocationInfo;

    internal void SetInvocationInfo(InvocationInfo invocationInfo)
    {
      this._invocationInfo = invocationInfo;
      if (invocationInfo == null || invocationInfo.PipelineIterationInfo == null)
        return;
      this.pipelineIterationInfo = new ReadOnlyCollection<int>((IList<int>) (int[]) invocationInfo.PipelineIterationInfo.Clone());
    }

    internal bool PreserveInvocationInfoOnce
    {
      get => this.preserveInvocationInfoOnce;
      set => this.preserveInvocationInfoOnce = value;
    }

    public ReadOnlyCollection<int> PipelineIterationInfo => this.pipelineIterationInfo;

    internal bool SerializeExtendedInfo
    {
      get => this.serializeExtendedInfo;
      set => this.serializeExtendedInfo = value;
    }

    internal static string NotNull(string s) => s == null ? "" : s;

    private string GetInvocationTypeName()
    {
      InvocationInfo invocationInfo = this.InvocationInfo;
      if (invocationInfo == null)
        return "";
      CommandInfo myCommand = invocationInfo.MyCommand;
      switch (myCommand)
      {
        case null:
          return "";
        case IScriptCommandInfo _:
          return myCommand.Name;
        case CmdletInfo cmdletInfo:
          return cmdletInfo.ImplementingType.FullName;
        default:
          return "";
      }
    }

    internal static ErrorRecord MakeRedirectedException(object o) => new ErrorRecord((Exception) new RedirectedException(ResourceManagerCache.FormatResourceString("ErrorPackage", "RedirectedException", o == null ? (object) "" : (object) ErrorCategoryInfo.Ellipsize(Thread.CurrentThread.CurrentUICulture, o.ToString()))), "RedirectedException", ErrorCategory.NotSpecified, o);

    public override string ToString()
    {
      if (this.ErrorDetails != null && !string.IsNullOrEmpty(this.ErrorDetails.Message))
        return this.ErrorDetails.Message;
      if (this.Exception == null)
        return base.ToString();
      return !string.IsNullOrEmpty(this.Exception.Message) ? this.Exception.Message : this.Exception.ToString();
    }
  }
}
