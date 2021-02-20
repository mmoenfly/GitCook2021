// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.CommandHelpProvider
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Security;
using System.Xml;

namespace System.Management.Automation
{
  internal class CommandHelpProvider : HelpProviderWithCache
  {
    private ExecutionContext _context;
    private Hashtable _helpFiles = new Hashtable();
    [TraceSource("CommandHelpProvider", "CommandHelpProvider")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (CommandHelpProvider), nameof (CommandHelpProvider));

    internal CommandHelpProvider(HelpSystem helpSystem)
      : base(helpSystem)
      => this._context = helpSystem.ExecutionContext;

    internal override string Name => "Command Help Provider";

    internal override HelpCategory HelpCategory => HelpCategory.Alias | HelpCategory.Cmdlet | HelpCategory.ScriptCommand | HelpCategory.Function | HelpCategory.Filter | HelpCategory.ExternalScript;

    private HelpInfo GetHelpInfo(CmdletInfo cmdletInfo, bool reportErrors)
    {
      if (this.GetFromCommandCache(cmdletInfo.ModuleName, cmdletInfo.Name) == null)
      {
        string helpFile = this.FindHelpFile(cmdletInfo);
        if (helpFile != null && !this._helpFiles.Contains((object) helpFile))
          this.LoadHelpFile(helpFile, cmdletInfo.ModuleName, cmdletInfo.Name, reportErrors);
      }
      HelpInfo cacheOrCmdletInfo = this.GetFromCommandCacheOrCmdletInfo(cmdletInfo);
      if (cacheOrCmdletInfo != null && !string.IsNullOrEmpty(cmdletInfo.ModuleName) && cacheOrCmdletInfo.FullHelp.Properties["PSSnapIn"] == null)
        cacheOrCmdletInfo.FullHelp.Properties.Add((PSPropertyInfo) new PSNoteProperty("PSSnapIn", (object) cmdletInfo.PSSnapIn));
      return cacheOrCmdletInfo;
    }

    private HelpInfo GetHelpInfo(IScriptCommandInfo scriptCommandInfo)
    {
      CommandInfo commandInfo = (CommandInfo) scriptCommandInfo;
      HelpInfo helpInfo = (HelpInfo) null;
      ScriptBlock scriptBlock = scriptCommandInfo.ScriptBlock;
      if (scriptBlock != null)
      {
        if (helpInfo == null)
        {
          string helpFile = scriptBlock.GetHelpFile(this._context, commandInfo);
          if (helpFile != null)
          {
            if (!this._helpFiles.Contains((object) helpFile))
              this.LoadHelpFile(helpFile, helpFile, commandInfo.Name, true);
            helpInfo = this.GetFromCommandCache(helpFile, commandInfo.Name);
          }
        }
        if (helpInfo == null)
          helpInfo = scriptBlock.GetHelpInfo(this._context, commandInfo);
        if (helpInfo == null && commandInfo != null)
          helpInfo = (HelpInfo) SyntaxHelpInfo.GetHelpInfo(commandInfo.Name, commandInfo.Syntax, commandInfo.HelpCategory);
      }
      return helpInfo;
    }

    internal override IEnumerable<HelpInfo> ExactMatchHelp(
      HelpRequest helpRequest)
    {
      int countHelpInfosFound = 0;
      string target = helpRequest.Target;
      Hashtable hashtable = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);
      CommandSearcher searcher = new CommandSearcher(target, SearchResolutionOptions.AllowDuplicateCmdletNames, CommandTypes.Function | CommandTypes.Filter | CommandTypes.Cmdlet | CommandTypes.ExternalScript, this._context);
      while (searcher.MoveNext())
      {
        CommandInfo current = ((IEnumerator<CommandInfo>) searcher).Current;
        if (SessionState.IsVisible(helpRequest.CommandOrigin, current))
        {
          CmdletInfo cmdletInfo = current as CmdletInfo;
          HelpInfo helpInfo = (HelpInfo) null;
          string helpName = (string) null;
          if (cmdletInfo != null)
          {
            helpInfo = this.GetHelpInfo(cmdletInfo, true);
            helpName = cmdletInfo.FullName;
          }
          else if (current is IScriptCommandInfo scriptCommandInfo)
          {
            helpName = current.Name;
            helpInfo = this.GetHelpInfo(scriptCommandInfo);
          }
          if (helpInfo != null && helpName != null && (!hashtable.ContainsKey((object) helpName) && CommandHelpProvider.Match(helpInfo, helpRequest, current)))
          {
            ++countHelpInfosFound;
            hashtable.Add((object) helpName, (object) null);
            yield return helpInfo;
            if (countHelpInfosFound >= helpRequest.MaxResults && helpRequest.MaxResults > 0)
              break;
          }
        }
      }
    }

