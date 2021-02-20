// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.ExternalScriptInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;

namespace System.Management.Automation
{
  public class ExternalScriptInfo : CommandInfo, IScriptCommandInfo
  {
    private const string requiresToken = "requires";
    private const string requiresTokenCaps = "REQUIRES";
    private const string forceExecFlag = "forceexec";
    private const string forceExecFlagCaps = "FORCEEXEC";
    private const string shellIDToken = "shellid";
    private const string shellIDUnambiguousPrefix = "s";
    private const string PSSnapinToken = "pssnapin";
    private const string PSSnapinUnambiguousPrefix = "p";
    private const string versionToken = "version";
    private const string versionUnambiguousPrefix = "v";
    private const string resourceBase = "DiscoveryExceptions";
    private const string scriptRequiresInvalidFormatResId = "ScriptRequiresInvalidFormat";
    private const string scriptRequiresMissingQuoteResId = "ScriptRequiresMissingQuote";
    private const string scriptRequiresEmptyArgumentResId = "ScriptRequiresEmptyArgument";
    private const string scriptRequiresInvalidVersionResId = "ScriptRequiresInvalidVersion";
    [TraceSource("ExternalScriptInfo", "The command information for MSH scripts that are directly executable by MSH.")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer(nameof (ExternalScriptInfo), "The command information for MSH scripts that are directly executable by MSH.");
    private string path = string.Empty;
    private ScriptBlock scriptBlock;
    private bool signatureChecked;
    private CommandMetadata commandMetadata;
    private string requiresApplicationID;
    private uint applicationIDLineNumber;
    private Version requiresPSVersion;
    private uint psVersionLineNumber;
    private Collection<PSSnapInNameVersionPair> requiresPSSnapIns;
    private string scriptContents;
    private Encoding originalEncoding;
    private bool searchedForRequires;
    private uint lineNumber = 1;

    internal ExternalScriptInfo(string name, string path, ExecutionContext context)
      : base(name, CommandTypes.ExternalScript, context)
      => this.path = !string.IsNullOrEmpty(path) ? path : throw ExternalScriptInfo.tracer.NewArgumentException(nameof (path));

    internal ExternalScriptInfo(string name, string path)
      : base(name, CommandTypes.ExternalScript)
      => this.path = !string.IsNullOrEmpty(path) ? path : throw ExternalScriptInfo.tracer.NewArgumentException(nameof (path));

    internal ExternalScriptInfo(ExternalScriptInfo other)
      : base((CommandInfo) other)
      => this.path = other.path;

    internal override CommandInfo CreateGetCommandCopy(object[] argumentList)
    {
      ExternalScriptInfo externalScriptInfo = new ExternalScriptInfo(this);
      externalScriptInfo.IsGetCommandCopy = true;
      externalScriptInfo.Arguments = argumentList;
      return (CommandInfo) externalScriptInfo;
    }

    internal override HelpCategory HelpCategory => HelpCategory.ExternalScript;

    public string Path => this.path;

    public override string Definition => this.Path;

    internal override string Syntax
    {
      get
      {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (CommandParameterSetInfo parameterSet in this.ParameterSets)
          stringBuilder.AppendLine(string.Format((IFormatProvider) Thread.CurrentThread.CurrentCulture, "{0} {1}", (object) this.Name, (object) parameterSet.ToString()));
        return stringBuilder.ToString();
      }
    }

    public override SessionStateEntryVisibility Visibility
    {
      get => this.Context == null ? SessionStateEntryVisibility.Public : this.Context.EngineSessionState.CheckScriptVisibility(this.path);
      set => throw ExternalScriptInfo.tracer.NewNotImplementedException();
    }

    public ScriptBlock ScriptBlock
    {
      get
      {
        if (this.scriptBlock == null)
        {
          if (!this.signatureChecked)
          {
            this.ReadScriptAndVerify();
            ExecutionContext context = this.Context ?? LocalPipeline.GetExecutionContextFromTLS();
            if (context != null)
            {
              CommandDiscovery.ShouldRun(context, (PSHost) null, (CommandInfo) this, CommandOrigin.Internal);
              this.signatureChecked = true;
            }
          }
          this.scriptBlock = ScriptBlock.Create(new Parser(this.path), this.ScriptContents);
        }
        return this.scriptBlock;
      }
    }

    public override ReadOnlyCollection<PSTypeName> OutputType => this.ScriptBlock.OutputType;

    internal bool SignatureChecked
    {
      get => this.signatureChecked;
      set => this.signatureChecked = value;
    }

    internal override CommandMetadata CommandMetadata
    {
      get
      {
        if (this.commandMetadata == null)
          this.commandMetadata = new CommandMetadata(this.ScriptBlock, this.Name, LocalPipeline.GetExecutionContextFromTLS());
        return this.commandMetadata;
      }
    }

    internal override bool ImplementsDynamicParameters
    {
      get
      {
        try
        {
          return this.ScriptBlock.DynamicParams != null;
        }
        catch (ParseException ex)
        {
        }
        catch (ScriptRequiresException ex)
        {
        }
        this.scriptBlock = (ScriptBlock) null;
        this.scriptContents = (string) null;
        return false;
      }
    }

    internal string RequiresApplicationID
    {
      get
      {
        this.ReadScriptAndVerify();
        return this.requiresApplicationID;
      }
    }

    internal uint ApplicationIDLineNumber => this.applicationIDLineNumber;

    internal Version RequiresPSVersion
    {
      get
      {
        this.ReadScriptAndVerify();
        return this.requiresPSVersion;
      }
    }

    internal uint PSVersionLineNumber => this.psVersionLineNumber;

    internal Collection<PSSnapInNameVersionPair> RequiresPSSnapIns
    {
      get
      {
        this.ReadScriptAndVerify();
        return this.requiresPSSnapIns;
      }
    }

    public string ScriptContents
    {
      get
      {
        if (this.scriptContents == null)
          this.ReadScriptContents();
        return this.scriptContents;
      }
    }

    public Encoding OriginalEncoding
    {
      get
      {
        if (this.scriptContents == null)
          this.ReadScriptContents();
        return this.originalEncoding;
      }
    }

    private void ReadScriptContents()
    {
      if (this.scriptContents != null)
        return;
      try
      {
        using (StreamReader streamReader = new StreamReader(this.path, Encoding.Default))
        {
          this.scriptContents = streamReader.ReadToEnd();
          this.originalEncoding = streamReader.CurrentEncoding;
        }
      }
      catch (ArgumentException ex)
      {
        ExternalScriptInfo.ThrowCommandNotFoundException((Exception) ex);
      }
      catch (IOException ex)
      {
        ExternalScriptInfo.ThrowCommandNotFoundException((Exception) ex);
      }
      catch (NotSupportedException ex)
      {
        ExternalScriptInfo.ThrowCommandNotFoundException((Exception) ex);
      }
      catch (UnauthorizedAccessException ex)
      {
        ExternalScriptInfo.ThrowCommandNotFoundException((Exception) ex);
      }
    }

    private void ReadScriptAndVerify()
    {
      this.ReadScriptContents();
      if (this.searchedForRequires)
        return;
      this.FindRequires(this.scriptContents);
      this.searchedForRequires = true;
    }

    private static void ThrowCommandNotFoundException(Exception innerException)
    {
      CommandNotFoundException notFoundException = new CommandNotFoundException(innerException.Message, innerException);
      ExternalScriptInfo.tracer.TraceException((Exception) notFoundException);
      throw notFoundException;
    }

    private void FindRequires(string script)
    {
      ExternalScriptInfo.ScriptState scriptState1 = ExternalScriptInfo.ScriptState.NewLine;
      ExternalScriptInfo.ScriptState scriptState2 = ExternalScriptInfo.ScriptState.NewLine;
      char ch = char.MinValue;
      for (int index = 0; index < script.Length; ++index)
      {
        char c = script[index];
        if (c == '\n')
          ++this.lineNumber;
        switch (scriptState1)
        {
          case ExternalScriptInfo.ScriptState.Processing:
            switch (c)
            {
              case '\n':
              case '\r':
                scriptState1 = ExternalScriptInfo.ScriptState.NewLine;
                continue;
              case '`':
                scriptState2 = scriptState1;
                scriptState1 = ExternalScriptInfo.ScriptState.BackTick;
                continue;
              default:
                if (SpecialCharacters.IsQuote(c))
                {
                  scriptState1 = ExternalScriptInfo.ScriptState.InString;
                  ch = SpecialCharacters.AsQuote(c);
                  continue;
                }
                continue;
            }
          case ExternalScriptInfo.ScriptState.NewLine:
            if (c == '\r' || c == '\n')
            {
              scriptState1 = ExternalScriptInfo.ScriptState.NewLine;
              break;
            }
            if (SpecialCharacters.IsQuote(c))
            {
              scriptState1 = ExternalScriptInfo.ScriptState.InString;
              ch = SpecialCharacters.AsQuote(c);
              break;
            }
            switch (c)
            {
              case '#':
                this.MatchRequires(script, ref index);
                continue;
              case '@':
                if (index < script.Length - 1 && SpecialCharacters.IsQuote(script[index + 1]))
                {
                  scriptState1 = ExternalScriptInfo.ScriptState.HereString;
                  ch = SpecialCharacters.AsQuote(script[index + 1]);
                  ++index;
                  continue;
                }
                break;
            }
            scriptState1 = ExternalScriptInfo.ScriptState.Processing;
            break;
          case ExternalScriptInfo.ScriptState.BackTick:
            scriptState1 = scriptState2;
            break;
          case ExternalScriptInfo.ScriptState.InString:
            if (c == '`')
            {
              scriptState2 = scriptState1;
              scriptState1 = ExternalScriptInfo.ScriptState.BackTick;
              break;
            }
            if ((int) SpecialCharacters.AsQuote(c) == (int) ch)
            {
              scriptState1 = ExternalScriptInfo.ScriptState.Processing;
              break;
            }
            break;
          case ExternalScriptInfo.ScriptState.HereString:
            switch (c)
            {
              case '\n':
              case '\r':
                scriptState1 = ExternalScriptInfo.ScriptState.HereStringNewLine;
                continue;
              case '`':
                scriptState2 = scriptState1;
                scriptState1 = ExternalScriptInfo.ScriptState.BackTick;
                continue;
              default:
                continue;
            }
          case ExternalScriptInfo.ScriptState.HereStringNewLine:
            if (c == '\r' || c == '\n')
            {
              scriptState1 = ExternalScriptInfo.ScriptState.HereStringNewLine;
              break;
            }
            if ((int) SpecialCharacters.AsQuote(c) == (int) ch && index < script.Length - 1 && script[index + 1] == '@')
            {
              scriptState1 = ExternalScriptInfo.ScriptState.Processing;
              break;
            }
            break;
        }
      }
    }

    private bool ParseRequiresToken(string script, ref int index)
    {
      int index1 = 0;
      while (index < script.Length)
      {
        char c = script[index];
        if (index1 == "requires".Length)
        {
          if (c == '\r' || c == '\n')
            return false;
          if (!Tokenizer.IsWhiteSpace(c))
            throw this.NewScriptRequireInvalidFormatException();
          return true;
        }
        if ((int) c != (int) "requires"[index1] && (int) c != (int) "REQUIRES"[index1])
          return false;
        ++index1;
        ++index;
      }
      return false;
    }

    private static bool ParseDisambiguatedParameterToken(
      string script,
      ref int index,
      string paramToken,
      int paramUnambiguousPrefixLength)
    {
      string upperInvariant = paramToken.ToUpperInvariant();
      int index1 = paramUnambiguousPrefixLength;
      while (index < script.Length)
      {
        char c = script[index];
        if (index1 == paramToken.Length)
          return c != '\r' && c != '\n' && Tokenizer.IsWhiteSpace(c);
        if (Tokenizer.IsWhiteSpace(c))
          return true;
        if ((int) c != (int) paramToken[index1] && (int) c != (int) upperInvariant[index1])
          return false;
        ++index1;
        ++index;
      }
      return false;
    }

    private static bool LookAhead(
      string script,
      int index,
      int numberOfCharLookAhead,
      ref char lookAheadChar)
    {
      long num = (long) index + (long) numberOfCharLookAhead;
      if (num >= (long) script.Length)
        return false;
      lookAheadChar = script[(int) num];
      return true;
    }

    private static bool SafeIncrementInt(ref int i, int increment)
    {
      long num = (long) i + (long) increment;
      if (num > (long) int.MaxValue)
        return false;
      i = (int) num;
      return true;
    }

    private static bool ParseAmbiguousParameters(
      string script,
      ref int index,
      ref ExternalScriptInfo.MatchRequiresState state)
    {
      char minValue = char.MinValue;
      if (!SpecialCharacters.IsDash(script[index]) || !ExternalScriptInfo.LookAhead(script, index, 1, ref minValue))
        return false;
      if ((int) minValue == (int) "v"[0] || (int) minValue == (int) char.ToUpperInvariant("v"[0]))
      {
        if (!ExternalScriptInfo.SafeIncrementInt(ref index, 1))
          return false;
        state = ExternalScriptInfo.MatchRequiresState.MonadVersionParameter;
        return true;
      }
      if ((int) minValue == (int) "s"[0] || (int) minValue == (int) char.ToUpperInvariant("s"[0]))
      {
        if (!ExternalScriptInfo.SafeIncrementInt(ref index, 1))
          return false;
        state = ExternalScriptInfo.MatchRequiresState.ShellIDParameter;
        return true;
      }
      if ((int) minValue != (int) "p"[0] && (int) minValue != (int) char.ToUpperInvariant("p"[0]) || !ExternalScriptInfo.SafeIncrementInt(ref index, 1))
        return false;
      state = ExternalScriptInfo.MatchRequiresState.PSSnapinParameter;
      return true;
    }

    private void BindParameters(
      string script,
      int index,
      int argStart,
      ExternalScriptInfo.BindRequiresParameter argumentForParameter)
    {
      switch (argumentForParameter)
      {
        case ExternalScriptInfo.BindRequiresParameter.PSSnapIn:
          if (this.requiresPSSnapIns == null)
            this.requiresPSSnapIns = new Collection<PSSnapInNameVersionPair>();
          string PSSnapinName = script.Substring(argStart, index - argStart);
          try
          {
            this.requiresPSSnapIns.Add(new PSSnapInNameVersionPair(PSSnapinName));
            break;
          }
          catch (PSArgumentException ex)
          {
            throw new ScriptRequiresSyntaxException(ResourceManagerCache.FormatResourceString("DiscoveryExceptions", "ScriptRequiresInvalidPSSnapInName", (object) this.lineNumber, (object) PSSnapinName, (object) ex.Message));
          }
        case ExternalScriptInfo.BindRequiresParameter.PSSnapInVersion:
          Version version1 = Utils.StringToVersion(script.Substring(argStart, index - argStart));
          this.requiresPSSnapIns[this.requiresPSSnapIns.Count - 1].Version = !(version1 == (Version) null) ? version1 : throw this.NewScriptRequiresInvalidVersionException(argumentForParameter);
          break;
        case ExternalScriptInfo.BindRequiresParameter.ShellID:
          if (!string.IsNullOrEmpty(this.requiresApplicationID))
            break;
          this.requiresApplicationID = script.Substring(argStart, index - argStart);
          this.applicationIDLineNumber = this.lineNumber;
          break;
        case ExternalScriptInfo.BindRequiresParameter.MonadVersion:
          if (!(this.requiresPSVersion == (Version) null))
            break;
          Version version2 = Utils.StringToVersion(script.Substring(argStart, index - argStart));
          if (version2 == (Version) null)
            throw this.NewScriptRequiresInvalidVersionException(argumentForParameter);
          this.psVersionLineNumber = this.lineNumber;
          this.requiresPSVersion = version2;
          break;
      }
    }

    private static string BindRequiresParameterToToken(
      ExternalScriptInfo.BindRequiresParameter argType)
    {
      switch (argType)
      {
        case ExternalScriptInfo.BindRequiresParameter.PSSnapIn:
          return "pssnapin";
        case ExternalScriptInfo.BindRequiresParameter.PSSnapInVersion:
        case ExternalScriptInfo.BindRequiresParameter.MonadVersion:
          return "version";
        case ExternalScriptInfo.BindRequiresParameter.ShellID:
          return "shellid";
        default:
          return "";
      }
    }

    private bool MatchRequires(string script, ref int index)
    {
      bool flag1 = false;
      bool flag2 = false;
      ExternalScriptInfo.MatchRequiresState state = ExternalScriptInfo.MatchRequiresState.RequiresToken;
      ExternalScriptInfo.MatchRequiresState matchRequiresState = ExternalScriptInfo.MatchRequiresState.AllowQuotedArgument;
      ExternalScriptInfo.BindRequiresParameter requiresParameter = ExternalScriptInfo.BindRequiresParameter.None;
      char ch = char.MinValue;
      int argStart = -1;
      ++index;
      while (index < script.Length)
      {
        char c = script[index];
        if (c == '\n')
          ++this.lineNumber;
        switch (state)
        {
          case ExternalScriptInfo.MatchRequiresState.RequiresToken:
            flag1 = this.ParseRequiresToken(script, ref index);
            if (flag1)
            {
              state = ExternalScriptInfo.MatchRequiresState.WhiteSpace;
              matchRequiresState = ExternalScriptInfo.MatchRequiresState.UnresolvedParameter;
              break;
            }
            flag2 = true;
            break;
          case ExternalScriptInfo.MatchRequiresState.WhiteSpace:
            if (!Tokenizer.IsWhiteSpace(c))
            {
              --index;
              state = matchRequiresState;
              break;
            }
            break;
          case ExternalScriptInfo.MatchRequiresState.AllowQuotedArgument:
            if (c == '\r' || c == '\n')
            {
              if (argStart == -1)
                throw this.NewScriptRequiresEmptyArgumentException(requiresParameter);
              this.BindParameters(script, index, argStart, requiresParameter);
              flag1 = true;
              flag2 = true;
              break;
            }
            if (SpecialCharacters.IsQuote(c) && argStart == -1)
            {
              ch = SpecialCharacters.AsQuote(c);
              state = ExternalScriptInfo.MatchRequiresState.QuotedArgument;
              break;
            }
            if (Tokenizer.IsWhiteSpace(c))
            {
              if (argStart == -1)
                throw this.NewScriptRequiresEmptyArgumentException(requiresParameter);
              flag1 = true;
              this.BindParameters(script, index, argStart, requiresParameter);
              --index;
              state = ExternalScriptInfo.MatchRequiresState.WhiteSpace;
              matchRequiresState = requiresParameter == ExternalScriptInfo.BindRequiresParameter.PSSnapIn ? ExternalScriptInfo.MatchRequiresState.PSSnapinVersionParameter : ExternalScriptInfo.MatchRequiresState.RemainingLine;
              break;
            }
            if (argStart == -1)
            {
              argStart = index;
              break;
            }
            break;
          case ExternalScriptInfo.MatchRequiresState.QuotedArgument:
            if ((int) SpecialCharacters.AsQuote(c) == (int) ch)
            {
              if (argStart == -1)
                throw this.NewScriptRequiresEmptyArgumentException(requiresParameter);
              this.BindParameters(script, index, argStart, requiresParameter);
              state = ExternalScriptInfo.MatchRequiresState.WhiteSpace;
              matchRequiresState = requiresParameter == ExternalScriptInfo.BindRequiresParameter.PSSnapIn ? ExternalScriptInfo.MatchRequiresState.PSSnapinVersionParameter : ExternalScriptInfo.MatchRequiresState.RemainingLine;
              break;
            }
            if (c == '\r' || c == '\n')
              throw this.NewScriptRequiresMissingQuoteException(requiresParameter);
            if (argStart == -1)
            {
              argStart = index;
              break;
            }
            break;
          case ExternalScriptInfo.MatchRequiresState.RemainingLine:
            if (c == '\r' || c == '\n')
            {
              flag2 = true;
              break;
            }
            if (!Tokenizer.IsWhiteSpace(c))
              throw this.NewScriptRequireInvalidFormatException();
            break;
          case ExternalScriptInfo.MatchRequiresState.UnresolvedParameter:
            if (c == '\r' || c == '\n')
            {
              flag1 = false;
              flag2 = true;
              break;
            }
            if (!ExternalScriptInfo.ParseAmbiguousParameters(script, ref index, ref state))
              throw this.NewScriptRequireInvalidFormatException();
            break;
          case ExternalScriptInfo.MatchRequiresState.ShellIDParameter:
            if (!ExternalScriptInfo.ParseDisambiguatedParameterToken(script, ref index, "shellid", "s".Length))
              throw this.NewScriptRequireInvalidFormatException();
            state = ExternalScriptInfo.MatchRequiresState.WhiteSpace;
            matchRequiresState = ExternalScriptInfo.MatchRequiresState.AllowQuotedArgument;
            requiresParameter = ExternalScriptInfo.BindRequiresParameter.ShellID;
            break;
          case ExternalScriptInfo.MatchRequiresState.PSSnapinParameter:
            if (!ExternalScriptInfo.ParseDisambiguatedParameterToken(script, ref index, "pssnapin", "p".Length))
              throw this.NewScriptRequireInvalidFormatException();
            state = ExternalScriptInfo.MatchRequiresState.WhiteSpace;
            matchRequiresState = ExternalScriptInfo.MatchRequiresState.AllowQuotedArgument;
            requiresParameter = ExternalScriptInfo.BindRequiresParameter.PSSnapIn;
            break;
          case ExternalScriptInfo.MatchRequiresState.MonadVersionParameter:
            if (!ExternalScriptInfo.ParseDisambiguatedParameterToken(script, ref index, "version", "v".Length))
              throw this.NewScriptRequireInvalidFormatException();
            state = ExternalScriptInfo.MatchRequiresState.WhiteSpace;
            matchRequiresState = ExternalScriptInfo.MatchRequiresState.AllowQuotedArgument;
            requiresParameter = ExternalScriptInfo.BindRequiresParameter.MonadVersion;
            break;
          case ExternalScriptInfo.MatchRequiresState.PSSnapinVersionParameter:
            if (c == '\r' || c == '\n')
            {
              flag1 = true;
              flag2 = true;
              break;
            }
            if (!SpecialCharacters.IsDash(script[index]))
              throw this.NewScriptRequireInvalidFormatException();
            char minValue = char.MinValue;
            if (!ExternalScriptInfo.LookAhead(script, index, 1, ref minValue))
              throw this.NewScriptRequireInvalidFormatException();
            if ((int) minValue != (int) "v"[0] && (int) minValue != (int) char.ToUpperInvariant("v"[0]))
              throw this.NewScriptRequireInvalidFormatException();
            index += 2;
            if (!ExternalScriptInfo.ParseDisambiguatedParameterToken(script, ref index, "version", "v".Length))
              throw this.NewScriptRequireInvalidFormatException();
            state = ExternalScriptInfo.MatchRequiresState.WhiteSpace;
            matchRequiresState = ExternalScriptInfo.MatchRequiresState.AllowQuotedArgument;
            requiresParameter = ExternalScriptInfo.BindRequiresParameter.PSSnapInVersion;
            argStart = -1;
            break;
        }
        if (!flag2)
          ++index;
        else
          break;
      }
      if (index >= script.Length)
      {
        if (state == ExternalScriptInfo.MatchRequiresState.QuotedArgument)
          throw this.NewScriptRequiresMissingQuoteException(requiresParameter);
        if (state == ExternalScriptInfo.MatchRequiresState.WhiteSpace && matchRequiresState == ExternalScriptInfo.MatchRequiresState.AllowQuotedArgument)
          throw this.NewScriptRequireInvalidFormatException();
        if (state == ExternalScriptInfo.MatchRequiresState.AllowQuotedArgument)
          this.BindParameters(script, script.Length, argStart, requiresParameter);
      }
      ExternalScriptInfo.tracer.WriteLine("result = {0}", (object) flag1);
      return flag1;
    }

    private ScriptRequiresSyntaxException NewScriptRequiresMissingQuoteException(
      ExternalScriptInfo.BindRequiresParameter argType)
    {
      ScriptRequiresSyntaxException requiresSyntaxException = new ScriptRequiresSyntaxException(ResourceManagerCache.FormatResourceString("DiscoveryExceptions", "ScriptRequiresMissingQuote", (object) this.lineNumber, (object) ExternalScriptInfo.BindRequiresParameterToToken(argType)));
      ExternalScriptInfo.tracer.TraceException((Exception) requiresSyntaxException);
      return requiresSyntaxException;
    }

    private ScriptRequiresSyntaxException NewScriptRequireInvalidFormatException()
    {
      ScriptRequiresSyntaxException requiresSyntaxException = new ScriptRequiresSyntaxException(ResourceManagerCache.FormatResourceString("DiscoveryExceptions", "ScriptRequiresInvalidFormat", (object) this.lineNumber));
      ExternalScriptInfo.tracer.TraceException((Exception) requiresSyntaxException);
      return requiresSyntaxException;
    }

    private ScriptRequiresSyntaxException NewScriptRequiresEmptyArgumentException(
      ExternalScriptInfo.BindRequiresParameter argType)
    {
      ScriptRequiresSyntaxException requiresSyntaxException = new ScriptRequiresSyntaxException(ResourceManagerCache.FormatResourceString("DiscoveryExceptions", "ScriptRequiresEmptyArgument", (object) this.lineNumber, (object) ExternalScriptInfo.BindRequiresParameterToToken(argType)));
      ExternalScriptInfo.tracer.TraceException((Exception) requiresSyntaxException);
      return requiresSyntaxException;
    }

    private ScriptRequiresSyntaxException NewScriptRequiresInvalidVersionException(
      ExternalScriptInfo.BindRequiresParameter argType)
    {
      ScriptRequiresSyntaxException requiresSyntaxException = new ScriptRequiresSyntaxException(ResourceManagerCache.FormatResourceString("DiscoveryExceptions", "ScriptRequiresInvalidVersion", (object) this.lineNumber, (object) ExternalScriptInfo.BindRequiresParameterToToken(argType)));
      ExternalScriptInfo.tracer.TraceException((Exception) requiresSyntaxException);
      return requiresSyntaxException;
    }

    private enum ScriptState
    {
      Processing,
      NewLine,
      BackTick,
      InString,
      HereString,
      HereStringNewLine,
    }

    private enum MatchRequiresState
    {
      RequiresToken,
      WhiteSpace,
      AllowQuotedArgument,
      QuotedArgument,
      NewLine,
      RemainingLine,
      UnresolvedParameter,
      ShellIDParameter,
      PSSnapinParameter,
      MonadVersionParameter,
      PSSnapinVersionParameter,
    }

    private enum BindRequiresParameter
    {
      PSSnapIn,
      PSSnapInVersion,
      ShellID,
      MonadVersion,
      None,
    }
  }
}
