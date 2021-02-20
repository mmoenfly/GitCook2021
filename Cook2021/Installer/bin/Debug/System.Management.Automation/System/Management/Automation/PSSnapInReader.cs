// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSSnapInReader
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Text;

namespace System.Management.Automation
{
  internal static class PSSnapInReader
  {
    private static readonly PSSnapInInfo[] _junk = new PSSnapInInfo[1]
    {
      new PSSnapInInfo("1", false, "3", "4", "5", new Version(), (Version) null, (Collection<string>) null, (Collection<string>) null, (string) null, (string) null, (string) null)
    };
    private static readonly PSSnapInReader.DefaultPSSnapInInformation[] _defaultMshSnapins = new PSSnapInReader.DefaultPSSnapInInformation[7]
    {
      new PSSnapInReader.DefaultPSSnapInInformation("Microsoft.PowerShell.Diagnostics", "Microsoft.PowerShell.Commands.Diagnostics", (string) null, "GetEventResources,Description", "GetEventResources,Vendor"),
      new PSSnapInReader.DefaultPSSnapInInformation("Microsoft.WSMan.Management", "Microsoft.WSMan.Management", (string) null, "WsManResources,Description", "WsManResources,Vendor"),
      new PSSnapInReader.DefaultPSSnapInInformation("Microsoft.PowerShell.Core", "System.Management.Automation", (string) null, "CoreMshSnapInResources,Description", "CoreMshSnapInResources,Vendor"),
      new PSSnapInReader.DefaultPSSnapInInformation("Microsoft.PowerShell.Utility", "Microsoft.PowerShell.Commands.Utility", (string) null, "UtilityMshSnapInResources,Description", "UtilityMshSnapInResources,Vendor"),
      new PSSnapInReader.DefaultPSSnapInInformation("Microsoft.PowerShell.Host", "Microsoft.PowerShell.ConsoleHost", (string) null, "HostMshSnapInResources,Description", "HostMshSnapInResources,Vendor"),
      new PSSnapInReader.DefaultPSSnapInInformation("Microsoft.PowerShell.Management", "Microsoft.PowerShell.Commands.Management", (string) null, "ManagementMshSnapInResources,Description", "ManagementMshSnapInResources,Vendor"),
      new PSSnapInReader.DefaultPSSnapInInformation("Microsoft.PowerShell.Security", "Microsoft.PowerShell.Security", (string) null, "SecurityMshSnapInResources,Description", "SecurityMshSnapInResources,Vendor")
    };
    [TraceSource("PSSnapInReader", "PSSnapInReader")]
    private static PSTraceSource _tracer = PSTraceSource.GetTracer(nameof (PSSnapInReader), nameof (PSSnapInReader));
    private static PSTraceSource _mshsnapinTracer = PSTraceSource.GetTracer("MshSnapinLoadUnload", "Loading and unloading mshsnapins", false);

    internal static Collection<PSSnapInInfo> ReadAll()
    {
      using (PSSnapInReader._tracer.TraceMethod())
      {
        Collection<PSSnapInInfo> collection1 = new Collection<PSSnapInInfo>();
        RegistryKey monadRootKey = PSSnapInReader.GetMonadRootKey();
        string[] subKeyNames = monadRootKey.GetSubKeyNames();
        if (subKeyNames == null)
          return collection1;
        foreach (string str in subKeyNames)
        {
          if (!string.IsNullOrEmpty(str))
          {
            if (!PSSnapInReader.MeetsVersionFormat(str))
            {
              PSSnapInReader._tracer.WriteLine("Found key {0} which doesn't meet version format", (object) str);
            }
            else
            {
              Collection<PSSnapInInfo> collection2 = (Collection<PSSnapInInfo>) null;
              try
              {
                collection2 = PSSnapInReader.ReadAll(monadRootKey, str);
              }
              catch (SecurityException ex)
              {
                PSSnapInReader._tracer.TraceException((Exception) ex);
              }
              catch (ArgumentException ex)
              {
                PSSnapInReader._tracer.TraceException((Exception) ex);
              }
              if (collection2 != null)
              {
                foreach (PSSnapInInfo psSnapInInfo in collection2)
                  collection1.Add(psSnapInInfo);
              }
            }
          }
        }
        return collection1;
      }
    }

    private static bool MeetsVersionFormat(string version)
    {
      using (PSSnapInReader._tracer.TraceMethod(version, new object[0]))
      {
        bool flag = true;
        try
        {
          LanguagePrimitives.ConvertTo((object) version, typeof (int), (IFormatProvider) CultureInfo.InvariantCulture);
        }
        catch (PSInvalidCastException ex)
        {
          flag = false;
        }
        return flag;
      }
    }

