// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Security.NativeConstants
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Security
{
  internal class NativeConstants
  {
    internal const int CRYPT_OID_INFO_OID_KEY = 1;
    internal const int CRYPT_OID_INFO_NAME_KEY = 2;
    internal const int CRYPT_OID_INFO_CNG_ALGID_KEY = 5;
    public const int SAFER_TOKEN_NULL_IF_EQUAL = 1;
    public const int SAFER_TOKEN_COMPARE_ONLY = 2;
    public const int SAFER_TOKEN_MAKE_INERT = 4;
    public const int SAFER_CRITERIA_IMAGEPATH = 1;
    public const int SAFER_CRITERIA_NOSIGNEDHASH = 2;
    public const int SAFER_CRITERIA_IMAGEHASH = 4;
    public const int SAFER_CRITERIA_AUTHENTICODE = 8;
    public const int SAFER_CRITERIA_URLZONE = 16;
    public const int SAFER_CRITERIA_IMAGEPATH_NT = 4096;
    public const int WTD_UI_NONE = 2;
    public const int ERROR_ACCESS_DISABLED_BY_POLICY = 1260;
    public const int ERROR_ACCESS_DISABLED_NO_SAFER_UI_BY_POLICY = 786;
    public const int SAFER_MAX_HASH_SIZE = 64;
    public const string SRP_POLICY_SCRIPT = "SCRIPT";
  }
}
