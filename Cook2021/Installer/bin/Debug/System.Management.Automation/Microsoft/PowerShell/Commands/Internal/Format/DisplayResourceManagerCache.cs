// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Format.DisplayResourceManagerCache
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System;
using System.Collections;
using System.Management.Automation;
using System.Reflection;
using System.Resources;
using System.Security;

namespace Microsoft.PowerShell.Commands.Internal.Format
{
  internal sealed class DisplayResourceManagerCache
  {
    [TraceSource("DisplayResourceManagerCache", "DisplayResourceManagerCache")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (DisplayResourceManagerCache), nameof (DisplayResourceManagerCache));
    private DisplayResourceManagerCache.AssemblyNameResolver _assemblyNameResolver = new DisplayResourceManagerCache.AssemblyNameResolver();
    private Hashtable _resourceReferenceToAssemblyCache = new Hashtable();

    internal string GetTextTokenString(TextToken tt)
    {
      if (tt.resource != null)
      {
        string str = this.GetString(tt.resource);
        if (str != null)
          return str;
      }
      return tt.text;
    }

    internal void VerifyResource(
      StringResourceReference resourceReference,
      out DisplayResourceManagerCache.LoadingResult result,
      out DisplayResourceManagerCache.AssemblyBindingStatus bindingStatus)
    {
      this.GetStringHelper(resourceReference, out result, out bindingStatus);
    }

    private string GetString(StringResourceReference resourceReference) => this.GetStringHelper(resourceReference, out DisplayResourceManagerCache.LoadingResult _, out DisplayResourceManagerCache.AssemblyBindingStatus _);

    private string GetStringHelper(
      StringResourceReference resourceReference,
      out DisplayResourceManagerCache.LoadingResult result,
      out DisplayResourceManagerCache.AssemblyBindingStatus bindingStatus)
    {
      result = DisplayResourceManagerCache.LoadingResult.AssemblyNotFound;
      bindingStatus = DisplayResourceManagerCache.AssemblyBindingStatus.NotFound;
      DisplayResourceManagerCache.AssemblyLoadResult assemblyLoadResult;
      if (this._resourceReferenceToAssemblyCache.Contains((object) resourceReference))
      {
        assemblyLoadResult = this._resourceReferenceToAssemblyCache[(object) resourceReference] as DisplayResourceManagerCache.AssemblyLoadResult;
        bindingStatus = assemblyLoadResult.status;
      }
      else
      {
        bool foundInGac;
        assemblyLoadResult = new DisplayResourceManagerCache.AssemblyLoadResult()
        {
          a = this.LoadAssemblyFromResourceReference(resourceReference, out foundInGac)
        };
        assemblyLoadResult.status = assemblyLoadResult.a != null ? (foundInGac ? DisplayResourceManagerCache.AssemblyBindingStatus.FoundInGac : DisplayResourceManagerCache.AssemblyBindingStatus.FoundInPath) : DisplayResourceManagerCache.AssemblyBindingStatus.NotFound;
        this._resourceReferenceToAssemblyCache.Add((object) resourceReference, (object) assemblyLoadResult);
      }
      bindingStatus = assemblyLoadResult.status;
      if (assemblyLoadResult.a == null)
      {
        result = DisplayResourceManagerCache.LoadingResult.AssemblyNotFound;
        return (string) null;
      }
      try
      {
        string resourceString = ResourceManagerCache.GetResourceString(assemblyLoadResult.a, resourceReference.baseName, resourceReference.resourceId);
        if (resourceString == null)
        {
          result = DisplayResourceManagerCache.LoadingResult.StringNotFound;
          return (string) null;
        }
        result = DisplayResourceManagerCache.LoadingResult.NoError;
        return resourceString;
      }
      catch (InvalidOperationException ex)
      {
        result = DisplayResourceManagerCache.LoadingResult.ResourceNotFound;
      }
      catch (MissingManifestResourceException ex)
      {
        result = DisplayResourceManagerCache.LoadingResult.ResourceNotFound;
      }
      catch (Exception ex)
      {
        throw;
      }
      return (string) null;
    }

    private Assembly LoadAssemblyFromResourceReference(
      StringResourceReference resourceReference,
      out bool foundInGac)
    {
      foundInGac = false;
      return this._assemblyNameResolver.ResolveAssemblyName(resourceReference.assemblyName);
    }

    internal enum LoadingResult
    {
      NoError,
      AssemblyNotFound,
      ResourceNotFound,
      StringNotFound,
    }

    internal enum AssemblyBindingStatus
    {
      NotFound,
      FoundInGac,
      FoundInPath,
    }

    private sealed class AssemblyLoadResult
    {
      internal Assembly a;
      internal DisplayResourceManagerCache.AssemblyBindingStatus status;
    }

    private class AssemblyNameResolver
    {
      private Hashtable _assemblyReferences = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);

      internal Assembly ResolveAssemblyName(string assemblyName)
      {
        if (string.IsNullOrEmpty(assemblyName))
          return (Assembly) null;
        if (this._assemblyReferences.Contains((object) assemblyName))
          return (Assembly) this._assemblyReferences[(object) assemblyName];
        Assembly assembly = DisplayResourceManagerCache.AssemblyNameResolver.ResolveAssemblyNameInLoadedAssemblies(assemblyName, true) ?? DisplayResourceManagerCache.AssemblyNameResolver.ResolveAssemblyNameInLoadedAssemblies(assemblyName, false);
        this._assemblyReferences.Add((object) assemblyName, (object) assembly);
        return assembly;
      }

      private static Assembly ResolveAssemblyNameInLoadedAssemblies(
        string assemblyName,
        bool fullName)
      {
        Assembly assembly1 = (Assembly) null;
        foreach (Assembly assembly2 in AppDomain.CurrentDomain.GetAssemblies())
        {
          AssemblyName name;
          try
          {
            name = assembly2.GetName();
          }
          catch (SecurityException ex)
          {
            continue;
          }
          catch (Exception ex)
          {
            throw;
          }
          if (fullName)
          {
            if (string.Equals(name.FullName, assemblyName, StringComparison.Ordinal))
              return assembly2;
          }
          else if (string.Equals(name.Name, assemblyName, StringComparison.Ordinal))
            return assembly2;
        }
        return assembly1;
      }
    }
  }
}