    internal static Collection<PSSnapInInfo> ReadAll(string psVersion)
    {
      using (PSSnapInReader._tracer.TraceMethod())
        return !string.IsNullOrEmpty(psVersion) ? PSSnapInReader.ReadAll(PSSnapInReader.GetMonadRootKey(), psVersion) : throw PSSnapInReader._tracer.NewArgumentNullException(nameof (psVersion));
    }

    private static Collection<PSSnapInInfo> ReadAll(
      RegistryKey monadRootKey,
      string psVersion)
    {
      using (PSSnapInReader._tracer.TraceMethod(psVersion, new object[0]))
      {
        Collection<PSSnapInInfo> collection = new Collection<PSSnapInInfo>();
        RegistryKey mshSnapinRootKey = PSSnapInReader.GetMshSnapinRootKey(PSSnapInReader.GetVersionRootKey(monadRootKey, psVersion), psVersion);
        foreach (string subKeyName in mshSnapinRootKey.GetSubKeyNames())
        {
          if (!string.IsNullOrEmpty(subKeyName))
          {
            try
            {
              collection.Add(PSSnapInReader.ReadOne(mshSnapinRootKey, subKeyName));
            }
            catch (SecurityException ex)
            {
              PSSnapInReader._tracer.TraceException((Exception) ex);
            }
            catch (ArgumentException ex)
            {
              PSSnapInReader._tracer.TraceException((Exception) ex);
            }
          }
        }
        return collection;
      }
    }

    internal static PSSnapInInfo Read(string psVersion, string mshsnapinId)
    {
      using (PSSnapInReader._tracer.TraceMethod("psVersion: {0}, mshsnapinId: {1}", (object) psVersion, (object) mshsnapinId))
      {
        if (string.IsNullOrEmpty(psVersion))
          throw PSSnapInReader._tracer.NewArgumentNullException(nameof (psVersion));
        if (string.IsNullOrEmpty(mshsnapinId))
          throw PSSnapInReader._tracer.NewArgumentNullException(nameof (mshsnapinId));
        PSSnapInInfo.VerifyPSSnapInFormatThrowIfError(mshsnapinId);
        return PSSnapInReader.ReadOne(PSSnapInReader.GetMshSnapinRootKey(PSSnapInReader.GetVersionRootKey(PSSnapInReader.GetMonadRootKey(), psVersion), psVersion), mshsnapinId);
      }
    }

    private static PSSnapInInfo ReadOne(RegistryKey mshSnapInRoot, string mshsnapinId)
    {
      using (PSSnapInReader._tracer.TraceMethod(mshsnapinId, new object[0]))
      {
        RegistryKey mshsnapinKey = mshSnapInRoot.OpenSubKey(mshsnapinId);
        if (mshsnapinKey == null)
        {
          PSSnapInReader._mshsnapinTracer.TraceError("Error opening registry key {0}\\{1}.", (object) mshSnapInRoot.Name, (object) mshsnapinId);
          throw PSSnapInReader._tracer.NewArgumentException(nameof (mshsnapinId), "MshSnapinInfo", "MshSnapinDoesNotExist", (object) mshsnapinId);
        }
        string applicationBase = PSSnapInReader.ReadStringValue(mshsnapinKey, "ApplicationBase", true);
        string assemblyName = PSSnapInReader.ReadStringValue(mshsnapinKey, "AssemblyName", true);
        string moduleName = PSSnapInReader.ReadStringValue(mshsnapinKey, "ModuleName", true);
        Version psVersion = PSSnapInReader.ReadVersionValue(mshsnapinKey, "PowerShellVersion", true);
        Version version = PSSnapInReader.ReadVersionValue(mshsnapinKey, "Version", false);
        string descriptionFallback = PSSnapInReader.ReadStringValue(mshsnapinKey, "Description", false);
        if (descriptionFallback == null)
        {
          PSSnapInReader._mshsnapinTracer.WriteLine("No description is specified for mshsnapin {0}. Using empty string for description.", (object) mshsnapinId);
          descriptionFallback = string.Empty;
        }
        string vendorFallback = PSSnapInReader.ReadStringValue(mshsnapinKey, "Vendor", false);
        if (vendorFallback == null)
        {
          PSSnapInReader._mshsnapinTracer.WriteLine("No vendor is specified for mshsnapin {0}. Using empty string for description.", (object) mshsnapinId);
          vendorFallback = string.Empty;
        }
        bool flag = false;
        string strB = PSSnapInReader.ReadStringValue(mshsnapinKey, "LogPipelineExecutionDetails", false);
        if (!string.IsNullOrEmpty(strB) && string.Compare("1", strB, StringComparison.OrdinalIgnoreCase) == 0)
          flag = true;
        string customPSSnapInType = PSSnapInReader.ReadStringValue(mshsnapinKey, "CustomPSSnapInType", false);
        if (string.IsNullOrEmpty(customPSSnapInType))
          customPSSnapInType = (string) null;
        Collection<string> types = PSSnapInReader.ReadMultiStringValue(mshsnapinKey, "Types", false);
        Collection<string> formats = PSSnapInReader.ReadMultiStringValue(mshsnapinKey, "Formats", false);
        PSSnapInReader._mshsnapinTracer.WriteLine("Successfully read registry values for mshsnapin {0}. Constructing PSSnapInInfo object.", (object) mshsnapinId);
        return new PSSnapInInfo(mshsnapinId, false, applicationBase, assemblyName, moduleName, psVersion, version, types, formats, descriptionFallback, vendorFallback, customPSSnapInType)
        {
          LogPipelineExecutionDetails = flag
        };
      }
    }

