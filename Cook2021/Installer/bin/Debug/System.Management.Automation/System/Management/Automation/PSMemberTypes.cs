// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSMemberTypes
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.ComponentModel;

namespace System.Management.Automation
{
  [System.Flags]
  [TypeConverter(typeof (LanguagePrimitives.EnumMultipleTypeConverter))]
  public enum PSMemberTypes
  {
    AliasProperty = 1,
    CodeProperty = 2,
    Property = 4,
    NoteProperty = 8,
    ScriptProperty = 16, // 0x00000010
    PropertySet = 32, // 0x00000020
    Method = 64, // 0x00000040
    CodeMethod = 128, // 0x00000080
    ScriptMethod = 256, // 0x00000100
    ParameterizedProperty = 512, // 0x00000200
    MemberSet = 1024, // 0x00000400
    Event = 2048, // 0x00000800
    Properties = ScriptProperty | NoteProperty | Property | CodeProperty | AliasProperty, // 0x0000001F
    Methods = ScriptMethod | CodeMethod | Method, // 0x000001C0
    All = Methods | Properties | Event | MemberSet | ParameterizedProperty | PropertySet, // 0x00000FFF
  }
}
