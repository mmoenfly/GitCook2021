// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.RunspaceConfigForSingleShell
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation.Provider;
using System.Reflection;

namespace System.Management.Automation.Runspaces
{
  internal class RunspaceConfigForSingleShell : RunspaceConfiguration
  {
    private MshConsoleInfo _consoleInfo;
    private RunspaceConfigurationEntryCollection<CmdletConfigurationEntry> _cmdlets = new RunspaceConfigurationEntryCollection<CmdletConfigurationEntry>();
    private RunspaceConfigurationEntryCollection<ProviderConfigurationEntry> _providers = new RunspaceConfigurationEntryCollection<ProviderConfigurationEntry>();
    private RunspaceConfigurationEntryCollection<TypeConfigurationEntry> _types;
    private RunspaceConfigurationEntryCollection<FormatConfigurationEntry> _formats;
    private RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> _initializationScripts;
    [TraceSource("SingleShell", "SingleShell")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("SingleShell", "SingleShell");
    private static PSTraceSource _mshsnapinTracer = PSTraceSource.GetTracer("MshSnapinLoadUnload", "Loading and unloading mshsnapins", false);

    internal static RunspaceConfigForSingleShell Create(
      string consoleFile,
      out PSConsoleLoadException warning)
    {
      using (RunspaceConfigForSingleShell.tracer.TraceMethod())
      {
        PSConsoleLoadException cle = (PSConsoleLoadException) null;
        RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Creating MshConsoleInfo. consoleFile={0}", (object) consoleFile);
        MshConsoleInfo fromConsoleFile = MshConsoleInfo.CreateFromConsoleFile(consoleFile, out cle);
        if (cle != null)
          RunspaceConfigForSingleShell._mshsnapinTracer.TraceWarning("There was a warning while creating MshConsoleInfo: {0}", (object) cle.Message);
        if (fromConsoleFile != null)
        {
          RunspaceConfigForSingleShell configForSingleShell = new RunspaceConfigForSingleShell(fromConsoleFile);
          PSConsoleLoadException warning1 = (PSConsoleLoadException) null;
          configForSingleShell.LoadConsole(out warning1);
          if (warning1 != null)
            RunspaceConfigForSingleShell._mshsnapinTracer.TraceWarning("There was a warning while loading console: {0}", (object) warning1.Message);
          warning = RunspaceConfigForSingleShell.CombinePSConsoleLoadException(cle, warning1);
          return configForSingleShell;
        }
        warning = (PSConsoleLoadException) null;
        return (RunspaceConfigForSingleShell) null;
      }
    }

    private static PSConsoleLoadException CombinePSConsoleLoadException(
      PSConsoleLoadException e1,
      PSConsoleLoadException e2)
    {
      using (RunspaceConfigForSingleShell.tracer.TraceMethod())
      {
        if ((e1 == null || e1.PSSnapInExceptions.Count == 0) && (e2 == null || e2.PSSnapInExceptions.Count == 0))
          return (PSConsoleLoadException) null;
        if (e1 == null || e1.PSSnapInExceptions.Count == 0)
          return e2;
        if (e2 == null || e2.PSSnapInExceptions.Count == 0)
          return e1;
        foreach (PSSnapInException psSnapInException in e2.PSSnapInExceptions)
          e1.PSSnapInExceptions.Add(psSnapInException);
        return e1;
      }
    }

    internal static RunspaceConfigForSingleShell CreateDefaultConfiguration()
    {
      using (RunspaceConfigForSingleShell.tracer.TraceMethod())
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Creating default runspace configuration.", new object[0]);
        MshConsoleInfo defaultConfiguration = MshConsoleInfo.CreateDefaultConfiguration();
        if (defaultConfiguration != null)
        {
          RunspaceConfigForSingleShell configForSingleShell = new RunspaceConfigForSingleShell(defaultConfiguration);
          PSConsoleLoadException warning = (PSConsoleLoadException) null;
          configForSingleShell.LoadConsole(out warning);
          if (warning != null)
            RunspaceConfigForSingleShell._mshsnapinTracer.TraceWarning("There was a warning while loading console: {0}", (object) warning.Message);
          return configForSingleShell;
        }
        RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Default runspace configuration created.", new object[0]);
        return (RunspaceConfigForSingleShell) null;
      }
    }

