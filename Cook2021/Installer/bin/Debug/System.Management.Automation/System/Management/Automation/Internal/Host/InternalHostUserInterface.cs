// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Internal.Host.InternalHostUserInterface
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;

namespace System.Management.Automation.Internal.Host
{
  internal class InternalHostUserInterface : 
    PSHostUserInterface,
    IHostUISupportsMultipleChoiceSelection
  {
    private const string resStringsBaseName = "InternalHostUserInterfaceStrings";
    private const string WriteDebugLineStoppedErrorResource = "WriteDebugLineStoppedError";
    private const string UnsupportedPreferenceErrorResource = "UnsupportedPreferenceError";
    private const string PromptEmptyDescriptionsErrorResource = "PromptEmptyDescriptionsError";
    private PSHostUserInterface externalUI;
    private InternalHostRawUserInterface internalRawUI;
    private InternalHost parent;
    private PSInformationalBuffers informationalBuffers;
    [TraceSource("InternalHost", "S.M.A.InternalHostUserInterface")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("InternalHost", "S.M.A.InternalHostUserInterface");

    internal InternalHostUserInterface(PSHostUserInterface externalUI, InternalHost parentHost)
    {
      using (InternalHostUserInterface.tracer.TraceConstructor((object) this))
      {
        InternalHostUserInterface.tracer.WriteLine("externalUI {0} null", externalUI == null ? (object) "is" : (object) "is not");
        this.externalUI = externalUI;
        this.parent = parentHost != null ? parentHost : throw InternalHostUserInterface.tracer.NewArgumentNullException(nameof (parentHost));
        PSHostRawUserInterface externalRawUI = (PSHostRawUserInterface) null;
        if (externalUI != null)
          externalRawUI = externalUI.RawUI;
        InternalHostUserInterface.tracer.WriteLine("raw ui {0} null", externalRawUI == null ? (object) "is" : (object) "is not");
        this.internalRawUI = new InternalHostRawUserInterface(externalRawUI, this.parent);
      }
    }

    private void ThrowNotInteractive() => this.internalRawUI.ThrowNotInteractive();

    public override PSHostRawUserInterface RawUI
    {
      get
      {
        using (InternalHostUserInterface.tracer.TraceProperty((object) this.internalRawUI))
          return (PSHostRawUserInterface) this.internalRawUI;
      }
    }

    public override string ReadLine()
    {
      using (InternalHostUserInterface.tracer.TraceMethod())
      {
        if (this.externalUI == null)
          this.ThrowNotInteractive();
        string format = (string) null;
        try
        {
          format = this.externalUI.ReadLine();
        }
        catch (PipelineStoppedException ex)
        {
          LocalPipeline currentlyRunningPipeline = (LocalPipeline) this.parent.Context.CurrentRunspace.GetCurrentlyRunningPipeline();
          if (currentlyRunningPipeline == null)
            throw;
          else
            currentlyRunningPipeline.Stopper.Stop();
        }
        InternalHostUserInterface.tracer.WriteLine(format, new object[0]);
        return format;
      }
    }

    public override SecureString ReadLineAsSecureString()
    {
      using (InternalHostUserInterface.tracer.TraceMethod())
      {
        if (this.externalUI == null)
          this.ThrowNotInteractive();
        SecureString secureString = (SecureString) null;
        try
        {
          secureString = this.externalUI.ReadLineAsSecureString();
        }
        catch (PipelineStoppedException ex)
        {
          LocalPipeline currentlyRunningPipeline = (LocalPipeline) this.parent.Context.CurrentRunspace.GetCurrentlyRunningPipeline();
          if (currentlyRunningPipeline == null)
            throw;
          else
            currentlyRunningPipeline.Stopper.Stop();
        }
        InternalHostUserInterface.tracer.WriteLine((object) secureString);
        return secureString;
      }
    }

    public override void Write(string value)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(value, new object[0]))
      {
        if (value == null)
          InternalHostUserInterface.tracer.WriteLine("value is null, doing nothing", new object[0]);
        else if (this.externalUI == null)
          InternalHostUserInterface.tracer.WriteLine("no external user interface implemented; doing nothing", new object[0]);
        else
          this.externalUI.Write(value);
      }
    }

