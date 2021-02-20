// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.Client.WSManNativeApi
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;

namespace System.Management.Automation.Remoting.Client
{
  internal static class WSManNativeApi
  {
    internal const uint INFINITE = 4294967295;
    internal const string PS_CREATION_XML_TAG = "creationXml";
    internal const string PS_CREATION_XML_NAMESPACE = "http://schemas.microsoft.com/powershell";
    internal const string WSMAN_STREAM_ID_STDOUT = "stdout";
    internal const string WSMAN_STREAM_ID_PROMPTRESPONSE = "pr";
    internal const string WSMAN_STREAM_ID_STDIN = "stdin";
    internal const string ResourceURIPrefix = "http://schemas.microsoft.com/powershell/";
    internal const int ERROR_WSMAN_REDIRECT_REQUESTED = -2144108135;
    internal const string WSManApiDll = "WsmSvc.dll";
    internal static readonly Version WSMAN_STACK_VERSION = new Version(2, 0);

    [DllImport("WsmSvc.dll", CharSet = CharSet.Unicode)]
    internal static extern int WSManInitialize(int flags, [In, Out] ref IntPtr wsManAPIHandle);

    [DllImport("WsmSvc.dll", CharSet = CharSet.Unicode)]
    internal static extern int WSManDeinitialize(IntPtr wsManAPIHandle, int flags);

    [DllImport("WsmSvc.dll", CharSet = CharSet.Unicode)]
    internal static extern int WSManCreateSession(
      IntPtr wsManAPIHandle,
      [MarshalAs(UnmanagedType.LPWStr)] string connection,
      int flags,
      IntPtr authenticationCredentials,
      IntPtr proxyInfo,
      [In, Out] ref IntPtr wsManSessionHandle);

    [DllImport("WsmSvc.dll", CharSet = CharSet.Unicode)]
    internal static extern void WSManCloseSession(IntPtr wsManSessionHandle, int flags);

    internal static int WSManSetSessionOption(
      IntPtr wsManSessionHandle,
      WSManNativeApi.WSManSessionOption option,
      WSManNativeApi.WSManDataDWord data)
    {
      WSManNativeApi.MarshalledObject marshalledObject = data.Marshal();
      using (marshalledObject)
        return WSManNativeApi.WSManSetSessionOption(wsManSessionHandle, option, marshalledObject.DataPtr);
    }

    [DllImport("WsmSvc.dll", CharSet = CharSet.Unicode)]
    internal static extern int WSManSetSessionOption(
      IntPtr wsManSessionHandle,
      WSManNativeApi.WSManSessionOption option,
      IntPtr data);

    [DllImport("WsmSvc.dll", CharSet = CharSet.Unicode)]
    internal static extern void WSManGetSessionOptionAsDword(
      IntPtr wsManSessionHandle,
      WSManNativeApi.WSManSessionOption option,
      out int value);

    internal static string WSManGetSessionOptionAsString(
      IntPtr wsManAPIHandle,
      WSManNativeApi.WSManSessionOption option)
    {
      string str = "";
      int optionLengthUsed = 0;
      if (122 != WSManNativeApi.WSManGetSessionOptionAsString(wsManAPIHandle, option, 0, (byte[]) null, out optionLengthUsed))
        return str;
      byte[] numArray = new byte[optionLengthUsed * 2];
      if (WSManNativeApi.WSManGetSessionOptionAsString(wsManAPIHandle, option, optionLengthUsed * 2, numArray, out int _) != 0)
        return str;
      try
      {
        str = Encoding.Unicode.GetString(numArray);
      }
      catch (ArgumentNullException ex)
      {
      }
      catch (DecoderFallbackException ex)
      {
      }
      return str;
    }

    [DllImport("WsmSvc.dll", CharSet = CharSet.Unicode)]
    private static extern int WSManGetSessionOptionAsString(
      IntPtr wsManSessionHandle,
      WSManNativeApi.WSManSessionOption option,
      int optionLength,
      byte[] optionAsString,
      out int optionLengthUsed);

