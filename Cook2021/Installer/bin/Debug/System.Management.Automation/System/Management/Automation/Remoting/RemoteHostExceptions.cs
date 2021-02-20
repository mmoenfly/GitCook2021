// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.RemoteHostExceptions
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Remoting
{
  internal static class RemoteHostExceptions
  {
    internal static Exception NewRemoteRunspaceDoesNotSupportPushRunspaceException() => (Exception) new PSRemotingDataStructureException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.RemoteRunspaceDoesNotSupportPushRunspace));

    internal static Exception NewDecodingFailedException() => (Exception) new PSRemotingDataStructureException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.RemoteHostDecodingFailed));

    internal static Exception NewNotImplementedException(RemoteHostMethodId methodId) => (Exception) new PSRemotingDataStructureException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.RemoteHostMethodNotImplemented, (object) RemoteHostMethodInfo.LookUp(methodId).Name), (Exception) new PSNotImplementedException());

    internal static Exception NewRemoteHostCallFailedException(RemoteHostMethodId methodId) => (Exception) new PSRemotingDataStructureException(PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.RemoteHostCallFailed, (object) RemoteHostMethodInfo.LookUp(methodId).Name));

    internal static Exception NewDecodingErrorForErrorRecordException() => (Exception) new PSRemotingDataStructureException(PSRemotingErrorId.DecodingErrorForErrorRecord, new object[0]);

    internal static Exception NewRemoteHostDataEncodingNotSupportedException(Type type) => (Exception) new PSRemotingDataStructureException(PSRemotingErrorId.RemoteHostDataEncodingNotSupported, new object[1]
    {
      (object) type.ToString()
    });

    internal static Exception NewRemoteHostDataDecodingNotSupportedException(Type type) => (Exception) new PSRemotingDataStructureException(PSRemotingErrorId.RemoteHostDataDecodingNotSupported, new object[1]
    {
      (object) type.ToString()
    });

    internal static Exception NewUnknownTargetClassException(string className) => (Exception) new PSRemotingDataStructureException(PSRemotingErrorId.UnknownTargetClass, new object[1]
    {
      (object) className
    });
  }
}
