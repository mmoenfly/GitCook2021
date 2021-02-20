// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.PSSessionConfiguration
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Security;

namespace System.Management.Automation.Remoting
{
  public abstract class PSSessionConfiguration : IDisposable
  {
    private const string configProvidersKeyName = "PSConfigurationProviders";
    private const string configProviderApplicationBaseKeyName = "ApplicationBase";
    private const string configProviderAssemblyNameKeyName = "AssemblyName";
    private const string resBaseName = "remotingerroridstrings";
    [TraceSource("ServerRemoteSession", "ServerRemoteSession")]
    private static readonly PSTraceSource tracer = PSTraceSource.GetTracer("ServerRemoteSession", "ServerRemoteSession");
    private static IETWTracer ETWTracer = PSETWTracerFactory.GetETWTracer(PSKeyword.Transport);
    private static Dictionary<string, ConfigurationDataFromXML> ssnStateProviders = new Dictionary<string, ConfigurationDataFromXML>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private static object syncObject = new object();

    public abstract InitialSessionState GetInitialSessionState(
      PSSenderInfo senderInfo);

    public virtual int? GetMaximumReceivedObjectSize(PSSenderInfo senderInfo) => new int?(10485760);

    public virtual int? GetMaximumReceivedDataSizePerCommand(PSSenderInfo senderInfo) => new int?(52428800);

    public virtual PSPrimitiveDictionary GetApplicationPrivateData(
      PSSenderInfo senderInfo)
    {
      return (PSPrimitiveDictionary) null;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool isDisposing)
    {
    }

    internal static ConfigurationDataFromXML LoadEndPointConfiguration(
      string shellId,
      string initializationParameters)
    {
      ConfigurationDataFromXML configurationDataFromXml = (ConfigurationDataFromXML) null;
      if (!PSSessionConfiguration.ssnStateProviders.ContainsKey(initializationParameters))
        PSSessionConfiguration.LoadRSConfigProvider(shellId, initializationParameters);
      lock (PSSessionConfiguration.syncObject)
      {
        if (!PSSessionConfiguration.ssnStateProviders.TryGetValue(initializationParameters, out configurationDataFromXml))
          throw PSSessionConfiguration.tracer.NewInvalidOperationException("remotingerroridstrings", "NonExistentInitialSessionStateProvider", (object) shellId);
      }
      return configurationDataFromXml;
    }

    private static void LoadRSConfigProvider(string shellId, string initializationParameters)
    {
      ConfigurationDataFromXML configurationDataFromXml = ConfigurationDataFromXML.Create(initializationParameters);
      Type type = PSSessionConfiguration.LoadAndAnalyzeAssembly(shellId, configurationDataFromXml.ApplicationBase, configurationDataFromXml.AssemblyName, configurationDataFromXml.EndPointConfigurationTypeName);
      configurationDataFromXml.EndPointConfigurationType = type;
      lock (PSSessionConfiguration.syncObject)
      {
        if (PSSessionConfiguration.ssnStateProviders.ContainsKey(initializationParameters))
          return;
        PSSessionConfiguration.ssnStateProviders.Add(initializationParameters, configurationDataFromXml);
      }
    }

    private static Type LoadAndAnalyzeAssembly(
      string shellId,
      string applicationBase,
      string assemblyName,
      string typeToLoad)
    {
      if (string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeToLoad) || !string.IsNullOrEmpty(assemblyName) && string.IsNullOrEmpty(typeToLoad))
        throw PSSessionConfiguration.tracer.NewInvalidOperationException("remotingerroridstrings", "TypeNeedsAssembly", (object) "assemblyname", (object) "pssessionconfigurationtypename", (object) "InitializationParameters");
      Assembly assembly = (Assembly) null;
      if (!string.IsNullOrEmpty(assemblyName))
      {
        PSSessionConfiguration.ETWTracer.AnalyticChannel.WriteVerbose(PSEventId.LoadingPSCustomShellAssembly, PSOpcode.Connect, PSTask.None, (object) assemblyName, (object) shellId);
        assembly = PSSessionConfiguration.LoadSsnStateProviderAssembly(applicationBase, assemblyName);
        if (assembly == null)
          throw PSSessionConfiguration.tracer.NewArgumentException(nameof (assemblyName), "remotingerroridstrings", "UnableToLoadAssembly", (object) assemblyName, (object) "InitializationParameters");
      }
      if (assembly != null)
      {
        try
        {
          PSSessionConfiguration.ETWTracer.AnalyticChannel.WriteVerbose(PSEventId.LoadingPSCustomShellType, PSOpcode.Connect, PSTask.None, (object) typeToLoad, (object) shellId);
          return assembly.GetType(typeToLoad, true, true) ?? throw PSSessionConfiguration.tracer.NewArgumentException(nameof (typeToLoad), "remotingerroridstrings", "UnableToLoadType", (object) typeToLoad, (object) "InitializationParameters");
        }
        catch (ReflectionTypeLoadException ex)
        {
        }
        catch (TypeLoadException ex)
        {
        }
        catch (ArgumentException ex)
        {
        }
        catch (MissingMethodException ex)
        {
        }
        catch (InvalidCastException ex)
        {
        }
        catch (TargetInvocationException ex)
        {
        }
        throw PSSessionConfiguration.tracer.NewArgumentException(nameof (typeToLoad), "remotingerroridstrings", "UnableToLoadType", (object) typeToLoad, (object) "InitializationParameters");
      }
      return typeof (DefaultRemotePowerShellConfiguration);
    }