    public override void Write(
      ConsoleColor foregroundColor,
      ConsoleColor backgroundColor,
      string value)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(value, new object[0]))
      {
        if (value == null)
          InternalHostUserInterface.tracer.WriteLine("value is null, doing nothing", new object[0]);
        else if (this.externalUI == null)
          InternalHostUserInterface.tracer.WriteLine("no external user interface implemented; doing nothing", new object[0]);
        else
          this.externalUI.Write(foregroundColor, backgroundColor, value);
      }
    }

    public override void WriteLine()
    {
      using (InternalHostUserInterface.tracer.TraceMethod())
      {
        if (this.externalUI == null)
          InternalHostUserInterface.tracer.WriteLine("no external user interface implemented; doing nothing", new object[0]);
        else
          this.externalUI.WriteLine();
      }
    }

    public override void WriteLine(string value)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(value, new object[0]))
      {
        if (value == null)
          InternalHostUserInterface.tracer.WriteLine("value was null, doing nothing", new object[0]);
        else if (this.externalUI == null)
          InternalHostUserInterface.tracer.WriteLine("no external user interface implemented; doing nothing", new object[0]);
        else
          this.externalUI.WriteLine(value);
      }
    }

    public override void WriteErrorLine(string value)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(value, new object[0]))
      {
        if (value == null)
          InternalHostUserInterface.tracer.WriteLine("value was null, doing nothing", new object[0]);
        else if (this.externalUI == null)
          InternalHostUserInterface.tracer.WriteLine("no external user interface implemented; doing nothing", new object[0]);
        else
          this.externalUI.WriteErrorLine(value);
      }
    }

    public override void WriteLine(
      ConsoleColor foregroundColor,
      ConsoleColor backgroundColor,
      string value)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(value, new object[0]))
      {
        if (value == null)
          InternalHostUserInterface.tracer.WriteLine("value was null, doing nothing", new object[0]);
        else if (this.externalUI == null)
          InternalHostUserInterface.tracer.WriteLine("no external user interface implemented; doing nothing", new object[0]);
        else
          this.externalUI.WriteLine(foregroundColor, backgroundColor, value);
      }
    }

    public override void WriteDebugLine(string message)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(message, new object[0]))
        this.WriteDebugLineHelper(message);
    }

    internal void WriteDebugRecord(DebugRecord record)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(record.Message, new object[0]))
      {
        if (this.informationalBuffers != null)
          this.informationalBuffers.AddDebug(record);
        if (this.externalUI == null)
          InternalHostUserInterface.tracer.WriteLine("no external user interface implemented; doing nothing", new object[0]);
        else
          this.externalUI.WriteDebugLine(record.Message);
      }
    }

    internal void WriteDebugLine(string message, ref ActionPreference preference)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(message, new object[0]))
      {
        InternalHostUserInterface.tracer.WriteLine((object) preference);
        switch (preference)
        {
          case ActionPreference.SilentlyContinue:
            break;
          case ActionPreference.Stop:
            this.WriteDebugLineHelper(message);
            ActionPreferenceStopException preferenceStopException1 = new ActionPreferenceStopException(new ErrorRecord((Exception) new ParentContainsErrorRecordException(ResourceManagerCache.GetResourceString("InternalHostUserInterfaceStrings", "WriteDebugLineStoppedError")), "ActionPreferenceStop", ErrorCategory.OperationStopped, (object) null));
            InternalHostUserInterface.tracer.TraceException((Exception) preferenceStopException1);
            throw preferenceStopException1;
          case ActionPreference.Continue:
            this.WriteDebugLineHelper(message);
            break;
          case ActionPreference.Inquire:
            if (!this.DebugShouldContinue(message, ref preference))
            {
              ActionPreferenceStopException preferenceStopException2 = new ActionPreferenceStopException(new ErrorRecord((Exception) new ParentContainsErrorRecordException(ResourceManagerCache.GetResourceString("InternalHostUserInterfaceStrings", "WriteDebugLineStoppedError")), "UserStopRequest", ErrorCategory.OperationStopped, (object) null));
              InternalHostUserInterface.tracer.TraceException((Exception) preferenceStopException2);
              throw preferenceStopException2;
            }
            this.WriteDebugLineHelper(message);
            break;
          default:
            throw InternalHostUserInterface.tracer.NewArgumentException(nameof (preference), "InternalHostUserInterfaceStrings", "UnsupportedPreferenceError", (object) preference);
        }
      }
    }

    internal void SetInformationalMessageBuffers(PSInformationalBuffers informationalBuffers) => this.informationalBuffers = informationalBuffers;

    private void WriteDebugLineHelper(string message)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(message, new object[0]))
      {
        if (message == null)
          InternalHostUserInterface.tracer.WriteLine("message was null, doing nothing", new object[0]);
        else
          this.WriteDebugRecord(new DebugRecord(message));
      }
    }

    private bool DebugShouldContinue(string message, ref ActionPreference actionPreference)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(message, new object[0]))
      {
        InternalHostUserInterface.tracer.WriteLine((object) actionPreference);
        bool flag1 = false;
        Collection<ChoiceDescription> choices = new Collection<ChoiceDescription>();
        choices.Add(new ChoiceDescription("InternalHostUserInterfaceStrings", "ShouldContinueYesLabel", "ShouldContinueYesHelp"));
        choices.Add(new ChoiceDescription("InternalHostUserInterfaceStrings", "ShouldContinueYesToAllLabel", "ShouldContinueYesToAllHelp"));
        choices.Add(new ChoiceDescription("InternalHostUserInterfaceStrings", "ShouldContinueNoLabel", "ShouldContinueNoHelp"));
        choices.Add(new ChoiceDescription("InternalHostUserInterfaceStrings", "ShouldContinueNoToAllLabel", "ShouldContinueNoToAllHelp"));
        choices.Add(new ChoiceDescription("InternalHostUserInterfaceStrings", "ShouldContinueSuspendLabel", "ShouldContinueSuspendHelp"));
        bool flag2;
        do
        {
          flag2 = true;
          switch (this.PromptForChoice(ResourceManagerCache.GetResourceString("InternalHostUserInterfaceStrings", "ShouldContinuePromptMessage"), message, choices, 0))
          {
            case 0:
              flag1 = true;
              break;
            case 1:
              actionPreference = ActionPreference.Continue;
              flag1 = true;
              break;
            case 2:
              flag1 = false;
              break;
            case 3:
              actionPreference = ActionPreference.Stop;
              flag1 = false;
              break;
            case 4:
              this.parent.EnterNestedPrompt();
              flag2 = false;
              break;
          }
        }
        while (!flag2);
        return flag1;
      }
    }

    public override void WriteProgress(long sourceId, ProgressRecord record)
    {
      using (InternalHostUserInterface.tracer.TraceMethod((object) record))
      {
        if (record == null)
          throw InternalHostUserInterface.tracer.NewArgumentNullException(nameof (record));
        if (this.informationalBuffers != null)
          this.informationalBuffers.AddProgress(record);
        if (this.externalUI == null)
          InternalHostUserInterface.tracer.WriteLine("no external user interface implemented; doing nothing", new object[0]);
        else
          this.externalUI.WriteProgress(sourceId, record);
      }
    }

    public override void WriteVerboseLine(string message)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(message, new object[0]))
      {
        if (message == null)
          InternalHostUserInterface.tracer.WriteLine("message was null, doing nothing", new object[0]);
        else
          this.WriteVerboseRecord(new VerboseRecord(message));
      }
    }

    internal void WriteVerboseRecord(VerboseRecord record)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(record.Message, new object[0]))
      {
        if (this.informationalBuffers != null)
          this.informationalBuffers.AddVerbose(record);
        if (this.externalUI == null)
          InternalHostUserInterface.tracer.WriteLine("no external user interface implemented; doing nothing", new object[0]);
        else
          this.externalUI.WriteVerboseLine(record.Message);
      }
    }

    public override void WriteWarningLine(string message)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(message, new object[0]))
      {
        if (message == null)
          InternalHostUserInterface.tracer.WriteLine("message was null, doing nothing", new object[0]);
        else
          this.WriteWarningRecord(new WarningRecord(message));
      }
    }

    internal void WriteWarningRecord(WarningRecord record)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(record.Message, new object[0]))
      {
        if (this.informationalBuffers != null)
          this.informationalBuffers.AddWarning(record);
        if (this.externalUI == null)
          InternalHostUserInterface.tracer.WriteLine("no external user interface implemented; doing nothing", new object[0]);
        else
          this.externalUI.WriteWarningLine(record.Message);
      }
    }

    internal static Type GetFieldType(FieldDescription field)
    {
      Type type = (Type) null;
      Exception exception;
      if (type == null && !string.IsNullOrEmpty(field.ParameterAssemblyFullName))
        type = LanguagePrimitives.ConvertStringToType(field.ParameterAssemblyFullName, out exception);
      if (type == null && !string.IsNullOrEmpty(field.ParameterTypeFullName))
        type = LanguagePrimitives.ConvertStringToType(field.ParameterTypeFullName, out exception);
      return type;
    }

    internal static bool IsSecuritySensitiveType(string typeName) => typeName.Equals(typeof (PSCredential).Name, StringComparison.OrdinalIgnoreCase) || typeName.Equals(typeof (SecureString).Name, StringComparison.OrdinalIgnoreCase);

    public override Dictionary<string, PSObject> Prompt(
      string caption,
      string message,
      Collection<FieldDescription> descriptions)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(caption, new object[0]))
      {
        if (descriptions == null)
          throw InternalHostUserInterface.tracer.NewArgumentNullException(nameof (descriptions));
        if (descriptions.Count < 1)
          throw InternalHostUserInterface.tracer.NewArgumentException(nameof (descriptions), "InternalHostUserInterfaceStrings", "PromptEmptyDescriptionsError", (object) nameof (descriptions));
        if (this.externalUI == null)
          this.ThrowNotInteractive();
        Dictionary<string, PSObject> dictionary = (Dictionary<string, PSObject>) null;
        try
        {
          dictionary = this.externalUI.Prompt(caption, message, descriptions);
        }
        catch (PipelineStoppedException ex)
        {
          LocalPipeline currentlyRunningPipeline = (LocalPipeline) this.parent.Context.CurrentRunspace.GetCurrentlyRunningPipeline();
          if (currentlyRunningPipeline == null)
            throw;
          else
            currentlyRunningPipeline.Stopper.Stop();
        }
        InternalHostUserInterface.tracer.WriteLine((object) dictionary);
        return dictionary;
      }
    }

    public override int PromptForChoice(
      string caption,
      string message,
      Collection<ChoiceDescription> choices,
      int defaultChoice)
    {
      using (InternalHostUserInterface.tracer.TraceMethod())
      {
        if (this.externalUI == null)
          this.ThrowNotInteractive();
        int num = -1;
        try
        {
          num = this.externalUI.PromptForChoice(caption, message, choices, defaultChoice);
        }
        catch (PipelineStoppedException ex)
        {
          LocalPipeline currentlyRunningPipeline = (LocalPipeline) this.parent.Context.CurrentRunspace.GetCurrentlyRunningPipeline();
          if (currentlyRunningPipeline == null)
            throw;
          else
            currentlyRunningPipeline.Stopper.Stop();
        }
        InternalHostUserInterface.tracer.WriteLine((object) num);
        return num;
      }
    }

    public Collection<int> PromptForChoice(
      string caption,
      string message,
      Collection<ChoiceDescription> choices,
      IEnumerable<int> defaultChoices)
    {
      using (InternalHostUserInterface.tracer.TraceMethod())
      {
        if (this.externalUI == null)
          this.ThrowNotInteractive();
        IHostUISupportsMultipleChoiceSelection externalUi = this.externalUI as IHostUISupportsMultipleChoiceSelection;
        Collection<int> collection = (Collection<int>) null;
        try
        {
          collection = externalUi != null ? externalUi.PromptForChoice(caption, message, choices, defaultChoices) : this.EmulatePromptForMultipleChoice(caption, message, choices, defaultChoices);
        }
        catch (PipelineStoppedException ex)
        {
          LocalPipeline currentlyRunningPipeline = (LocalPipeline) this.parent.Context.CurrentRunspace.GetCurrentlyRunningPipeline();
          if (currentlyRunningPipeline == null)
            throw;
          else
            currentlyRunningPipeline.Stopper.Stop();
        }
        return collection;
      }
    }

    private Collection<int> EmulatePromptForMultipleChoice(
      string caption,
      string message,
      Collection<ChoiceDescription> choices,
      IEnumerable<int> defaultChoices)
    {
      if (choices == null)
        throw InternalHostUserInterface.tracer.NewArgumentNullException(nameof (choices));
      if (choices.Count == 0)
        throw InternalHostUserInterface.tracer.NewArgumentException(nameof (choices), "InternalHostUserInterfaceStrings", "EmptyChoicesError", (object) nameof (choices));
      Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
      if (defaultChoices != null)
      {
        foreach (int defaultChoice in defaultChoices)
        {
          if (defaultChoice < 0 || defaultChoice >= choices.Count)
            throw InternalHostUserInterface.tracer.NewArgumentOutOfRangeException("defaultChoice", (object) defaultChoice, "InternalHostUserInterfaceStrings", "InvalidDefaultChoiceForMultipleSelection", (object) "defaultChoice", (object) nameof (choices), (object) defaultChoice);
          if (!dictionary.ContainsKey(defaultChoice))
            dictionary.Add(defaultChoice, true);
        }
      }
      StringBuilder stringBuilder1 = new StringBuilder();
      char ch = '\n';
      if (!string.IsNullOrEmpty(caption))
      {
        stringBuilder1.Append(caption);
        stringBuilder1.Append(ch);
      }
      if (!string.IsNullOrEmpty(message))
      {
        stringBuilder1.Append(message);
        stringBuilder1.Append(ch);
      }
      string[,] hotkeysAndPlainLabels = (string[,]) null;
      HostUIHelperMethods.BuildHotkeysAndPlainLabels(choices, out hotkeysAndPlainLabels);
      string format = "[{0}] {1}  ";
      for (int index = 0; index < hotkeysAndPlainLabels.GetLength(1); ++index)
      {
        string str = string.Format((IFormatProvider) CultureInfo.InvariantCulture, format, (object) hotkeysAndPlainLabels[0, index], (object) hotkeysAndPlainLabels[1, index]);
        stringBuilder1.Append(str);
        stringBuilder1.Append(ch);
      }
      string str1 = "";
      if (dictionary.Count > 0)
      {
        string str2 = "";
        StringBuilder stringBuilder2 = new StringBuilder();
        foreach (int key in dictionary.Keys)
        {
          string str3 = hotkeysAndPlainLabels[0, key];
          if (string.IsNullOrEmpty(str3))
            str3 = hotkeysAndPlainLabels[1, key];
          stringBuilder2.Append(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}{1}", (object) str2, (object) str3));
          str2 = ",";
        }
        string str4 = stringBuilder2.ToString();
        if (dictionary.Count == 1)
          str1 = ResourceManagerCache.FormatResourceString("InternalHostUserInterfaceStrings", "DefaultChoice", (object) str4);
        else
          str1 = ResourceManagerCache.FormatResourceString("InternalHostUserInterfaceStrings", "DefaultChoicesForMultipleChoices", (object) str4);
      }
      string str5 = stringBuilder1.ToString() + str1 + (object) ch;
      Collection<int> collection = new Collection<int>();
      int num = 0;
      while (true)
      {
        string str2 = ResourceManagerCache.FormatResourceString("InternalHostUserInterfaceStrings", "ChoiceMessage", (object) num);
        this.externalUI.WriteLine(str5 + str2);
        string str3 = this.externalUI.ReadLine();
        if (str3.Length != 0)
        {
          int choicePicked = HostUIHelperMethods.DetermineChoicePicked(str3.Trim(), choices, hotkeysAndPlainLabels);
          if (choicePicked >= 0)
          {
            collection.Add(choicePicked);
            ++num;
          }
          str5 = "";
        }
        else
          break;
      }
      if (collection.Count == 0 && dictionary.Keys.Count >= 0)
      {
        foreach (int key in dictionary.Keys)
          collection.Add(key);
      }
      return collection;
    }

    public override PSCredential PromptForCredential(
      string caption,
      string message,
      string userName,
      string targetName)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(message, new object[0]))
        return this.PromptForCredential(caption, message, userName, targetName, PSCredentialTypes.Default, PSCredentialUIOptions.Default);
    }

    public override PSCredential PromptForCredential(
      string caption,
      string message,
      string userName,
      string targetName,
      PSCredentialTypes allowedCredentialTypes,
      PSCredentialUIOptions options)
    {
      using (InternalHostUserInterface.tracer.TraceMethod(message, new object[0]))
      {
        if (this.externalUI == null)
          this.ThrowNotInteractive();
        PSCredential psCredential = (PSCredential) null;
        try
        {
          psCredential = this.externalUI.PromptForCredential(caption, message, userName, targetName, allowedCredentialTypes, options);
        }
        catch (PipelineStoppedException ex)
        {
          LocalPipeline currentlyRunningPipeline = (LocalPipeline) this.parent.Context.CurrentRunspace.GetCurrentlyRunningPipeline();
          if (currentlyRunningPipeline == null)
            throw;
          else
            currentlyRunningPipeline.Stopper.Stop();
        }
        InternalHostUserInterface.tracer.WriteLine((object) psCredential);
        return psCredential;
      }
    }
  }
}
