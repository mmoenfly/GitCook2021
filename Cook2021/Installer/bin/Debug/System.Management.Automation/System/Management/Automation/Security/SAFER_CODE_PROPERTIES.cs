// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Security.SAFER_CODE_PROPERTIES
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.InteropServices;

namespace System.Management.Automation.Security
{
  internal struct SAFER_CODE_PROPERTIES
  {
    public uint cbSize;
    public uint dwCheckFlags;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string ImagePath;
    public IntPtr hImageFileHandle;
    public uint UrlZoneId;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64, ArraySubType = UnmanagedType.I1)]
    public byte[] ImageHash;
    public uint dwImageHashSize;
    public LARGE_INTEGER ImageSize;
    public uint HashAlgorithm;
    public IntPtr pByteBlock;
    public IntPtr hWndParent;
    public uint dwWVTUIChoice;
  }
}