    private static string GetCmdletAssemblyPath(CmdletInfo cmdletInfo)
    {
      if (cmdletInfo == null)
        return (string) null;
      return cmdletInfo.ImplementingType == null ? (string) null : Path.GetDirectoryName(cmdletInfo.ImplementingType.Assembly.Location);
    }

    private string FindHelpFile(CmdletInfo cmdletInfo)
    {
      string path2 = cmdletInfo != null ? cmdletInfo.HelpFile : throw CommandHelpProvider.tracer.NewArgumentNullException(nameof (cmdletInfo));
      if (string.IsNullOrEmpty(path2))
        return path2;
      string file = path2;
      PSSnapInInfo psSnapIn = cmdletInfo.PSSnapIn;
      Collection<string> searchPaths = new Collection<string>();
      if (psSnapIn != null)
        file = Path.Combine(psSnapIn.ApplicationBase, path2);
      else if (cmdletInfo.Module != null && !string.IsNullOrEmpty(cmdletInfo.Module.Path))
      {
        file = Path.Combine(cmdletInfo.Module.ModuleBase, path2);
      }
      else
      {
        searchPaths.Add(this.GetDefaultShellSearchPath());
        searchPaths.Add(CommandHelpProvider.GetCmdletAssemblyPath(cmdletInfo));
      }
      string str = MUIFileSearcher.LocateFile(file, searchPaths);
      if (string.IsNullOrEmpty(str))
        CommandHelpProvider.tracer.WriteLine("Unable to load file {0}", (object) file);
      return str;
    }

    private void LoadHelpFile(
      string helpFile,
      string helpFileIdentifier,
      string commandName,
      bool reportErrors)
    {
      try
      {
        this.LoadHelpFile(helpFile, helpFileIdentifier);
      }
      catch (IOException ex)
      {
        if (!reportErrors)
          return;
        this.ReportHelpFileError((Exception) ex, commandName, helpFile);
      }
      catch (SecurityException ex)
      {
        if (!reportErrors)
          return;
        this.ReportHelpFileError((Exception) ex, commandName, helpFile);
      }
      catch (XmlException ex)
      {
        if (!reportErrors)
          return;
        this.ReportHelpFileError((Exception) ex, commandName, helpFile);
      }
    }