    private RunspaceConfigForSingleShell(MshConsoleInfo consoleInfo)
    {
      using (RunspaceConfigForSingleShell.tracer.TraceConstructor((object) this))
        this._consoleInfo = consoleInfo;
    }

    internal MshConsoleInfo ConsoleInfo => this._consoleInfo;

    internal void SaveConsoleFile()
    {
      if (this._consoleInfo == null)
        return;
      this._consoleInfo.Save();
    }

    internal void SaveAsConsoleFile(string filename)
    {
      if (this._consoleInfo == null)
        return;
      this._consoleInfo.SaveAsConsoleFile(filename);
    }

    internal override PSSnapInInfo DoAddPSSnapIn(
      string name,
      out PSSnapInException warning)
    {
      warning = (PSSnapInException) null;
      RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Adding mshsnapin {0}", (object) name);
      if (this._consoleInfo == null)
        return (PSSnapInInfo) null;
      PSSnapInInfo mshsnapinInfo;
      try
      {
        mshsnapinInfo = this._consoleInfo.AddPSSnapIn(name);
      }
      catch (PSArgumentException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceError(ex.Message);
        RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Adding mshsnapin {0} failed.", (object) name);
        throw;
      }
      catch (PSArgumentNullException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceError(ex.Message);
        RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Adding mshsnapin {0} failed.", (object) name);
        throw;
      }
      if (mshsnapinInfo == null)
        return (PSSnapInInfo) null;
      this.LoadPSSnapIn(mshsnapinInfo, out warning);
      if (warning != null)
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceWarning("There was a warning when loading mshsnapin {0}: {1}", (object) name, (object) warning.Message);
      RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("MshSnapin {0} added", (object) name);
      return mshsnapinInfo;
    }

    internal override PSSnapInInfo DoRemovePSSnapIn(
      string name,
      out PSSnapInException warning)
    {
      warning = (PSSnapInException) null;
      if (this._consoleInfo == null)
        return (PSSnapInInfo) null;
      RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Removing mshsnapin {0}", (object) name);
      PSSnapInInfo mshsnapinInfo = this._consoleInfo.RemovePSSnapIn(name);
      this.UnloadPSSnapIn(mshsnapinInfo, out warning);
      RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("MshSnapin {0} removed", (object) name);
      return mshsnapinInfo;
    }

    internal void UpdateAll()
    {
      string errors = "";
      this.UpdateAll(out errors);
    }

    internal void UpdateAll(out string errors)
    {
      errors = "";
      this.Cmdlets.Update();
      this.Providers.Update();
      RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Updating types and formats", new object[0]);
      try
      {
        this.Types.Update();
      }
      catch (RuntimeException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceWarning("There was a warning updating types: {0}", (object) ex.Message);
        ref string local = ref errors;
        local = local + ex.Message + "\n";
      }
      try
      {
        this.Formats.Update();
      }
      catch (RuntimeException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceWarning("There was a warning updating formats: {0}", (object) ex.Message);
        ref string local = ref errors;
        local = local + ex.Message + "\n";
      }
      try
      {
        this.Assemblies.Update();
      }
      catch (RuntimeException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceWarning("There was a warning updating assemblies: {0}", (object) ex.Message);
        ref string local = ref errors;
        local = local + ex.Message + "\n";
      }
      RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Types and formats updated successfully", new object[0]);
    }

    private void LoadConsole(out PSConsoleLoadException warning)
    {
      if (this._consoleInfo == null)
        warning = (PSConsoleLoadException) null;
      else
        this.LoadPSSnapIns(this._consoleInfo.PSSnapIns, out warning);
    }

    private void LoadPSSnapIn(PSSnapInInfo mshsnapinInfo, out PSSnapInException warning)
    {
      warning = (PSSnapInException) null;
      try
      {
        this.LoadPSSnapIn(mshsnapinInfo);
      }
      catch (PSSnapInException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceException((Exception) ex);
        if (!mshsnapinInfo.IsDefault)
          this._consoleInfo.RemovePSSnapIn(mshsnapinInfo.Name);
        throw;
      }
      string errors;
      this.UpdateAll(out errors);
      if (string.IsNullOrEmpty(errors))
        return;
      RunspaceConfigForSingleShell._mshsnapinTracer.TraceWarning("There was a warning while loading mshsnapin {0}:{1}", (object) mshsnapinInfo.Name, (object) errors);
      warning = new PSSnapInException(mshsnapinInfo.Name, errors, true);
    }