    internal static void WSManCreateShellEx(
      IntPtr wsManSessionHandle,
      int flags,
      string resourceUri,
      WSManNativeApi.WSManShellStartupInfo startupInfo,
      WSManNativeApi.WSManOptionSet optionSet,
      WSManNativeApi.WSManData openContent,
      IntPtr asyncCallback,
      ref IntPtr shellOperationHandle)
    {
      WSManNativeApi.WSManCreateShellExInternal(wsManSessionHandle, flags, resourceUri, (IntPtr) startupInfo, (IntPtr) optionSet, (IntPtr) openContent, asyncCallback, ref shellOperationHandle);
    }

    [DllImport("WsmSvc.dll", EntryPoint = "WSManCreateShell", CharSet = CharSet.Unicode)]
    private static extern void WSManCreateShellExInternal(
      IntPtr wsManSessionHandle,
      int flags,
      [MarshalAs(UnmanagedType.LPWStr)] string resourceUri,
      IntPtr startupInfo,
      IntPtr optionSet,
      IntPtr openContent,
      IntPtr asyncCallback,
      [In, Out] ref IntPtr shellOperationHandle);

    [DllImport("WsmSvc.dll", EntryPoint = "WSManRunShellCommand", CharSet = CharSet.Unicode)]
    internal static extern void WSManRunShellCommandEx(
      IntPtr shellOperationHandle,
      int flags,
      [MarshalAs(UnmanagedType.LPWStr)] string commandLine,
      IntPtr commandArgSet,
      IntPtr optionSet,
      IntPtr asyncCallback,
      ref IntPtr commandOperationHandle);

    [DllImport("WsmSvc.dll", EntryPoint = "WSManReceiveShellOutput", CharSet = CharSet.Unicode)]
    internal static extern void WSManReceiveShellOutputEx(
      IntPtr shellOperationHandle,
      IntPtr commandOperationHandle,
      int flags,
      IntPtr desiredStreamSet,
      IntPtr asyncCallback,
      [In, Out] ref IntPtr receiveOperationHandle);

    internal static void WSManSendShellInputEx(
      IntPtr shellOperationHandle,
      IntPtr commandOperationHandle,
      int flags,
      [MarshalAs(UnmanagedType.LPWStr)] string streamId,
      WSManNativeApi.WSManData streamData,
      IntPtr asyncCallback,
      ref IntPtr sendOperationHandle)
    {
      WSManNativeApi.WSManSendShellInputExInternal(shellOperationHandle, commandOperationHandle, flags, streamId, (IntPtr) streamData, false, asyncCallback, ref sendOperationHandle);
    }

    [DllImport("WsmSvc.dll", EntryPoint = "WSManSendShellInput", CharSet = CharSet.Unicode)]
    private static extern void WSManSendShellInputExInternal(
      IntPtr shellOperationHandle,
      IntPtr commandOperationHandle,
      int flags,
      [MarshalAs(UnmanagedType.LPWStr)] string streamId,
      IntPtr streamData,
      bool endOfStream,
      IntPtr asyncCallback,
      [In, Out] ref IntPtr sendOperationHandle);

    [DllImport("WsmSvc.dll", CharSet = CharSet.Unicode)]
    internal static extern void WSManCloseShell(
      IntPtr shellHandle,
      int flags,
      IntPtr asyncCallback);

    [DllImport("WsmSvc.dll", CharSet = CharSet.Unicode)]
    internal static extern void WSManCloseCommand(
      IntPtr cmdHandle,
      int flags,
      IntPtr asyncCallback);

    [DllImport("WsmSvc.dll", EntryPoint = "WSManSignalShell", CharSet = CharSet.Unicode)]
    internal static extern void WSManSignalShellEx(
      IntPtr shellOperationHandle,
      IntPtr cmdOperationHandle,
      int flags,
      string code,
      IntPtr asyncCallback,
      [In, Out] ref IntPtr signalOperationHandle);

    [DllImport("WsmSvc.dll", CharSet = CharSet.Unicode)]
    internal static extern void WSManCloseOperation(IntPtr operationHandle, int flags);

