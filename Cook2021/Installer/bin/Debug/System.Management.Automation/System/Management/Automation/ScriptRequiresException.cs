// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ScriptRequiresException
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Resources;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace System.Management.Automation
{
  [Serializable]
  public class ScriptRequiresException : RuntimeException
  {
    private string _commandName = string.Empty;
    private Version _requiresPSVersion;
    private ReadOnlyCollection<string> _missingPSSnapIns = new ReadOnlyCollection<string>((IList<string>) new string[0]);
    private string _requiresShellId;
    private string _requiresShellPath;
    [TraceSource("ScriptRequiresException", "Exception thrown when a script's requirements to run specified by the #requires statements are not met.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ScriptRequiresException), "Exception thrown when a script's requirements to run specified by the #requires statements are not met.");

    internal ScriptRequiresException(
      string commandName,
      uint lineNumber,
      string requiresShellId,
      string requiresShellPath,
      string errorId)
      : base(ScriptRequiresException.BuildMessage(commandName, lineNumber, requiresShellId, requiresShellPath, true))
    {
      this._commandName = commandName;
      this._requiresShellId = requiresShellId;
      this._requiresShellPath = requiresShellPath;
      this.SetErrorId(errorId);
      this.SetTargetObject((object) commandName);
      this.SetErrorCategory(ErrorCategory.ResourceUnavailable);
    }

    internal ScriptRequiresException(
      string commandName,
      uint lineNumber,
      Version requiresPSVersion,
      string currentPSVersion,
      string errorId)
      : base(ScriptRequiresException.BuildMessage(commandName, lineNumber, requiresPSVersion.ToString(), currentPSVersion, false))
    {
      this._commandName = commandName;
      this._requiresPSVersion = requiresPSVersion;
      this.SetErrorId(errorId);
      this.SetTargetObject((object) commandName);
      this.SetErrorCategory(ErrorCategory.ResourceUnavailable);
    }

    internal ScriptRequiresException(
      string commandName,
      Collection<string> missingPSSnapIns,
      string errorId)
      : base(ScriptRequiresException.BuildMessage(commandName, missingPSSnapIns))
    {
      this._commandName = commandName;
      this._missingPSSnapIns = new ReadOnlyCollection<string>((IList<string>) missingPSSnapIns);
      this.SetErrorId(errorId);
      this.SetTargetObject((object) commandName);
      this.SetErrorCategory(ErrorCategory.ResourceUnavailable);
    }

    public ScriptRequiresException()
    {
    }

    public ScriptRequiresException(string message)
      : base(message)
    {
    }

    public ScriptRequiresException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected ScriptRequiresException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this._commandName = info.GetString(nameof (CommandName));
      this._requiresPSVersion = (Version) info.GetValue(nameof (RequiresPSVersion), typeof (Version));
      this._missingPSSnapIns = (ReadOnlyCollection<string>) info.GetValue(nameof (MissingPSSnapIns), typeof (ReadOnlyCollection<string>));
      this._requiresShellId = info.GetString(nameof (RequiresShellId));
      this._requiresShellPath = info.GetString(nameof (RequiresShellPath));
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new PSArgumentNullException(nameof (info));
      base.GetObjectData(info, context);
      info.AddValue("CommandName", (object) this._commandName);
      info.AddValue("RequiresPSVersion", (object) this._requiresPSVersion, typeof (Version));
      info.AddValue("MissingPSSnapIns", (object) this._missingPSSnapIns, typeof (ReadOnlyCollection<string>));
      info.AddValue("RequiresShellId", (object) this._requiresShellId);
      info.AddValue("RequiresShellPath", (object) this._requiresShellPath);
    }

    public string CommandName => this._commandName;

    public Version RequiresPSVersion => this._requiresPSVersion;

    public ReadOnlyCollection<string> MissingPSSnapIns => this._missingPSSnapIns;

    public string RequiresShellId => this._requiresShellId;

    public string RequiresShellPath => this._requiresShellPath;

    private static string BuildMessage(string commandName, Collection<string> missingPSSnapIns)
    {
      string resourceId = "RequiresMissingPSSnapIns";
      StringBuilder stringBuilder = new StringBuilder();
      if (missingPSSnapIns == null)
        throw ScriptRequiresException.tracer.NewArgumentNullException(nameof (missingPSSnapIns));
      foreach (string missingPsSnapIn in missingPSSnapIns)
        stringBuilder.Append(missingPsSnapIn).Append(", ");
      if (stringBuilder.Length > 1)
        stringBuilder.Remove(stringBuilder.Length - 2, 2);
      try
      {
        return ResourceManagerCache.FormatResourceString("DiscoveryExceptions", resourceId, (object) commandName, (object) stringBuilder.ToString());
      }
      catch (MissingManifestResourceException ex)
      {
        ScriptRequiresException.tracer.TraceException((Exception) ex);
        return ResourceManagerCache.FormatResourceString("SessionStateStrings", "ResourceStringLoadError", (object) commandName, (object) "DiscoveryExceptions", (object) resourceId, (object) ex.Message);
      }
      catch (FormatException ex)
      {
        ScriptRequiresException.tracer.TraceException((Exception) ex);
        return ResourceManagerCache.FormatResourceString("SessionStateStrings", "ResourceStringFormatError", (object) commandName, (object) "DiscoveryExceptions", (object) resourceId, (object) ex.Message);
      }
    }

    private static string BuildMessage(
      string commandName,
      uint lineNumber,
      string first,
      string second,
      bool forShellId)
    {
      string resourceId = !forShellId ? "RequiresPSVersionNotCompatible" : (!string.IsNullOrEmpty(first) ? (string.IsNullOrEmpty(second) ? "RequiresInterpreterNotCompatibleNoPath" : "RequiresInterpreterNotCompatible") : "RequiresShellIDInvalidForSingleShell");
      try
      {
        return ResourceManagerCache.FormatResourceString("DiscoveryExceptions", resourceId, (object) commandName, (object) lineNumber, (object) first, (object) second);
      }
      catch (MissingManifestResourceException ex)
      {
        ScriptRequiresException.tracer.TraceException((Exception) ex);
        return ResourceManagerCache.FormatResourceString("SessionStateStrings", "ResourceStringLoadError", (object) commandName, (object) "DiscoveryExceptions", (object) resourceId, (object) ex.Message);
      }
      catch (FormatException ex)
      {
        ScriptRequiresException.tracer.TraceException((Exception) ex);
        return ResourceManagerCache.FormatResourceString("SessionStateStrings", "ResourceStringFormatError", (object) commandName, (object) "DiscoveryExceptions", (object) resourceId, (object) ex.Message);
      }
    }
  }
}