    private static Assembly LoadSsnStateProviderAssembly(
      string applicationBase,
      string assemblyName)
    {
      string path = string.Empty;
      if (!string.IsNullOrEmpty(applicationBase))
      {
        try
        {
          path = Directory.GetCurrentDirectory();
          Directory.SetCurrentDirectory(applicationBase);
        }
        catch (ArgumentException ex)
        {
          PSSessionConfiguration.tracer.TraceWarning("Not able to change curent working directory to {0}: {1}", (object) applicationBase, (object) ex.Message);
        }
        catch (PathTooLongException ex)
        {
          PSSessionConfiguration.tracer.TraceWarning("Not able to change curent working directory to {0}: {1}", (object) applicationBase, (object) ex.Message);
        }
        catch (FileNotFoundException ex)
        {
          PSSessionConfiguration.tracer.TraceWarning("Not able to change curent working directory to {0}: {1}", (object) applicationBase, (object) ex.Message);
        }
        catch (IOException ex)
        {
          PSSessionConfiguration.tracer.TraceWarning("Not able to change curent working directory to {0}: {1}", (object) applicationBase, (object) ex.Message);
        }
        catch (SecurityException ex)
        {
          PSSessionConfiguration.tracer.TraceWarning("Not able to change curent working directory to {0}: {1}", (object) applicationBase, (object) ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
          PSSessionConfiguration.tracer.TraceWarning("Not able to change curent working directory to {0}: {1}", (object) applicationBase, (object) ex.Message);
        }
      }
      Assembly assembly = (Assembly) null;
      try
      {
        try
        {
          assembly = Assembly.Load(assemblyName);
        }
        catch (FileLoadException ex)
        {
          PSSessionConfiguration.tracer.TraceWarning("Not able to load assembly {0}: {1}", (object) assemblyName, (object) ex.Message);
        }
        catch (BadImageFormatException ex)
        {
          PSSessionConfiguration.tracer.TraceWarning("Not able to load assembly {0}: {1}", (object) assemblyName, (object) ex.Message);
        }
        catch (FileNotFoundException ex)
        {
          PSSessionConfiguration.tracer.TraceWarning("Not able to load assembly {0}: {1}", (object) assemblyName, (object) ex.Message);
        }
        if (assembly != null)
          return assembly;
        PSSessionConfiguration.tracer.WriteLine("Loading assembly from path {0}", (object) applicationBase);
        try
        {
          assembly = Assembly.LoadFrom(assemblyName);
        }
        catch (FileLoadException ex)
        {
          PSSessionConfiguration.tracer.TraceWarning("Not able to load assembly {0}: {1}", (object) assemblyName, (object) ex.Message);
        }
        catch (BadImageFormatException ex)
        {
          PSSessionConfiguration.tracer.TraceWarning("Not able to load assembly {0}: {1}", (object) assemblyName, (object) ex.Message);
        }
        catch (FileNotFoundException ex)
        {
          PSSessionConfiguration.tracer.TraceWarning("Not able to load assembly {0}: {1}", (object) assemblyName, (object) ex.Message);
        }
      }
      finally
      {
        if (!string.IsNullOrEmpty(applicationBase))
          Directory.SetCurrentDirectory(path);
      }
      return assembly;
    }

    private static RegistryKey GetConfigurationProvidersRegistryKey()
    {
      try
      {
        return PSSnapInReader.GetVersionRootKey(PSSnapInReader.GetMonadRootKey(), Utils.GetCurrentMajorVersion()).OpenSubKey("PSConfigurationProviders");
      }
      catch (ArgumentException ex)
      {
      }
      catch (SecurityException ex)
      {
      }
      return (RegistryKey) null;
    }

    private static string ReadStringValue(RegistryKey registryKey, string name, bool mandatory)
    {
      object obj = registryKey.GetValue(name);
      if (obj == null && mandatory)
      {
        PSSessionConfiguration.tracer.TraceError("Mandatory property {0} not specified for registry key {1}", (object) name, (object) registryKey.Name);
        throw PSSessionConfiguration.tracer.NewArgumentException(nameof (name), "remotingerroridstrings", "MandatoryValueNotPresent", (object) name, (object) registryKey.Name);
      }
      string str = obj as string;
      if (string.IsNullOrEmpty(str) && mandatory)
      {
        PSSessionConfiguration.tracer.TraceError("Value is null or empty for mandatory property {0} in {1}", (object) name, (object) registryKey.Name);
        throw PSSessionConfiguration.tracer.NewArgumentException(nameof (name), "remotingerroridstrings", "MandatoryValueNotInCorrectFormat", (object) name, (object) registryKey.Name);
      }
      return str;
    }
  }
}