    internal static string WSManGetErrorMessage(IntPtr wsManAPIHandle, int errorCode)
    {
      string name = Thread.CurrentThread.CurrentUICulture.Name;
      string str = "";
      int messageLengthUsed = 0;
      if (122 != WSManNativeApi.WSManGetErrorMessage(wsManAPIHandle, 0, name, errorCode, 0, (byte[]) null, out messageLengthUsed))
        return str;
      byte[] numArray = new byte[messageLengthUsed * 2];
      if (WSManNativeApi.WSManGetErrorMessage(wsManAPIHandle, 0, name, errorCode, messageLengthUsed * 2, numArray, out int _) != 0)
        return str;
      try
      {
        str = Encoding.Unicode.GetString(numArray);
      }
      catch (ArgumentNullException ex)
      {
      }
      catch (DecoderFallbackException ex)
      {
      }
      return str;
    }

    [DllImport("WsmSvc.dll", CharSet = CharSet.Unicode)]
    internal static extern int WSManGetErrorMessage(
      IntPtr wsManAPIHandle,
      int flags,
      string languageCode,
      int errorCode,
      int messageLength,
      byte[] message,
      out int messageLengthUsed);

    internal struct MarshalledObject : IDisposable
    {
      private IntPtr dataPtr;

      internal MarshalledObject(IntPtr dataPtr) => this.dataPtr = dataPtr;

      internal IntPtr DataPtr => this.dataPtr;

      internal static WSManNativeApi.MarshalledObject Create<T>(T obj)
      {
        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (T)));
        Marshal.StructureToPtr((object) obj, ptr, false);
        return new WSManNativeApi.MarshalledObject()
        {
          dataPtr = ptr
        };
      }

      public void Dispose()
      {
        if (!(IntPtr.Zero != this.dataPtr))
          return;
        Marshal.FreeHGlobal(this.dataPtr);
        this.dataPtr = IntPtr.Zero;
      }

