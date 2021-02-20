// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Verbs
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Reflection;

namespace System.Management.Automation
{
  internal static class Verbs
  {
    private static Dictionary<string, bool> validVerbs = new Dictionary<string, bool>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private static Dictionary<string, string[]> recommendedAlternateVerbs = new Dictionary<string, string[]>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);

    static Verbs()
    {
      Type[] typeArray = new Type[7]
      {
        typeof (VerbsCommon),
        typeof (VerbsCommunications),
        typeof (VerbsData),
        typeof (VerbsDiagnostic),
        typeof (VerbsLifecycle),
        typeof (VerbsOther),
        typeof (VerbsSecurity)
      };
      foreach (Type type in typeArray)
      {
        foreach (FieldInfo field in type.GetFields())
        {
          if (field.IsLiteral)
            Verbs.validVerbs.Add((string) field.GetValue((object) null), true);
        }
      }
      Verbs.recommendedAlternateVerbs.Add("accept", new string[1]
      {
        "Receive"
      });
      Verbs.recommendedAlternateVerbs.Add("acquire", new string[2]
      {
        "Get",
        "Read"
      });
      Verbs.recommendedAlternateVerbs.Add("allocate", new string[1]
      {
        "New"
      });
      Verbs.recommendedAlternateVerbs.Add("allow", new string[3]
      {
        "Enable",
        "Grant",
        "Unblock"
      });
      Verbs.recommendedAlternateVerbs.Add("amend", new string[1]
      {
        "Edit"
      });
      Verbs.recommendedAlternateVerbs.Add("analyze", new string[2]
      {
        "Measure",
        "Test"
      });
      Verbs.recommendedAlternateVerbs.Add("append", new string[1]
      {
        "Add"
      });
      Verbs.recommendedAlternateVerbs.Add("assign", new string[1]
      {
        "Set"
      });
      Verbs.recommendedAlternateVerbs.Add("associate", new string[2]
      {
        "Join",
        "Merge"
      });
      Verbs.recommendedAlternateVerbs.Add("attach", new string[2]
      {
        "Add",
        "Debug"
      });
      Verbs.recommendedAlternateVerbs.Add("bc", new string[1]
      {
        "Compare"
      });
      Verbs.recommendedAlternateVerbs.Add("boot", new string[1]
      {
        "Start"
      });
      Verbs.recommendedAlternateVerbs.Add("break", new string[1]
      {
        "Disconnect"
      });
      Verbs.recommendedAlternateVerbs.Add("broadcast", new string[1]
      {
        "Send"
      });
      Verbs.recommendedAlternateVerbs.Add("build", new string[1]
      {
        "New"
      });
      Verbs.recommendedAlternateVerbs.Add("burn", new string[1]
      {
        "Backup"
      });
      Verbs.recommendedAlternateVerbs.Add("calculate", new string[1]
      {
        "Measure"
      });
      Verbs.recommendedAlternateVerbs.Add("cancel", new string[1]
      {
        "Stop"
      });
      Verbs.recommendedAlternateVerbs.Add("cat", new string[1]
      {
        "Get"
      });
      Verbs.recommendedAlternateVerbs.Add("change", new string[3]
      {
        "Convert",
        "Edit",
        "Rename"
      });
      Verbs.recommendedAlternateVerbs.Add("clean", new string[1]
      {
        "Uninstall"
      });
      Verbs.recommendedAlternateVerbs.Add("clone", new string[1]
      {
        "Copy"
      });
      Verbs.recommendedAlternateVerbs.Add("combine", new string[2]
      {
        "Join",
        "Merge"
      });
      Verbs.recommendedAlternateVerbs.Add("compact", new string[1]
      {
        "Compress"
      });
      Verbs.recommendedAlternateVerbs.Add("concatenate", new string[1]
      {
        "Add"
      });
      Verbs.recommendedAlternateVerbs.Add("configure", new string[1]
      {
        "Set"
      });
      Verbs.recommendedAlternateVerbs.Add("create", new string[1]
      {
        "New"
      });
      Verbs.recommendedAlternateVerbs.Add("cut", new string[1]
      {
        "Remove"
      });
      Verbs.recommendedAlternateVerbs.Add("delete", new string[1]
      {
        "Remove"
      });
      Verbs.recommendedAlternateVerbs.Add("deploy", new string[2]
      {
        "Install",
        "Publish"
      });
      Verbs.recommendedAlternateVerbs.Add("detach", new string[2]
      {
        "Dismount",
        "Remove"
      });
      Verbs.recommendedAlternateVerbs.Add("determine", new string[2]
      {
        "Measure",
        "Resolve"
      });
      Verbs.recommendedAlternateVerbs.Add("diagnose", new string[2]
      {
        "Debug",
        "Test"
      });
      Verbs.recommendedAlternateVerbs.Add("diff", new string[2]
      {
        "Checkpoint",
        "Compare"
      });
      Verbs.recommendedAlternateVerbs.Add("difference", new string[2]
      {
        "Checkpoint",
        "Compare"
      });
      Verbs.recommendedAlternateVerbs.Add("dig", new string[1]
      {
        "Trace"
      });
      Verbs.recommendedAlternateVerbs.Add("dir", new string[1]
      {
        "Get"
      });
      Verbs.recommendedAlternateVerbs.Add("discard", new string[1]
      {
        "Remove"
      });
      Verbs.recommendedAlternateVerbs.Add("display", new string[2]
      {
        "Show",
        "Write"
      });
      Verbs.recommendedAlternateVerbs.Add("dispose", new string[1]
      {
        "Remove"
      });
      Verbs.recommendedAlternateVerbs.Add("divide", new string[1]
      {
        "Split"
      });
      Verbs.recommendedAlternateVerbs.Add("dump", new string[1]
      {
        "Get"
      });
      Verbs.recommendedAlternateVerbs.Add("duplicate", new string[1]
      {
        "Copy"
      });
      Verbs.recommendedAlternateVerbs.Add("empty", new string[1]
      {
        "Clear"
      });
      Verbs.recommendedAlternateVerbs.Add("end", new string[1]
      {
        "Stop"
      });
      Verbs.recommendedAlternateVerbs.Add("erase", new string[2]
      {
        "Clear",
        "Remove"
      });
      Verbs.recommendedAlternateVerbs.Add("examine", new string[1]
      {
        "Get"
      });
      Verbs.recommendedAlternateVerbs.Add("execute", new string[1]
      {
        "Invoke"
      });
      Verbs.recommendedAlternateVerbs.Add("explode", new string[1]
      {
        "Expand"
      });
      Verbs.recommendedAlternateVerbs.Add("extract", new string[1]
      {
        "Export"
      });
      Verbs.recommendedAlternateVerbs.Add("fix", new string[2]
      {
        "Repair",
        "Restore"
      });
      Verbs.recommendedAlternateVerbs.Add("flush", new string[1]
      {
        "Clear"
      });
      Verbs.recommendedAlternateVerbs.Add("follow", new string[1]
      {
        "Trace"
      });
      Verbs.recommendedAlternateVerbs.Add("generate", new string[1]
      {
        "New"
      });
      Verbs.recommendedAlternateVerbs.Add("halt", new string[1]
      {
        "Disable"
      });
      Verbs.recommendedAlternateVerbs.Add("in", new string[1]
      {
        "ConvertTo"
      });
      Verbs.recommendedAlternateVerbs.Add("index", new string[1]
      {
        "Update"
      });
      Verbs.recommendedAlternateVerbs.Add("initiate", new string[1]
      {
        "Start"
      });
      Verbs.recommendedAlternateVerbs.Add("input", new string[2]
      {
        "ConvertTo",
        "Unregister"
      });
      Verbs.recommendedAlternateVerbs.Add("insert", new string[2]
      {
        "Add",
        "Unregister"
      });
      Verbs.recommendedAlternateVerbs.Add("inspect", new string[1]
      {
        "Trace"
      });
      Verbs.recommendedAlternateVerbs.Add("kill", new string[1]
      {
        "Stop"
      });
      Verbs.recommendedAlternateVerbs.Add("launch", new string[1]
      {
        "Start"
      });
      Verbs.recommendedAlternateVerbs.Add("load", new string[1]
      {
        "Import"
      });
      Verbs.recommendedAlternateVerbs.Add("locate", new string[2]
      {
        "Search",
        "Select"
      });
      Verbs.recommendedAlternateVerbs.Add("logoff", new string[1]
      {
        "Disconnect"
      });
      Verbs.recommendedAlternateVerbs.Add("mail", new string[1]
      {
        "Send"
      });
      Verbs.recommendedAlternateVerbs.Add("make", new string[1]
      {
        "New"
      });
      Verbs.recommendedAlternateVerbs.Add("match", new string[1]
      {
        "Select"
      });
      Verbs.recommendedAlternateVerbs.Add("migrate", new string[1]
      {
        "Move"
      });
      Verbs.recommendedAlternateVerbs.Add("modify", new string[1]
      {
        "Edit"
      });
      Verbs.recommendedAlternateVerbs.Add("name", new string[1]
      {
        "Move"
      });
      Verbs.recommendedAlternateVerbs.Add("nullify", new string[1]
      {
        "Clear"
      });
      Verbs.recommendedAlternateVerbs.Add("obtain", new string[1]
      {
        "Get"
      });
      Verbs.recommendedAlternateVerbs.Add("output", new string[1]
      {
        "ConvertFrom"
      });
      Verbs.recommendedAlternateVerbs.Add("pause", new string[2]
      {
        "Suspend",
        "Wait"
      });
      Verbs.recommendedAlternateVerbs.Add("peek", new string[1]
      {
        "Receive"
      });
      Verbs.recommendedAlternateVerbs.Add("permit", new string[1]
      {
        "Enable"
      });
      Verbs.recommendedAlternateVerbs.Add("purge", new string[2]
      {
        "Clear",
        "Remove"
      });
      Verbs.recommendedAlternateVerbs.Add("pick", new string[1]
      {
        "Select"
      });
      Verbs.recommendedAlternateVerbs.Add("prevent", new string[1]
      {
        "Block"
      });
      Verbs.recommendedAlternateVerbs.Add("print", new string[1]
      {
        "Write"
      });
      Verbs.recommendedAlternateVerbs.Add("prompt", new string[1]
      {
        "Read"
      });
      Verbs.recommendedAlternateVerbs.Add("put", new string[2]
      {
        "Send",
        "Write"
      });
      Verbs.recommendedAlternateVerbs.Add("puts", new string[1]
      {
        "Write"
      });
      Verbs.recommendedAlternateVerbs.Add("quota", new string[1]
      {
        "Limit"
      });
      Verbs.recommendedAlternateVerbs.Add("quote", new string[1]
      {
        "Limit"
      });
      Verbs.recommendedAlternateVerbs.Add("rebuild", new string[1]
      {
        "Initialize"
      });
      Verbs.recommendedAlternateVerbs.Add("recycle", new string[1]
      {
        "Restart"
      });
      Verbs.recommendedAlternateVerbs.Add("refresh", new string[1]
      {
        "Update"
      });
      Verbs.recommendedAlternateVerbs.Add("reinitialize", new string[1]
      {
        "Initialize"
      });
      Verbs.recommendedAlternateVerbs.Add("release", new string[4]
      {
        "Clear",
        "Install",
        "Publish",
        "Unlock"
      });
      Verbs.recommendedAlternateVerbs.Add("reload", new string[1]
      {
        "Update"
      });
      Verbs.recommendedAlternateVerbs.Add("renew", new string[2]
      {
        "Initialize",
        "Update"
      });
      Verbs.recommendedAlternateVerbs.Add("replicate", new string[1]
      {
        "Copy"
      });
      Verbs.recommendedAlternateVerbs.Add("resample", new string[1]
      {
        "Convert"
      });
      Verbs.recommendedAlternateVerbs.Add("resize", new string[1]
      {
        "Convert"
      });
      Verbs.recommendedAlternateVerbs.Add("restrict", new string[1]
      {
        "Lock"
      });
      Verbs.recommendedAlternateVerbs.Add("return", new string[2]
      {
        "Repair",
        "Restore"
      });
      Verbs.recommendedAlternateVerbs.Add("revert", new string[1]
      {
        "Unpublish"
      });
      Verbs.recommendedAlternateVerbs.Add("revise", new string[1]
      {
        "Edit"
      });
      Verbs.recommendedAlternateVerbs.Add("run", new string[2]
      {
        "Invoke",
        "Start"
      });
      Verbs.recommendedAlternateVerbs.Add("salvage", new string[1]
      {
        "Test"
      });
      Verbs.recommendedAlternateVerbs.Add("secure", new string[1]
      {
        "Lock"
      });
      Verbs.recommendedAlternateVerbs.Add("separate", new string[1]
      {
        "Split"
      });
      Verbs.recommendedAlternateVerbs.Add("setup", new string[2]
      {
        "Initialize",
        "Install"
      });
      Verbs.recommendedAlternateVerbs.Add("sleep", new string[2]
      {
        "Suspend",
        "Wait"
      });
      Verbs.recommendedAlternateVerbs.Add("starttransaction", new string[1]
      {
        "Checkpoint"
      });
      Verbs.recommendedAlternateVerbs.Add("telnet", new string[1]
      {
        "Connect"
      });
      Verbs.recommendedAlternateVerbs.Add("terminate", new string[1]
      {
        "Stop"
      });
      Verbs.recommendedAlternateVerbs.Add("track", new string[1]
      {
        "Trace"
      });
      Verbs.recommendedAlternateVerbs.Add("transfer", new string[1]
      {
        "Move"
      });
      Verbs.recommendedAlternateVerbs.Add("type", new string[1]
      {
        "Get"
      });
      Verbs.recommendedAlternateVerbs.Add("unite", new string[2]
      {
        "Join",
        "Merge"
      });
      Verbs.recommendedAlternateVerbs.Add("unlink", new string[1]
      {
        "Dismount"
      });
      Verbs.recommendedAlternateVerbs.Add("unmark", new string[1]
      {
        "Clear"
      });
      Verbs.recommendedAlternateVerbs.Add("unrestrict", new string[1]
      {
        "Unlock"
      });
      Verbs.recommendedAlternateVerbs.Add("unsecure", new string[1]
      {
        "Unlock"
      });
      Verbs.recommendedAlternateVerbs.Add("unset", new string[1]
      {
        "Clear"
      });
      Verbs.recommendedAlternateVerbs.Add("verify", new string[1]
      {
        "Test"
      });
    }

    internal static bool IsStandard(string verb) => Verbs.validVerbs.ContainsKey(verb);

    internal static string[] SuggestedAlternates(string verb)
    {
      string[] strArray = (string[]) null;
      Verbs.recommendedAlternateVerbs.TryGetValue(verb, out strArray);
      return strArray;
    }
  }
}
