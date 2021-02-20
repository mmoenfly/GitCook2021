// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.PSSnapInHelpers
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Provider;
using System.Reflection;

namespace System.Management.Automation.Runspaces
{
  internal static class PSSnapInHelpers
  {
    private static object _syncObject = new object();
    private static Dictionary<Assembly, Dictionary<string, SessionStateCmdletEntry>> _cmdletCache;
    private static Dictionary<Assembly, Dictionary<string, SessionStateProviderEntry>> _providerCache;
    private static PSTraceSource _PSSnapInTracer = PSTraceSource.GetTracer("PSSnapInLoadUnload", "Loading and unloading mshsnapins", false);

    internal static Assembly LoadPSSnapInAssembly(
      PSSnapInInfo psSnapInInfo,
      out Dictionary<string, SessionStateCmdletEntry> cmdlets,
      out Dictionary<string, SessionStateProviderEntry> providers)
    {
      Assembly assembly1 = (Assembly) null;
      cmdlets = (Dictionary<string, SessionStateCmdletEntry>) null;
      providers = (Dictionary<string, SessionStateProviderEntry>) null;
      PSSnapInHelpers._PSSnapInTracer.WriteLine("Loading assembly from GAC. Assembly Name: {0}", (object) psSnapInInfo.AssemblyName);
      try
      {
        assembly1 = Assembly.Load(psSnapInInfo.AssemblyName);
      }
      catch (FileLoadException ex)
      {
        PSSnapInHelpers._PSSnapInTracer.TraceWarning("Not able to load assembly {0}: {1}", (object) psSnapInInfo.AssemblyName, (object) ex.Message);
      }
      catch (BadImageFormatException ex)
      {
        PSSnapInHelpers._PSSnapInTracer.TraceWarning("Not able to load assembly {0}: {1}", (object) psSnapInInfo.AssemblyName, (object) ex.Message);
      }
      catch (FileNotFoundException ex)
      {
        PSSnapInHelpers._PSSnapInTracer.TraceWarning("Not able to load assembly {0}: {1}", (object) psSnapInInfo.AssemblyName, (object) ex.Message);
      }
      if (assembly1 != null)
        return assembly1;
      PSSnapInHelpers._PSSnapInTracer.WriteLine("Loading assembly from path: {0}", (object) psSnapInInfo.AssemblyName);
      try
      {
        Assembly assembly2 = Assembly.ReflectionOnlyLoadFrom(psSnapInInfo.AbsoluteModulePath);
        if (assembly2 == null)
          return (Assembly) null;
        if (!string.Equals(assembly2.FullName, psSnapInInfo.AssemblyName, StringComparison.OrdinalIgnoreCase))
        {
          string str = ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "PSSnapInAssemblyNameMismatch", (object) psSnapInInfo.AbsoluteModulePath, (object) psSnapInInfo.AssemblyName);
          PSSnapInHelpers._PSSnapInTracer.TraceError(str);
          throw new PSSnapInException(psSnapInInfo.Name, str);
        }
        return Assembly.LoadFrom(psSnapInInfo.AbsoluteModulePath);
      }
      catch (FileLoadException ex)
      {
        PSSnapInHelpers._PSSnapInTracer.TraceError("Not able to load assembly {0}: {1}", (object) psSnapInInfo.AssemblyName, (object) ex.Message);
        throw new PSSnapInException(psSnapInInfo.Name, ex.Message);
      }
      catch (BadImageFormatException ex)
      {
        PSSnapInHelpers._PSSnapInTracer.TraceError("Not able to load assembly {0}: {1}", (object) psSnapInInfo.AssemblyName, (object) ex.Message);
        throw new PSSnapInException(psSnapInInfo.Name, ex.Message);
      }
      catch (FileNotFoundException ex)
      {
        PSSnapInHelpers._PSSnapInTracer.TraceError("Not able to load assembly {0}: {1}", (object) psSnapInInfo.AssemblyName, (object) ex.Message);
        throw new PSSnapInException(psSnapInInfo.Name, ex.Message);
      }
    }

