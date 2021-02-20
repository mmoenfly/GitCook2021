// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSVersionInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Management.Automation.Remoting.Client;
using System.Reflection;

namespace System.Management.Automation
{
  internal class PSVersionInfo
  {
    internal const string PSVersionTableName = "PSVersionTable";
    internal const string PSRemotingProtocolVersionName = "PSRemotingProtocolVersion";
    internal const string PSVersionName = "PSVersion";
    internal const string SerializationVersionName = "SerializationVersion";
    private static Hashtable _psVersionTable = (Hashtable) null;
    private static object lockObject = new object();
    private static Version _psV1Version = new Version(1, 0);
    private static Version _psV2Version = new Version(2, 0);

    internal static Hashtable GetPSVersionTable()
    {
      if (PSVersionInfo._psVersionTable == null)
      {
        lock (PSVersionInfo.lockObject)
        {
          if (PSVersionInfo._psVersionTable == null)
          {
            Hashtable hashtable = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);
            hashtable[(object) "PSVersion"] = (object) PSVersionInfo._psV2Version;
            hashtable[(object) "CLRVersion"] = (object) Environment.Version;
            string fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            hashtable[(object) "BuildVersion"] = (object) new Version(fileVersion);
            hashtable[(object) "PSCompatibleVersions"] = (object) new Version[2]
            {
              PSVersionInfo._psV1Version,
              PSVersionInfo._psV2Version
            };
            hashtable[(object) "SerializationVersion"] = (object) new Version("1.1.0.1");
            hashtable[(object) "PSRemotingProtocolVersion"] = (object) RemotingConstants.ProtocolVersion;
            hashtable[(object) "WSManStackVersion"] = (object) WSManNativeApi.WSMAN_STACK_VERSION;
            PSVersionInfo._psVersionTable = hashtable;
          }
        }
      }
      return PSVersionInfo._psVersionTable;
    }

    internal static Version PSVersion => (Version) PSVersionInfo.GetPSVersionTable()[(object) nameof (PSVersion)];

    internal static Version CLRVersion => (Version) PSVersionInfo.GetPSVersionTable()[(object) nameof (CLRVersion)];

    internal static Version BuildVersion => (Version) PSVersionInfo.GetPSVersionTable()[(object) nameof (BuildVersion)];

    internal static Version[] PSCompatibleVersions => (Version[]) PSVersionInfo.GetPSVersionTable()[(object) nameof (PSCompatibleVersions)];

    internal static Version SerializationVersion => (Version) PSVersionInfo.GetPSVersionTable()[(object) nameof (SerializationVersion)];

    internal static string RegistryVersionKey => "1";

    internal static string GetRegisterVersionKeyForMajorVersion(string majorVersion)
    {
      switch (majorVersion)
      {
        case "1":
        case "2":
          return "1";
        default:
          return (string) null;
      }
    }

    internal static string FeatureVersionString => string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}.{1}", (object) PSVersionInfo.PSVersion.Major, (object) PSVersionInfo.PSVersion.Minor);

    internal static bool IsValidPSVersion(Version version)
    {
      if (version.Major == PSVersionInfo._psV1Version.Major)
        return version.Minor == PSVersionInfo._psV1Version.Minor;
      return version.Major == PSVersionInfo._psV2Version.Major && version.Minor == PSVersionInfo._psV2Version.Minor;
    }
  }
}
