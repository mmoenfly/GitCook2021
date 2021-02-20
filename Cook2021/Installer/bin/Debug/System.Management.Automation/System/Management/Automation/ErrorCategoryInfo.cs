// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ErrorCategoryInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.Threading;

namespace System.Management.Automation
{
  public class ErrorCategoryInfo
  {
    [TraceSource("ErrorCategoryInfo", "ErrorCategoryInfo")]
    internal static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ErrorCategoryInfo), nameof (ErrorCategoryInfo));
    private ErrorRecord _errorRecord;

    internal ErrorCategoryInfo(ErrorRecord errorRecord) => this._errorRecord = errorRecord != null ? errorRecord : throw new ArgumentNullException(nameof (errorRecord));

    public ErrorCategory Category => this._errorRecord._category;

    public string Activity
    {
      get
      {
        if (!string.IsNullOrEmpty(this._errorRecord._activityOverride))
          return this._errorRecord._activityOverride;
        return this._errorRecord.InvocationInfo != null && (this._errorRecord.InvocationInfo.MyCommand is CmdletInfo || this._errorRecord.InvocationInfo.MyCommand is IScriptCommandInfo) && !string.IsNullOrEmpty(this._errorRecord.InvocationInfo.MyCommand.Name) ? this._errorRecord.InvocationInfo.MyCommand.Name : "";
      }
      set => this._errorRecord._activityOverride = value;
    }

    public string Reason
    {
      get
      {
        if (!string.IsNullOrEmpty(this._errorRecord._reasonOverride))
          return this._errorRecord._reasonOverride;
        return this._errorRecord.Exception != null ? this._errorRecord.Exception.GetType().Name : "";
      }
      set => this._errorRecord._reasonOverride = value;
    }

    public string TargetName
    {
      get
      {
        if (!string.IsNullOrEmpty(this._errorRecord._targetNameOverride))
          return this._errorRecord._targetNameOverride;
        return this._errorRecord.TargetObject != null ? ErrorRecord.NotNull(this._errorRecord.TargetObject.ToString()) : "";
      }
      set => this._errorRecord._targetNameOverride = value;
    }

    public string TargetType
    {
      get
      {
        if (!string.IsNullOrEmpty(this._errorRecord._targetTypeOverride))
          return this._errorRecord._targetTypeOverride;
        return this._errorRecord.TargetObject != null ? this._errorRecord.TargetObject.GetType().Name : "";
      }
      set => this._errorRecord._targetTypeOverride = value;
    }

    public string GetMessage() => this.GetMessage(Thread.CurrentThread.CurrentUICulture);

    public string GetMessage(CultureInfo uiCultureInfo)
    {
      string resourceId = this.Category.ToString();
      if (string.IsNullOrEmpty(resourceId))
        resourceId = ErrorCategory.NotSpecified.ToString();
      string stringForUiCulture1 = ResourceManagerCache.GetResourceStringForUICulture("ErrorCategory", resourceId, uiCultureInfo);
      if (string.IsNullOrEmpty(stringForUiCulture1))
        stringForUiCulture1 = ResourceManagerCache.GetResourceStringForUICulture("ErrorCategory", "NotSpecified", uiCultureInfo);
      try
      {
        return string.Format((IFormatProvider) uiCultureInfo, stringForUiCulture1, (object) ErrorCategoryInfo.Ellipsize(uiCultureInfo, this.Activity), (object) ErrorCategoryInfo.Ellipsize(uiCultureInfo, this.TargetName), (object) ErrorCategoryInfo.Ellipsize(uiCultureInfo, this.TargetType), (object) ErrorCategoryInfo.Ellipsize(uiCultureInfo, this.Reason), (object) resourceId);
      }
      catch (FormatException ex)
      {
        string stringForUiCulture2 = ResourceManagerCache.GetResourceStringForUICulture("ErrorCategory", "InvalidErrorCategory", uiCultureInfo);
        return string.Format((IFormatProvider) uiCultureInfo, stringForUiCulture2, (object) ErrorCategoryInfo.Ellipsize(uiCultureInfo, this.Activity), (object) ErrorCategoryInfo.Ellipsize(uiCultureInfo, this.TargetName), (object) ErrorCategoryInfo.Ellipsize(uiCultureInfo, this.TargetType), (object) ErrorCategoryInfo.Ellipsize(uiCultureInfo, this.Reason), (object) resourceId);
      }
    }

    public override string ToString() => this.GetMessage(Thread.CurrentThread.CurrentUICulture);

    internal static string Ellipsize(CultureInfo uiCultureInfo, string original)
    {
      if (40 >= original.Length)
        return original;
      string str1 = original.Substring(0, 15);
      string str2 = original.Substring(original.Length - 15, 15);
      return ResourceManagerCache.FormatResourceStringUsingCulture(uiCultureInfo, (CultureInfo) null, "ErrorPackage", nameof (Ellipsize), (object) str1, (object) str2);
    }
  }
}
