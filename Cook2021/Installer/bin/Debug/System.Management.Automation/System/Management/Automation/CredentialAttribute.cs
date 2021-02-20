// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CredentialAttribute
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
  public sealed class CredentialAttribute : ArgumentTransformationAttribute
  {
    [TraceSource("CredentialAttribute", "CredentialAttribute")]
    private static readonly PSTraceSource credTracer = PSTraceSource.GetTracer(nameof (CredentialAttribute), nameof (CredentialAttribute));

    public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
    {
      using (CredentialAttribute.credTracer.TraceMethod())
      {
        PSCredential psCredential = (PSCredential) null;
        string userName = (string) null;
        bool flag = false;
        if (engineIntrinsics == null || engineIntrinsics.Host == null || engineIntrinsics.Host.UI == null)
          throw CredentialAttribute.credTracer.NewArgumentNullException(nameof (engineIntrinsics));
        if (inputData == null)
        {
          flag = true;
        }
        else
        {
          psCredential = LanguagePrimitives.FromObjectAs<PSCredential>(inputData);
          if (psCredential == null)
          {
            flag = true;
            userName = LanguagePrimitives.FromObjectAs<string>(inputData);
            if (userName == null)
              throw new PSArgumentException("userName");
          }
        }
        if (flag)
        {
          try
          {
            string resourceString1 = ResourceManagerCache.GetResourceString(nameof (CredentialAttribute), "CredentialAttribute_Prompt_Caption");
            string resourceString2 = ResourceManagerCache.GetResourceString(nameof (CredentialAttribute), "CredentialAttribute_Prompt");
            psCredential = engineIntrinsics.Host.UI.PromptForCredential(resourceString1, resourceString2, userName, "");
          }
          catch (ArgumentTransformationMetadataException ex)
          {
            CredentialAttribute.credTracer.TraceException((Exception) ex);
            throw;
          }
        }
        return (object) psCredential;
      }
    }
  }
}