    internal static void AnalyzePSSnapInAssembly(
      Assembly assembly,
      string name,
      PSSnapInInfo psSnapInInfo,
      out Dictionary<string, SessionStateCmdletEntry> cmdlets,
      out Dictionary<string, SessionStateProviderEntry> providers)
    {
      if (assembly == null)
        throw new ArgumentNullException(nameof (assembly));
      if (PSSnapInHelpers._cmdletCache != null && PSSnapInHelpers._providerCache != null && (PSSnapInHelpers._cmdletCache.ContainsKey(assembly) && PSSnapInHelpers._providerCache.ContainsKey(assembly)))
      {
        cmdlets = PSSnapInHelpers._cmdletCache[assembly];
        providers = PSSnapInHelpers._providerCache[assembly];
      }
      else
      {
        cmdlets = (Dictionary<string, SessionStateCmdletEntry>) null;
        providers = (Dictionary<string, SessionStateProviderEntry>) null;
        PSSnapInHelpers._PSSnapInTracer.WriteLine("Analyzing assembly {0} for cmdlet and providers", (object) assembly.Location);
        string helpFile = PSSnapInHelpers.GetHelpFile(assembly.Location);
        Type[] types;
        try
        {
          types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
          string str = ex.Message + "\nLoader Exceptions: \n";
          if (ex.LoaderExceptions != null)
          {
            foreach (Exception loaderException in ex.LoaderExceptions)
              str = str + "\n" + loaderException.Message;
          }
          PSSnapInHelpers._PSSnapInTracer.TraceError(str);
          throw new PSSnapInException(name, str);
        }
        foreach (Type type in types)
        {
          if ((type.IsPublic || type.IsNestedPublic) && !type.IsAbstract)
          {
            object[] customAttributes = type.GetCustomAttributes(false);
            string str1 = (string) null;
            string str2 = (string) null;
            foreach (object obj in customAttributes)
            {
              if (obj.GetType() == typeof (CmdletAttribute))
              {
                str1 = PSSnapInHelpers.GetCmdletName(obj as CmdletAttribute);
                break;
              }
              if (obj.GetType() == typeof (CmdletProviderAttribute))
              {
                str2 = PSSnapInHelpers.GetProviderName(obj as CmdletProviderAttribute);
                break;
              }
            }
            if (!string.IsNullOrEmpty(str1))
            {
              if (PSSnapInHelpers.IsCmdletClass(type) && PSSnapInHelpers.HasDefaultConstructor(type))
              {
                if (cmdlets != null && cmdlets.ContainsKey(str1))
                {
                  string str3 = ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "PSSnapInDuplicateCmdlets", (object) str1, (object) name);
                  PSSnapInHelpers._PSSnapInTracer.TraceError(str3);
                  throw new PSSnapInException(name, str3);
                }
                SessionStateCmdletEntry stateCmdletEntry = new SessionStateCmdletEntry(str1, type, helpFile);
                if (psSnapInInfo != null)
                  stateCmdletEntry.SetPSSnapIn(psSnapInInfo);
                if (cmdlets == null)
                  cmdlets = new Dictionary<string, SessionStateCmdletEntry>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
                cmdlets.Add(str1, stateCmdletEntry);
                PSSnapInHelpers._PSSnapInTracer.WriteLine("{0} from type {1} is added as a cmdlet. ", (object) str1, (object) type.FullName);
                continue;
              }
              PSSnapInHelpers._PSSnapInTracer.TraceWarning("{0} is not valid cmdlet because it doesn't derive from the Cmdlet type or it doesn't have a default constructor.", (object) str1);
            }
            if (!string.IsNullOrEmpty(str2))
            {
              if (PSSnapInHelpers.IsProviderClass(type) && PSSnapInHelpers.HasDefaultConstructor(type))
              {
                if (providers != null && providers.ContainsKey(str2))
                {
                  string str3 = ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "PSSnapInDuplicateProviders", (object) str2, (object) psSnapInInfo.Name);
                  PSSnapInHelpers._PSSnapInTracer.TraceError(str3);
                  throw new PSSnapInException(psSnapInInfo.Name, str3);
                }
                SessionStateProviderEntry stateProviderEntry = new SessionStateProviderEntry(str2, type, helpFile);
                stateProviderEntry.SetPSSnapIn(psSnapInInfo);
                if (providers == null)
                  providers = new Dictionary<string, SessionStateProviderEntry>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
                providers.Add(str2, stateProviderEntry);
                PSSnapInHelpers._PSSnapInTracer.WriteLine("{0} from type {1} is added as a provider. ", (object) str2, (object) type.FullName);
              }
              else
                PSSnapInHelpers._PSSnapInTracer.TraceWarning("{0} is not valid provider because it doesn't derive from the provider type or it doesn't have a default constructor.", (object) str2);
            }
          }
        }
        lock (PSSnapInHelpers._syncObject)
        {
          if (cmdlets != null)
          {
            if (PSSnapInHelpers._cmdletCache == null)
              PSSnapInHelpers._cmdletCache = new Dictionary<Assembly, Dictionary<string, SessionStateCmdletEntry>>();
            PSSnapInHelpers._cmdletCache[assembly] = cmdlets;
          }
          if (providers == null)
            return;
          if (PSSnapInHelpers._providerCache == null)
            PSSnapInHelpers._providerCache = new Dictionary<Assembly, Dictionary<string, SessionStateProviderEntry>>();
          PSSnapInHelpers._providerCache[assembly] = providers;
        }
      }
    }

    private static string GetCmdletName(CmdletAttribute cmdletAttribute) => cmdletAttribute.VerbName + "-" + cmdletAttribute.NounName;

    private static string GetProviderName(CmdletProviderAttribute providerAttribute) => providerAttribute.ProviderName;

    private static bool IsCmdletClass(Type type) => type != null && type.IsSubclassOf(typeof (Cmdlet));

    private static bool IsProviderClass(Type type) => type != null && type.IsSubclassOf(typeof (CmdletProvider));

    private static bool HasDefaultConstructor(Type type) => type.GetConstructor(Type.EmptyTypes) != null;

    private static string GetHelpFile(string assemblyPath) => Path.GetFileName(assemblyPath) + "-Help.xml";
  }
}
