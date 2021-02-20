// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.RemoteHostMethodId
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Remoting
{
  internal enum RemoteHostMethodId
  {
    GetName = 1,
    GetVersion = 2,
    GetInstanceId = 3,
    GetCurrentCulture = 4,
    GetCurrentUICulture = 5,
    SetShouldExit = 6,
    EnterNestedPrompt = 7,
    ExitNestedPrompt = 8,
    NotifyBeginApplication = 9,
    NotifyEndApplication = 10, // 0x0000000A
    ReadLine = 11, // 0x0000000B
    ReadLineAsSecureString = 12, // 0x0000000C
    Write1 = 13, // 0x0000000D
    Write2 = 14, // 0x0000000E
    WriteLine1 = 15, // 0x0000000F
    WriteLine2 = 16, // 0x00000010
    WriteLine3 = 17, // 0x00000011
    WriteErrorLine = 18, // 0x00000012
    WriteDebugLine = 19, // 0x00000013
    WriteProgress = 20, // 0x00000014
    WriteVerboseLine = 21, // 0x00000015
    WriteWarningLine = 22, // 0x00000016
    Prompt = 23, // 0x00000017
    PromptForCredential1 = 24, // 0x00000018
    PromptForCredential2 = 25, // 0x00000019
    PromptForChoice = 26, // 0x0000001A
    GetForegroundColor = 27, // 0x0000001B
    SetForegroundColor = 28, // 0x0000001C
    GetBackgroundColor = 29, // 0x0000001D
    SetBackgroundColor = 30, // 0x0000001E
    GetCursorPosition = 31, // 0x0000001F
    SetCursorPosition = 32, // 0x00000020
    GetWindowPosition = 33, // 0x00000021
    SetWindowPosition = 34, // 0x00000022
    GetCursorSize = 35, // 0x00000023
    SetCursorSize = 36, // 0x00000024
    GetBufferSize = 37, // 0x00000025
    SetBufferSize = 38, // 0x00000026
    GetWindowSize = 39, // 0x00000027
    SetWindowSize = 40, // 0x00000028
    GetWindowTitle = 41, // 0x00000029
    SetWindowTitle = 42, // 0x0000002A
    GetMaxWindowSize = 43, // 0x0000002B
    GetMaxPhysicalWindowSize = 44, // 0x0000002C
    GetKeyAvailable = 45, // 0x0000002D
    ReadKey = 46, // 0x0000002E
    FlushInputBuffer = 47, // 0x0000002F
    SetBufferContents1 = 48, // 0x00000030
    SetBufferContents2 = 49, // 0x00000031
    GetBufferContents = 50, // 0x00000032
    ScrollBufferContents = 51, // 0x00000033
    PushRunspace = 52, // 0x00000034
    PopRunspace = 53, // 0x00000035
    GetIsRunspacePushed = 54, // 0x00000036
    GetRunspace = 55, // 0x00000037
    PromptForChoiceMultipleSelection = 56, // 0x00000038
  }
}
