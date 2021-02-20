// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.PSOpcode
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Internal
{
  internal enum PSOpcode : byte
  {
    Open = 10, // 0x0A
    Close = 11, // 0x0B
    Connect = 12, // 0x0C
    Disconnect = 13, // 0x0D
    Negotiate = 14, // 0x0E
    Create = 15, // 0x0F
    Constructor = 16, // 0x10
    Dispose = 17, // 0x11
    EventHandler = 18, // 0x12
    Exception = 19, // 0x13
    Method = 20, // 0x14
    Send = 21, // 0x15
    Receive = 22, // 0x16
    Rehydration = 23, // 0x17
    SerializationSettings = 24, // 0x18
  }
}
