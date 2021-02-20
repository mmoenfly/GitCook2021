// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.PSSenderInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Remoting
{
  public sealed class PSSenderInfo
  {
    private PSPrincipal userPrinicpal;
    private TimeZone clientTimeZone;
    private string connectionString;
    private PSPrimitiveDictionary applicationArguments;

    internal PSSenderInfo(PSPrincipal userPrincipal, string httpURL)
    {
      this.userPrinicpal = userPrincipal;
      this.connectionString = httpURL;
    }

    public PSPrincipal UserInfo => this.userPrinicpal;

    public TimeZone ClientTimeZone
    {
      get => this.clientTimeZone;
      internal set => this.clientTimeZone = value;
    }

    public string ConnectionString => this.connectionString;

    public PSPrimitiveDictionary ApplicationArguments
    {
      get => this.applicationArguments;
      internal set => this.applicationArguments = value;
    }
  }
}
