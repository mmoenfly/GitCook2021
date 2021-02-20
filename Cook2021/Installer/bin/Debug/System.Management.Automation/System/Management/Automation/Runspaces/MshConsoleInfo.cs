// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Runspaces.MshConsoleInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Security;

namespace System.Management.Automation.Runspaces
{
  internal class MshConsoleInfo
  {
    internal const string MONADCONSOLEFILEEXTENSION = ".psc1";
    internal const string CONSOLEGLOBALVARIABLE = "ConsoleFileName";
    internal const string ConsoleInfoResourceBaseName = "ConsoleInfoErrorStrings";
    private Version psVersion;
    private Collection<PSSnapInInfo> externalPSSnapIns;
    private Collection<PSSnapInInfo> defaultPSSnapIns;
    private bool isDirty;
    private string fileName;
    [TraceSource("MshConsoleInfo", "MshConsoleInfo object that is constructed from a console file.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (MshConsoleInfo), "MshConsoleInfo object that is constructed from a console file.");
    private static PSTraceSource _mshsnapinTracer = PSTraceSource.GetTracer("MshSnapinLoadUnload", "Loading and unloading mshsnapins", false);

    internal Version PSVersion
    {
      get
      {
        using (MshConsoleInfo.tracer.TraceProperty())
          return this.psVersion;
      }
    }

    internal string MajorVersion
    {
      get
      {
        using (MshConsoleInfo.tracer.TraceProperty())
          return this.psVersion.Major.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      }
    }

    internal Collection<PSSnapInInfo> PSSnapIns
    {
      get
      {
        using (MshConsoleInfo.tracer.TraceProperty())
          return this.MergeDefaultExternalMshSnapins();
      }
    }

    internal Collection<PSSnapInInfo> ExternalPSSnapIns
    {
      get
      {
        using (MshConsoleInfo.tracer.TraceProperty())
          return this.externalPSSnapIns;
      }
    }

    internal bool IsDirty
    {
      get
      {
        using (MshConsoleInfo.tracer.TraceProperty())
          return this.isDirty;
      }
    }

    internal string Filename
    {
      get
      {
        using (MshConsoleInfo.tracer.TraceProperty())
          return this.fileName;
      }
    }

    private MshConsoleInfo(Version version)
    {
      using (MshConsoleInfo.tracer.TraceMethod())
      {
        this.psVersion = version;
        this.isDirty = false;
        this.fileName = (string) null;
        this.defaultPSSnapIns = new Collection<PSSnapInInfo>();
        this.externalPSSnapIns = new Collection<PSSnapInInfo>();
      }
    }

    internal static MshConsoleInfo CreateDefaultConfiguration()
    {
      using (MshConsoleInfo.tracer.TraceMethod())
      {
        MshConsoleInfo mshConsoleInfo = new MshConsoleInfo(PSVersionInfo.PSVersion);
        try
        {
          mshConsoleInfo.defaultPSSnapIns = PSSnapInReader.ReadEnginePSSnapIns();
        }
        catch (PSArgumentException ex)
        {
          string resourceString = ResourceManagerCache.GetResourceString("ConsoleInfoErrorStrings", "CannotLoadDefaults");
          MshConsoleInfo._mshsnapinTracer.TraceError(resourceString);
          throw new PSSnapInException(resourceString, (Exception) ex);
        }
        catch (SecurityException ex)
        {
          string resourceString = ResourceManagerCache.GetResourceString("ConsoleInfoErrorStrings", "CannotLoadDefaults");
          MshConsoleInfo._mshsnapinTracer.TraceError(resourceString);
          throw new PSSnapInException(resourceString, (Exception) ex);
        }
        return mshConsoleInfo;
      }
    }

    internal static MshConsoleInfo CreateFromConsoleFile(
      string fileName,
      out PSConsoleLoadException cle)
    {
      using (MshConsoleInfo.tracer.TraceMethod())
      {
        MshConsoleInfo._mshsnapinTracer.WriteLine("Creating console info from file {0}", (object) fileName);
        MshConsoleInfo defaultConfiguration = MshConsoleInfo.CreateDefaultConfiguration();
        string fullPath = Path.GetFullPath(fileName);
        defaultConfiguration.fileName = fullPath;
        defaultConfiguration.Load(fullPath, out cle);
        MshConsoleInfo._mshsnapinTracer.WriteLine("Console info created successfully", new object[0]);
        return defaultConfiguration;
      }
    }

    internal void SaveAsConsoleFile(string path)
    {
      using (MshConsoleInfo.tracer.TraceMethod())
      {
        string path1 = path != null ? path : throw MshConsoleInfo.tracer.NewArgumentNullException(nameof (path));
        if (!Path.IsPathRooted(path1))
          path1 = Path.GetFullPath(this.fileName);
        if (!path1.EndsWith(".psc1", StringComparison.OrdinalIgnoreCase))
        {
          MshConsoleInfo._mshsnapinTracer.TraceError("Console file {0} doesn't have the right extension {1}.", (object) path, (object) ".psc1");
          throw MshConsoleInfo.tracer.NewArgumentException("absolutePath", "ConsoleInfoErrorStrings", "BadConsoleExtension", (object) "");
        }
        PSConsoleFileElement.WriteToFile(this, path1);
        this.fileName = path1;
        this.isDirty = false;
      }
    }

    internal void Save()
    {
      using (MshConsoleInfo.tracer.TraceMethod())
      {
        if (this.fileName == null)
          throw MshConsoleInfo.tracer.NewInvalidOperationException("ConsoleInfoErrorStrings", "SaveDefaultError", (object) "");
        PSConsoleFileElement.WriteToFile(this, this.fileName);
        this.isDirty = false;
      }
    }

    internal PSSnapInInfo AddPSSnapIn(string mshSnapInID)
    {
      using (MshConsoleInfo.tracer.TraceMethod())
      {
        if (string.IsNullOrEmpty(mshSnapInID))
          MshConsoleInfo.tracer.NewArgumentNullException(nameof (mshSnapInID));
        if (this.IsDefaultPSSnapIn(mshSnapInID))
        {
          MshConsoleInfo._mshsnapinTracer.TraceError("MshSnapin {0} can't be added since it is a default mshsnapin", (object) mshSnapInID);
          throw MshConsoleInfo.tracer.NewArgumentException(nameof (mshSnapInID), "ConsoleInfoErrorStrings", "CannotLoadDefault");
        }
        if (this.IsActiveExternalPSSnapIn(mshSnapInID))
        {
          MshConsoleInfo._mshsnapinTracer.TraceError("MshSnapin {0} is already loaded.", (object) mshSnapInID);
          throw MshConsoleInfo.tracer.NewArgumentException(nameof (mshSnapInID), "ConsoleInfoErrorStrings", "PSSnapInAlreadyExists", (object) mshSnapInID);
        }
        PSSnapInInfo psSnapInInfo = PSSnapInReader.Read(this.MajorVersion, mshSnapInID);
        if (!Utils.IsVersionSupported(psSnapInInfo.PSVersion.ToString()))
        {
          MshConsoleInfo._mshsnapinTracer.TraceError("MshSnapin {0} and current monad engine's versions don't match.", (object) mshSnapInID);
          throw MshConsoleInfo.tracer.NewArgumentException(nameof (mshSnapInID), "ConsoleInfoErrorStrings", "AddPSSnapInBadMonadVersion", (object) psSnapInInfo.PSVersion.ToString(), (object) this.psVersion.ToString());
        }
        this.externalPSSnapIns.Add(psSnapInInfo);
        MshConsoleInfo._mshsnapinTracer.WriteLine("MshSnapin {0} successfully added to consoleinfo list.", (object) mshSnapInID);
        this.isDirty = true;
        return psSnapInInfo;
      }
    }

    internal PSSnapInInfo RemovePSSnapIn(string mshSnapInID)
    {
      using (MshConsoleInfo.tracer.TraceMethod())
      {
        if (string.IsNullOrEmpty(mshSnapInID))
          MshConsoleInfo.tracer.NewArgumentNullException(nameof (mshSnapInID));
        PSSnapInInfo.VerifyPSSnapInFormatThrowIfError(mshSnapInID);
        PSSnapInInfo psSnapInInfo = (PSSnapInInfo) null;
        foreach (PSSnapInInfo externalPsSnapIn in this.externalPSSnapIns)
        {
          if (string.Equals(mshSnapInID, externalPsSnapIn.Name, StringComparison.OrdinalIgnoreCase))
          {
            psSnapInInfo = externalPsSnapIn;
            this.externalPSSnapIns.Remove(externalPsSnapIn);
            this.isDirty = true;
            break;
          }
        }
        if (psSnapInInfo != null)
          return psSnapInInfo;
        if (this.IsDefaultPSSnapIn(mshSnapInID))
        {
          MshConsoleInfo._mshsnapinTracer.WriteLine("MshSnapin {0} can't be removed since it is a default mshsnapin.", (object) mshSnapInID);
          throw MshConsoleInfo.tracer.NewArgumentException(nameof (mshSnapInID), "ConsoleInfoErrorStrings", "CannotRemoveDefault", (object) mshSnapInID);
        }
        throw MshConsoleInfo.tracer.NewArgumentException(nameof (mshSnapInID), "ConsoleInfoErrorStrings", "CannotRemovePSSnapIn", (object) mshSnapInID);
      }
    }

    internal Collection<PSSnapInInfo> GetPSSnapIn(
      string pattern,
      bool searchRegistry)
    {
      bool flag = WildcardPattern.ContainsWildcardCharacters(pattern);
      if (!flag)
        PSSnapInInfo.VerifyPSSnapInFormatThrowIfError(pattern);
      Collection<PSSnapInInfo> collection1 = searchRegistry ? PSSnapInReader.ReadAll() : this.PSSnapIns;
      Collection<PSSnapInInfo> collection2 = new Collection<PSSnapInInfo>();
      if (collection1 == null)
        return collection2;
      if (!flag)
      {
        foreach (PSSnapInInfo psSnapInInfo in collection1)
        {
          if (string.Equals(psSnapInInfo.Name, pattern, StringComparison.OrdinalIgnoreCase))
            collection2.Add(psSnapInInfo);
        }
      }
      else
      {
        WildcardPattern wildcardPattern = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
        foreach (PSSnapInInfo psSnapInInfo in collection1)
        {
          if (wildcardPattern.IsMatch(psSnapInInfo.Name))
            collection2.Add(psSnapInInfo);
        }
      }
      return collection2;
    }

    private Collection<PSSnapInInfo> Load(
      string path,
      out PSConsoleLoadException cle)
    {
      using (MshConsoleInfo.tracer.TraceMethod())
      {
        cle = (PSConsoleLoadException) null;
        MshConsoleInfo._mshsnapinTracer.WriteLine("Load mshsnapins from console file {0}", (object) path);
        if (string.IsNullOrEmpty(path))
          throw MshConsoleInfo.tracer.NewArgumentNullException(nameof (path));
        if (!Path.IsPathRooted(path))
        {
          MshConsoleInfo._mshsnapinTracer.TraceError("Console file {0} needs to be a absolute path.", (object) path);
          throw MshConsoleInfo.tracer.NewArgumentException(nameof (path), "ConsoleInfoErrorStrings", "PathNotAbsolute", (object) path);
        }
        if (!path.EndsWith(".psc1", StringComparison.OrdinalIgnoreCase))
        {
          MshConsoleInfo._mshsnapinTracer.TraceError("Console file {0} needs to have {1} extension.", (object) path, (object) ".psc1");
          throw MshConsoleInfo.tracer.NewArgumentException(nameof (path), "ConsoleInfoErrorStrings", "BadConsoleExtension", (object) "");
        }
        PSConsoleFileElement fromFile = PSConsoleFileElement.CreateFromFile(path);
        if (!Utils.IsVersionSupported(fromFile.MonadVersion))
        {
          MshConsoleInfo._mshsnapinTracer.TraceError("Console version {0} is not supported in current monad session.", (object) fromFile.MonadVersion);
          throw MshConsoleInfo.tracer.NewArgumentException("PSVersion", "ConsoleInfoErrorStrings", "BadMonadVersion", (object) fromFile.MonadVersion, (object) this.psVersion.ToString());
        }
        Collection<PSSnapInException> exceptions = new Collection<PSSnapInException>();
        foreach (string psSnapIn in fromFile.PSSnapIns)
        {
          try
          {
            this.AddPSSnapIn(psSnapIn);
          }
          catch (PSArgumentException ex)
          {
            PSSnapInException psSnapInException = new PSSnapInException(psSnapIn, ex.Message, (Exception) ex);
            MshConsoleInfo._mshsnapinTracer.TraceException((Exception) psSnapInException);
            exceptions.Add(psSnapInException);
          }
          catch (SecurityException ex)
          {
            string message = ResourceManagerCache.FormatResourceString("ConsoleInfoErrorStrings", "PSSnapInReadError");
            PSSnapInException psSnapInException = new PSSnapInException(psSnapIn, message, (Exception) ex);
            MshConsoleInfo._mshsnapinTracer.TraceException((Exception) psSnapInException);
            exceptions.Add(psSnapInException);
          }
        }
        if (exceptions.Count > 0)
          cle = new PSConsoleLoadException(this, exceptions);
        this.isDirty = false;
        return this.externalPSSnapIns;
      }
    }

    private bool IsDefaultPSSnapIn(string mshSnapInID)
    {
      foreach (PSSnapInInfo defaultPsSnapIn in this.defaultPSSnapIns)
      {
        if (string.Equals(mshSnapInID, defaultPsSnapIn.Name, StringComparison.OrdinalIgnoreCase))
          return true;
      }
      return false;
    }

    private bool IsActiveExternalPSSnapIn(string mshSnapInID)
    {
      foreach (PSSnapInInfo externalPsSnapIn in this.externalPSSnapIns)
      {
        if (string.Equals(mshSnapInID, externalPsSnapIn.Name, StringComparison.OrdinalIgnoreCase))
          return true;
      }
      return false;
    }

    private Collection<PSSnapInInfo> MergeDefaultExternalMshSnapins()
    {
      using (MshConsoleInfo.tracer.TraceMethod())
      {
        Collection<PSSnapInInfo> collection = new Collection<PSSnapInInfo>();
        foreach (PSSnapInInfo defaultPsSnapIn in this.defaultPSSnapIns)
          collection.Add(defaultPsSnapIn);
        foreach (PSSnapInInfo externalPsSnapIn in this.externalPSSnapIns)
          collection.Add(externalPsSnapIn);
        return collection;
      }
    }
  }
}