    private void LoadPSSnapIns(
      Collection<PSSnapInInfo> mshsnapinInfos,
      out PSConsoleLoadException warning)
    {
      warning = (PSConsoleLoadException) null;
      Collection<PSSnapInException> exceptions = new Collection<PSSnapInException>();
      bool flag = false;
      foreach (PSSnapInInfo mshsnapinInfo in mshsnapinInfos)
      {
        try
        {
          this.LoadPSSnapIn(mshsnapinInfo);
          flag = true;
        }
        catch (PSSnapInException ex)
        {
          RunspaceConfigForSingleShell._mshsnapinTracer.TraceException((Exception) ex);
          if (!mshsnapinInfo.IsDefault)
          {
            this._consoleInfo.RemovePSSnapIn(mshsnapinInfo.Name);
            exceptions.Add(ex);
          }
          else
            throw;
        }
      }
      if (flag)
      {
        string errors;
        this.UpdateAll(out errors);
        if (!string.IsNullOrEmpty(errors))
        {
          RunspaceConfigForSingleShell._mshsnapinTracer.TraceWarning(errors);
          exceptions.Add(new PSSnapInException((string) null, errors, true));
        }
      }
      if (exceptions.Count <= 0)
        return;
      warning = new PSConsoleLoadException(this._consoleInfo, exceptions);
      RunspaceConfigForSingleShell._mshsnapinTracer.TraceWarning(warning.Message);
    }

    private void LoadPSSnapIn(PSSnapInInfo mshsnapinInfo)
    {
      if (mshsnapinInfo == null)
        return;
      if (!string.IsNullOrEmpty(mshsnapinInfo.CustomPSSnapInType))
      {
        this.LoadCustomPSSnapIn(mshsnapinInfo);
      }
      else
      {
        try
        {
          RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Loading assembly for mshsnapin {0}", (object) mshsnapinInfo.Name);
          Assembly assembly = this.LoadMshSnapinAssembly(mshsnapinInfo);
          if (assembly == null)
          {
            RunspaceConfigForSingleShell._mshsnapinTracer.TraceError("Loading assembly for mshsnapin {0} failed", (object) mshsnapinInfo.Name);
            return;
          }
          RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Loading assembly for mshsnapin {0} succeeded", (object) mshsnapinInfo.Name);
          this.AnalyzeMshSnapinAssembly(assembly, mshsnapinInfo);
        }
        catch (PSSnapInException ex)
        {
          RunspaceConfigForSingleShell._mshsnapinTracer.TraceException((Exception) ex);
          throw;
        }
        foreach (string type in mshsnapinInfo.Types)
        {
          string str = Path.Combine(mshsnapinInfo.ApplicationBase, type);
          this.Types.AddBuiltInItem(new TypeConfigurationEntry(str, str, mshsnapinInfo));
        }
        foreach (string format in mshsnapinInfo.Formats)
        {
          string str = Path.Combine(mshsnapinInfo.ApplicationBase, format);
          this.Formats.AddBuiltInItem(new FormatConfigurationEntry(str, str, mshsnapinInfo));
        }
        this.Assemblies.AddBuiltInItem(new AssemblyConfigurationEntry(mshsnapinInfo.AssemblyName, mshsnapinInfo.AbsoluteModulePath, mshsnapinInfo));
      }
    }

