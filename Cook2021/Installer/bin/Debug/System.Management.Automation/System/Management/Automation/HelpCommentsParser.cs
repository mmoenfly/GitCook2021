// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.HelpCommentsParser
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace System.Management.Automation
{
  internal class HelpCommentsParser
  {
    private HelpCommentsParser.CommentBlockSections sections = new HelpCommentsParser.CommentBlockSections();
    private ScriptBlock scriptBlock;
    private CommandMetadata commandMetadata;
    private string commandName;
    private List<List<Token>> parameterComments;
    private XmlDocument doc;
    private static readonly string mamlURI = "http://schemas.microsoft.com/maml/2004/10";
    private static readonly string commandURI = "http://schemas.microsoft.com/maml/dev/command/2004/10";
    private static readonly string devURI = "http://schemas.microsoft.com/maml/dev/2004/10";
    private static readonly string directive = "^\\s*\\.(\\w+)(\\s+(\\S+))?\\s*$";
    private static readonly string blankline = "^\\s*$";

    private HelpCommentsParser()
    {
    }

    private HelpCommentsParser(CommandInfo commandInfo, List<List<Token>> parameterComments)
    {
      switch (commandInfo)
      {
        case FunctionInfo functionInfo:
          this.scriptBlock = functionInfo.ScriptBlock;
          this.commandName = functionInfo.Name;
          break;
        case ExternalScriptInfo externalScriptInfo:
          this.scriptBlock = externalScriptInfo.ScriptBlock;
          this.commandName = externalScriptInfo.Path;
          break;
      }
      this.commandMetadata = commandInfo.CommandMetadata;
      this.parameterComments = parameterComments;
    }

    private void DetermineParameterDescriptions()
    {
      int index = 0;
      foreach (string key in this.commandMetadata.StaticCommandParameterMetadata.BindableParameters.Keys)
      {
        string section;
        if (!this.sections.parameters.TryGetValue(key.ToUpperInvariant(), out section) && index < this.parameterComments.Count)
        {
          List<string> commentLines = new List<string>();
          foreach (Token comment in this.parameterComments[index])
            this.CollectCommentText(comment, commentLines);
          int i = -1;
          section = this.GetSection(commentLines, ref i);
          this.sections.parameters.Add(key.ToUpperInvariant(), section);
        }
        ++index;
      }
    }

    private string GetParameterDescription(string parameterName)
    {
      string str = "";
      this.sections.parameters.TryGetValue(parameterName.ToUpperInvariant(), out str);
      return str;
    }

    private XmlElement BuildXmlForParameter(
      string parameterName,
      bool isMandatory,
      bool valueFromPipeline,
      bool valueFromPipelineByPropertyName,
      string position,
      Type type,
      string description,
      bool forSyntax)
    {
      XmlElement element1 = this.doc.CreateElement("command:parameter", HelpCommentsParser.commandURI);
      element1.SetAttribute("required", isMandatory ? "true" : "false");
      string str = !valueFromPipeline || !valueFromPipelineByPropertyName ? (!valueFromPipeline ? (!valueFromPipelineByPropertyName ? "false" : "true (ByPropertyName)") : "true (ByValue)") : "true (ByValue, ByPropertyName)";
      element1.SetAttribute("pipelineInput", str);
      element1.SetAttribute(nameof (position), position);
      XmlElement element2 = this.doc.CreateElement("maml:name", HelpCommentsParser.mamlURI);
      XmlText textNode1 = this.doc.CreateTextNode(parameterName);
      element1.AppendChild((XmlNode) element2).AppendChild((XmlNode) textNode1);
      if (!string.IsNullOrEmpty(description))
      {
        XmlElement element3 = this.doc.CreateElement("maml:description", HelpCommentsParser.mamlURI);
        XmlElement element4 = this.doc.CreateElement("maml:para", HelpCommentsParser.mamlURI);
        XmlText textNode2 = this.doc.CreateTextNode(description);
        element1.AppendChild((XmlNode) element3).AppendChild((XmlNode) element4).AppendChild((XmlNode) textNode2);
      }
      if (type == null)
        type = typeof (object);
      if (type.IsEnum)
      {
        XmlElement element3 = this.doc.CreateElement("command:parameterValueGroup", HelpCommentsParser.commandURI);
        foreach (string name in Enum.GetNames(type))
        {
          XmlElement element4 = this.doc.CreateElement("command:parameterValue", HelpCommentsParser.commandURI);
          element4.SetAttribute("required", "false");
          XmlText textNode2 = this.doc.CreateTextNode(name);
          element3.AppendChild((XmlNode) element4).AppendChild((XmlNode) textNode2);
        }
        element1.AppendChild((XmlNode) element3);
      }
      else
      {
        bool flag = type == typeof (SwitchParameter);
        if (!forSyntax || !flag)
        {
          XmlElement element3 = this.doc.CreateElement("command:parameterValue", HelpCommentsParser.commandURI);
          element3.SetAttribute("required", flag ? "false" : "true");
          XmlText textNode2 = this.doc.CreateTextNode(type.Name);
          element1.AppendChild((XmlNode) element3).AppendChild((XmlNode) textNode2);
        }
      }
      if (!forSyntax)
      {
        XmlElement element3 = this.doc.CreateElement("dev:type", HelpCommentsParser.devURI);
        XmlElement element4 = this.doc.CreateElement("maml:name", HelpCommentsParser.mamlURI);
        XmlText textNode2 = this.doc.CreateTextNode(type.Name);
        element1.AppendChild((XmlNode) element3).AppendChild((XmlNode) element4).AppendChild((XmlNode) textNode2);
      }
      return element1;
    }

    internal XmlDocument BuildXmlFromComments()
    {
      this.doc = new XmlDocument();
      XmlElement element1 = this.doc.CreateElement("command:command", HelpCommentsParser.commandURI);
      element1.SetAttribute("xmlns:maml", HelpCommentsParser.mamlURI);
      element1.SetAttribute("xmlns:command", HelpCommentsParser.commandURI);
      element1.SetAttribute("xmlns:dev", HelpCommentsParser.devURI);
      this.doc.AppendChild((XmlNode) element1);
      XmlElement element2 = this.doc.CreateElement("command:details", HelpCommentsParser.commandURI);
      element1.AppendChild((XmlNode) element2);
      XmlElement element3 = this.doc.CreateElement("command:name", HelpCommentsParser.commandURI);
      XmlText textNode1 = this.doc.CreateTextNode(this.commandName);
      element2.AppendChild((XmlNode) element3).AppendChild((XmlNode) textNode1);
      if (!string.IsNullOrEmpty(this.sections.synopsis))
      {
        XmlElement element4 = this.doc.CreateElement("maml:description", HelpCommentsParser.mamlURI);
        XmlElement element5 = this.doc.CreateElement("maml:para", HelpCommentsParser.mamlURI);
        XmlText textNode2 = this.doc.CreateTextNode(this.sections.synopsis);
        element2.AppendChild((XmlNode) element4).AppendChild((XmlNode) element5).AppendChild((XmlNode) textNode2);
      }
      this.DetermineParameterDescriptions();
      XmlElement element6 = this.doc.CreateElement("command:syntax", HelpCommentsParser.commandURI);
      MergedCommandParameterMetadata parameterMetadata = this.commandMetadata.StaticCommandParameterMetadata;
      if (parameterMetadata.ParameterSetCount > 0)
      {
        for (int i = 0; i < parameterMetadata.ParameterSetCount; ++i)
          this.BuildSyntaxForParameterSet(element1, element6, parameterMetadata, i);
      }
      else
        this.BuildSyntaxForParameterSet(element1, element6, parameterMetadata, int.MaxValue);
      XmlElement element7 = this.doc.CreateElement("command:parameters", HelpCommentsParser.commandURI);
      foreach (KeyValuePair<string, MergedCompiledCommandParameter> bindableParameter in parameterMetadata.BindableParameters)
      {
        MergedCompiledCommandParameter commandParameter = bindableParameter.Value;
        if (commandParameter.BinderAssociation != ParameterBinderAssociation.CommonParameters)
        {
          string key = bindableParameter.Key;
          string parameterDescription = this.GetParameterDescription(key);
          ParameterSetSpecificMetadata specificMetadata = (ParameterSetSpecificMetadata) null;
          bool isMandatory = false;
          bool valueFromPipeline = false;
          bool valueFromPipelineByPropertyName = false;
          string position = "named";
          int num = 0;
          CompiledCommandParameter parameter = commandParameter.Parameter;
          parameter.ParameterSetData.TryGetValue("__AllParameterSets", out specificMetadata);
          while (specificMetadata == null && num < 32)
            specificMetadata = parameter.GetParameterSetData((uint) (1 << num++));
          if (specificMetadata != null)
          {
            isMandatory = specificMetadata.IsMandatory;
            valueFromPipeline = specificMetadata.ValueFromPipeline;
            valueFromPipelineByPropertyName = specificMetadata.ValueFromPipelineByPropertyName;
            position = specificMetadata.IsPositional ? (1 + specificMetadata.Position).ToString((IFormatProvider) CultureInfo.InvariantCulture) : "named";
          }
          XmlElement xmlElement = this.BuildXmlForParameter(key, isMandatory, valueFromPipeline, valueFromPipelineByPropertyName, position, parameter.Type, parameterDescription, false);
          element7.AppendChild((XmlNode) xmlElement);
        }
      }
      element1.AppendChild((XmlNode) element7);
      if (!string.IsNullOrEmpty(this.sections.description))
      {
        XmlElement element4 = this.doc.CreateElement("maml:description", HelpCommentsParser.mamlURI);
        XmlElement element5 = this.doc.CreateElement("maml:para", HelpCommentsParser.mamlURI);
        XmlText textNode2 = this.doc.CreateTextNode(this.sections.description);
        element1.AppendChild((XmlNode) element4).AppendChild((XmlNode) element5).AppendChild((XmlNode) textNode2);
      }
      if (!string.IsNullOrEmpty(this.sections.notes))
      {
        XmlElement element4 = this.doc.CreateElement("maml:alertSet", HelpCommentsParser.mamlURI);
        XmlElement element5 = this.doc.CreateElement("maml:alert", HelpCommentsParser.mamlURI);
        XmlElement element8 = this.doc.CreateElement("maml:para", HelpCommentsParser.mamlURI);
        XmlText textNode2 = this.doc.CreateTextNode(this.sections.notes);
        element1.AppendChild((XmlNode) element4).AppendChild((XmlNode) element5).AppendChild((XmlNode) element8).AppendChild((XmlNode) textNode2);
      }
      if (this.sections.examples.Count > 0)
      {
        XmlElement element4 = this.doc.CreateElement("command:examples", HelpCommentsParser.commandURI);
        int num = 1;
        foreach (string example in this.sections.examples)
        {
          XmlElement element5 = this.doc.CreateElement("command:example", HelpCommentsParser.commandURI);
          XmlElement element8 = this.doc.CreateElement("maml:title", HelpCommentsParser.mamlURI);
          XmlText textNode2 = this.doc.CreateTextNode(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "\t\t\t\t-------------------------- {0} {1} --------------------------", (object) ResourceManagerCache.GetResourceString("HelpDisplayStrings", "ExampleUpperCase"), (object) num++));
          element5.AppendChild((XmlNode) element8).AppendChild((XmlNode) textNode2);
          string prompt_str;
          string code_str;
          string remarks_str;
          this.GetExampleSections(example, out prompt_str, out code_str, out remarks_str);
          XmlElement element9 = this.doc.CreateElement("maml:introduction", HelpCommentsParser.mamlURI);
          XmlElement element10 = this.doc.CreateElement("maml:para", HelpCommentsParser.mamlURI);
          XmlText textNode3 = this.doc.CreateTextNode(prompt_str);
          element5.AppendChild((XmlNode) element9).AppendChild((XmlNode) element10).AppendChild((XmlNode) textNode3);
          XmlElement element11 = this.doc.CreateElement("dev:code", HelpCommentsParser.devURI);
          XmlText textNode4 = this.doc.CreateTextNode(code_str);
          element5.AppendChild((XmlNode) element11).AppendChild((XmlNode) textNode4);
          XmlElement element12 = this.doc.CreateElement("dev:remarks", HelpCommentsParser.devURI);
          XmlElement element13 = this.doc.CreateElement("maml:para", HelpCommentsParser.mamlURI);
          XmlText textNode5 = this.doc.CreateTextNode(remarks_str);
          element5.AppendChild((XmlNode) element12).AppendChild((XmlNode) element13).AppendChild((XmlNode) textNode5);
          for (int index = 0; index < 4; ++index)
            element12.AppendChild((XmlNode) this.doc.CreateElement("maml:para", HelpCommentsParser.mamlURI));
          element4.AppendChild((XmlNode) element5);
        }
        element1.AppendChild((XmlNode) element4);
      }
      if (this.sections.inputs.Count > 0)
      {
        XmlElement element4 = this.doc.CreateElement("command:inputTypes", HelpCommentsParser.commandURI);
        foreach (string input in this.sections.inputs)
        {
          XmlElement element5 = this.doc.CreateElement("command:inputType", HelpCommentsParser.commandURI);
          XmlElement element8 = this.doc.CreateElement("dev:type", HelpCommentsParser.devURI);
          XmlElement element9 = this.doc.CreateElement("maml:name", HelpCommentsParser.mamlURI);
          XmlText textNode2 = this.doc.CreateTextNode(input);
          element4.AppendChild((XmlNode) element5).AppendChild((XmlNode) element8).AppendChild((XmlNode) element9).AppendChild((XmlNode) textNode2);
        }
        element1.AppendChild((XmlNode) element4);
      }
      IEnumerable enumerable = (IEnumerable) null;
      if (this.sections.outputs.Count > 0)
        enumerable = (IEnumerable) this.sections.outputs;
      else if (this.scriptBlock.OutputType.Count > 0)
        enumerable = (IEnumerable) this.scriptBlock.OutputType;
      if (enumerable != null)
      {
        XmlElement element4 = this.doc.CreateElement("command:returnValues", HelpCommentsParser.commandURI);
        foreach (object obj in enumerable)
        {
          XmlElement element5 = this.doc.CreateElement("command:returnValue", HelpCommentsParser.commandURI);
          XmlElement element8 = this.doc.CreateElement("dev:type", HelpCommentsParser.devURI);
          XmlElement element9 = this.doc.CreateElement("maml:name", HelpCommentsParser.mamlURI);
          if (!(obj is string text))
            text = ((PSTypeName) obj).Name;
          XmlText textNode2 = this.doc.CreateTextNode(text);
          element4.AppendChild((XmlNode) element5).AppendChild((XmlNode) element8).AppendChild((XmlNode) element9).AppendChild((XmlNode) textNode2);
        }
        element1.AppendChild((XmlNode) element4);
      }
      if (this.sections.links.Count > 0)
      {
        XmlElement element4 = this.doc.CreateElement("maml:relatedLinks", HelpCommentsParser.mamlURI);
        foreach (string link in this.sections.links)
        {
          XmlElement element5 = this.doc.CreateElement("maml:navigationLink", HelpCommentsParser.mamlURI);
          XmlElement element8 = this.doc.CreateElement(Uri.IsWellFormedUriString(Uri.EscapeUriString(link), UriKind.Absolute) ? "maml:uri" : "maml:linkText", HelpCommentsParser.mamlURI);
          XmlText textNode2 = this.doc.CreateTextNode(link);
          element4.AppendChild((XmlNode) element5).AppendChild((XmlNode) element8).AppendChild((XmlNode) textNode2);
        }
        element1.AppendChild((XmlNode) element4);
      }
      return this.doc;
    }

    private void BuildSyntaxForParameterSet(
      XmlElement command,
      XmlElement syntax,
      MergedCommandParameterMetadata parameterMetadata,
      int i)
    {
      XmlElement element1 = this.doc.CreateElement("command:syntaxItem", HelpCommentsParser.commandURI);
      XmlElement element2 = this.doc.CreateElement("maml:name", HelpCommentsParser.mamlURI);
      XmlText textNode = this.doc.CreateTextNode(this.commandName);
      element1.AppendChild((XmlNode) element2).AppendChild((XmlNode) textNode);
      foreach (MergedCompiledCommandParameter parametersInParameter in parameterMetadata.GetParametersInParameterSet((uint) (1 << i)))
      {
        if (parametersInParameter.BinderAssociation != ParameterBinderAssociation.CommonParameters)
        {
          CompiledCommandParameter parameter = parametersInParameter.Parameter;
          ParameterSetSpecificMetadata parameterSetData = parameter.GetParameterSetData((uint) (1 << i));
          string parameterDescription = this.GetParameterDescription(parameter.Name);
          XmlElement xmlElement = this.BuildXmlForParameter(parameter.Name, parameterSetData.IsMandatory, parameterSetData.ValueFromPipeline, parameterSetData.ValueFromPipelineByPropertyName, parameterSetData.IsPositional ? (1 + parameterSetData.Position).ToString((IFormatProvider) CultureInfo.InvariantCulture) : "named", parameter.Type, parameterDescription, true);
          element1.AppendChild((XmlNode) xmlElement);
        }
      }
      command.AppendChild((XmlNode) syntax).AppendChild((XmlNode) element1);
    }

    private void GetExampleSections(
      string content,
      out string prompt_str,
      out string code_str,
      out string remarks_str)
    {
      prompt_str = code_str = remarks_str = "";
      StringBuilder stringBuilder = new StringBuilder();
      int num = 1;
      foreach (char ch in content)
      {
        if (ch == '>' && num == 1)
        {
          stringBuilder.Append(ch);
          prompt_str = stringBuilder.ToString().Trim();
          stringBuilder = new StringBuilder();
          ++num;
        }
        else if (ch == '\n' && num < 3)
        {
          if (num == 1)
            prompt_str = "C:\\PS>";
          code_str = stringBuilder.ToString().Trim();
          stringBuilder = new StringBuilder();
          num = 3;
        }
        else
          stringBuilder.Append(ch);
      }
      if (num == 1)
      {
        prompt_str = "C:\\PS>";
        code_str = stringBuilder.ToString().Trim();
        remarks_str = "";
      }
      else
        remarks_str = stringBuilder.ToString();
    }

    private void CollectCommentText(Token comment, List<string> commentLines)
    {
      string fullText = comment.FullText;
      int num = 0;
      if (fullText[0] == '<')
      {
        int startIndex = 2;
        int index;
        for (index = 2; index < fullText.Length - 2; ++index)
        {
          if (fullText[index] == '\n')
          {
            commentLines.Add(fullText.Substring(startIndex, index - startIndex));
            startIndex = index + 1;
          }
          else if (fullText[index] == '\r')
          {
            commentLines.Add(fullText.Substring(startIndex, index - startIndex));
            if (fullText[index + 1] == '\n')
              ++index;
            startIndex = index + 1;
          }
        }
        commentLines.Add(fullText.Substring(startIndex, index - startIndex));
      }
      else
      {
        while (num < fullText.Length && fullText[num] == '#')
          ++num;
        commentLines.Add(fullText.Substring(num));
      }
    }

    private string GetSection(List<string> commentLines, ref int i)
    {
      bool flag = false;
      int num1 = 0;
      StringBuilder stringBuilder = new StringBuilder();
      ++i;
      while (i < commentLines.Count)
      {
        string commentLine = commentLines[i];
        if (flag || !Regex.IsMatch(commentLine, HelpCommentsParser.blankline))
        {
          if (Regex.IsMatch(commentLine, HelpCommentsParser.directive))
          {
            --i;
            break;
          }
          if (!flag)
          {
            for (int index = 0; index < commentLine.Length && (commentLine[index] == ' ' || commentLine[index] == '\t' || commentLine[index] == ' '); ++index)
              ++num1;
          }
          flag = true;
          int num2 = 0;
          while (num2 < commentLine.Length && num2 < num1 && (commentLine[num2] == ' ' || commentLine[num2] == '\t' || commentLine[num2] == ' '))
            ++num2;
          stringBuilder.Append(commentLine.Substring(num2));
          stringBuilder.Append('\n');
        }
        ++i;
      }
      return stringBuilder.ToString();
    }

    internal string GetHelpFile(CommandInfo commandInfo)
    {
      if (this.sections.mamlHelpFile == null)
        return (string) null;
      string file1 = this.sections.mamlHelpFile;
      Collection<string> searchPaths = new Collection<string>();
      string file2 = ((IScriptCommandInfo) commandInfo).ScriptBlock.File;
      if (!string.IsNullOrEmpty(file2))
        file1 = Path.Combine(Path.GetDirectoryName(file2), this.sections.mamlHelpFile);
      return MUIFileSearcher.LocateFile(file1, searchPaths);
    }

    internal RemoteHelpInfo GetRemoteHelpInfo(
      ExecutionContext context,
      CommandInfo commandInfo)
    {
      if (string.IsNullOrEmpty(this.sections.forwardHelpTargetName) || string.IsNullOrEmpty(this.sections.remoteHelpRunspace))
        return (RemoteHelpInfo) null;
      object valueToConvert = ((IScriptCommandInfo) commandInfo).ScriptBlock.SessionState.PSVariable.GetValue(this.sections.remoteHelpRunspace);
      PSSession result;
      if (valueToConvert == null || !LanguagePrimitives.TryConvertTo<PSSession>(valueToConvert, out result))
        throw new InvalidOperationException(ResourceManagerCache.FormatResourceString("HelpErrors", "RemoteRunspaceNotAvailable"));
      return new RemoteHelpInfo(context, (RemoteRunspace) result.Runspace, this.sections.forwardHelpTargetName, this.sections.fowardHelpCategory, commandInfo.HelpCategory);
    }

    internal bool AnalyzeCommentBlock(List<Token> comments)
    {
      if (comments == null || comments.Count == 0)
        return false;
      List<string> commentLines = new List<string>();
      foreach (Token comment in comments)
        this.CollectCommentText(comment, commentLines);
      bool flag = false;
      for (int i = 0; i < commentLines.Count; ++i)
      {
        Match match = Regex.Match(commentLines[i], HelpCommentsParser.directive);
        if (match.Success)
        {
          flag = true;
          if (match.Groups[3].Success)
          {
            switch (match.Groups[1].Value.ToUpperInvariant())
            {
              case "PARAMETER":
                string upperInvariant = match.Groups[3].Value.ToUpperInvariant();
                string section = this.GetSection(commentLines, ref i);
                if (!this.sections.parameters.ContainsKey(upperInvariant))
                {
                  this.sections.parameters.Add(upperInvariant, section);
                  continue;
                }
                continue;
              case "FORWARDHELPTARGETNAME":
                this.sections.forwardHelpTargetName = match.Groups[3].Value;
                continue;
              case "FORWARDHELPCATEGORY":
                this.sections.fowardHelpCategory = match.Groups[3].Value;
                continue;
              case "REMOTEHELPRUNSPACE":
                this.sections.remoteHelpRunspace = match.Groups[3].Value;
                continue;
              case "EXTERNALHELP":
                this.sections.mamlHelpFile = match.Groups[3].Value;
                continue;
              default:
                return false;
            }
          }
          else
          {
            switch (match.Groups[1].Value.ToUpperInvariant())
            {
              case "SYNOPSIS":
                this.sections.synopsis = this.GetSection(commentLines, ref i);
                continue;
              case "DESCRIPTION":
                this.sections.description = this.GetSection(commentLines, ref i);
                continue;
              case "NOTES":
                this.sections.notes = this.GetSection(commentLines, ref i);
                continue;
              case "LINK":
                this.sections.links.Add(this.GetSection(commentLines, ref i).Trim());
                continue;
              case "EXAMPLE":
                this.sections.examples.Add(this.GetSection(commentLines, ref i));
                continue;
              case "INPUTS":
                this.sections.inputs.Add(this.GetSection(commentLines, ref i));
                continue;
              case "OUTPUTS":
                this.sections.outputs.Add(this.GetSection(commentLines, ref i));
                continue;
              case "COMPONENT":
                this.sections.component = this.GetSection(commentLines, ref i).Trim();
                continue;
              case "ROLE":
                this.sections.role = this.GetSection(commentLines, ref i).Trim();
                continue;
              case "FUNCTIONALITY":
                this.sections.functionality = this.GetSection(commentLines, ref i).Trim();
                continue;
              default:
                return false;
            }
          }
        }
        else if (!Regex.IsMatch(commentLines[i], HelpCommentsParser.blankline))
          return false;
      }
      return flag;
    }

    internal void SetAdditionalData(MamlCommandHelpInfo helpInfo) => helpInfo.SetAdditionalDataFromHelpComment(this.sections.component, this.sections.functionality, this.sections.role);

    internal static HelpInfo CreateFromComments(
      ExecutionContext context,
      CommandInfo commandInfo,
      List<Token> comments,
      List<List<Token>> parameterComments,
      out string helpFile)
    {
      HelpCommentsParser helpCommentsParser = new HelpCommentsParser(commandInfo, parameterComments);
      helpCommentsParser.AnalyzeCommentBlock(comments);
      helpFile = helpCommentsParser.GetHelpFile(commandInfo);
      RemoteHelpInfo remoteHelpInfo = helpCommentsParser.GetRemoteHelpInfo(context, commandInfo);
      if (remoteHelpInfo != null)
        return (HelpInfo) remoteHelpInfo;
      MamlCommandHelpInfo helpInfo = MamlCommandHelpInfo.Load((XmlNode) helpCommentsParser.BuildXmlFromComments().DocumentElement, commandInfo.HelpCategory);
      if (helpInfo != null)
      {
        helpCommentsParser.SetAdditionalData(helpInfo);
        if (!string.IsNullOrEmpty(helpCommentsParser.sections.forwardHelpTargetName) || !string.IsNullOrEmpty(helpCommentsParser.sections.fowardHelpCategory))
        {
          if (string.IsNullOrEmpty(helpCommentsParser.sections.forwardHelpTargetName))
            helpInfo.ForwardTarget = helpInfo.Name;
          else
            helpInfo.ForwardTarget = helpCommentsParser.sections.forwardHelpTargetName;
          if (!string.IsNullOrEmpty(helpCommentsParser.sections.fowardHelpCategory))
          {
            try
            {
              helpInfo.ForwardHelpCategory = (HelpCategory) Enum.Parse(typeof (HelpCategory), helpCommentsParser.sections.fowardHelpCategory, true);
            }
            catch (ArgumentException ex)
            {
            }
          }
          else
            helpInfo.ForwardHelpCategory = HelpCategory.Alias | HelpCategory.Cmdlet | HelpCategory.ScriptCommand | HelpCategory.Function | HelpCategory.Filter | HelpCategory.ExternalScript;
        }
      }
      return (HelpInfo) helpInfo;
    }

    internal static bool IsCommentHelpText(List<Token> commentBlock) => commentBlock != null && commentBlock.Count != 0 && new HelpCommentsParser().AnalyzeCommentBlock(commentBlock);

    private class CommentBlockSections
    {
      internal string synopsis;
      internal string description;
      internal string notes;
      internal Dictionary<string, string> parameters = new Dictionary<string, string>();
      internal List<string> links = new List<string>();
      internal List<string> examples = new List<string>();
      internal List<string> inputs = new List<string>();
      internal List<string> outputs = new List<string>();
      internal string component;
      internal string role;
      internal string functionality;
      internal string forwardHelpTargetName;
      internal string fowardHelpCategory;
      internal string remoteHelpRunspace;
      internal string mamlHelpFile;
    }
  }
}
