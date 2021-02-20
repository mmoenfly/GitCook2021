// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.Client.WSManTransportManagerUtils
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Remoting.Client
{
  internal static class WSManTransportManagerUtils
  {
    internal static TransportErrorOccuredEventArgs ConstructTransportErrorEventArgs(
      IntPtr wsmanAPIHandle,
      IntPtr wsmanSessionHandle,
      WSManNativeApi.WSManError errorStruct,
      TransportMethodEnum transportMethodReportingError,
      PSRemotingErrorId errorResourceID,
      params object[] resourceArgs)
    {
      PSRemotingTransportException e;
      if (errorStruct.errorCode == -2144108135)
      {
        string sessionOptionAsString = WSManNativeApi.WSManGetSessionOptionAsString(wsmanSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_REDIRECT_LOCATION);
        string str = WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(WSManNativeApi.WSManGetErrorMessage(wsmanAPIHandle, errorStruct.errorCode)).Trim();
        e = (PSRemotingTransportException) new PSRemotingTransportRedirectException(sessionOptionAsString, PSRemotingErrorId.URIEndPointNotResolved, new object[2]
        {
          (object) str,
          (object) sessionOptionAsString
        });
      }
      else
      {
        e = new PSRemotingTransportException(PSRemotingErrorId.TroubleShootingHelpTopic, new object[1]
        {
          (object) PSRemotingErrorInvariants.FormatResourceString(errorResourceID, resourceArgs)
        });
        e.TransportMessage = WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(WSManNativeApi.WSManGetErrorMessage(wsmanAPIHandle, errorStruct.errorCode));
      }
      e.ErrorCode = errorStruct.errorCode;
      return new TransportErrorOccuredEventArgs(e, transportMethodReportingError);
    }

    internal static string ParseEscapeWSManErrorMessage(string errorMessage) => string.IsNullOrEmpty(errorMessage) || !errorMessage.Contains("@{") ? errorMessage : errorMessage.Replace("@{", "'@{").Replace("}", "}'");
  }
}