      public static implicit operator IntPtr(WSManNativeApi.MarshalledObject obj) => obj.dataPtr;
    }

    [System.Flags]
    internal enum WSManAuthenticationMechanism
    {
      WSMAN_FLAG_DEFAULT_AUTHENTICATION = 0,
      WSMAN_FLAG_NO_AUTHENTICATION = 1,
      WSMAN_FLAG_AUTH_DIGEST = 2,
      WSMAN_FLAG_AUTH_NEGOTIATE = 4,
      WSMAN_FLAG_AUTH_BASIC = 8,
      WSMAN_FLAG_AUTH_KERBEROS = 16, // 0x00000010
      WSMAN_FLAG_AUTH_CLIENT_CERTIFICATE = 32, // 0x00000020
      WSMAN_FLAG_AUTH_CREDSSP = 128, // 0x00000080
    }

    internal abstract class BaseWSManAuthenticationCredentials : IDisposable
    {
      public abstract WSManNativeApi.MarshalledObject GetMarshalledObject();

      public void Dispose()
      {
        this.Dispose(true);
        GC.SuppressFinalize((object) this);
      }

      protected virtual void Dispose(bool isDisposing)
      {
      }
    }

    internal class WSManUserNameAuthenticationCredentials : 
      WSManNativeApi.BaseWSManAuthenticationCredentials
    {
      private WSManNativeApi.WSManUserNameAuthenticationCredentials.WSManUserNameCredentialStruct cred;
      private WSManNativeApi.MarshalledObject data;

      internal WSManUserNameAuthenticationCredentials()
      {
        this.cred = new WSManNativeApi.WSManUserNameAuthenticationCredentials.WSManUserNameCredentialStruct();
        this.data = WSManNativeApi.MarshalledObject.Create<WSManNativeApi.WSManUserNameAuthenticationCredentials.WSManUserNameCredentialStruct>(this.cred);
      }

      internal WSManUserNameAuthenticationCredentials(
        string name,
        SecureString pwd,
        WSManNativeApi.WSManAuthenticationMechanism authMechanism)
      {
        this.cred = new WSManNativeApi.WSManUserNameAuthenticationCredentials.WSManUserNameCredentialStruct();
        this.cred.authenticationMechanism = authMechanism;
        this.cred.userName = name;
        if (pwd != null)
          this.cred.password = Marshal.SecureStringToGlobalAllocUnicode(pwd);
        this.data = WSManNativeApi.MarshalledObject.Create<WSManNativeApi.WSManUserNameAuthenticationCredentials.WSManUserNameCredentialStruct>(this.cred);
      }

      internal WSManNativeApi.WSManUserNameAuthenticationCredentials.WSManUserNameCredentialStruct CredentialStruct => this.cred;

      public override WSManNativeApi.MarshalledObject GetMarshalledObject() => this.data;

      protected override void Dispose(bool isDisposing)
      {
        if (this.cred.password != IntPtr.Zero)
        {
          Marshal.ZeroFreeGlobalAllocUnicode(this.cred.password);
          this.cred.password = IntPtr.Zero;
        }
        this.data.Dispose();
      }

      internal struct WSManUserNameCredentialStruct
      {
        internal WSManNativeApi.WSManAuthenticationMechanism authenticationMechanism;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string userName;
        internal IntPtr password;
      }
    }

    internal class WSManCertificateThumbprintCredentials : 
      WSManNativeApi.BaseWSManAuthenticationCredentials
    {
      private WSManNativeApi.MarshalledObject data;

      internal WSManCertificateThumbprintCredentials(string thumbPrint) => this.data = WSManNativeApi.MarshalledObject.Create<WSManNativeApi.WSManCertificateThumbprintCredentials.WSManThumbprintStruct>(new WSManNativeApi.WSManCertificateThumbprintCredentials.WSManThumbprintStruct()
      {
        authenticationMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_CLIENT_CERTIFICATE,
        certificateThumbprint = thumbPrint,
        reserved = IntPtr.Zero
      });

      public override WSManNativeApi.MarshalledObject GetMarshalledObject() => this.data;

      protected override void Dispose(bool isDisposing) => this.data.Dispose();

      private struct WSManThumbprintStruct
      {
        internal WSManNativeApi.WSManAuthenticationMechanism authenticationMechanism;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string certificateThumbprint;
        internal IntPtr reserved;
      }
    }

    internal enum WSManSessionOption
    {
      WSMAN_OPTION_DEFAULT_OPERATION_TIMEOUTMS = 1,
      WSMAN_OPTION_TIMEOUTMS_CREATE_SHELL = 12, // 0x0000000C
      WSMAN_OPTION_TIMEOUTMS_RECEIVE_SHELL_OUTPUT = 14, // 0x0000000E
      WSMAN_OPTION_TIMEOUTMS_SEND_SHELL_INPUT = 15, // 0x0000000F
      WSMAN_OPTION_TIMEOUTMS_SIGNAL_SHELL = 16, // 0x00000010
      WSMAN_OPTION_TIMEOUTMS_CLOSE_SHELL_OPERATION = 17, // 0x00000011
      WSMAN_OPTION_SKIP_CA_CHECK = 18, // 0x00000012
      WSMAN_OPTION_SKIP_CN_CHECK = 19, // 0x00000013
      WSMAN_OPTION_UNENCRYPTED_MESSAGES = 20, // 0x00000014
      WSMAN_OPTION_UTF16 = 21, // 0x00000015
      WSMAN_OPTION_ENABLE_SPN_SERVER_PORT = 22, // 0x00000016
      WSMAN_OPTION_MACHINE_ID = 23, // 0x00000017
      WSMAN_OPTION_LOCALE = 25, // 0x00000019
      WSMAN_OPTION_UI_LANGUAGE = 26, // 0x0000001A
      WSMAN_OPTION_MAX_ENVELOPE_SIZE_KB = 28, // 0x0000001C
      WSMAN_OPTION_SHELL_MAX_DATA_SIZE_PER_MESSAGE_KB = 29, // 0x0000001D
      WSMAN_OPTION_REDIRECT_LOCATION = 30, // 0x0000001E
      WSMAN_OPTION_SKIP_REVOCATION_CHECK = 31, // 0x0000001F
      WSMAN_OPTION_ALLOW_NEGOTIATE_IMPLICIT_CREDENTIALS = 32, // 0x00000020
      WSMAN_OPTION_USE_SSL = 33, // 0x00000021
    }

    internal enum WSManShellFlag
    {
      WSMAN_FLAG_NO_COMPRESSION = 1,
    }

    internal enum WSManDataType : uint
    {
      WSMAN_DATA_NONE,
      WSMAN_DATA_TYPE_TEXT,
      WSMAN_DATA_TYPE_BINARY,
      WSMAN_DATA_TYPE_WS_XML_READER,
      WSMAN_DATA_TYPE_DWORD,
    }

    internal class WSManData : IDisposable
    {
      private WSManNativeApi.WSManData.WSManDataInternal internalData;
      private IntPtr marshalledObject = IntPtr.Zero;

      internal WSManData()
      {
      }

      internal WSManData(byte[] data)
      {
        this.internalData = new WSManNativeApi.WSManData.WSManDataInternal();
        this.internalData.binaryOrTextData = new WSManNativeApi.WSManData.WSManBinaryOrTextDataInternal();
        this.internalData.binaryOrTextData.bufferLength = data.Length;
        this.internalData.type = 2U;
        this.internalData.binaryOrTextData.data = Marshal.AllocHGlobal(this.internalData.binaryOrTextData.bufferLength);
        Marshal.Copy(data, 0, this.internalData.binaryOrTextData.data, this.internalData.binaryOrTextData.bufferLength);
        this.marshalledObject = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (WSManNativeApi.WSManData.WSManDataInternal)));
        Marshal.StructureToPtr((object) this.internalData, this.marshalledObject, false);
      }

      internal WSManData(string data)
      {
        this.internalData = new WSManNativeApi.WSManData.WSManDataInternal();
        this.internalData.binaryOrTextData = new WSManNativeApi.WSManData.WSManBinaryOrTextDataInternal();
        this.internalData.binaryOrTextData.bufferLength = data.Length;
        this.internalData.type = 1U;
        this.internalData.binaryOrTextData.data = Marshal.StringToHGlobalUni(data);
        this.marshalledObject = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (WSManNativeApi.WSManData.WSManDataInternal)));
        Marshal.StructureToPtr((object) this.internalData, this.marshalledObject, false);
      }

      ~WSManData() => this.Dispose(false);

      internal uint Type
      {
        get => this.internalData.type;
        set => this.internalData.type = value;
      }

      internal int BufferLength
      {
        get => this.internalData.binaryOrTextData.bufferLength;
        set => this.internalData.binaryOrTextData.bufferLength = value;
      }

      public void Dispose()
      {
        this.Dispose(true);
        GC.SuppressFinalize((object) this);
      }

      private void Dispose(bool isDisposing)
      {
        if (this.internalData.binaryOrTextData.data != IntPtr.Zero)
        {
          Marshal.FreeHGlobal(this.internalData.binaryOrTextData.data);
          this.internalData.binaryOrTextData.data = IntPtr.Zero;
        }
        if (!(this.marshalledObject != IntPtr.Zero))
          return;
        Marshal.FreeHGlobal(this.marshalledObject);
        this.marshalledObject = IntPtr.Zero;
      }

      public static implicit operator IntPtr(WSManNativeApi.WSManData data) => data != null ? data.marshalledObject : IntPtr.Zero;

      private struct WSManDataInternal
      {
        internal uint type;
        internal WSManNativeApi.WSManData.WSManBinaryOrTextDataInternal binaryOrTextData;
      }

      private struct WSManBinaryOrTextDataInternal
      {
        internal int bufferLength;
        internal IntPtr data;
      }
    }

    internal struct WSManDataDWord
    {
      private WSManNativeApi.WSManDataType type;
      private WSManNativeApi.WSManDataDWord.WSManDWordDataInternal dwordData;

      internal WSManDataDWord(int data)
      {
        this.dwordData = new WSManNativeApi.WSManDataDWord.WSManDWordDataInternal();
        this.dwordData.number = data;
        this.type = WSManNativeApi.WSManDataType.WSMAN_DATA_TYPE_DWORD;
      }

      internal WSManNativeApi.MarshalledObject Marshal() => WSManNativeApi.MarshalledObject.Create<WSManNativeApi.WSManDataDWord>(this);

      private struct WSManDWordDataInternal
      {
        internal int number;
        internal IntPtr reserved;
      }
    }

    internal struct WSManStreamIDSet
    {
      private WSManNativeApi.WSManStreamIDSet.WSManStreamIDSetInternal streamSetInfo;
      private WSManNativeApi.MarshalledObject data;

      internal WSManStreamIDSet(string[] streamIds)
      {
        int num = Marshal.SizeOf(typeof (IntPtr));
        this.streamSetInfo = new WSManNativeApi.WSManStreamIDSet.WSManStreamIDSetInternal();
        this.streamSetInfo.streamIDsCount = streamIds.Length;
        this.streamSetInfo.streamIDs = Marshal.AllocHGlobal(num * streamIds.Length);
        for (int index = 0; index < streamIds.Length; ++index)
        {
          IntPtr hglobalUni = Marshal.StringToHGlobalUni(streamIds[index]);
          Marshal.WriteIntPtr(this.streamSetInfo.streamIDs, index * num, hglobalUni);
        }
        this.data = WSManNativeApi.MarshalledObject.Create<WSManNativeApi.WSManStreamIDSet.WSManStreamIDSetInternal>(this.streamSetInfo);
      }

      internal void Dispose()
      {
        if (IntPtr.Zero != this.streamSetInfo.streamIDs)
        {
          int num = Marshal.SizeOf(typeof (IntPtr));
          for (int index = 0; index < this.streamSetInfo.streamIDsCount; ++index)
          {
            IntPtr zero = IntPtr.Zero;
            IntPtr hglobal = Marshal.ReadIntPtr(this.streamSetInfo.streamIDs, index * num);
            if (IntPtr.Zero != hglobal)
            {
              Marshal.FreeHGlobal(hglobal);
              zero = IntPtr.Zero;
            }
          }
          Marshal.FreeHGlobal(this.streamSetInfo.streamIDs);
          this.streamSetInfo.streamIDs = IntPtr.Zero;
        }
        this.data.Dispose();
      }

      public static implicit operator IntPtr(WSManNativeApi.WSManStreamIDSet obj) => obj.data.DataPtr;

      [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
      internal struct WSManStreamIDSetInternal
      {
        internal int streamIDsCount;
        internal IntPtr streamIDs;
      }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct WSManOption
    {
      internal const string NoProfile = "WINRS_NOPROFILE";
      internal const string CodePage = "WINRS_CODEPAGE";
      internal string name;
      internal string value;
      internal bool mustComply;
    }

    internal struct WSManOptionSet : IDisposable
    {
      private WSManNativeApi.WSManOptionSet.WSManOptionSetInternal optionSet;
      private WSManNativeApi.MarshalledObject data;

      internal WSManOptionSet(WSManNativeApi.WSManOption[] options)
      {
        int num = Marshal.SizeOf(typeof (WSManNativeApi.WSManOption));
        this.optionSet = new WSManNativeApi.WSManOptionSet.WSManOptionSetInternal();
        this.optionSet.optionsCount = options.Length;
        this.optionSet.optionsMustUnderstand = true;
        this.optionSet.options = Marshal.AllocHGlobal(num * options.Length);
        for (int index = 0; index < options.Length; ++index)
          Marshal.StructureToPtr((object) options[index], (IntPtr) (this.optionSet.options.ToInt64() + (long) (num * index)), false);
        this.data = WSManNativeApi.MarshalledObject.Create<WSManNativeApi.WSManOptionSet.WSManOptionSetInternal>(this.optionSet);
      }

      public void Dispose()
      {
        if (IntPtr.Zero != this.optionSet.options)
        {
          Marshal.FreeHGlobal(this.optionSet.options);
          this.optionSet.options = IntPtr.Zero;
        }
        this.data.Dispose();
      }

      public static implicit operator IntPtr(WSManNativeApi.WSManOptionSet optionSet) => optionSet.data.DataPtr;

      [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
      private struct WSManOptionSetInternal
      {
        internal int optionsCount;
        internal IntPtr options;
        internal bool optionsMustUnderstand;
      }
    }

    internal struct WSManCommandArgSet : IDisposable
    {
      private WSManNativeApi.WSManCommandArgSet.WSManCommandArgSetInternal internalData;
      private WSManNativeApi.MarshalledObject data;

      internal WSManCommandArgSet(byte[] firstArgument)
      {
        this.internalData = new WSManNativeApi.WSManCommandArgSet.WSManCommandArgSetInternal();
        this.internalData.argsCount = 1;
        this.internalData.args = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (IntPtr)));
        Marshal.WriteIntPtr(this.internalData.args, Marshal.StringToHGlobalUni(Convert.ToBase64String(firstArgument, Base64FormattingOptions.None)));
        this.data = WSManNativeApi.MarshalledObject.Create<WSManNativeApi.WSManCommandArgSet.WSManCommandArgSetInternal>(this.internalData);
      }

      public void Dispose()
      {
        IntPtr hglobal = Marshal.ReadIntPtr(this.internalData.args);
        if (IntPtr.Zero != hglobal)
          Marshal.FreeHGlobal(hglobal);
        Marshal.FreeHGlobal(this.internalData.args);
        this.data.Dispose();
      }

      public static implicit operator IntPtr(WSManNativeApi.WSManCommandArgSet obj) => obj.data.DataPtr;

      internal struct WSManCommandArgSetInternal
      {
        internal int argsCount;
        internal IntPtr args;
      }
    }

    internal struct WSManShellStartupInfo : IDisposable
    {
      private WSManNativeApi.WSManShellStartupInfo.WSManShellStartupInfoInternal internalInfo;
      internal WSManNativeApi.MarshalledObject data;

      internal WSManShellStartupInfo(
        WSManNativeApi.WSManStreamIDSet inputStreamSet,
        WSManNativeApi.WSManStreamIDSet outputStreamSet,
        uint serverIdleTimeOut)
      {
        this.internalInfo = new WSManNativeApi.WSManShellStartupInfo.WSManShellStartupInfoInternal();
        this.internalInfo.inputStreamSet = (IntPtr) inputStreamSet;
        this.internalInfo.outputStreamSet = (IntPtr) outputStreamSet;
        this.internalInfo.idleTimeoutMs = serverIdleTimeOut;
        this.internalInfo.workingDirectory = (string) null;
        this.internalInfo.environmentVariableSet = IntPtr.Zero;
        this.data = WSManNativeApi.MarshalledObject.Create<WSManNativeApi.WSManShellStartupInfo.WSManShellStartupInfoInternal>(this.internalInfo);
      }

      public void Dispose() => this.data.Dispose();

      public static implicit operator IntPtr(WSManNativeApi.WSManShellStartupInfo startupInfo) => startupInfo.data.DataPtr;

      [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
      private struct WSManShellStartupInfoInternal
      {
        internal IntPtr inputStreamSet;
        internal IntPtr outputStreamSet;
        internal uint idleTimeoutMs;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string workingDirectory;
        internal IntPtr environmentVariableSet;
      }
    }

    internal class WSManProxyInfo : IDisposable
    {
      private WSManNativeApi.MarshalledObject data;

      internal WSManProxyInfo(
        ProxyAccessType proxyAccessType,
        WSManNativeApi.WSManUserNameAuthenticationCredentials authCredentials)
      {
        WSManNativeApi.WSManProxyInfo.WSManProxyInfoInternal proxyInfoInternal = new WSManNativeApi.WSManProxyInfo.WSManProxyInfoInternal()
        {
          proxyAccessType = (int) proxyAccessType,
          proxyAuthCredentialsStruct = new WSManNativeApi.WSManUserNameAuthenticationCredentials.WSManUserNameCredentialStruct()
        };
        proxyInfoInternal.proxyAuthCredentialsStruct.authenticationMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_DEFAULT_AUTHENTICATION;
        if (authCredentials != null)
          proxyInfoInternal.proxyAuthCredentialsStruct = authCredentials.CredentialStruct;
        this.data = WSManNativeApi.MarshalledObject.Create<WSManNativeApi.WSManProxyInfo.WSManProxyInfoInternal>(proxyInfoInternal);
      }

      public void Dispose()
      {
        this.data.Dispose();
        GC.SuppressFinalize((object) this);
      }

      public static implicit operator IntPtr(WSManNativeApi.WSManProxyInfo proxyInfo) => proxyInfo.data.DataPtr;

      [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
      private struct WSManProxyInfoInternal
      {
        public int proxyAccessType;
        public WSManNativeApi.WSManUserNameAuthenticationCredentials.WSManUserNameCredentialStruct proxyAuthCredentialsStruct;
      }
    }

    internal enum WSManCallbackFlags
    {
      WSMAN_FLAG_CALLBACK_END_OF_OPERATION = 1,
      WSMAN_FLAG_CALLBACK_END_OF_STREAM = 8,
    }

    internal delegate void WSManShellCompletionFunction(
      IntPtr operationContext,
      int flags,
      IntPtr error,
      IntPtr shellOperationHandle,
      IntPtr commandOperationHandle,
      IntPtr operationHandle,
      IntPtr data);

    internal struct WSManShellAsyncCallback
    {
      private GCHandle gcHandle;
      private IntPtr asyncCallback;

      internal WSManShellAsyncCallback(
        WSManNativeApi.WSManShellCompletionFunction callback)
      {
        this.gcHandle = GCHandle.Alloc((object) callback);
        this.asyncCallback = Marshal.GetFunctionPointerForDelegate((Delegate) callback);
      }

      public static implicit operator IntPtr(WSManNativeApi.WSManShellAsyncCallback callback) => callback.asyncCallback;
    }

    internal class WSManShellAsync
    {
      private WSManNativeApi.MarshalledObject data;
      private WSManNativeApi.WSManShellAsync.WSManShellAsyncInternal internalData;

      internal WSManShellAsync(IntPtr context, WSManNativeApi.WSManShellAsyncCallback callback)
      {
        this.internalData = new WSManNativeApi.WSManShellAsync.WSManShellAsyncInternal();
        this.internalData.operationContext = context;
        this.internalData.asyncCallback = (IntPtr) callback;
        this.data = WSManNativeApi.MarshalledObject.Create<WSManNativeApi.WSManShellAsync.WSManShellAsyncInternal>(this.internalData);
      }

      public void Dispose() => this.data.Dispose();

      public static implicit operator IntPtr(WSManNativeApi.WSManShellAsync async) => (IntPtr) async.data;

      internal struct WSManShellAsyncInternal
      {
        internal IntPtr operationContext;
        internal IntPtr asyncCallback;
      }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct WSManError
    {
      internal int errorCode;
      internal string errorDetail;
      internal string language;
      internal string machineName;

      internal static WSManNativeApi.WSManError UnMarshal(IntPtr unmanagedData) => (WSManNativeApi.WSManError) Marshal.PtrToStructure(unmanagedData, typeof (WSManNativeApi.WSManError));
    }

    internal class WSManReceiveDataResult
    {
      internal byte[] data;
      internal string stream;

      internal static WSManNativeApi.WSManReceiveDataResult UnMarshal(
        IntPtr unmanagedData)
      {
        WSManNativeApi.WSManReceiveDataResult.WSManReceiveDataResultInternal structure = (WSManNativeApi.WSManReceiveDataResult.WSManReceiveDataResultInternal) Marshal.PtrToStructure(unmanagedData, typeof (WSManNativeApi.WSManReceiveDataResult.WSManReceiveDataResultInternal));
        byte[] destination = (byte[]) null;
        if (structure.data.binaryData.bufferLength > 0)
        {
          destination = new byte[structure.data.binaryData.bufferLength];
          Marshal.Copy(structure.data.binaryData.buffer, destination, 0, structure.data.binaryData.bufferLength);
        }
        return new WSManNativeApi.WSManReceiveDataResult()
        {
          data = destination,
          stream = structure.streamId
        };
      }

      private struct WSManReceiveDataResultInternal
      {
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string streamId;
        internal WSManNativeApi.WSManReceiveDataResult.WSManDataInternal data;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string commandState;
        internal int exitCode;
      }

      private struct WSManDataInternal
      {
        internal uint type;
        internal WSManNativeApi.WSManReceiveDataResult.WSManBinaryDataInternal binaryData;
      }

      private struct WSManBinaryDataInternal
      {
        internal int bufferLength;
        internal IntPtr buffer;
      }
    }
  }
}