    private static Collection<string> ReadMultiStringValue(
      RegistryKey mshsnapinKey,
      string name,
      bool mandatory)
    {
      using (PSSnapInReader._tracer.TraceMethod())
      {
        object obj = mshsnapinKey.GetValue(name);
        if (obj == null)
        {
          if (mandatory)
          {
            PSSnapInReader._mshsnapinTracer.TraceError("Mandatory property {0} not specified for registry key {1}", (object) name, (object) mshsnapinKey.Name);
            throw PSSnapInReader._tracer.NewArgumentException(nameof (name), "MshSnapinInfo", "MandatoryValueNotPresent", (object) name, (object) mshsnapinKey.Name);
          }
          return (Collection<string>) null;
        }
        if (!(obj is string[] strArray) && obj is string str)
          strArray = new string[1]{ str };
        if (strArray == null)
        {
          if (mandatory)
          {
            PSSnapInReader._mshsnapinTracer.TraceError("Cannot get string/multi-string value for mandatory property {0} in registry key {1}", (object) name, (object) mshsnapinKey.Name);
            throw PSSnapInReader._tracer.NewArgumentException(nameof (name), "MshSnapinInfo", "MandatoryValueNotInCorrectFormatMultiString", (object) name, (object) mshsnapinKey.Name);
          }
          return (Collection<string>) null;
        }
        PSSnapInReader._mshsnapinTracer.WriteLine("Successfully read property {0} from {1}", (object) name, (object) mshsnapinKey.Name);
        return new Collection<string>((IList<string>) strArray);
      }
    }

    internal static string ReadStringValue(RegistryKey mshsnapinKey, string name, bool mandatory)
    {
      using (PSSnapInReader._tracer.TraceMethod(name, new object[0]))
      {
        object obj = mshsnapinKey.GetValue(name);
        if (obj == null && mandatory)
        {
          PSSnapInReader._mshsnapinTracer.TraceError("Mandatory property {0} not specified for registry key {1}", (object) name, (object) mshsnapinKey.Name);
          throw PSSnapInReader._tracer.NewArgumentException(nameof (name), "MshSnapinInfo", "MandatoryValueNotPresent", (object) name, (object) mshsnapinKey.Name);
        }
        string str = obj as string;
        if (string.IsNullOrEmpty(str) && mandatory)
        {
          PSSnapInReader._mshsnapinTracer.TraceError("Value is null or empty for mandatory property {0} in {1}", (object) name, (object) mshsnapinKey.Name);
          throw PSSnapInReader._tracer.NewArgumentException(nameof (name), "MshSnapinInfo", "MandatoryValueNotInCorrectFormat", (object) name, (object) mshsnapinKey.Name);
        }
        PSSnapInReader._mshsnapinTracer.WriteLine("Successfully read value {0} for property {1} from {2}", (object) str, (object) name, (object) mshsnapinKey.Name);
        return str;
      }
    }

