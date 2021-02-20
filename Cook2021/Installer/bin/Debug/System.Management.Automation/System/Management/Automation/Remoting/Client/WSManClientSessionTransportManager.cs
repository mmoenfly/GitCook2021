// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.Client.WSManClientSessionTransportManager
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Runspaces.Internal;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace System.Management.Automation.Remoting.Client
{
  internal sealed class WSManClientSessionTransportManager : BaseClientSessionTransportManager
  {
    internal const string MAX_URI_REDIRECTION_COUNT_VARIABLE = "WSManMaxRedirectionCount";
    internal const int MAX_URI_REDIRECTION_COUNT = 5;
    private const string resBaseName = "remotingerroridstrings";
    private IntPtr wsManSessionHandle;
    private IntPtr wsManShellOperationHandle;
    private IntPtr wsManRecieveOperationHandle;
    private IntPtr wsManSendOperationHandle;
    private long sessionContextID;
    private bool isCreateCallbackReceived;
    private bool isClosePending;
    private string resourceUri;
    private PrioritySendDataCollection.OnDataAvailableCallback onDataAvailableToSendCallback;
    private WSManNativeApi.WSManShellAsync createSessionCompleted;
    private WSManNativeApi.WSManShellAsync receivedFromRemote;
    private WSManNativeApi.WSManShellAsync sendToRemoteCompleted;
    private GCHandle createSessionCompletedGCHandle;
    private WSManNativeApi.WSManShellAsync closeSessionCompleted;
    private WSManNativeApi.WSManData openContent;
    private bool noCompression;
    private bool noMachineProfile;
    private long idleTimeout = 240000;
    internal static WSManClientSessionTransportManager.WSManAPIStaticData wsManApiStaticData;
    private static WSManNativeApi.WSManShellAsyncCallback sessionCreateCallback;
    private static WSManNativeApi.WSManShellAsyncCallback sessionCloseCallback;
    private static WSManNativeApi.WSManShellAsyncCallback sessionReceiveCallback;
    private static WSManNativeApi.WSManShellAsyncCallback sessionSendCallback;
    private static Dictionary<long, WSManClientSessionTransportManager> SessionTMHandles = new Dictionary<long, WSManClientSessionTransportManager>();
    private static long SessionTMSeed;
    private static Delegate sessionSendRedirect = (Delegate) null;
    private static Delegate protocolVersionRedirect = (Delegate) null;

    private static long GetNextSessionTMHandleId() => Interlocked.Increment(ref WSManClientSessionTransportManager.SessionTMSeed);

    private static void AddSessionTransportManager(
      long sessnTMId,
      WSManClientSessionTransportManager sessnTransportManager)
    {
      lock (WSManClientSessionTransportManager.SessionTMHandles)
        WSManClientSessionTransportManager.SessionTMHandles.Add(sessnTMId, sessnTransportManager);
    }

    private static void RemoveSessionTransportManager(long sessnTMId)
    {
      lock (WSManClientSessionTransportManager.SessionTMHandles)
      {
        if (!WSManClientSessionTransportManager.SessionTMHandles.ContainsKey(sessnTMId))
          return;
        WSManClientSessionTransportManager.SessionTMHandles[sessnTMId] = (WSManClientSessionTransportManager) null;
        WSManClientSessionTransportManager.SessionTMHandles.Remove(sessnTMId);
      }
    }

    private static bool TryGetSessionTransportManager(
      IntPtr operationContext,
      out WSManClientSessionTransportManager sessnTransportManager,
      out long sessnTMId)
    {
      sessnTMId = operationContext.ToInt64();
      sessnTransportManager = (WSManClientSessionTransportManager) null;
      lock (WSManClientSessionTransportManager.SessionTMHandles)
        return WSManClientSessionTransportManager.SessionTMHandles.TryGetValue(sessnTMId, out sessnTransportManager);
    }

    static WSManClientSessionTransportManager()
    {
      WSManClientSessionTransportManager.wsManApiStaticData = new WSManClientSessionTransportManager.WSManAPIStaticData();
      WSManClientSessionTransportManager.sessionCreateCallback = new WSManNativeApi.WSManShellAsyncCallback(new WSManNativeApi.WSManShellCompletionFunction(WSManClientSessionTransportManager.OnCreateSessionCompleted));
      WSManClientSessionTransportManager.sessionCloseCallback = new WSManNativeApi.WSManShellAsyncCallback(new WSManNativeApi.WSManShellCompletionFunction(WSManClientSessionTransportManager.OnCloseSessionCompleted));
      WSManClientSessionTransportManager.sessionReceiveCallback = new WSManNativeApi.WSManShellAsyncCallback(new WSManNativeApi.WSManShellCompletionFunction(WSManClientSessionTransportManager.OnRemoteSessionDataReceived));
      WSManClientSessionTransportManager.sessionSendCallback = new WSManNativeApi.WSManShellAsyncCallback(new WSManNativeApi.WSManShellCompletionFunction(WSManClientSessionTransportManager.OnRemoteSessionSendCompleted));
    }

    internal WSManClientSessionTransportManager(
      Guid runspacePoolInstanceId,
      WSManConnectionInfo connectionInfo,
      PSRemotingCryptoHelper cryptoHelper)
      : base(runspacePoolInstanceId, cryptoHelper)
    {
      using (BaseClientTransportManager.tracer.TraceConstructor((object) this))
      {
        this.resourceUri = connectionInfo.ShellUri;
        this.CryptoHelper = cryptoHelper;
        this.dataToBeSent.Fragmentor = this.Fragmentor;
        this.ReceivedDataCollection.MaximumReceivedDataSize = new int?();
        this.ReceivedDataCollection.MaximumReceivedObjectSize = connectionInfo.MaximumReceivedObjectSize;
        this.onDataAvailableToSendCallback = new PrioritySendDataCollection.OnDataAvailableCallback(this.OnDataAvailableCallback);
        this.Initialize(connectionInfo.ConnectionUri, connectionInfo);
      }
    }

    internal void SetDefaultTimeOut(int milliseconds)
    {
      using (BaseClientTransportManager.tracer.TraceMethod("Setting Default timeout: {0} milliseconds", (object) milliseconds))
      {
        int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_DEFAULT_OPERATION_TIMEOUTMS, new WSManNativeApi.WSManDataDWord(milliseconds));
        if (errorCode != 0)
          throw new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, errorCode));
      }
    }

    internal void SetConnectTimeOut(int milliseconds)
    {
      using (BaseClientTransportManager.tracer.TraceMethod("Setting CreateShell timeout: {0} milliseconds", (object) milliseconds))
      {
        int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_CREATE_SHELL, new WSManNativeApi.WSManDataDWord(milliseconds));
        if (errorCode != 0)
          throw new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, errorCode));
      }
    }

    internal void SetCloseTimeOut(int milliseconds)
    {
      using (BaseClientTransportManager.tracer.TraceMethod("Setting CloseShell timeout: {0} milliseconds", (object) milliseconds))
      {
        int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_CLOSE_SHELL_OPERATION, new WSManNativeApi.WSManDataDWord(milliseconds));
        if (errorCode != 0)
          throw new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, errorCode));
      }
    }

    internal void SetSendTimeOut(int milliseconds)
    {
      using (BaseClientTransportManager.tracer.TraceMethod("Setting SendShellInput timeout: {0} milliseconds", (object) milliseconds))
      {
        int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_SEND_SHELL_INPUT, new WSManNativeApi.WSManDataDWord(milliseconds));
        if (errorCode != 0)
          throw new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, errorCode));
      }
    }

    internal void SetReceiveTimeOut(int milliseconds)
    {
      using (BaseClientTransportManager.tracer.TraceMethod("Setting ReceiveShellOutput timeout: {0} milliseconds", (object) milliseconds))
      {
        int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_RECEIVE_SHELL_OUTPUT, new WSManNativeApi.WSManDataDWord(milliseconds));
        if (errorCode != 0)
          throw new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, errorCode));
      }
    }

    internal void SetSignalTimeOut(int milliseconds)
    {
      using (BaseClientTransportManager.tracer.TraceMethod("Setting SignalShell timeout: {0} milliseconds", (object) milliseconds))
      {
        int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_SIGNAL_SHELL, new WSManNativeApi.WSManDataDWord(milliseconds));
        if (errorCode != 0)
          throw new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, errorCode));
      }
    }

    internal void SetWSManSessionOption(WSManNativeApi.WSManSessionOption option, int dwordData)
    {
      int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, option, new WSManNativeApi.WSManDataDWord(dwordData));
      if (errorCode != 0)
        throw new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, errorCode));
    }

    internal void SetWSManSessionOption(WSManNativeApi.WSManSessionOption option, string stringData)
    {
      using (WSManNativeApi.WSManData wsManData = new WSManNativeApi.WSManData(stringData))
      {
        int errorCode = WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, option, (IntPtr) wsManData);
        if (errorCode != 0)
          throw new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, errorCode));
      }
    }

    internal override void ConnectAsync()
    {
      List<WSManNativeApi.WSManOption> wsManOptionList = new List<WSManNativeApi.WSManOption>((IEnumerable<WSManNativeApi.WSManOption>) WSManClientSessionTransportManager.wsManApiStaticData.CommonOptionSet);
      if ((object) WSManClientSessionTransportManager.protocolVersionRedirect != null)
      {
        string str = (string) WSManClientSessionTransportManager.protocolVersionRedirect.DynamicInvoke();
        wsManOptionList.Clear();
        wsManOptionList.Add(new WSManNativeApi.WSManOption()
        {
          name = "protocolversion",
          value = str,
          mustComply = true
        });
      }
      WSManNativeApi.WSManShellStartupInfo startupInfo = new WSManNativeApi.WSManShellStartupInfo(WSManClientSessionTransportManager.wsManApiStaticData.InputStreamSet, WSManClientSessionTransportManager.wsManApiStaticData.OutputStreamSet, 0L > this.idleTimeout || this.idleTimeout >= (long) uint.MaxValue ? uint.MaxValue : (uint) this.idleTimeout);
      if (this.openContent == null)
      {
        byte[] inArray = this.dataToBeSent.ReadOrRegisterCallback((PrioritySendDataCollection.OnDataAvailableCallback) null, out DataPriorityType _);
        bool flag = true;
        if ((object) WSManClientSessionTransportManager.sessionSendRedirect != null)
        {
          object[] objArray = new object[2]
          {
            null,
            (object) inArray
          };
          flag = (bool) WSManClientSessionTransportManager.sessionSendRedirect.DynamicInvoke(objArray);
          inArray = (byte[]) objArray[0];
        }
        if (!flag)
          return;
        if (inArray != null)
          this.openContent = new WSManNativeApi.WSManData(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "<{0} xmlns=\"{1}\">{2}</{0}>", (object) "creationXml", (object) "http://schemas.microsoft.com/powershell", (object) Convert.ToBase64String(inArray, Base64FormattingOptions.None)));
      }
      this.sessionContextID = WSManClientSessionTransportManager.GetNextSessionTMHandleId();
      WSManClientSessionTransportManager.AddSessionTransportManager(this.sessionContextID, this);
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManCreateShell, PSOpcode.Connect, PSTask.CreateRunspace, (object) this.RunspacePoolInstanceId);
      this.createSessionCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.sessionContextID), WSManClientSessionTransportManager.sessionCreateCallback);
      this.createSessionCompletedGCHandle = GCHandle.Alloc((object) this.createSessionCompleted);
      try
      {
        lock (this.syncObject)
        {
          if (this.isClosed)
            return;
          if (this.noMachineProfile)
            wsManOptionList.Add(new WSManNativeApi.WSManOption()
            {
              name = "WINRS_NOPROFILE",
              mustComply = true,
              value = "1"
            });
          using (WSManNativeApi.WSManOptionSet optionSet = new WSManNativeApi.WSManOptionSet(wsManOptionList.ToArray()))
            WSManNativeApi.WSManCreateShellEx(this.wsManSessionHandle, this.noCompression ? 1 : 0, this.resourceUri, startupInfo, optionSet, this.openContent, (IntPtr) this.createSessionCompleted, ref this.wsManShellOperationHandle);
        }
        if (!(this.wsManShellOperationHandle == IntPtr.Zero))
          return;
        this.RaiseErrorHandler(WSManTransportManagerUtils.ConstructTransportErrorEventArgs(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, IntPtr.Zero, new WSManNativeApi.WSManError(), TransportMethodEnum.CreateShellEx, PSRemotingErrorId.ConnectExFailed));
      }
      finally
      {
        startupInfo.Dispose();
      }
    }

    internal override void CloseAsync()
    {
      bool flag = false;
      lock (this.syncObject)
      {
        if (this.isClosed)
          return;
        if (!this.isCreateCallbackReceived)
        {
          this.isClosePending = true;
          return;
        }
        this.isClosed = true;
        if (IntPtr.Zero == this.wsManShellOperationHandle)
          flag = true;
      }
      base.CloseAsync();
      if (flag)
      {
        try
        {
          this.RaiseCloseCompleted();
        }
        finally
        {
          WSManClientSessionTransportManager.RemoveSessionTransportManager(this.sessionContextID);
        }
      }
      else
      {
        BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManCloseShell, PSOpcode.Disconnect, PSTask.None, (object) this.RunspacePoolInstanceId);
        this.closeSessionCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.sessionContextID), WSManClientSessionTransportManager.sessionCloseCallback);
        WSManNativeApi.WSManCloseShell(this.wsManShellOperationHandle, 0, (IntPtr) this.closeSessionCompleted);
      }
    }

    internal override void PrepareForRedirection()
    {
      this.closeSessionCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.sessionContextID), WSManClientSessionTransportManager.sessionCloseCallback);
      WSManNativeApi.WSManCloseShell(this.wsManShellOperationHandle, 0, (IntPtr) this.closeSessionCompleted);
    }

    internal override void Redirect(Uri newUri, RunspaceConnectionInfo connectionInfo)
    {
      this.CloseSessionAndClearResources();
      BaseClientTransportManager.tracer.WriteLine("Redirecting to URI: {0}", (object) newUri);
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.URIRedirection, PSOpcode.Connect, PSTask.None, (object) this.RunspacePoolInstanceId, (object) newUri);
      this.Initialize(newUri, (WSManConnectionInfo) connectionInfo);
      this.ConnectAsync();
    }

    internal override BaseClientCommandTransportManager CreateClientCommandTransportManager(
      RunspaceConnectionInfo connectionInfo,
      ClientRemotePowerShell cmd,
      bool noInput)
    {
      return (BaseClientCommandTransportManager) new WSManClientCommandTransportManager(connectionInfo as WSManConnectionInfo, this.wsManShellOperationHandle, cmd, noInput, this);
    }

    private void Initialize(Uri connectionUri, WSManConnectionInfo connectionInfo)
    {
      bool isSSLSpecified = false;
      string str = connectionUri.OriginalString;
      if (connectionUri == connectionInfo.ConnectionUri && connectionInfo.UseDefaultWSManPort)
        str = WSManConnectionInfo.GetConnectionString(connectionInfo.ConnectionUri, out isSSLSpecified);
      string connection;
      if (string.IsNullOrEmpty(connectionUri.Query))
        connection = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}?PSVersion={1}", (object) str.TrimEnd('/'), (object) PSVersionInfo.PSVersion);
      else
        connection = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0};PSVersion={1}", (object) str, (object) PSVersionInfo.PSVersion);
      WSManNativeApi.BaseWSManAuthenticationCredentials authenticationCredentials;
      if (connectionInfo.CertificateThumbprint != null)
      {
        authenticationCredentials = (WSManNativeApi.BaseWSManAuthenticationCredentials) new WSManNativeApi.WSManCertificateThumbprintCredentials(connectionInfo.CertificateThumbprint);
      }
      else
      {
        string name = (string) null;
        SecureString pwd = (SecureString) null;
        if (connectionInfo.Credential != null && !string.IsNullOrEmpty(connectionInfo.Credential.UserName))
        {
          name = connectionInfo.Credential.UserName;
          pwd = connectionInfo.Credential.Password;
        }
        authenticationCredentials = (WSManNativeApi.BaseWSManAuthenticationCredentials) new WSManNativeApi.WSManUserNameAuthenticationCredentials(name, pwd, connectionInfo.WSManAuthenticationMechanism);
      }
      WSManNativeApi.WSManUserNameAuthenticationCredentials authCredentials = (WSManNativeApi.WSManUserNameAuthenticationCredentials) null;
      if (connectionInfo.ProxyCredential != null)
      {
        WSManNativeApi.WSManAuthenticationMechanism authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_NEGOTIATE;
        string name = (string) null;
        SecureString pwd = (SecureString) null;
        switch (connectionInfo.ProxyAuthentication)
        {
          case AuthenticationMechanism.Basic:
            authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_BASIC;
            break;
          case AuthenticationMechanism.Negotiate:
            authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_NEGOTIATE;
            break;
          case AuthenticationMechanism.Digest:
            authMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_DIGEST;
            break;
        }
        if (!string.IsNullOrEmpty(connectionInfo.ProxyCredential.UserName))
        {
          name = connectionInfo.ProxyCredential.UserName;
          pwd = connectionInfo.ProxyCredential.Password;
        }
        authCredentials = new WSManNativeApi.WSManUserNameAuthenticationCredentials(name, pwd, authMechanism);
      }
      WSManNativeApi.WSManProxyInfo wsManProxyInfo = connectionInfo.ProxyAccessType == ProxyAccessType.None ? (WSManNativeApi.WSManProxyInfo) null : new WSManNativeApi.WSManProxyInfo(connectionInfo.ProxyAccessType, authCredentials);
      int errorCode = 0;
      try
      {
        errorCode = WSManNativeApi.WSManCreateSession(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, connection, 0, (IntPtr) authenticationCredentials.GetMarshalledObject(), wsManProxyInfo == null ? IntPtr.Zero : (IntPtr) wsManProxyInfo, ref this.wsManSessionHandle);
      }
      finally
      {
        authCredentials?.Dispose();
        wsManProxyInfo?.Dispose();
        authenticationCredentials?.Dispose();
      }
      if (errorCode != 0)
        throw new PSInvalidOperationException(WSManNativeApi.WSManGetErrorMessage(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, errorCode));
      int num;
      WSManNativeApi.WSManGetSessionOptionAsDword(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_SHELL_MAX_DATA_SIZE_PER_MESSAGE_KB, out num);
      this.Fragmentor.FragmentSize = num << 10;
      this.dataToBeSent.Fragmentor = this.Fragmentor;
      this.noCompression = !connectionInfo.UseCompression;
      this.noMachineProfile = connectionInfo.NoMachineProfile;
      this.idleTimeout = (long) connectionInfo.IdleTimeout;
      if (isSSLSpecified)
        this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_USE_SSL, 1);
      if (connectionInfo.NoEncryption)
        this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_UNENCRYPTED_MESSAGES, 1);
      if (connectionInfo.AllowImplicitCredentialForNegotiate)
        WSManNativeApi.WSManSetSessionOption(this.wsManSessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_ALLOW_NEGOTIATE_IMPLICIT_CREDENTIALS, new WSManNativeApi.WSManDataDWord(1));
      if (connectionInfo.UseUTF16)
        this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_UTF16, 1);
      if (connectionInfo.SkipCACheck)
        this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_SKIP_CA_CHECK, 1);
      if (connectionInfo.SkipCNCheck)
        this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_SKIP_CN_CHECK, 1);
      if (connectionInfo.SkipRevocationCheck)
        this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_SKIP_REVOCATION_CHECK, 1);
      string name1 = connectionInfo.UICulture.Name;
      if (!string.IsNullOrEmpty(name1))
        this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_UI_LANGUAGE, name1);
      string name2 = connectionInfo.Culture.Name;
      if (!string.IsNullOrEmpty(name2))
        this.SetWSManSessionOption(WSManNativeApi.WSManSessionOption.WSMAN_OPTION_LOCALE, name2);
      this.SetDefaultTimeOut(connectionInfo.OperationTimeout);
      this.SetConnectTimeOut(connectionInfo.OpenTimeout);
      this.SetCloseTimeOut(connectionInfo.CancelTimeout);
      this.SetSignalTimeOut(connectionInfo.CancelTimeout);
    }

    internal override void RaiseErrorHandler(TransportErrorOccuredEventArgs eventArgs)
    {
      BaseTransportManager.ETWTracer.OperationalChannel.WriteError(PSEventId.TransportError, PSOpcode.Open, PSTask.None, (object) this.RunspacePoolInstanceId, (object) Guid.Empty, (object) eventArgs.Exception.ErrorCode.ToString((IFormatProvider) CultureInfo.InvariantCulture), (object) eventArgs.Exception.Message, (object) eventArgs.Exception.StackTrace);
      BaseTransportManager.ETWTracer.AnalyticChannel.WriteError(PSEventId.TransportError_Analytic, PSOpcode.Open, PSTask.None, (object) this.RunspacePoolInstanceId, (object) Guid.Empty, (object) eventArgs.Exception.ErrorCode.ToString((IFormatProvider) CultureInfo.InvariantCulture), (object) eventArgs.Exception.Message, (object) eventArgs.Exception.StackTrace);
      base.RaiseErrorHandler(eventArgs);
    }

    internal void ClearReceiveOrSendResources(int flags, bool shouldClearSend)
    {
      if (shouldClearSend)
      {
        if (this.sendToRemoteCompleted != null)
        {
          this.sendToRemoteCompleted.Dispose();
          this.sendToRemoteCompleted = (WSManNativeApi.WSManShellAsync) null;
        }
        if (!(IntPtr.Zero != this.wsManSendOperationHandle))
          return;
        WSManNativeApi.WSManCloseOperation(this.wsManSendOperationHandle, 0);
        this.wsManSendOperationHandle = IntPtr.Zero;
      }
      else
      {
        if (flags != 1)
          return;
        if (IntPtr.Zero != this.wsManRecieveOperationHandle)
        {
          WSManNativeApi.WSManCloseOperation(this.wsManRecieveOperationHandle, 0);
          this.wsManRecieveOperationHandle = IntPtr.Zero;
        }
        if (this.receivedFromRemote == null)
          return;
        this.receivedFromRemote.Dispose();
        this.receivedFromRemote = (WSManNativeApi.WSManShellAsync) null;
      }
    }

    private static void OnCreateSessionCompleted(
      IntPtr operationContext,
      int flags,
      IntPtr error,
      IntPtr shellOperationHandle,
      IntPtr commandOperationHandle,
      IntPtr operationHandle,
      IntPtr data)
    {
      BaseClientTransportManager.tracer.WriteLine("Client Session TM: CreateShell callback received", new object[0]);
      long sessnTMId = 0;
      WSManClientSessionTransportManager sessnTransportManager = (WSManClientSessionTransportManager) null;
      if (!WSManClientSessionTransportManager.TryGetSessionTransportManager(operationContext, out sessnTransportManager, out sessnTMId))
      {
        BaseClientTransportManager.tracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Unable to find a transport manager for context {0}.", (object) sessnTMId), new object[0]);
      }
      else
      {
        BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManCreateShellCallbackReceived, PSOpcode.Connect, PSTask.None, (object) sessnTransportManager.RunspacePoolInstanceId);
        if (sessnTransportManager.createSessionCompleted != null)
        {
          sessnTransportManager.createSessionCompletedGCHandle.Free();
          sessnTransportManager.createSessionCompleted.Dispose();
          sessnTransportManager.createSessionCompleted = (WSManNativeApi.WSManShellAsync) null;
        }
        sessnTransportManager.wsManShellOperationHandle = shellOperationHandle;
        bool flag = false;
        lock (sessnTransportManager.syncObject)
        {
          sessnTransportManager.isCreateCallbackReceived = true;
          if (sessnTransportManager.isClosePending)
            flag = true;
        }
        if (flag)
        {
          sessnTransportManager.CloseAsync();
        }
        else
        {
          if (IntPtr.Zero != error)
          {
            WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
            if (errorStruct.errorCode != 0)
            {
              BaseClientTransportManager.tracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Got error with error code {0}. Message {1}", (object) errorStruct.errorCode, (object) errorStruct.errorDetail), new object[0]);
              TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, sessnTransportManager.wsManSessionHandle, errorStruct, TransportMethodEnum.CreateShellEx, PSRemotingErrorId.ConnectExCallBackError, (object) WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail));
              sessnTransportManager.RaiseErrorHandler(eventArgs);
              return;
            }
          }
          if (sessnTransportManager.openContent != null)
          {
            sessnTransportManager.openContent.Dispose();
            sessnTransportManager.openContent = (WSManNativeApi.WSManData) null;
          }
          lock (sessnTransportManager.syncObject)
          {
            if (sessnTransportManager.isClosed)
            {
              BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
              return;
            }
            sessnTransportManager.RaiseConnectCompleted();
            BaseClientTransportManager.tracer.WriteLine("Client Session TM: Placing Receive request using WSManReceiveShellOutputEx", new object[0]);
            BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManReceiveShellOutputEx, PSOpcode.Receive, PSTask.None, (object) sessnTransportManager.RunspacePoolInstanceId, (object) Guid.Empty);
            sessnTransportManager.receivedFromRemote = new WSManNativeApi.WSManShellAsync(operationContext, WSManClientSessionTransportManager.sessionReceiveCallback);
            WSManNativeApi.WSManReceiveShellOutputEx(sessnTransportManager.wsManShellOperationHandle, IntPtr.Zero, 0, (IntPtr) WSManClientSessionTransportManager.wsManApiStaticData.OutputStreamSet, (IntPtr) sessnTransportManager.receivedFromRemote, ref sessnTransportManager.wsManRecieveOperationHandle);
          }
          sessnTransportManager.SendOneItem();
        }
      }
    }

    private static void OnCloseSessionCompleted(
      IntPtr operationContext,
      int flags,
      IntPtr error,
      IntPtr shellOperationHandle,
      IntPtr commandOperationHandle,
      IntPtr operationHandle,
      IntPtr data)
    {
      BaseClientTransportManager.tracer.WriteLine("Client Session TM: CloseShell callback received", new object[0]);
      long sessnTMId = 0;
      WSManClientSessionTransportManager sessnTransportManager = (WSManClientSessionTransportManager) null;
      if (!WSManClientSessionTransportManager.TryGetSessionTransportManager(operationContext, out sessnTransportManager, out sessnTMId))
      {
        BaseClientTransportManager.tracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Unable to find a transport manager for context {0}.", (object) sessnTMId), new object[0]);
      }
      else
      {
        BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManCloseShellCallbackReceived, PSOpcode.Disconnect, PSTask.None, (object) sessnTransportManager.RunspacePoolInstanceId);
        if (IntPtr.Zero != error)
        {
          WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
          if (errorStruct.errorCode != 0)
          {
            BaseClientTransportManager.tracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Got error with error code {0}. Message {1}", (object) errorStruct.errorCode, (object) errorStruct.errorDetail), new object[0]);
            TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, sessnTransportManager.wsManSessionHandle, errorStruct, TransportMethodEnum.CloseShellOperationEx, PSRemotingErrorId.CloseExCallBackError, (object) WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail));
            sessnTransportManager.RaiseErrorHandler(eventArgs);
            return;
          }
        }
        sessnTransportManager.RaiseCloseCompleted();
      }
    }

    private static void OnRemoteSessionSendCompleted(
      IntPtr operationContext,
      int flags,
      IntPtr error,
      IntPtr shellOperationHandle,
      IntPtr commandOperationHandle,
      IntPtr operationHandle,
      IntPtr data)
    {
      BaseClientTransportManager.tracer.WriteLine("Client Session TM: SendComplete callback received", new object[0]);
      long sessnTMId = 0;
      WSManClientSessionTransportManager sessnTransportManager = (WSManClientSessionTransportManager) null;
      if (!WSManClientSessionTransportManager.TryGetSessionTransportManager(operationContext, out sessnTransportManager, out sessnTMId))
      {
        BaseClientTransportManager.tracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Unable to find a transport manager for context {0}.", (object) sessnTMId), new object[0]);
      }
      else
      {
        BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManSendShellInputExCallbackReceived, PSOpcode.Connect, PSTask.None, (object) sessnTransportManager.RunspacePoolInstanceId, (object) Guid.Empty);
        if (!shellOperationHandle.Equals((object) sessnTransportManager.wsManShellOperationHandle))
        {
          TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(new PSRemotingTransportException(PSRemotingErrorId.SendExFailed, new object[0]), TransportMethodEnum.SendShellInputEx);
          sessnTransportManager.RaiseErrorHandler(eventArgs);
        }
        else
        {
          sessnTransportManager.ClearReceiveOrSendResources(flags, true);
          if (sessnTransportManager.isClosed)
          {
            BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
          }
          else
          {
            if (IntPtr.Zero != error)
            {
              WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
              if (errorStruct.errorCode != 0 && errorStruct.errorCode != 995)
              {
                BaseClientTransportManager.tracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Got error with error code {0}. Message {1}", (object) errorStruct.errorCode, (object) errorStruct.errorDetail), new object[0]);
                TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, sessnTransportManager.wsManSessionHandle, errorStruct, TransportMethodEnum.SendShellInputEx, PSRemotingErrorId.SendExCallBackError, (object) WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail));
                sessnTransportManager.RaiseErrorHandler(eventArgs);
                return;
              }
            }
            sessnTransportManager.SendOneItem();
          }
        }
      }
    }

    private static void OnRemoteSessionDataReceived(
      IntPtr operationContext,
      int flags,
      IntPtr error,
      IntPtr shellOperationHandle,
      IntPtr commandOperationHandle,
      IntPtr operationHandle,
      IntPtr data)
    {
      BaseClientTransportManager.tracer.WriteLine("Client Session TM: OnRemoteDataReceived callback.", new object[0]);
      long sessnTMId = 0;
      WSManClientSessionTransportManager sessnTransportManager = (WSManClientSessionTransportManager) null;
      if (!WSManClientSessionTransportManager.TryGetSessionTransportManager(operationContext, out sessnTransportManager, out sessnTMId))
      {
        BaseClientTransportManager.tracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Unable to find a transport manager for context {0}.", (object) sessnTMId), new object[0]);
      }
      else
      {
        sessnTransportManager.ClearReceiveOrSendResources(flags, false);
        if (sessnTransportManager.isClosed)
          BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
        else if (!shellOperationHandle.Equals((object) sessnTransportManager.wsManShellOperationHandle))
        {
          TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(new PSRemotingTransportException(PSRemotingErrorId.ReceiveExFailed, new object[0]), TransportMethodEnum.ReceiveShellOutputEx);
          sessnTransportManager.RaiseErrorHandler(eventArgs);
        }
        else
        {
          if (IntPtr.Zero != error)
          {
            WSManNativeApi.WSManError errorStruct = WSManNativeApi.WSManError.UnMarshal(error);
            if (errorStruct.errorCode != 0)
            {
              BaseClientTransportManager.tracer.WriteLine(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Got error with error code {0}. Message {1}", (object) errorStruct.errorCode, (object) errorStruct.errorDetail), new object[0]);
              TransportErrorOccuredEventArgs eventArgs = WSManTransportManagerUtils.ConstructTransportErrorEventArgs(WSManClientSessionTransportManager.wsManApiStaticData.WSManAPIHandle, sessnTransportManager.wsManSessionHandle, errorStruct, TransportMethodEnum.ReceiveShellOutputEx, PSRemotingErrorId.ReceiveExCallBackError, (object) WSManTransportManagerUtils.ParseEscapeWSManErrorMessage(errorStruct.errorDetail));
              sessnTransportManager.RaiseErrorHandler(eventArgs);
              return;
            }
          }
          WSManNativeApi.WSManReceiveDataResult receiveDataResult = WSManNativeApi.WSManReceiveDataResult.UnMarshal(data);
          if (receiveDataResult.data == null)
            return;
          BaseClientTransportManager.tracer.WriteLine("Session Received Data : {0}", (object) receiveDataResult.data.Length);
          BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManReceiveShellOutputExCallbackReceived, PSOpcode.Receive, PSTask.None, (object) sessnTransportManager.RunspacePoolInstanceId, (object) Guid.Empty, (object) receiveDataResult.data.Length.ToString((IFormatProvider) CultureInfo.InvariantCulture));
          sessnTransportManager.ProcessRawData(receiveDataResult.data, receiveDataResult.stream);
        }
      }
    }

    private void SendOneItem()
    {
      DataPriorityType priorityType;
      byte[] data = this.dataToBeSent.ReadOrRegisterCallback(this.onDataAvailableToSendCallback, out priorityType);
      if (data == null)
        return;
      this.SendData(data, priorityType);
    }

    private void OnDataAvailableCallback(byte[] data, DataPriorityType priorityType)
    {
      BaseClientTransportManager.tracer.WriteLine("Received data to be sent from the callback.", new object[0]);
      this.SendData(data, priorityType);
    }

    private void SendData(byte[] data, DataPriorityType priorityType)
    {
      BaseClientTransportManager.tracer.WriteLine("Session sending data of size : {0}", (object) data.Length);
      byte[] data1 = data;
      bool flag = true;
      if ((object) WSManClientSessionTransportManager.sessionSendRedirect != null)
      {
        object[] objArray = new object[2]
        {
          null,
          (object) data1
        };
        flag = (bool) WSManClientSessionTransportManager.sessionSendRedirect.DynamicInvoke(objArray);
        data1 = (byte[]) objArray[0];
      }
      if (!flag)
        return;
      using (WSManNativeApi.WSManData streamData = new WSManNativeApi.WSManData(data1))
      {
        BaseTransportManager.ETWTracer.AnalyticChannel.WriteInformation(PSEventId.WSManSendShellInputEx, PSOpcode.Send, PSTask.None, (object) this.RunspacePoolInstanceId, (object) Guid.Empty, (object) streamData.BufferLength.ToString((IFormatProvider) CultureInfo.InvariantCulture));
        lock (this.syncObject)
        {
          if (this.isClosed)
          {
            BaseClientTransportManager.tracer.WriteLine("Client Session TM: Transport manager is closed. So returning", new object[0]);
          }
          else
          {
            this.sendToRemoteCompleted = new WSManNativeApi.WSManShellAsync(new IntPtr(this.sessionContextID), WSManClientSessionTransportManager.sessionSendCallback);
            WSManNativeApi.WSManSendShellInputEx(this.wsManShellOperationHandle, IntPtr.Zero, 0, priorityType == DataPriorityType.Default ? "stdin" : "pr", streamData, (IntPtr) this.sendToRemoteCompleted, ref this.wsManSendOperationHandle);
          }
        }
      }
    }

    internal override void Dispose(bool isDisposing)
    {
      BaseClientTransportManager.tracer.WriteLine("Disposing session with session context: {0} Operation Context: {1}", (object) this.sessionContextID, (object) this.wsManShellOperationHandle);
      this.CloseSessionAndClearResources();
      if (isDisposing && this.openContent != null)
      {
        this.openContent.Dispose();
        this.openContent = (WSManNativeApi.WSManData) null;
      }
      base.Dispose(isDisposing);
    }

    private void CloseSessionAndClearResources()
    {
      BaseClientTransportManager.tracer.WriteLine("Clearing session with session context: {0} Operation Context: {1}", (object) this.sessionContextID, (object) this.wsManShellOperationHandle);
      IntPtr manSessionHandle = this.wsManSessionHandle;
      this.wsManSessionHandle = IntPtr.Zero;
      object obj;
      ThreadPool.QueueUserWorkItem((WaitCallback) (state =>
      {
        obj = state;
        if (!(IntPtr.Zero != (IntPtr) obj))
          return;
        WSManNativeApi.WSManCloseSession((IntPtr) obj, 0);
      }), (object) manSessionHandle);
      WSManClientSessionTransportManager.RemoveSessionTransportManager(this.sessionContextID);
      if (this.closeSessionCompleted == null)
        return;
      this.closeSessionCompleted.Dispose();
      this.closeSessionCompleted = (WSManNativeApi.WSManShellAsync) null;
    }

    internal class WSManAPIStaticData : IDisposable
    {
      private IntPtr handle;
      private int errorCode;
      private WSManNativeApi.WSManStreamIDSet inputStreamSet;
      private WSManNativeApi.WSManStreamIDSet outputStreamSet;
      private List<WSManNativeApi.WSManOption> commonOptionSet;

      internal WSManAPIStaticData()
      {
        this.handle = IntPtr.Zero;
        this.errorCode = WSManNativeApi.WSManInitialize(0, ref this.handle);
        this.inputStreamSet = new WSManNativeApi.WSManStreamIDSet(new string[2]
        {
          "stdin",
          "pr"
        });
        this.outputStreamSet = new WSManNativeApi.WSManStreamIDSet(new string[1]
        {
          "stdout"
        });
        WSManNativeApi.WSManOption wsManOption = new WSManNativeApi.WSManOption();
        wsManOption.name = "protocolversion";
        wsManOption.value = RemotingConstants.ProtocolVersion.ToString();
        wsManOption.mustComply = true;
        this.commonOptionSet = new List<WSManNativeApi.WSManOption>();
        this.commonOptionSet.Add(wsManOption);
      }

      ~WSManAPIStaticData() => this.Dispose(false);

      internal int ErrorCode => this.errorCode;

      internal WSManNativeApi.WSManStreamIDSet InputStreamSet => this.inputStreamSet;

      internal WSManNativeApi.WSManStreamIDSet OutputStreamSet => this.outputStreamSet;

      internal List<WSManNativeApi.WSManOption> CommonOptionSet => this.commonOptionSet;

      internal IntPtr WSManAPIHandle => this.handle;

      public void Dispose()
      {
        this.Dispose(true);
        GC.SuppressFinalize((object) this);
      }

      private void Dispose(bool isDisposing)
      {
        if (isDisposing)
          return;
        this.inputStreamSet.Dispose();
        this.outputStreamSet.Dispose();
        if (!(IntPtr.Zero != this.handle))
          return;
        WSManNativeApi.WSManDeinitialize(this.handle, 0);
        this.handle = IntPtr.Zero;
      }
    }
  }
}