    private void LoadCustomPSSnapIn(PSSnapInInfo mshsnapinInfo)
    {
      if (mshsnapinInfo == null || string.IsNullOrEmpty(mshsnapinInfo.CustomPSSnapInType))
        return;
      Assembly assembly;
      try
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Loading assembly for mshsnapin {0}", (object) mshsnapinInfo.Name);
        assembly = this.LoadMshSnapinAssembly(mshsnapinInfo);
        if (assembly == null)
        {
          RunspaceConfigForSingleShell._mshsnapinTracer.TraceError("Loading assembly for mshsnapin {0} failed", (object) mshsnapinInfo.Name);
          return;
        }
      }
      catch (PSSnapInException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceException((Exception) ex);
        throw;
      }
      CustomPSSnapIn customPSSnapIn = (CustomPSSnapIn) null;
      try
      {
        if (assembly.GetType(mshsnapinInfo.CustomPSSnapInType, true) != null)
          customPSSnapIn = (CustomPSSnapIn) assembly.CreateInstance(mshsnapinInfo.CustomPSSnapInType);
        RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Loading assembly for mshsnapin {0} succeeded", (object) mshsnapinInfo.Name);
      }
      catch (TypeLoadException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceException((Exception) ex);
        throw new PSSnapInException(mshsnapinInfo.Name, ex.Message);
      }
      catch (ArgumentException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceException((Exception) ex);
        throw new PSSnapInException(mshsnapinInfo.Name, ex.Message);
      }
      catch (MissingMethodException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceException((Exception) ex);
        throw new PSSnapInException(mshsnapinInfo.Name, ex.Message);
      }
      catch (InvalidCastException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceException((Exception) ex);
        throw new PSSnapInException(mshsnapinInfo.Name, ex.Message);
      }
      catch (TargetInvocationException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceException((Exception) ex);
        if (ex.InnerException != null)
          throw new PSSnapInException(mshsnapinInfo.Name, ex.InnerException.Message);
        throw new PSSnapInException(mshsnapinInfo.Name, ex.Message);
      }
      this.MergeCustomPSSnapIn(mshsnapinInfo, customPSSnapIn);
    }

    private void MergeCustomPSSnapIn(PSSnapInInfo mshsnapinInfo, CustomPSSnapIn customPSSnapIn)
    {
      if (mshsnapinInfo == null || customPSSnapIn == null)
        return;
      RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Merging configuration from custom mshsnapin {0}", (object) mshsnapinInfo.Name);
      if (customPSSnapIn.Cmdlets != null)
      {
        foreach (CmdletConfigurationEntry cmdlet in customPSSnapIn.Cmdlets)
          this._cmdlets.AddBuiltInItem(new CmdletConfigurationEntry(cmdlet.Name, cmdlet.ImplementingType, cmdlet.HelpFileName, mshsnapinInfo));
      }
      if (customPSSnapIn.Providers != null)
      {
        foreach (ProviderConfigurationEntry provider in customPSSnapIn.Providers)
          this._providers.AddBuiltInItem(new ProviderConfigurationEntry(provider.Name, provider.ImplementingType, provider.HelpFileName, mshsnapinInfo));
      }
      if (customPSSnapIn.Types != null)
      {
        foreach (TypeConfigurationEntry type in customPSSnapIn.Types)
        {
          string fileName = Path.Combine(mshsnapinInfo.ApplicationBase, type.FileName);
          this._types.AddBuiltInItem(new TypeConfigurationEntry(type.Name, fileName, mshsnapinInfo));
        }
      }
      if (customPSSnapIn.Formats != null)
      {
        foreach (FormatConfigurationEntry format in customPSSnapIn.Formats)
        {
          string fileName = Path.Combine(mshsnapinInfo.ApplicationBase, format.FileName);
          this._formats.AddBuiltInItem(new FormatConfigurationEntry(format.Name, fileName, mshsnapinInfo));
        }
      }
      this.Assemblies.AddBuiltInItem(new AssemblyConfigurationEntry(mshsnapinInfo.AssemblyName, mshsnapinInfo.AbsoluteModulePath, mshsnapinInfo));
      RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Configuration from custom mshsnapin {0} merged", (object) mshsnapinInfo.Name);
    }

    private void UnloadPSSnapIn(PSSnapInInfo mshsnapinInfo, out PSSnapInException warning)
    {
      warning = (PSSnapInException) null;
      if (mshsnapinInfo == null)
        return;
      this.Cmdlets.RemovePSSnapIn(mshsnapinInfo.Name);
      this.Providers.RemovePSSnapIn(mshsnapinInfo.Name);
      this.Assemblies.RemovePSSnapIn(mshsnapinInfo.Name);
      this.Types.RemovePSSnapIn(mshsnapinInfo.Name);
      this.Formats.RemovePSSnapIn(mshsnapinInfo.Name);
      string errors;
      this.UpdateAll(out errors);
      if (string.IsNullOrEmpty(errors))
        return;
      RunspaceConfigForSingleShell._mshsnapinTracer.TraceWarning(errors);
      warning = new PSSnapInException(mshsnapinInfo.Name, errors, true);
    }

