// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Host.ControlKeyStates
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Host
{
  [System.Flags]
  public enum ControlKeyStates
  {
    RightAltPressed = 1,
    LeftAltPressed = 2,
    RightCtrlPressed = 4,
    LeftCtrlPressed = 8,
    ShiftPressed = 16, // 0x00000010
    NumLockOn = 32, // 0x00000020
    ScrollLockOn = 64, // 0x00000040
    CapsLockOn = 128, // 0x00000080
    EnhancedKey = 256, // 0x00000100
  }
}
