// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.PSCredential
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Net;
using System.Security;

namespace System.Management.Automation
{
  public sealed class PSCredential
  {
    private const string resBaseName = "Credential";
    [TraceSource("PSCredential", "PSCredential")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (PSCredential), nameof (PSCredential));
    private string _userName;
    private SecureString _password;
    private NetworkCredential _netCred;
    private static readonly PSCredential empty = new PSCredential();

    public string UserName => this._userName;

    public SecureString Password => this._password;

    public PSCredential(string userName, SecureString password)
    {
      using (PSCredential.tracer.TraceConstructor((object) userName))
      {
        Utils.CheckArgForNullOrEmpty(PSCredential.tracer, userName, nameof (userName));
        Utils.CheckArgForNull(PSCredential.tracer, (object) password, nameof (password));
        this._userName = userName;
        this._password = password;
      }
    }

    private PSCredential()
    {
      using (PSCredential.tracer.TraceConstructor((object) "default-ctor"))
        ;
    }

    public NetworkCredential GetNetworkCredential()
    {
      using (PSCredential.tracer.TraceMethod())
      {
        if (this._netCred == null)
        {
          string user = (string) null;
          string domain = (string) null;
          if (PSCredential.IsValidUserName(this._userName, out user, out domain))
            this._netCred = new NetworkCredential(user, Utils.GetStringFromSecureString(this._password), domain);
        }
        return this._netCred;
      }
    }

    public static explicit operator NetworkCredential(PSCredential credential) => credential != null ? credential.GetNetworkCredential() : throw PSCredential.tracer.NewArgumentNullException(nameof (credential));

    public static PSCredential Empty
    {
      get
      {
        using (PSCredential.tracer.TraceProperty())
          return PSCredential.empty;
      }
    }

    private static bool IsValidUserName(string input, out string user, out string domain)
    {
      using (PSCredential.tracer.TraceMethod(input, new object[0]))
      {
        PSCredential.SplitUserDomain(input, out user, out domain);
        if (user == null || domain == null || user.Length == 0)
          throw PSCredential.tracer.NewArgumentException("UserName", "Credential", "InvalidUserNameFormat");
        return true;
      }
    }

    private static void SplitUserDomain(string input, out string user, out string domain)
    {
      user = (string) null;
      domain = (string) null;
      int length1;
      if ((length1 = input.IndexOf('\\')) >= 0)
      {
        user = input.Substring(length1 + 1);
        domain = input.Substring(0, length1);
      }
      else
      {
        int length2;
        if ((length2 = input.LastIndexOf('@')) >= 0)
        {
          domain = input.Substring(length2 + 1);
          user = input.Substring(0, length2);
        }
        else
        {
          user = input;
          domain = "";
        }
      }
    }
  }
}
