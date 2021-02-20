// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.ConfigurationDataFromXML
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace System.Management.Automation.Remoting
{
  internal class ConfigurationDataFromXML
  {
    internal const string INITPARAMETERSTOKEN = "InitializationParameters";
    internal const string PARAMTOKEN = "Param";
    internal const string NAMETOKEN = "Name";
    internal const string VALUETOKEN = "Value";
    internal const string APPBASETOKEN = "applicationbase";
    internal const string ASSEMBLYTOKEN = "assemblyname";
    internal const string SHELLCONFIGTYPETOKEN = "pssessionconfigurationtypename";
    internal const string STARTUPSCRIPTTOKEN = "startupscript";
    internal const string MAXRCVDOBJSIZETOKEN = "psmaximumreceivedobjectsizemb";
    internal const string MAXRCVDCMDSIZETOKEN = "psmaximumreceiveddatasizepercommandmb";
    internal const string THREADOPTIONSTOKEN = "pssessionthreadoptions";
    internal const string THREADAPTSTATETOKEN = "pssessionthreadapartmentstate";
    private const string resBaseName = "remotingerroridstrings";
    [TraceSource("ServerRemoteSession", "ServerRemoteSession")]
    private static readonly PSTraceSource tracer = PSTraceSource.GetTracer("ServerRemoteSession", "ServerRemoteSession");
    internal string StartupScript;
    internal string InitializationScriptForOutOfProcessRunspace;
    internal string ApplicationBase;
    internal string AssemblyName;
    internal string EndPointConfigurationTypeName;
    internal Type EndPointConfigurationType;
    internal int? MaxReceivedObjectSizeMB;
    internal int? MaxReceivedCommandSizeMB;
    internal PSThreadOptions? ShellThreadOptions;
    internal ApartmentState? ShellThreadApartmentState;

    private void Update(string optionName, string optionValue)
    {
      switch (optionName.ToLower(CultureInfo.InvariantCulture))
      {
        case "applicationbase":
          this.AssertValueNotAssigned("applicationbase", (object) this.ApplicationBase);
          this.ApplicationBase = Environment.ExpandEnvironmentVariables(optionValue);
          break;
        case "assemblyname":
          this.AssertValueNotAssigned("assemblyname", (object) this.AssemblyName);
          this.AssemblyName = optionValue;
          break;
        case "pssessionconfigurationtypename":
          this.AssertValueNotAssigned("pssessionconfigurationtypename", (object) this.EndPointConfigurationTypeName);
          this.EndPointConfigurationTypeName = optionValue;
          break;
        case "startupscript":
          this.AssertValueNotAssigned("startupscript", (object) this.StartupScript);
          this.StartupScript = optionValue.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase) ? Environment.ExpandEnvironmentVariables(optionValue) : throw ConfigurationDataFromXML.tracer.NewArgumentException("startupscript", "remotingerroridstrings", "StartupScriptNotCorrect", (object) "startupscript");
          break;
        case "psmaximumreceivedobjectsizemb":
          this.AssertValueNotAssigned("psmaximumreceivedobjectsizemb", (object) this.MaxReceivedObjectSizeMB);
          this.MaxReceivedObjectSizeMB = ConfigurationDataFromXML.GetIntValueInBytes(optionValue);
          break;
        case "psmaximumreceiveddatasizepercommandmb":
          this.AssertValueNotAssigned("psmaximumreceiveddatasizepercommandmb", (object) this.MaxReceivedCommandSizeMB);
          this.MaxReceivedCommandSizeMB = ConfigurationDataFromXML.GetIntValueInBytes(optionValue);
          break;
        case "pssessionthreadoptions":
          this.AssertValueNotAssigned("pssessionthreadoptions", (object) this.ShellThreadOptions);
          this.ShellThreadOptions = new PSThreadOptions?((PSThreadOptions) LanguagePrimitives.ConvertTo((object) optionValue, typeof (PSThreadOptions), (IFormatProvider) CultureInfo.InvariantCulture));
          break;
        case "pssessionthreadapartmentstate":
          this.AssertValueNotAssigned("pssessionthreadapartmentstate", (object) this.ShellThreadApartmentState);
          this.ShellThreadApartmentState = new ApartmentState?((ApartmentState) LanguagePrimitives.ConvertTo((object) optionValue, typeof (ApartmentState), (IFormatProvider) CultureInfo.InvariantCulture));
          break;
      }
    }

    private void AssertValueNotAssigned(string optionName, object originalValue)
    {
      if (originalValue != null)
        throw ConfigurationDataFromXML.tracer.NewArgumentException(optionName, "remotingerroridstrings", "DuplicateInitializationParameterFound", (object) optionName, (object) "InitializationParameters");
    }

    private static int? GetIntValueInBytes(string optionValueInMB)
    {
      int? nullable1 = new int?();
      try
      {
        nullable1 = new int?((int) ((double) LanguagePrimitives.ConvertTo((object) optionValueInMB, typeof (double), (IFormatProvider) CultureInfo.InvariantCulture) * 1024.0 * 1024.0));
      }
      catch (InvalidCastException ex)
      {
      }
      int? nullable2 = nullable1;
      if ((nullable2.GetValueOrDefault() >= 0 ? 0 : (nullable2.HasValue ? 1 : 0)) != 0)
        nullable1 = new int?();
      return nullable1;
    }

    internal static ConfigurationDataFromXML Create(
      string initializationParameters)
    {
      ConfigurationDataFromXML configurationDataFromXml = new ConfigurationDataFromXML();
      if (string.IsNullOrEmpty(initializationParameters))
        return configurationDataFromXml;
      using (XmlReader xmlReader = XmlReader.Create((TextReader) new StringReader(initializationParameters), new XmlReaderSettings()
      {
        CheckCharacters = false,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
        MaxCharactersInDocument = 1000L,
        XmlResolver = (XmlResolver) null,
        ConformanceLevel = ConformanceLevel.Fragment
      }))
      {
        if (xmlReader.ReadToFollowing("InitializationParameters"))
        {
          for (bool flag = xmlReader.ReadToDescendant("Param"); flag; flag = xmlReader.ReadToFollowing("Param"))
          {
            string optionName = xmlReader.MoveToAttribute("Name") ? xmlReader.Value : throw ConfigurationDataFromXML.tracer.NewArgumentException(initializationParameters, "remotingerroridstrings", "NoAttributesFoundForParamElement", (object) "Name", (object) "Value", (object) "Param");
            string optionValue = xmlReader.MoveToAttribute("Value") ? xmlReader.Value : throw ConfigurationDataFromXML.tracer.NewArgumentException(initializationParameters, "remotingerroridstrings", "NoAttributesFoundForParamElement", (object) "Name", (object) "Value", (object) "Param");
            configurationDataFromXml.Update(optionName, optionValue);
          }
        }
      }
      if (!configurationDataFromXml.MaxReceivedObjectSizeMB.HasValue)
        configurationDataFromXml.MaxReceivedObjectSizeMB = new int?(10485760);
      if (!configurationDataFromXml.MaxReceivedCommandSizeMB.HasValue)
        configurationDataFromXml.MaxReceivedCommandSizeMB = new int?(52428800);
      return configurationDataFromXml;
    }

    internal PSSessionConfiguration CreateEndPointConfigurationInstance()
    {
      try
      {
        return (PSSessionConfiguration) this.EndPointConfigurationType.Assembly.CreateInstance(this.EndPointConfigurationType.FullName);
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
      throw ConfigurationDataFromXML.tracer.NewArgumentException("typeToLoad", "remotingerroridstrings", "UnableToLoadType", (object) this.EndPointConfigurationTypeName, (object) "InitializationParameters");
    }
  }
}