    internal static Version ReadVersionValue(
      RegistryKey mshsnapinKey,
      string name,
      bool mandatory)
    {
      using (PSSnapInReader._tracer.TraceMethod(name, new object[0]))
      {
        string version1 = PSSnapInReader.ReadStringValue(mshsnapinKey, name, mandatory);
        if (version1 == null)
        {
          PSSnapInReader._mshsnapinTracer.TraceError("Cannot read value for property {0} in registry key {1}", (object) name, (object) mshsnapinKey.ToString());
          return (Version) null;
        }
        Version version2;
        try
        {
          version2 = new Version(version1);
        }
        catch (ArgumentOutOfRangeException ex)
        {
          PSSnapInReader._mshsnapinTracer.TraceError("Cannot convert value {0} to version format", (object) version1);
          PSSnapInReader._tracer.TraceException((Exception) ex);
          throw PSSnapInReader._tracer.NewArgumentException(nameof (name), "MshSnapinInfo", "VersionValueInCorrect", (object) name, (object) mshsnapinKey.Name);
        }
        catch (ArgumentException ex)
        {
          PSSnapInReader._mshsnapinTracer.TraceError("Cannot convert value {0} to version format", (object) version1);
          PSSnapInReader._tracer.TraceException((Exception) ex);
          throw PSSnapInReader._tracer.NewArgumentException(nameof (name), "MshSnapinInfo", "VersionValueInCorrect", (object) name, (object) mshsnapinKey.Name);
        }
        catch (OverflowException ex)
        {
          PSSnapInReader._mshsnapinTracer.TraceError("Cannot convert value {0} to version format", (object) version1);
          PSSnapInReader._tracer.TraceException((Exception) ex);
          throw PSSnapInReader._tracer.NewArgumentException(nameof (name), "MshSnapinInfo", "VersionValueInCorrect", (object) name, (object) mshsnapinKey.Name);
        }
        catch (FormatException ex)
        {
          PSSnapInReader._mshsnapinTracer.TraceError("Cannot convert value {0} to version format", (object) version1);
          PSSnapInReader._tracer.TraceException((Exception) ex);
          throw PSSnapInReader._tracer.NewArgumentException(nameof (name), "MshSnapinInfo", "VersionValueInCorrect", (object) name, (object) mshsnapinKey.Name);
        }
        PSSnapInReader._mshsnapinTracer.WriteLine("Successfully converted string {0} to version format.", (object) version2.ToString());
        return version2;
      }
    }

    private static string ConvertByteArrayToString(byte[] tokens)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (byte token in tokens)
        stringBuilder.AppendFormat("{0:x2}", (object) token);
      return stringBuilder.ToString();
    }

