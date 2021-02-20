// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ResourceManagerCache
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace System.Management.Automation
{
  internal static class ResourceManagerCache
  {
    [TraceSource("ResourceManagerCache", "Maintains a cache of the loaded resource managers")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ResourceManagerCache), "Maintains a cache of the loaded resource managers");
    private static Dictionary<string, Dictionary<string, ResourceManager>> resourceManagerCache = new Dictionary<string, Dictionary<string, ResourceManager>>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private static object syncRoot = new object();
    private static bool DFT_monitorFailingResourceLookup = true;

    internal static ResourceManager GetResourceManager(string baseName)
    {
      using (ResourceManagerCache.tracer.TraceMethod())
        return !string.IsNullOrEmpty(baseName) ? ResourceManagerCache.GetResourceManager(Assembly.GetCallingAssembly(), baseName) : throw ResourceManagerCache.tracer.NewArgumentException(nameof (baseName));
    }

    internal static ResourceManager GetResourceManager(
      Assembly assembly,
      string baseName)
    {
      using (ResourceManagerCache.tracer.TraceMethod())
      {
        if (assembly == null)
          throw ResourceManagerCache.tracer.NewArgumentNullException(nameof (assembly));
        if (string.IsNullOrEmpty(baseName))
          throw ResourceManagerCache.tracer.NewArgumentException(nameof (baseName));
        ResourceManager resourceManager = (ResourceManager) null;
        Dictionary<string, ResourceManager> dictionary1 = (Dictionary<string, ResourceManager>) null;
        lock (ResourceManagerCache.syncRoot)
        {
          if (ResourceManagerCache.resourceManagerCache.ContainsKey(assembly.Location))
          {
            dictionary1 = ResourceManagerCache.resourceManagerCache[assembly.Location];
            if (dictionary1 != null)
            {
              if (dictionary1.ContainsKey(baseName))
                resourceManager = dictionary1[baseName];
            }
          }
        }
        if (resourceManager == null)
        {
          ResourceManagerCache.tracer.WriteLine("Initializing new ResourceManager instance", new object[0]);
          ResourceManagerCache.tracer.WriteLine("Resource base name: {0}", (object) baseName);
          ResourceManagerCache.tracer.WriteLine("Resource assembly location: {0}", (object) assembly.Location);
          resourceManager = ResourceManagerCache.InitRMWithAssembly(baseName, assembly, (Type) null);
          if (dictionary1 != null)
          {
            lock (ResourceManagerCache.syncRoot)
              dictionary1[baseName] = resourceManager;
          }
          else
          {
            Dictionary<string, ResourceManager> dictionary2 = new Dictionary<string, ResourceManager>();
            dictionary2[baseName] = resourceManager;
            lock (ResourceManagerCache.syncRoot)
              ResourceManagerCache.resourceManagerCache[assembly.Location] = dictionary2;
          }
        }
        return resourceManager;
      }
    }

    internal static string GetResourceString(string baseName, string resourceId)
    {
      using (ResourceManagerCache.tracer.TraceMethod())
        return ResourceManagerCache.GetResourceString(Assembly.GetCallingAssembly(), baseName, resourceId);
    }

    internal static bool DFT_DoMonitorFailingResourceLookup
    {
      get => ResourceManagerCache.DFT_monitorFailingResourceLookup;
      set => ResourceManagerCache.DFT_monitorFailingResourceLookup = value;
    }

    internal static string GetResourceString(Assembly assembly, string baseName, string resourceId)
    {
      using (ResourceManagerCache.tracer.TraceMethod())
      {
        if (assembly == null)
          throw ResourceManagerCache.tracer.NewArgumentNullException(nameof (assembly));
        if (string.IsNullOrEmpty(baseName))
          throw ResourceManagerCache.tracer.NewArgumentException(nameof (baseName));
        if (string.IsNullOrEmpty(resourceId))
          throw ResourceManagerCache.tracer.NewArgumentException(nameof (resourceId));
        ResourceManagerCache.tracer.WriteLine("Assembly: {0}", (object) assembly.Location);
        ResourceManagerCache.tracer.WriteLine("BaseName: {0}", (object) baseName);
        ResourceManagerCache.tracer.WriteLine("Resource: {0}", (object) resourceId);
        string str = ResourceManagerCache.GetResourceManager(assembly, baseName).GetString(resourceId);
        if (string.IsNullOrEmpty(str))
        {
          int num = ResourceManagerCache.DFT_monitorFailingResourceLookup ? 1 : 0;
        }
        return str;
      }
    }

    internal static string GetResourceStringForUICulture(
      Assembly assembly,
      string baseName,
      string resourceId,
      CultureInfo currentUICulture)
    {
      using (ResourceManagerCache.tracer.TraceMethod())
      {
        if (assembly == null)
          throw ResourceManagerCache.tracer.NewArgumentNullException(nameof (assembly));
        if (string.IsNullOrEmpty(baseName))
          throw ResourceManagerCache.tracer.NewArgumentException(nameof (baseName));
        if (string.IsNullOrEmpty(resourceId))
          throw ResourceManagerCache.tracer.NewArgumentException(nameof (resourceId));
        if (currentUICulture == null)
          currentUICulture = Thread.CurrentThread.CurrentUICulture;
        ResourceManagerCache.tracer.WriteLine("Assembly: {0}", (object) assembly.Location);
        ResourceManagerCache.tracer.WriteLine("BaseName: {0}", (object) baseName);
        ResourceManagerCache.tracer.WriteLine("Resource: {0}", (object) resourceId);
        ResourceManagerCache.tracer.WriteLine("Culture: {0}", (object) currentUICulture.DisplayName);
        string str = ResourceManagerCache.GetResourceManager(assembly, baseName).GetString(resourceId, currentUICulture);
        if (string.IsNullOrEmpty(str))
        {
          int num = ResourceManagerCache.DFT_monitorFailingResourceLookup ? 1 : 0;
        }
        return str;
      }
    }

    internal static string GetResourceStringForUICulture(
      string baseName,
      string resourceId,
      CultureInfo currentUICulture)
    {
      using (ResourceManagerCache.tracer.TraceMethod())
      {
        if (string.IsNullOrEmpty(baseName))
          throw ResourceManagerCache.tracer.NewArgumentException(nameof (baseName));
        if (string.IsNullOrEmpty(resourceId))
          throw ResourceManagerCache.tracer.NewArgumentException(nameof (resourceId));
        if (currentUICulture == null)
          currentUICulture = Thread.CurrentThread.CurrentUICulture;
        ResourceManagerCache.tracer.WriteLine("BaseName: {0}", (object) baseName);
        ResourceManagerCache.tracer.WriteLine("Resource: {0}", (object) resourceId);
        ResourceManagerCache.tracer.WriteLine("Culture: {0}", (object) currentUICulture.DisplayName);
        string str = ResourceManagerCache.GetResourceManager(Assembly.GetCallingAssembly(), baseName).GetString(resourceId, currentUICulture);
        if (string.IsNullOrEmpty(str))
        {
          int num = ResourceManagerCache.DFT_monitorFailingResourceLookup ? 1 : 0;
        }
        return str;
      }
    }

    internal static string FormatResourceStringUsingCulture(
      CultureInfo currentUICulture,
      CultureInfo currentCulture,
      string baseName,
      string resourceId,
      params object[] args)
    {
      using (ResourceManagerCache.tracer.TraceMethod(resourceId, new object[0]))
      {
        if (currentUICulture == null)
          currentUICulture = Thread.CurrentThread.CurrentUICulture;
        if (currentCulture == null)
          currentCulture = Thread.CurrentThread.CurrentCulture;
        if (string.IsNullOrEmpty(baseName))
          throw ResourceManagerCache.tracer.NewArgumentException(nameof (baseName));
        if (string.IsNullOrEmpty(resourceId))
          throw ResourceManagerCache.tracer.NewArgumentException(nameof (resourceId));
        string stringForUiCulture = ResourceManagerCache.GetResourceStringForUICulture(Assembly.GetCallingAssembly(), baseName, resourceId, currentUICulture);
        string str = (string) null;
        if (stringForUiCulture != null)
          str = string.Format((IFormatProvider) currentCulture, stringForUiCulture, args);
        return str;
      }
    }

    internal static string FormatResourceStringUsingCulture(
      CultureInfo currentUICulture,
      CultureInfo currentCulture,
      Assembly assembly,
      string baseName,
      string resourceId,
      params object[] args)
    {
      using (ResourceManagerCache.tracer.TraceMethod(resourceId, new object[0]))
      {
        if (currentUICulture == null)
          currentUICulture = Thread.CurrentThread.CurrentUICulture;
        if (currentCulture == null)
          currentCulture = Thread.CurrentThread.CurrentCulture;
        if (assembly == null)
          throw ResourceManagerCache.tracer.NewArgumentNullException(nameof (assembly));
        if (string.IsNullOrEmpty(baseName))
          throw ResourceManagerCache.tracer.NewArgumentException(nameof (baseName));
        if (string.IsNullOrEmpty(resourceId))
          throw ResourceManagerCache.tracer.NewArgumentException(nameof (resourceId));
        string stringForUiCulture = ResourceManagerCache.GetResourceStringForUICulture(assembly, baseName, resourceId, currentUICulture);
        string str = (string) null;
        if (stringForUiCulture != null)
          str = string.Format((IFormatProvider) currentCulture, stringForUiCulture, args);
        return str;
      }
    }

    internal static string FormatResourceString(
      Assembly assembly,
      string baseName,
      string resourceId,
      params object[] args)
    {
      using (ResourceManagerCache.tracer.TraceMethod(resourceId, new object[0]))
      {
        if (assembly == null)
          throw ResourceManagerCache.tracer.NewArgumentNullException(nameof (assembly));
        if (string.IsNullOrEmpty(baseName))
          throw ResourceManagerCache.tracer.NewArgumentException(nameof (baseName));
        if (string.IsNullOrEmpty(resourceId))
          throw ResourceManagerCache.tracer.NewArgumentException(nameof (resourceId));
        string resourceString = ResourceManagerCache.GetResourceString(assembly, baseName, resourceId);
        string str = (string) null;
        if (resourceString != null)
          str = string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, resourceString, args);
        return str;
      }
    }

    internal static string FormatResourceString(
      string baseName,
      string resourceId,
      params object[] args)
    {
      using (ResourceManagerCache.tracer.TraceMethod(resourceId, new object[0]))
      {
        if (string.IsNullOrEmpty(baseName))
          throw ResourceManagerCache.tracer.NewArgumentException(nameof (baseName));
        string format = !string.IsNullOrEmpty(resourceId) ? ResourceManagerCache.GetResourceString(Assembly.GetCallingAssembly(), baseName, resourceId) : throw ResourceManagerCache.tracer.NewArgumentException(nameof (resourceId));
        string str = (string) null;
        if (format != null)
          str = string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, format, args);
        return str;
      }
    }

    private static ResourceManager InitRMWithAssembly(
      string baseName,
      Assembly assemblyToUse,
      Type usingResourceSet)
    {
      using (ResourceManagerCache.tracer.TraceMethod(baseName, new object[0]))
      {
        ResourceManager resourceManager;
        if (usingResourceSet != null && baseName != null && assemblyToUse != null)
          resourceManager = new ResourceManager(baseName, assemblyToUse, usingResourceSet);
        else if (usingResourceSet != null && baseName == null && assemblyToUse == null)
        {
          resourceManager = new ResourceManager(usingResourceSet);
        }
        else
        {
          if (usingResourceSet != null || baseName == null || assemblyToUse == null)
            throw ResourceManagerCache.tracer.NewArgumentException(nameof (assemblyToUse));
          resourceManager = new ResourceManager(baseName, assemblyToUse);
        }
        return resourceManager;
      }
    }
  }
}
