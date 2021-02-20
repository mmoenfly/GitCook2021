// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ConversionRank
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  internal enum ConversionRank
  {
    None = 0,
    UnrelatedArraysS2A = 7,
    UnrelatedArrays = 15, // 0x0000000F
    ToStringS2A = 23, // 0x00000017
    ToString = 31, // 0x0000001F
    CustomS2A = 39, // 0x00000027
    Custom = 47, // 0x0000002F
    IConvertibleS2A = 55, // 0x00000037
    IConvertible = 63, // 0x0000003F
    ImplicitCastS2A = 71, // 0x00000047
    ImplicitCast = 79, // 0x0000004F
    ExplicitCastS2A = 87, // 0x00000057
    ExplicitCast = 95, // 0x0000005F
    ConstructorS2A = 103, // 0x00000067
    Constructor = 111, // 0x0000006F
    ParseS2A = 119, // 0x00000077
    Parse = 127, // 0x0000007F
    PSObjectS2A = 135, // 0x00000087
    PSObject = 143, // 0x0000008F
    LanguageS2A = 151, // 0x00000097
    Language = 159, // 0x0000009F
    NullToValue = 175, // 0x000000AF
    NullToRef = 191, // 0x000000BF
    NumericExplicitS2A = 199, // 0x000000C7
    NumericExplicit = 207, // 0x000000CF
    NumericExplicit1S2A = 215, // 0x000000D7
    NumericExplicit1 = 223, // 0x000000DF
    NumericStringS2A = 231, // 0x000000E7
    NumericString = 239, // 0x000000EF
    NumericImplicitS2A = 247, // 0x000000F7
    NumericImplicit = 255, // 0x000000FF
    AssignableS2A = 263, // 0x00000107
    Assignable = 271, // 0x0000010F
    IdentityS2A = 279, // 0x00000117
    StringToCharArray = 282, // 0x0000011A
    Identity = 287, // 0x0000011F
    ValueDependent = 65527, // 0x0000FFF7
  }
}