    internal static Collection<PSSnapInInfo> ReadEnginePSSnapIns()
    {
      string str1 = PSSnapInReader.ReadStringValue(PSSnapInReader.GetPSEngineKey(PSVersionInfo.RegistryVersionKey), "ApplicationBase", true);
      Version psVersion = PSVersionInfo.PSVersion;
      Assembly executingAssembly = Assembly.GetExecutingAssembly();
      Version version = executingAssembly.GetName().Version;
      byte[] publicKeyToken = executingAssembly.GetName().GetPublicKeyToken();
      string str2 = publicKeyToken.Length != 0 ? PSSnapInReader.ConvertByteArrayToString(publicKeyToken) : throw PSSnapInReader._tracer.NewArgumentException("PublicKeyToken", "MshSnapinInfo", "PublicKeyTokenAccessFailed");
      string str3 = "neutral";
      string str4 = executingAssembly.GetName().ProcessorArchitecture.ToString();
      Collection<string> collection1 = new Collection<string>((IList<string>) new string[7]
      {
        "Certificate.format.ps1xml",
        "DotNetTypes.format.ps1xml",
        "FileSystem.format.ps1xml",
        "Help.format.ps1xml",
        "PowerShellCore.format.ps1xml",
        "PowerShellTrace.format.ps1xml",
        "Registry.format.ps1xml"
      });
      Collection<string> collection2 = new Collection<string>((IList<string>) new string[1]
      {
        "types.ps1xml"
      });
      Collection<PSSnapInInfo> collection3 = new Collection<PSSnapInInfo>();
      for (int index = 0; index < PSSnapInReader._defaultMshSnapins.Length; ++index)
      {
        PSSnapInReader.DefaultPSSnapInInformation defaultMshSnapin = PSSnapInReader._defaultMshSnapins[index];
        string assemblyName = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}, Version={1}, Culture={2}, PublicKeyToken={3}, ProcessorArchitecture={4}", (object) defaultMshSnapin.AssemblyName, (object) version.ToString(), (object) str3, (object) str2, (object) str4);
        Collection<string> formats = (Collection<string>) null;
        Collection<string> types = (Collection<string>) null;
        if (defaultMshSnapin.AssemblyName.Equals("System.Management.Automation", StringComparison.OrdinalIgnoreCase))
        {
          formats = collection1;
          types = collection2;
        }
        else if (defaultMshSnapin.AssemblyName.Equals("Microsoft.PowerShell.Commands.Diagnostics", StringComparison.OrdinalIgnoreCase))
        {
          types = new Collection<string>((IList<string>) new string[1]
          {
            "GetEvent.types.ps1xml"
          });
          formats = new Collection<string>((IList<string>) new string[1]
          {
            "Diagnostics.Format.ps1xml"
          });
        }
        else if (defaultMshSnapin.AssemblyName.Equals("Microsoft.WSMan.Management", StringComparison.OrdinalIgnoreCase))
          formats = new Collection<string>((IList<string>) new string[1]
          {
            "WSMan.format.ps1xml"
          });
        string moduleName = Path.Combine(str1, defaultMshSnapin.AssemblyName + ".dll");
        PSSnapInInfo psSnapInInfo = new PSSnapInInfo(defaultMshSnapin.PSSnapInName, true, str1, assemblyName, moduleName, psVersion, version, types, formats, (string) null, defaultMshSnapin.Description, defaultMshSnapin.DescriptionIndirect, (string) null, (string) null, defaultMshSnapin.VendorIndirect, (string) null);
        collection3.Add(psSnapInInfo);
        PSSnapInReader._tracer.WriteLine("Constructed PSSnapInInfo object for default mshsnapin {0}.", (object) defaultMshSnapin.PSSnapInName);
      }
      return collection3;
    }

    internal static RegistryKey GetMonadRootKey()
    {
      using (PSSnapInReader._tracer.TraceMethod())
        return Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\PowerShell") ?? throw PSSnapInReader._tracer.NewArgumentException("monad", "MshSnapinInfo", "MonadRootRegistryAccessFailed");
    }

    internal static RegistryKey GetPSEngineKey(string psVersion)
    {
      using (PSSnapInReader._tracer.TraceMethod())
        return PSSnapInReader.GetVersionRootKey(PSSnapInReader.GetMonadRootKey(), psVersion).OpenSubKey("PowerShellEngine") ?? throw PSSnapInReader._tracer.NewArgumentException("monad", "MshSnapinInfo", "MonadEngineRegistryAccessFailed");
    }

    internal static RegistryKey GetVersionRootKey(RegistryKey rootKey, string psVersion)
    {
      using (PSSnapInReader._tracer.TraceMethod())
      {
        string keyForMajorVersion = PSVersionInfo.GetRegisterVersionKeyForMajorVersion(psVersion);
        return rootKey.OpenSubKey(keyForMajorVersion) ?? throw PSSnapInReader._tracer.NewArgumentException(nameof (psVersion), "MshSnapinInfo", "SpecifiedVersionNotFound", (object) keyForMajorVersion);
      }
    }

    private static RegistryKey GetMshSnapinRootKey(
      RegistryKey versionRootKey,
      string psVersion)
    {
      using (PSSnapInReader._tracer.TraceMethod())
        return versionRootKey.OpenSubKey("PowerShellSnapIns") ?? throw PSSnapInReader._tracer.NewArgumentException(nameof (psVersion), "MshSnapinInfo", "NoMshSnapinPresentForVersion", (object) psVersion);
    }

    internal static RegistryKey GetMshSnapinKey(string mshSnapInName, string psVersion)
    {
      using (PSSnapInReader._tracer.TraceMethod())
        return (PSSnapInReader.GetVersionRootKey(PSSnapInReader.GetMonadRootKey(), psVersion).OpenSubKey("PowerShellSnapIns") ?? throw PSSnapInReader._tracer.NewArgumentException(nameof (psVersion), "MshSnapinInfo", "NoMshSnapinPresentForVersion", (object) psVersion)).OpenSubKey(mshSnapInName);
    }

    private struct DefaultPSSnapInInformation
    {
      public string PSSnapInName;
      public string AssemblyName;
      public string Description;
      public string DescriptionIndirect;
      public string VendorIndirect;

      public DefaultPSSnapInInformation(
        string sName,
        string sAssemblyName,
        string sDescription,
        string sDescriptionIndirect,
        string sVendorIndirect)
      {
        this.PSSnapInName = sName;
        this.AssemblyName = sAssemblyName;
        this.Description = sDescription;
        this.DescriptionIndirect = sDescriptionIndirect;
        this.VendorIndirect = sVendorIndirect;
      }
    }
  }
}