    private void LoadHelpFile(string helpFile, string helpFileIdentifier)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.Load(helpFile);
      this._helpFiles[(object) helpFile] = (object) 0;
      XmlNode helpItemsNode = (XmlNode) null;
      if (xmlDocument.HasChildNodes)
      {
        for (int i = 0; i < xmlDocument.ChildNodes.Count; ++i)
        {
          XmlNode childNode = xmlDocument.ChildNodes[i];
          if (childNode.NodeType == XmlNodeType.Element && string.Compare(childNode.LocalName, "helpItems", StringComparison.OrdinalIgnoreCase) == 0)
          {
            helpItemsNode = childNode;
            break;
          }
        }
      }
      if (helpItemsNode == null)
      {
        CommandHelpProvider.tracer.WriteLine("Unable to find 'helpItems' element in file {0}", (object) helpFile);
      }
      else
      {
        bool flag = CommandHelpProvider.IsMamlHelp(helpFile, helpItemsNode);
        using (this.HelpSystem.Trace(helpFile))
        {
          if (!helpItemsNode.HasChildNodes)
            return;
          for (int i = 0; i < helpItemsNode.ChildNodes.Count; ++i)
          {
            XmlNode childNode = helpItemsNode.ChildNodes[i];
            if (childNode.NodeType == XmlNodeType.Element && string.Compare(childNode.LocalName, "command", StringComparison.OrdinalIgnoreCase) == 0)
            {
              MamlCommandHelpInfo helpInfo = (MamlCommandHelpInfo) null;
              if (flag)
                helpInfo = MamlCommandHelpInfo.Load(childNode, HelpCategory.Cmdlet);
              if (helpInfo != null)
              {
                this.HelpSystem.TraceErrors(helpInfo.Errors);
                this.AddToCommandCache(helpFileIdentifier, helpInfo.Name, helpInfo);
              }
            }
            if (childNode.NodeType == XmlNodeType.Element && string.Compare(childNode.Name, "UserDefinedData", StringComparison.OrdinalIgnoreCase) == 0)
            {
              UserDefinedHelpData userDefinedHelpData = UserDefinedHelpData.Load(childNode);
              this.ProcessUserDefineddHelpData(helpFileIdentifier, userDefinedHelpData);
            }
          }
        }
      }
    }

    private void ProcessUserDefineddHelpData(
      string mshSnapInId,
      UserDefinedHelpData userDefinedHelpData)
    {
      if (userDefinedHelpData == null || string.IsNullOrEmpty(userDefinedHelpData.Name))
        return;
      HelpInfo fromCommandCache = this.GetFromCommandCache(mshSnapInId, userDefinedHelpData.Name);
      if (fromCommandCache == null || !(fromCommandCache is MamlCommandHelpInfo mamlCommandHelpInfo))
        return;
      mamlCommandHelpInfo.AddUserDefinedData(userDefinedHelpData);
    }

    private HelpInfo GetFromCommandCache(string helpFileIdentifier, string commandName)
    {
      string target = commandName;
      if (!string.IsNullOrEmpty(helpFileIdentifier))
        target = helpFileIdentifier + "\\" + target;
      return this.GetCache(target);
    }

    private HelpInfo GetFromCommandCacheOrCmdletInfo(CmdletInfo cmdletInfo) => this.GetFromCommandCache(cmdletInfo.ModuleName, cmdletInfo.Name) ?? (HelpInfo) new CmdletHelpInfo(cmdletInfo);

    private void AddToCommandCache(
      string mshSnapInId,
      string cmdletName,
      MamlCommandHelpInfo helpInfo)
    {
      string target = cmdletName;
      helpInfo.FullHelp.TypeNames.Insert(0, string.Format((IFormatProvider) CultureInfo.InvariantCulture, "MamlCommandHelpInfo#{0}#{1}", (object) mshSnapInId, (object) cmdletName));
      if (!string.IsNullOrEmpty(mshSnapInId))
      {
        target = mshSnapInId + "\\" + target;
        helpInfo.FullHelp.TypeNames.Insert(1, string.Format((IFormatProvider) CultureInfo.InvariantCulture, "MamlCommandHelpInfo#{0}", (object) mshSnapInId));
      }
      this.AddCache(target, (HelpInfo) helpInfo);
    }

    internal static bool IsMamlHelp(string helpFile, XmlNode helpItemsNode)
    {
      if (helpFile.EndsWith(".maml", true, CultureInfo.CurrentCulture))
        return true;
      if (helpItemsNode.Attributes == null)
        return false;
      foreach (XmlNode attribute in (XmlNamedNodeMap) helpItemsNode.Attributes)
      {
        if (attribute.Name.Equals("schema", StringComparison.OrdinalIgnoreCase) && attribute.Value.Equals("maml", StringComparison.OrdinalIgnoreCase))
          return true;
      }
      return false;
    }

    internal override IEnumerable<HelpInfo> SearchHelp(
      HelpRequest helpRequest,
      bool searchOnlyContent)
    {
      string target = helpRequest.Target;
      Collection<string> patternList = new Collection<string>();
      WildcardPattern wildCardPattern = (WildcardPattern) null;
      bool decoratedSearch = !WildcardPattern.ContainsWildcardCharacters(helpRequest.Target);
      if (!searchOnlyContent)
      {
        if (decoratedSearch)
        {
          if (target.IndexOf('-') >= 0)
          {
            patternList.Add(target + "*");
          }
          else
          {
            patternList.Add("*" + (object) '-' + "*" + target + "*");
            patternList.Add("*" + target + "*" + (object) '-' + "*");
          }
        }
        else
          patternList.Add(target);
      }
      else
      {
        patternList.Add("*");
        string pattern = helpRequest.Target;
        if (decoratedSearch)
          pattern = "*" + helpRequest.Target + "*";
        wildCardPattern = new WildcardPattern(pattern, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
      }
      int countOfHelpInfoObjectsFound = 0;
      Hashtable hashtable = new Hashtable((IEqualityComparer) StringComparer.OrdinalIgnoreCase);
      foreach (string commandName in patternList)
      {
        CommandSearcher searcher = new CommandSearcher(commandName, SearchResolutionOptions.CommandNameIsPattern | SearchResolutionOptions.AllowDuplicateCmdletNames, CommandTypes.Cmdlet, this._context);
        while (searcher.MoveNext())
        {
          CmdletInfo cmdletInfo = ((IEnumerator<CommandInfo>) searcher).Current as CmdletInfo;
          if (cmdletInfo != null && SessionState.IsVisible(helpRequest.CommandOrigin, (CommandInfo) cmdletInfo))
          {
            HelpInfo helpInfo = this.GetHelpInfo(cmdletInfo, !decoratedSearch);
            if (helpInfo != null && !hashtable.ContainsKey((object) cmdletInfo.FullName) && CommandHelpProvider.Match(helpInfo, helpRequest, (CommandInfo) cmdletInfo) && (!searchOnlyContent || helpInfo.MatchPatternInContent(wildCardPattern)))
            {
              hashtable.Add((object) cmdletInfo.FullName, (object) null);
              ++countOfHelpInfoObjectsFound;
              yield return helpInfo;
              if (countOfHelpInfoObjectsFound >= helpRequest.MaxResults && helpRequest.MaxResults > 0)
                yield break;
            }
          }
        }
      }
    }

    private static bool Match(HelpInfo helpInfo, HelpRequest helpRequest, CommandInfo commandInfo)
    {
      if (helpRequest == null)
        return true;
      if ((helpRequest.HelpCategory & commandInfo.HelpCategory) == HelpCategory.None)
        return false;
      switch (helpInfo)
      {
        case BaseCommandHelpInfo _:
        case CmdletHelpInfo _:
          return CommandHelpProvider.Match(helpInfo.Component, helpRequest.Component) && CommandHelpProvider.Match(helpInfo.Role, helpRequest.Role) && CommandHelpProvider.Match(helpInfo.Functionality, helpRequest.Functionality);
        default:
          return false;
      }
    }

    private static bool Match(string target, string pattern)
    {
      if (string.IsNullOrEmpty(pattern))
        return true;
      if (string.IsNullOrEmpty(target))
        target = "";
      return new WildcardPattern(pattern, WildcardOptions.IgnoreCase).IsMatch(target);
    }

    private static bool Match(string target, string[] patterns)
    {
      if (patterns == null || patterns.Length == 0)
        return true;
      foreach (string pattern in patterns)
      {
        if (CommandHelpProvider.Match(target, pattern))
          return true;
      }
      return false;
    }

    internal override IEnumerable<HelpInfo> ProcessForwardedHelp(
      HelpInfo helpInfo,
      HelpRequest helpRequest)
    {
      HelpCategory categoriesHandled = HelpCategory.Alias | HelpCategory.ScriptCommand | HelpCategory.Function | HelpCategory.Filter | HelpCategory.ExternalScript;
      if ((helpInfo.HelpCategory & categoriesHandled) != HelpCategory.None)
      {
        HelpRequest commandHelpRequest = helpRequest.Clone();
        commandHelpRequest.Target = helpInfo.ForwardTarget;
        commandHelpRequest.CommandOrigin = CommandOrigin.Internal;
        if (helpInfo.ForwardHelpCategory != HelpCategory.None)
        {
          if (helpInfo.HelpCategory != HelpCategory.Alias)
          {
            commandHelpRequest.HelpCategory = helpInfo.ForwardHelpCategory;
            goto label_6;
          }
        }
        try
        {
          commandHelpRequest.HelpCategory = this._context.CommandDiscovery.LookupCommandInfo(commandHelpRequest.Target).HelpCategory;
        }
        catch (CommandNotFoundException ex)
        {
        }
label_6:
        foreach (HelpInfo helpInfo1 in this.ExactMatchHelp(commandHelpRequest))
          yield return helpInfo1;
      }
      else
        yield return helpInfo;
    }

    internal override void Reset()
    {
      base.Reset();
      this._helpFiles.Clear();
    }
  }
}