    private Assembly LoadMshSnapinAssembly(PSSnapInInfo mshsnapinInfo)
    {
      Assembly assembly1 = (Assembly) null;
      RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Loading assembly from GAC. Assembly Name: {0}", (object) mshsnapinInfo.AssemblyName);
      try
      {
        assembly1 = Assembly.Load(mshsnapinInfo.AssemblyName);
      }
      catch (FileLoadException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceWarning("Not able to load assembly {0}: {1}", (object) mshsnapinInfo.AssemblyName, (object) ex.Message);
      }
      catch (BadImageFormatException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceWarning("Not able to load assembly {0}: {1}", (object) mshsnapinInfo.AssemblyName, (object) ex.Message);
      }
      catch (FileNotFoundException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceWarning("Not able to load assembly {0}: {1}", (object) mshsnapinInfo.AssemblyName, (object) ex.Message);
      }
      if (assembly1 != null)
        return assembly1;
      RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Loading assembly from path: {0}", (object) mshsnapinInfo.AssemblyName);
      try
      {
        Assembly assembly2 = Assembly.ReflectionOnlyLoadFrom(mshsnapinInfo.AbsoluteModulePath);
        if (assembly2 == null)
          return (Assembly) null;
        if (string.Compare(assembly2.FullName, mshsnapinInfo.AssemblyName, StringComparison.OrdinalIgnoreCase) != 0)
        {
          string str = ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "PSSnapInAssemblyNameMismatch", (object) mshsnapinInfo.AbsoluteModulePath, (object) mshsnapinInfo.AssemblyName);
          RunspaceConfigForSingleShell._mshsnapinTracer.TraceError(str);
          throw new PSSnapInException(mshsnapinInfo.Name, str);
        }
        return Assembly.LoadFrom(mshsnapinInfo.AbsoluteModulePath);
      }
      catch (FileLoadException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceError("Not able to load assembly {0}: {1}", (object) mshsnapinInfo.AssemblyName, (object) ex.Message);
        throw new PSSnapInException(mshsnapinInfo.Name, ex.Message);
      }
      catch (BadImageFormatException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceError("Not able to load assembly {0}: {1}", (object) mshsnapinInfo.AssemblyName, (object) ex.Message);
        throw new PSSnapInException(mshsnapinInfo.Name, ex.Message);
      }
      catch (FileNotFoundException ex)
      {
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceError("Not able to load assembly {0}: {1}", (object) mshsnapinInfo.AssemblyName, (object) ex.Message);
        throw new PSSnapInException(mshsnapinInfo.Name, ex.Message);
      }
    }

    private void AnalyzeMshSnapinAssembly(Assembly assembly, PSSnapInInfo mshsnapinInfo)
    {
      if (assembly == null)
        return;
      RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("Analyzing assembly {0} for cmdlet and providers", (object) assembly.Location);
      string helpFile = RunspaceConfigForSingleShell.GetHelpFile(assembly.Location);
      Type[] exportedTypes;
      try
      {
        exportedTypes = assembly.GetExportedTypes();
      }
      catch (ReflectionTypeLoadException ex)
      {
        string str = ex.Message + "\nLoader Exceptions: \n";
        if (ex.LoaderExceptions != null)
        {
          foreach (Exception loaderException in ex.LoaderExceptions)
            str = str + "\n" + loaderException.Message;
        }
        RunspaceConfigForSingleShell._mshsnapinTracer.TraceError(str);
        throw new PSSnapInException(mshsnapinInfo.Name, str);
      }
      Hashtable hashtable1 = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);
      Hashtable hashtable2 = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);
      foreach (Type type in exportedTypes)
      {
        string name1 = (string) null;
        string name2 = (string) null;
        object[] customAttributes1 = type.GetCustomAttributes(typeof (CmdletAttribute), false);
        if (customAttributes1.Length > 0)
        {
          name1 = RunspaceConfigForSingleShell.GetCmdletName(customAttributes1[0] as CmdletAttribute);
        }
        else
        {
          object[] customAttributes2 = type.GetCustomAttributes(typeof (CmdletProviderAttribute), false);
          if (customAttributes2.Length > 0)
            name2 = RunspaceConfigForSingleShell.GetProviderName(customAttributes2[0] as CmdletProviderAttribute);
        }
        if (!string.IsNullOrEmpty(name1))
        {
          if (RunspaceConfigForSingleShell.IsCmdletClass(type) && RunspaceConfigForSingleShell.HasDefaultConstructor(type))
          {
            if (hashtable1.ContainsKey((object) name1))
            {
              string str = ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "PSSnapInDuplicateCmdlets", (object) name1, (object) mshsnapinInfo.Name);
              RunspaceConfigForSingleShell._mshsnapinTracer.TraceError(str);
              throw new PSSnapInException(mshsnapinInfo.Name, str);
            }
            hashtable1.Add((object) name1, (object) null);
            this._cmdlets.AddBuiltInItem(new CmdletConfigurationEntry(name1, type, helpFile, mshsnapinInfo));
            RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("{0} from type {1} is added as a cmdlet. ", (object) name1, (object) type.FullName);
            continue;
          }
          RunspaceConfigForSingleShell._mshsnapinTracer.TraceWarning("{0} is not valid cmdlet because it doesn't derive from the Cmdlet type or it doesn't have a default constructor.", (object) name1);
        }
        if (!string.IsNullOrEmpty(name2))
        {
          if (RunspaceConfigForSingleShell.IsProviderClass(type) && RunspaceConfigForSingleShell.HasDefaultConstructor(type))
          {
            if (hashtable2.ContainsKey((object) name2))
            {
              string str = ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "PSSnapInDuplicateProviders", (object) name2, (object) mshsnapinInfo.Name);
              RunspaceConfigForSingleShell._mshsnapinTracer.TraceError(str);
              throw new PSSnapInException(mshsnapinInfo.Name, str);
            }
            hashtable2.Add((object) name2, (object) null);
            this._providers.AddBuiltInItem(new ProviderConfigurationEntry(name2, type, helpFile, mshsnapinInfo));
            RunspaceConfigForSingleShell._mshsnapinTracer.WriteLine("{0} from type {1} is added as a provider. ", (object) name2, (object) type.FullName);
          }
          else
            RunspaceConfigForSingleShell._mshsnapinTracer.TraceWarning("{0} is not valid provider because it doesn't derive from the provider type or it doesn't have a default constructor.", (object) name2);
        }
      }
    }

    private static string GetCmdletName(CmdletAttribute cmdletAttribute) => cmdletAttribute.VerbName + "-" + cmdletAttribute.NounName;

    private static string GetProviderName(CmdletProviderAttribute providerAttribute) => providerAttribute.ProviderName;

    private static string GetProperty(object obj, string propertyName)
    {
      PropertyInfo property = obj.GetType().GetProperty(propertyName);
      return property == null ? (string) null : (string) property.GetValue(obj, (object[]) null);
    }

    private static bool IsCmdletClass(Type type) => type != null && type.IsSubclassOf(typeof (Cmdlet));

    private static bool IsProviderClass(Type type) => type != null && type.IsSubclassOf(typeof (CmdletProvider));

    private static bool HasDefaultConstructor(Type type) => type.GetConstructor(Type.EmptyTypes) != null;

    private static string GetHelpFile(string assemblyPath) => Path.GetFileName(assemblyPath) + "-Help.xml";

    public override string ShellId => Utils.DefaultPowerShellShellID;

    public override RunspaceConfigurationEntryCollection<CmdletConfigurationEntry> Cmdlets
    {
      get
      {
        using (RunspaceConfigForSingleShell.tracer.TraceProperty())
          return this._cmdlets;
      }
    }

    public override RunspaceConfigurationEntryCollection<ProviderConfigurationEntry> Providers
    {
      get
      {
        using (RunspaceConfigForSingleShell.tracer.TraceProperty())
          return this._providers;
      }
    }

    public override RunspaceConfigurationEntryCollection<TypeConfigurationEntry> Types
    {
      get
      {
        using (RunspaceConfigForSingleShell.tracer.TraceProperty())
        {
          if (this._types == null)
            this._types = new RunspaceConfigurationEntryCollection<TypeConfigurationEntry>();
          return this._types;
        }
      }
    }

    public override RunspaceConfigurationEntryCollection<FormatConfigurationEntry> Formats
    {
      get
      {
        using (RunspaceConfigForSingleShell.tracer.TraceProperty())
        {
          if (this._formats == null)
            this._formats = new RunspaceConfigurationEntryCollection<FormatConfigurationEntry>();
          return this._formats;
        }
      }
    }

    public override RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> InitializationScripts
    {
      get
      {
        using (RunspaceConfigForSingleShell.tracer.TraceProperty())
        {
          if (this._initializationScripts == null)
            this._initializationScripts = new RunspaceConfigurationEntryCollection<ScriptConfigurationEntry>();
          return this._initializationScripts;
        }
      }
    }
  }
}
