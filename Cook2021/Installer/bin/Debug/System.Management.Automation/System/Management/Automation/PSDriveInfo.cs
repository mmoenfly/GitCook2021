// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSDriveInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;

namespace System.Management.Automation
{
  public class PSDriveInfo : IComparable
  {
    [TraceSource("PSDriveInfo", "The namespace navigation tracer")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (PSDriveInfo), "The namespace navigation tracer");
    private string currentWorkingDirectory;
    private string name;
    private ProviderInfo provider;
    private string root;
    private string description;
    private PSCredential credentials = PSCredential.Empty;
    private bool driveBeingCreated;
    private bool isAutoMounted;
    private bool isAutoMountedManuallyRemoved;
    private bool hidden;

    public string CurrentLocation
    {
      get
      {
        using (PSDriveInfo.tracer.TraceProperty(this.currentWorkingDirectory, new object[0]))
          return this.currentWorkingDirectory;
      }
      set
      {
        using (PSDriveInfo.tracer.TraceProperty(value, new object[0]))
          this.currentWorkingDirectory = value;
      }
    }

    public string Name => this.name;

    public ProviderInfo Provider => this.provider;

    public string Root => this.root;

    internal void SetRoot(string path)
    {
      if (path == null)
        throw PSDriveInfo.tracer.NewArgumentNullException(nameof (path));
      if (!this.driveBeingCreated)
        throw (NotSupportedException) PSDriveInfo.tracer.NewNotSupportedException();
      this.root = path;
    }

    public string Description
    {
      get => this.description;
      set => this.description = value;
    }

    public PSCredential Credential => this.credentials;

    internal bool DriveBeingCreated
    {
      set => this.driveBeingCreated = value;
    }

    internal bool IsAutoMounted
    {
      get => this.isAutoMounted;
      set => this.isAutoMounted = value;
    }

    internal bool IsAutoMountedManuallyRemoved
    {
      get => this.isAutoMountedManuallyRemoved;
      set => this.isAutoMountedManuallyRemoved = value;
    }

    protected PSDriveInfo(PSDriveInfo driveInfo)
    {
      using (PSDriveInfo.tracer.TraceConstructor((object) this))
      {
        this.name = !(driveInfo == (PSDriveInfo) null) ? driveInfo.Name : throw PSDriveInfo.tracer.NewArgumentNullException(nameof (driveInfo));
        this.provider = driveInfo.Provider;
        this.credentials = driveInfo.Credential;
        this.currentWorkingDirectory = driveInfo.CurrentLocation;
        this.description = driveInfo.Description;
        this.driveBeingCreated = driveInfo.driveBeingCreated;
        this.hidden = driveInfo.hidden;
        this.isAutoMounted = driveInfo.isAutoMounted;
        this.root = driveInfo.root;
        this.Trace();
      }
    }

    public PSDriveInfo(
      string name,
      ProviderInfo provider,
      string root,
      string description,
      PSCredential credential)
    {
      using (PSDriveInfo.tracer.TraceConstructor((object) this))
      {
        if (name == null)
          throw PSDriveInfo.tracer.NewArgumentNullException(nameof (name));
        if (provider == null)
          throw PSDriveInfo.tracer.NewArgumentNullException(nameof (provider));
        if (root == null)
          throw PSDriveInfo.tracer.NewArgumentNullException(nameof (root));
        this.name = name;
        this.provider = provider;
        this.root = root;
        this.description = description;
        if (credential != null)
          this.credentials = credential;
        this.currentWorkingDirectory = string.Empty;
        this.Trace();
      }
    }

    public override string ToString() => this.Name;

    internal bool Hidden
    {
      get => this.hidden;
      set => this.hidden = value;
    }

    internal void SetName(string newName) => this.name = !string.IsNullOrEmpty(newName) ? newName : throw PSDriveInfo.tracer.NewArgumentException(nameof (newName));

    internal void SetProvider(ProviderInfo newProvider) => this.provider = newProvider != null ? newProvider : throw PSDriveInfo.tracer.NewArgumentNullException(nameof (newProvider));

    internal void Trace()
    {
      PSDriveInfo.tracer.WriteLine("A drive was found:", new object[0]);
      if (this.Name != null)
        PSDriveInfo.tracer.WriteLine("\tName: {0}", (object) this.Name);
      if (this.Provider != null)
        PSDriveInfo.tracer.WriteLine("\tProvider: {0}", (object) this.Provider);
      if (this.Root != null)
        PSDriveInfo.tracer.WriteLine("\tRoot: {0}", (object) this.Root);
      if (this.CurrentLocation != null)
        PSDriveInfo.tracer.WriteLine("\tCWD: {0}", (object) this.CurrentLocation);
      if (this.Description == null)
        return;
      PSDriveInfo.tracer.WriteLine("\tDescription: {0}", (object) this.Description);
    }

    public int CompareTo(PSDriveInfo drive)
    {
      if (drive == (PSDriveInfo) null)
        throw PSDriveInfo.tracer.NewArgumentNullException(nameof (drive));
      return string.Compare(this.Name, drive.Name, true, CultureInfo.CurrentCulture);
    }

    public int CompareTo(object obj)
    {
      PSDriveInfo drive = obj as PSDriveInfo;
      return !(drive == (PSDriveInfo) null) ? this.CompareTo(drive) : throw (ArgumentException) PSDriveInfo.tracer.NewArgumentException(nameof (obj), "SessionStateStrings", "OnlyAbleToComparePSDriveInfo");
    }

    public override bool Equals(object obj) => (object) (obj as PSDriveInfo) != null && this.CompareTo(obj) == 0;

    public bool Equals(PSDriveInfo drive) => this.CompareTo(drive) == 0;

    public static bool operator ==(PSDriveInfo drive1, PSDriveInfo drive2)
    {
      object obj1 = (object) drive1;
      object obj2 = (object) drive2;
      if (obj1 == null != (obj2 == null))
        return false;
      return obj1 == null || drive1.Equals(drive2);
    }

    public static bool operator !=(PSDriveInfo drive1, PSDriveInfo drive2) => !(drive1 == drive2);

    public static bool operator <(PSDriveInfo drive1, PSDriveInfo drive2)
    {
      object obj1 = (object) drive1;
      object obj2 = (object) drive2;
      return obj1 == null ? obj2 != null : obj2 != null && drive1.CompareTo(drive2) < 0;
    }

    public static bool operator >(PSDriveInfo drive1, PSDriveInfo drive2)
    {
      object obj1 = (object) drive1;
      object obj2 = (object) drive2;
      return obj1 == null ? obj2 == null && false : obj2 == null || drive1.CompareTo(drive2) > 0;
    }

    public override int GetHashCode() => base.GetHashCode();
  }
}
