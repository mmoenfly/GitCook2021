// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandPathSearch
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace System.Management.Automation
{
  internal class CommandPathSearch : 
    IEnumerable<string>,
    IEnumerable,
    IEnumerator<string>,
    IDisposable,
    IEnumerator
  {
    [TraceSource("CommandSearch", "CommandSearch")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("CommandSearch", "CommandSearch");
    private LookupPathCollection lookupPaths;
    private IEnumerator<string> lookupPathsEnumerator;
    private Collection<string> currentDirectoryResults;
    private IEnumerator<string> currentDirectoryResultsEnumerator;
    private IEnumerable<string> patterns;
    private IEnumerator<string> patternEnumerator;
    private ExecutionContext _context;
    private bool justReset;

    internal CommandPathSearch(
      IEnumerable<string> patterns,
      IEnumerable<string> lookupPaths,
      ExecutionContext context)
    {
      if (patterns == null)
        throw CommandPathSearch.tracer.NewArgumentNullException(nameof (patterns));
      if (lookupPaths == null)
        throw CommandPathSearch.tracer.NewArgumentNullException(nameof (lookupPaths));
      this._context = context != null ? context : throw CommandPathSearch.tracer.NewArgumentNullException(nameof (context));
      this.patterns = patterns;
      this.lookupPaths = new LookupPathCollection(lookupPaths);
      this.ResolveCurrentDirectoryInLookupPaths();
      this.Reset();
    }

    private void ResolveCurrentDirectoryInLookupPaths()
    {
      SortedList sortedList = new SortedList();
      int num1 = 0;
      string fileSystem = this._context.ProviderNames.FileSystem;
      SessionStateInternal engineSessionState = this._context.EngineSessionState;
      bool flag = engineSessionState.CurrentDrive != (PSDriveInfo) null && engineSessionState.CurrentDrive.Provider.NameEquals(fileSystem) && engineSessionState.IsProviderLoaded(fileSystem);
      string currentDirectory = Environment.CurrentDirectory;
      LocationGlobber locationGlobber = this._context.LocationGlobber;
      foreach (int index in this.lookupPaths.IndexOfRelativePath())
      {
        string str1 = (string) null;
        string str2 = (string) null;
        CommandDiscovery.discoveryTracer.WriteLine("Lookup directory \"{0}\" appears to be a relative path. Attempting resolution...", (object) this.lookupPaths[index]);
        if (flag)
        {
          ProviderInfo provider = (ProviderInfo) null;
          try
          {
            str2 = locationGlobber.GetProviderPath(this.lookupPaths[index], out provider);
          }
          catch (ProviderInvocationException ex)
          {
            CommandDiscovery.discoveryTracer.WriteLine("The relative path '{0}', could not be resolved because the provider threw an exception: '{1}'", (object) this.lookupPaths[index], (object) ex.Message);
          }
          catch (InvalidOperationException ex)
          {
            CommandDiscovery.discoveryTracer.WriteLine("The relative path '{0}', could not resolve a home directory for the provider", (object) this.lookupPaths[index]);
          }
          if (!string.IsNullOrEmpty(str2))
          {
            CommandDiscovery.discoveryTracer.TraceError("The relative path resolved to: {0}", (object) str2);
            str1 = str2;
          }
          else
            CommandDiscovery.discoveryTracer.WriteLine("The relative path was not a file system path. {0}", (object) this.lookupPaths[index]);
        }
        else
        {
          CommandDiscovery.discoveryTracer.TraceWarning("The current drive is not set, using the process current directory: {0}", (object) currentDirectory);
          str1 = currentDirectory;
        }
        if (str1 != null)
        {
          int num2 = this.lookupPaths.IndexOf(str1);
          if (num2 != -1)
          {
            if (num2 > index)
            {
              sortedList.Add((object) num1++, (object) num2);
              this.lookupPaths[index] = str1;
            }
            else
              sortedList.Add((object) num1++, (object) index);
          }
          else
            this.lookupPaths[index] = str1;
        }
        else
          sortedList.Add((object) num1++, (object) index);
      }
      for (int count = sortedList.Count; count > 0; --count)
        this.lookupPaths.RemoveAt((int) sortedList[(object) (count - 1)]);
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator() => (IEnumerator<string>) this;

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this;

    public bool MoveNext()
    {
      bool flag = false;
      if (this.justReset)
      {
        this.justReset = false;
        if (!this.patternEnumerator.MoveNext())
        {
          CommandPathSearch.tracer.TraceError("No patterns were specified");
          return false;
        }
        if (!this.lookupPathsEnumerator.MoveNext())
        {
          CommandPathSearch.tracer.TraceError("No lookup paths were specified");
          return false;
        }
        this.GetNewDirectoryResults(this.patternEnumerator.Current, this.lookupPathsEnumerator.Current);
      }
      do
      {
        if (!this.currentDirectoryResultsEnumerator.MoveNext())
        {
          CommandPathSearch.tracer.WriteLine("Current directory results are invalid", new object[0]);
          if (!this.patternEnumerator.MoveNext())
          {
            CommandPathSearch.tracer.WriteLine("Current patterns exhausted in current directory: {0}", (object) this.lookupPathsEnumerator.Current);
          }
          else
          {
            this.GetNewDirectoryResults(this.patternEnumerator.Current, this.lookupPathsEnumerator.Current);
            if (!flag)
              continue;
          }
        }
        else
        {
          CommandPathSearch.tracer.WriteLine("Next path found: {0}", (object) this.currentDirectoryResultsEnumerator.Current);
          flag = true;
        }
        if (!flag)
        {
          if (!this.lookupPathsEnumerator.MoveNext())
          {
            CommandPathSearch.tracer.WriteLine("All lookup paths exhausted, no more matches can be found", new object[0]);
            break;
          }
          this.patternEnumerator = this.patterns.GetEnumerator();
          if (!this.patternEnumerator.MoveNext())
          {
            CommandPathSearch.tracer.WriteLine("All patterns exhausted, no more matches can be found", new object[0]);
            break;
          }
          this.GetNewDirectoryResults(this.patternEnumerator.Current, this.lookupPathsEnumerator.Current);
        }
        else
          break;
      }
      while (!flag);
      CommandPathSearch.tracer.WriteLine("result = {0}", (object) flag);
      return flag;
    }

    public void Reset()
    {
      this.lookupPathsEnumerator = this.lookupPaths.GetEnumerator();
      this.patternEnumerator = this.patterns.GetEnumerator();
      this.currentDirectoryResults = new Collection<string>();
      this.currentDirectoryResultsEnumerator = this.currentDirectoryResults.GetEnumerator();
      this.justReset = true;
    }

    string IEnumerator<string>.Current
    {
      get
      {
        if (this.currentDirectoryResults == null)
          throw CommandPathSearch.tracer.NewInvalidOperationException();
        return this.currentDirectoryResultsEnumerator.Current;
      }
    }

    object IEnumerator.Current => (object) ((IEnumerator<string>) this).Current;

    public void Dispose()
    {
      this.Reset();
      GC.SuppressFinalize((object) this);
    }

    private void GetNewDirectoryResults(string pattern, string directory)
    {
      using (CommandPathSearch.tracer.TraceMethod("pattern: {0} lookupPath: {1}", (object) pattern, (object) directory))
      {
        this.currentDirectoryResults = CommandPathSearch.GetMatchingPathsInDirectory(pattern, directory);
        this.currentDirectoryResultsEnumerator = this.currentDirectoryResults.GetEnumerator();
      }
    }

    private static Collection<string> GetMatchingPathsInDirectory(
      string pattern,
      string directory)
    {
      Collection<string> collection = new Collection<string>();
      try
      {
        CommandDiscovery.discoveryTracer.WriteLine("Looking for {0} in {1}", (object) pattern, (object) directory);
        collection = SessionStateUtilities.ConvertArrayToCollection<string>(Directory.GetFiles(directory, pattern));
      }
      catch (ArgumentException ex)
      {
        CommandPathSearch.tracer.TraceException((Exception) ex);
        CommandDiscovery.discoveryTracer.TraceException((Exception) ex);
      }
      catch (IOException ex)
      {
        CommandPathSearch.tracer.TraceException((Exception) ex);
        CommandDiscovery.discoveryTracer.TraceException((Exception) ex);
      }
      catch (UnauthorizedAccessException ex)
      {
        CommandPathSearch.tracer.TraceException((Exception) ex);
        CommandDiscovery.discoveryTracer.TraceException((Exception) ex);
      }
      catch (NotSupportedException ex)
      {
        CommandPathSearch.tracer.TraceException((Exception) ex);
        CommandDiscovery.discoveryTracer.TraceException((Exception) ex);
      }
      return collection;
    }
  }
}
