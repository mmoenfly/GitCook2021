// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.RemoteHostResponse
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Remoting
{
  internal class RemoteHostResponse
  {
    private long _callId;
    private RemoteHostMethodId _methodId;
    private object _returnValue;
    private Exception _exception;

    internal long CallId => this._callId;

    internal RemoteHostResponse(
      long callId,
      RemoteHostMethodId methodId,
      object returnValue,
      Exception exception)
    {
      this._callId = callId;
      this._methodId = methodId;
      this._returnValue = returnValue;
      this._exception = exception;
    }

    internal object SimulateExecution()
    {
      if (this._exception != null)
        throw this._exception;
      return this._returnValue;
    }

    private static void EncodeAndAddReturnValue(PSObject psObject, object returnValue)
    {
      if (returnValue == null)
        return;
      RemoteHostEncoder.EncodeAndAddAsProperty(psObject, "mr", returnValue);
    }

    private static object DecodeReturnValue(PSObject psObject, Type returnType) => RemoteHostEncoder.DecodePropertyValue(psObject, "mr", returnType);

    private static void EncodeAndAddException(PSObject psObject, Exception exception) => RemoteHostEncoder.EncodeAndAddAsProperty(psObject, "me", (object) exception);

    private static Exception DecodeException(PSObject psObject)
    {
      object obj = RemoteHostEncoder.DecodePropertyValue(psObject, "me", typeof (Exception));
      if (obj == null)
        return (Exception) null;
      return obj is Exception ? (Exception) obj : throw RemoteHostExceptions.NewDecodingFailedException();
    }

    internal PSObject Encode()
    {
      PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
      RemoteHostResponse.EncodeAndAddReturnValue(emptyPsObject, this._returnValue);
      RemoteHostResponse.EncodeAndAddException(emptyPsObject, this._exception);
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("ci", (object) this._callId));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("mi", (object) this._methodId));
      return emptyPsObject;
    }

    internal static RemoteHostResponse Decode(PSObject data)
    {
      long propertyValue1 = RemotingDecoder.GetPropertyValue<long>(data, "ci");
      RemoteHostMethodId propertyValue2 = RemotingDecoder.GetPropertyValue<RemoteHostMethodId>(data, "mi");
      RemoteHostMethodInfo remoteHostMethodInfo = RemoteHostMethodInfo.LookUp(propertyValue2);
      object returnValue = RemoteHostResponse.DecodeReturnValue(data, remoteHostMethodInfo.ReturnType);
      Exception exception = RemoteHostResponse.DecodeException(data);
      return new RemoteHostResponse(propertyValue1, propertyValue2, returnValue, exception);
    }
  }
}
