// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSEventId
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Internal
{
  internal enum PSEventId
  {
    HostNameResolve = 4097, // 0x00001001
    SchemeResolve = 4098, // 0x00001002
    ShellResolve = 4099, // 0x00001003
    RunspaceConstructor = 8193, // 0x00002001
    RunspacePoolConstructor = 8194, // 0x00002002
    RunspacePoolOpen = 8195, // 0x00002003
    OperationalTransferEventRunspacePool = 8196, // 0x00002004
    Port = 12033, // 0x00002F01
    AppName = 12034, // 0x00002F02
    ComputerName = 12035, // 0x00002F03
    Scheme = 12036, // 0x00002F04
    TestAnalytic = 12037, // 0x00002F05
    WSManConnectionInfoDump = 12038, // 0x00002F06
    AnalyticTransferEventRunspacePool = 12039, // 0x00002F07
    Serializer_RehydrationSuccess = 28673, // 0x00007001
    Serializer_RehydrationFailure = 28674, // 0x00007002
    Serializer_DepthOverride = 28675, // 0x00007003
    Serializer_ModeOverride = 28676, // 0x00007004
    Serializer_ScriptPropertyWithoutRunspace = 28677, // 0x00007005
    Serializer_PropertyGetterFailed = 28678, // 0x00007006
    Serializer_EnumerationFailed = 28679, // 0x00007007
    Serializer_ToStringFailed = 28680, // 0x00007008
    Serializer_MaxDepthWhenSerializing = 28682, // 0x0000700A
    Serializer_XmlExceptionWhenDeserializing = 28683, // 0x0000700B
    Serializer_SpecificPropertyMissing = 28684, // 0x0000700C
    TransportReceivedObject = 32769, // 0x00008001
    TransportSendingData = 32770, // 0x00008002
    TransportReceivedData = 32771, // 0x00008003
    AppDomainUnhandledException_Analytic = 32775, // 0x00008007
    TransportError_Analytic = 32776, // 0x00008008
    AppDomainUnhandledException = 32777, // 0x00008009
    TransportError = 32784, // 0x00008010
    WSManCreateShell = 32785, // 0x00008011
    WSManCreateShellCallbackReceived = 32786, // 0x00008012
    WSManCloseShell = 32787, // 0x00008013
    WSManCloseShellCallbackReceived = 32788, // 0x00008014
    WSManSendShellInputEx = 32789, // 0x00008015
    WSManSendShellInputExCallbackReceived = 32790, // 0x00008016
    WSManReceiveShellOutputEx = 32791, // 0x00008017
    WSManReceiveShellOutputExCallbackReceived = 32792, // 0x00008018
    WSManCreateCommand = 32793, // 0x00008019
    WSManCreateCommandCallbackReceived = 32800, // 0x00008020
    WSManCloseCommand = 32801, // 0x00008021
    WSManCloseCommandCallbackReceived = 32802, // 0x00008022
    WSManSignal = 32803, // 0x00008023
    WSManSignalCallbackReceived = 32804, // 0x00008024
    URIRedirection = 32805, // 0x00008025
    ServerSendData = 32849, // 0x00008051
    ServerCreateRemoteSession = 32850, // 0x00008052
    ReportContext = 32851, // 0x00008053
    ReportOperationComplete = 32852, // 0x00008054
    ServerCreateCommandSession = 32853, // 0x00008055
    ServerStopCommand = 32854, // 0x00008056
    ServerReceivedData = 32855, // 0x00008057
    ServerClientReceiveRequest = 32856, // 0x00008058
    ServerCloseOperation = 32857, // 0x00008059
    LoadingPSCustomShellAssembly = 32865, // 0x00008061
    LoadingPSCustomShellType = 32866, // 0x00008062
    ReceivedRemotingFragment = 32867, // 0x00008063
    SentRemotingFragment = 32868, // 0x00008064
  }
}
