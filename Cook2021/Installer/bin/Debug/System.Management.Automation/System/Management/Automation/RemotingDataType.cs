// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemotingDataType
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal enum RemotingDataType : uint
  {
    InvalidDataType = 0,
    ExceptionAsErrorRecord = 1,
    SessionCapability = 65538, // 0x00010002
    CloseSession = 65539, // 0x00010003
    CreateRunspacePool = 65540, // 0x00010004
    PublicKey = 65541, // 0x00010005
    EncryptedSessionKey = 65542, // 0x00010006
    PublicKeyRequest = 65543, // 0x00010007
    SetMaxRunspaces = 135170, // 0x00021002
    SetMinRunspaces = 135171, // 0x00021003
    RunspacePoolOperationResponse = 135172, // 0x00021004
    RunspacePoolStateInfo = 135173, // 0x00021005
    CreatePowerShell = 135174, // 0x00021006
    AvailableRunspaces = 135175, // 0x00021007
    PSEventArgs = 135176, // 0x00021008
    ApplicationPrivateData = 135177, // 0x00021009
    GetCommandMetadata = 135178, // 0x0002100A
    RemoteHostCallUsingRunspaceHost = 135424, // 0x00021100
    RemoteRunspaceHostResponseData = 135425, // 0x00021101
    PowerShellInput = 266242, // 0x00041002
    PowerShellInputEnd = 266243, // 0x00041003
    PowerShellOutput = 266244, // 0x00041004
    PowerShellErrorRecord = 266245, // 0x00041005
    PowerShellStateInfo = 266246, // 0x00041006
    PowerShellDebug = 266247, // 0x00041007
    PowerShellVerbose = 266248, // 0x00041008
    PowerShellWarning = 266249, // 0x00041009
    PowerShellProgress = 266256, // 0x00041010
    StopPowerShell = 266258, // 0x00041012
    RemoteHostCallUsingPowerShellHost = 266496, // 0x00041100
    RemotePowerShellHostResponseData = 266497, // 0x00041101
  }
}
