// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.NativeCommandProcessor
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Management.Automation.Internal;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Management.Automation
{
  internal class NativeCommandProcessor : CommandProcessorBase
  {
    private const int MaxExecutablePath = 1024;
    private const int SCS_32BIT_BINARY = 0;
    private const int SCS_DOS_BINARY = 1;
    private const int SCS_WOW_BINARY = 2;
    private const int SCS_PIF_BINARY = 3;
    private const int SCS_POSIX_BINARY = 4;
    private const int SCS_OS216_BINARY = 5;
    private const int SCS_64BIT_BINARY = 6;
    private const uint SHGFI_EXETYPE = 8192;
    [TraceSource("NativeCP", "NativeCP")]
    private static PSTraceSource tracer = PSTraceSource.GetTracer("NativeCP", "NativeCP");
    private ApplicationInfo applicationInfo;
    private bool isPreparedCalled;
    private NativeCommandParameterBinderController nativeParameterBinderController;
    private Process nativeProcess;
    private ProcessInputWriter inputWriter;
    private ProcessOutputReader outputReader;
    private bool _runStandAlone;
    private object sync = new object();
    private bool stopped;
    private bool isMiniShell;
    private static bool _isServerSide;

    internal NativeCommandProcessor(ApplicationInfo applicationInfo, ExecutionContext context)
      : base((CommandInfo) applicationInfo)
    {
      using (NativeCommandProcessor.tracer.TraceConstructor((object) this))
      {
        if (applicationInfo == null)
          throw NativeCommandProcessor.tracer.NewArgumentNullException(nameof (applicationInfo));
        NativeCommandProcessor.tracer.WriteLine("NativeCommnadName: {0} Path: {1}", (object) applicationInfo.Name, (object) applicationInfo.Path);
        this.applicationInfo = applicationInfo;
        this.context = context;
        this.Command = (InternalCommand) new NativeCommand();
        this.Command.CommandInfo = (CommandInfo) applicationInfo;
        this.Command.Context = context;
        this.Command.commandRuntime = (ICommandRuntime) (this.commandRuntime = new MshCommandRuntime(context, (CommandInfo) applicationInfo, this.Command));
        ((NativeCommand) this.Command).MyCommandProcessor = this;
        this.inputWriter = new ProcessInputWriter(this.Command);
      }
    }

    private NativeCommand nativeCommand => this.Command as NativeCommand;

    private string NativeCommandName => this.applicationInfo.Name;

    private string Path => this.applicationInfo.Path;

    internal override ParameterBinderController NewParameterBinderController(
      InternalCommand command)
    {
      this.nativeParameterBinderController = !this.isMiniShell ? new NativeCommandParameterBinderController(this.nativeCommand) : (NativeCommandParameterBinderController) new MinishellParameterBinderController(this.nativeCommand);
      return (ParameterBinderController) this.nativeParameterBinderController;
    }

    internal NativeCommandParameterBinderController NativeParameterBinderController
    {
      get
      {
        if (this.nativeParameterBinderController == null)
          this.NewParameterBinderController(this.Command);
        return this.nativeParameterBinderController;
      }
    }

    internal override void Prepare(params CommandParameterInternal[] myParameters)
    {
      this.isPreparedCalled = true;
      foreach (CommandParameterInternal myParameter in myParameters)
        this.arguments.Add(myParameter);
      this.isMiniShell = this.IsMiniShell();
      if (!this.isMiniShell)
        this.NativeParameterBinderController.BindParameters(this.arguments);
      this.commandRuntime.ClearOutputAndErrorPipes();
    }

    internal override void ProcessRecord()
    {
      while (this.Read())
        this.inputWriter.Add((object) this.Command.CurrentPipelineObject);
    }

    internal override void Complete()
    {
      bool redirectOutput;
      bool redirectError;
      bool redirectInput;
      this.CalculateIORedirection(out redirectOutput, out redirectError, out redirectInput);
      bool soloCommand = this.Command.MyInvocation.PipelineLength == 1;
      ProcessStartInfo processStartInfo = this.GetProcessStartInfo(redirectOutput, redirectError, redirectInput, soloCommand);
      if (this.Command.Context.CurrentPipelineStopping)
        throw new PipelineStoppedException();
      Exception innerException = (Exception) null;
      try
      {
        if (!redirectOutput)
          this.Command.Context.EngineHostInterface.NotifyBeginApplication();
        lock (this.sync)
        {
          if (this.stopped)
            throw new PipelineStoppedException();
          try
          {
            this.nativeProcess = new Process();
            this.nativeProcess.StartInfo = processStartInfo;
            this.nativeProcess.Start();
          }
          catch (Win32Exception ex1)
          {
            string executable = NativeCommandProcessor.FindExecutable(processStartInfo.FileName);
            bool flag = true;
            if (!string.IsNullOrEmpty(executable))
            {
              if (NativeCommandProcessor.IsConsoleApplication(executable))
                ConsoleVisibility.AllocateHiddenConsole();
              string arguments = processStartInfo.Arguments;
              string fileName = processStartInfo.FileName;
              processStartInfo.Arguments = "\"" + processStartInfo.FileName + "\" " + processStartInfo.Arguments;
              processStartInfo.FileName = executable;
              try
              {
                this.nativeProcess.Start();
                flag = false;
              }
              catch (Win32Exception ex2)
              {
                processStartInfo.Arguments = arguments;
                processStartInfo.FileName = fileName;
              }
            }
            if (flag)
            {
              if (soloCommand && !processStartInfo.UseShellExecute)
              {
                processStartInfo.UseShellExecute = true;
                processStartInfo.RedirectStandardInput = false;
                processStartInfo.RedirectStandardOutput = false;
                processStartInfo.RedirectStandardError = false;
                this.nativeProcess.Start();
              }
              else
                throw;
            }
          }
        }
        bool flag1;
        if (this.Command.MyInvocation.PipelinePosition < this.Command.MyInvocation.PipelineLength)
        {
          flag1 = false;
        }
        else
        {
          flag1 = true;
          if (!processStartInfo.UseShellExecute)
            flag1 = NativeCommandProcessor.IsWindowsApplication(this.nativeProcess.StartInfo.FileName);
        }
        try
        {
          if (processStartInfo.RedirectStandardInput)
          {
            NativeCommandIOFormat inputFormat = NativeCommandIOFormat.Text;
            if (this.isMiniShell)
              inputFormat = ((MinishellParameterBinderController) this.NativeParameterBinderController).InputFormat;
            lock (this.sync)
            {
              if (!this.stopped)
                this.inputWriter.Start(this.nativeProcess, inputFormat);
            }
          }
          if (!flag1)
          {
            if (!processStartInfo.RedirectStandardOutput)
            {
              if (!processStartInfo.RedirectStandardError)
                goto label_54;
            }
            lock (this.sync)
            {
              if (!this.stopped)
              {
                this.outputReader = new ProcessOutputReader(this.nativeProcess, this.Path, redirectOutput, redirectError);
                this.outputReader.Start();
              }
            }
            if (this.outputReader != null)
              this.ProcessOutputHelper();
          }
        }
        catch (Exception ex)
        {
          NativeCommandProcessor.KillProcess(this.nativeProcess);
          throw;
        }
        finally
        {
          if (!flag1)
          {
            this.nativeProcess.WaitForExit();
            this.inputWriter.Done();
            if (this.outputReader != null)
              this.outputReader.Done();
            this.Command.Context.SetVariable("global:LASTEXITCODE", (object) this.nativeProcess.ExitCode);
            if (this.nativeProcess.ExitCode != 0)
              this.commandRuntime.PipelineProcessor.ExecutionFailed = true;
          }
        }
      }
      catch (Win32Exception ex)
      {
        innerException = (Exception) ex;
      }
      catch (PipelineStoppedException ex)
      {
        throw;
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        innerException = ex;
      }
      finally
      {
        if (!redirectOutput)
          this.Command.Context.EngineHostInterface.NotifyEndApplication();
        this.CleanUp();
      }
label_54:
      if (innerException != null)
      {
        string message = ResourceManagerCache.FormatResourceString("Parser", "ProgramFailedToExecute", (object) this.NativeCommandName, (object) innerException.Message, (object) this.Command.MyInvocation.PositionMessage);
        if (message == null)
          message = StringUtil.Format("Program '{0}' failed to execute: {1}{2}", (object) this.NativeCommandName, (object) innerException.Message, (object) this.Command.MyInvocation.PositionMessage);
        ApplicationFailedException applicationFailedException = new ApplicationFailedException(message, innerException);
        NativeCommandProcessor.tracer.TraceException((Exception) applicationFailedException);
        throw applicationFailedException;
      }
    }

    private static void KillProcess(Process processToKill)
    {
      if (NativeCommandProcessor.IsServerSide)
      {
        NativeCommandProcessor.ProcessWithParentId[] currentlyRunningProcs = NativeCommandProcessor.ProcessWithParentId.Construct(Process.GetProcesses());
        NativeCommandProcessor.KillProcessAndChildProcesses(processToKill, currentlyRunningProcs);
      }
      else
      {
        try
        {
          processToKill.Kill();
        }
        catch (Win32Exception ex1)
        {
          try
          {
            Process.GetProcessById(processToKill.Id).Kill();
          }
          catch (Exception ex2)
          {
            CommandProcessorBase.CheckForSevereException(ex2);
          }
        }
        catch (Exception ex)
        {
          CommandProcessorBase.CheckForSevereException(ex);
        }
      }
    }

    private static void KillProcessAndChildProcesses(
      Process processToKill,
      NativeCommandProcessor.ProcessWithParentId[] currentlyRunningProcs)
    {
      try
      {
        NativeCommandProcessor.KillChildProcesses(processToKill.Id, currentlyRunningProcs);
        processToKill.Kill();
      }
      catch (Win32Exception ex1)
      {
        try
        {
          Process.GetProcessById(processToKill.Id).Kill();
        }
        catch (Exception ex2)
        {
          CommandProcessorBase.CheckForSevereException(ex2);
        }
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
    }

    private static void KillChildProcesses(
      int parentId,
      NativeCommandProcessor.ProcessWithParentId[] currentlyRunningProcs)
    {
      foreach (NativeCommandProcessor.ProcessWithParentId currentlyRunningProc in currentlyRunningProcs)
      {
        if (currentlyRunningProc.ParentId > 0 && currentlyRunningProc.ParentId == parentId)
          NativeCommandProcessor.KillProcessAndChildProcesses(currentlyRunningProc.OriginalProcessInstance, currentlyRunningProcs);
      }
    }

    private static bool IsConsoleApplication(string fileName) => !NativeCommandProcessor.IsWindowsApplication(fileName);

    [ArchitectureSensitive]
    private static bool IsWindowsApplication(string fileName)
    {
      NativeCommandProcessor.SHFILEINFO psfi = new NativeCommandProcessor.SHFILEINFO();
      switch ((int) NativeCommandProcessor.SHGetFileInfo(fileName, 0U, ref psfi, (uint) Marshal.SizeOf((object) psfi), 8192U))
      {
        case 0:
          return false;
        case 17744:
          return false;
        case 23117:
          return false;
        default:
          return true;
      }
    }

    internal void StopProcessing()
    {
      lock (this.sync)
      {
        if (this.stopped)
          return;
        this.stopped = true;
      }
      if (this.nativeProcess == null || this._runStandAlone)
        return;
      this.inputWriter.Stop();
      if (this.outputReader != null)
        this.outputReader.Stop();
      NativeCommandProcessor.KillProcess(this.nativeProcess);
    }

    private void CleanUp()
    {
      try
      {
        if (this.nativeProcess == null)
          return;
        this.nativeProcess.Close();
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
      }
    }

    private void ProcessOutputHelper()
    {
      for (object obj = this.outputReader.Read(); obj != AutomationNull.Value; obj = this.outputReader.Read())
      {
        ProcessOutputObject processOutputObject = obj as ProcessOutputObject;
        if (processOutputObject.Stream == MinishellStream.Error)
        {
          ErrorRecord data = processOutputObject.Data as ErrorRecord;
          data.SetInvocationInfo(this.Command.MyInvocation);
          this.commandRuntime._WriteErrorSkipAllowCheck(data);
        }
        else if (processOutputObject.Stream == MinishellStream.Output)
          this.commandRuntime._WriteObjectSkipAllowCheck(processOutputObject.Data);
        else if (processOutputObject.Stream == MinishellStream.Debug)
          this.Command.PSHostInternal.UI.WriteDebugLine(processOutputObject.Data as string);
        else if (processOutputObject.Stream == MinishellStream.Verbose)
          this.Command.PSHostInternal.UI.WriteVerboseLine(processOutputObject.Data as string);
        else if (processOutputObject.Stream == MinishellStream.Warning)
          this.Command.PSHostInternal.UI.WriteWarningLine(processOutputObject.Data as string);
        else if (processOutputObject.Stream == MinishellStream.Progress && processOutputObject.Data is PSObject data)
        {
          long sourceId = 0;
          PSMemberInfo property1 = (PSMemberInfo) data.Properties["SourceId"];
          if (property1 != null)
            sourceId = (long) property1.Value;
          PSMemberInfo property2 = (PSMemberInfo) data.Properties["Record"];
          ProgressRecord record = (ProgressRecord) null;
          if (property2 != null)
            record = property2.Value as ProgressRecord;
          if (record != null)
            this.Command.PSHostInternal.UI.WriteProgress(sourceId, record);
        }
        if (this.Command.Context.CurrentPipelineStopping)
        {
          this.StopProcessing();
          break;
        }
      }
    }

    private ProcessStartInfo GetProcessStartInfo(
      bool redirectOutput,
      bool redirectError,
      bool redirectInput,
      bool soloCommand)
    {
      ProcessStartInfo processStartInfo = new ProcessStartInfo();
      processStartInfo.FileName = this.Path;
      if (this.validateExtension(this.Path))
      {
        processStartInfo.UseShellExecute = false;
        if (redirectInput)
          processStartInfo.RedirectStandardInput = true;
        if (redirectOutput)
          processStartInfo.RedirectStandardOutput = true;
        if (redirectError)
          processStartInfo.RedirectStandardError = true;
      }
      else
      {
        if (!soloCommand)
          throw InterpreterError.NewInterpreterException((object) this.Path, typeof (RuntimeException), this.Command.CallingToken, "CantActivateDocumentInPipeline", (object) this.Path);
        processStartInfo.UseShellExecute = true;
      }
      if (this.isMiniShell)
      {
        MinishellParameterBinderController binderController = (MinishellParameterBinderController) this.NativeParameterBinderController;
        binderController.BindParameters(this.arguments, redirectOutput, this.Command.Context.EngineHostInterface.Name);
        processStartInfo.CreateNoWindow = binderController.NonInteractive;
      }
      processStartInfo.Arguments = this.NativeParameterBinderController.Arguments;
      ExecutionContext context = this.Command.Context;
      string providerPath = context.EngineSessionState.GetNamespaceCurrentLocation(context.ProviderNames.FileSystem).ProviderPath;
      processStartInfo.WorkingDirectory = WildcardPattern.Unescape(providerPath);
      return processStartInfo;
    }

    private void CalculateIORedirection(
      out bool redirectOutput,
      out bool redirectError,
      out bool redirectInput)
    {
      redirectInput = true;
      redirectOutput = true;
      redirectError = true;
      if (this.Command.MyInvocation.PipelinePosition == this.Command.MyInvocation.PipelineLength)
      {
        if (this.context.IsTopLevelPipe(this.commandRuntime.OutputPipe))
        {
          redirectOutput = false;
        }
        else
        {
          CommandProcessorBase downstreamCmdlet = this.commandRuntime.OutputPipe.DownstreamCmdlet;
          if (downstreamCmdlet != null && string.Equals(downstreamCmdlet.CommandInfo.Name, "Out-Default", StringComparison.OrdinalIgnoreCase))
            redirectOutput = false;
        }
      }
      if (!this.CommandRuntime.MergeMyErrorOutputWithSuccess)
      {
        if (this.context.IsTopLevelPipe(this.commandRuntime.ErrorOutputPipe))
        {
          redirectError = false;
        }
        else
        {
          CommandProcessorBase downstreamCmdlet = this.commandRuntime.ErrorOutputPipe.DownstreamCmdlet;
          if (downstreamCmdlet != null && string.Equals(downstreamCmdlet.CommandInfo.Name, "Out-Default", StringComparison.OrdinalIgnoreCase))
            redirectError = false;
        }
      }
      if (!redirectError && redirectOutput && this.isMiniShell)
        redirectError = true;
      if (this.inputWriter.Count == 0 && !this.Command.MyInvocation.ExpectingInput)
        redirectInput = false;
      if (NativeCommandProcessor.IsServerSide)
      {
        redirectInput = true;
        redirectOutput = true;
        redirectError = true;
      }
      else if (NativeCommandProcessor.IsConsoleApplication(this.Path))
      {
        ConsoleVisibility.AllocateHiddenConsole();
        if (ConsoleVisibility.AlwaysCaptureApplicationIO)
        {
          redirectOutput = true;
          redirectError = true;
        }
      }
      if (redirectInput || redirectOutput)
        return;
      this._runStandAlone = true;
    }

    private bool validateExtension(string path)
    {
      string extension = System.IO.Path.GetExtension(path);
      string str = (string) LanguagePrimitives.ConvertTo(this.Command.Context.GetVariable("ENV:PATHEXT"), typeof (string), (IFormatProvider) CultureInfo.InvariantCulture);
      string[] strArray;
      if (str == null)
        strArray = new string[1]{ ".exe" };
      else
        strArray = str.Split(';');
      foreach (string a in strArray)
      {
        if (string.Equals(a, extension, StringComparison.OrdinalIgnoreCase))
          return true;
      }
      return false;
    }

    [DllImport("shell32.dll", EntryPoint = "FindExecutable")]
    private static extern IntPtr FindExecutableW(
      string fileName,
      string directoryPath,
      StringBuilder pathFound);

    [ArchitectureSensitive]
    private static string FindExecutable(string filename)
    {
      StringBuilder pathFound = new StringBuilder(1024);
      IntPtr num = (IntPtr) 0;
      try
      {
        num = NativeCommandProcessor.FindExecutableW(filename, string.Empty, pathFound);
      }
      catch (IndexOutOfRangeException ex)
      {
        WindowsErrorReporting.FailFast((Exception) ex);
      }
      return (long) num >= 32L ? pathFound.ToString() : (string) null;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int GetBinaryTypeA(string lpApplicationName, ref int lpBinaryType);

    [DllImport("shell32.dll")]
    private static extern IntPtr SHGetFileInfo(
      string pszPath,
      uint dwFileAttributes,
      ref NativeCommandProcessor.SHFILEINFO psfi,
      uint cbSizeFileInfo,
      uint uFlags);

    private bool IsMiniShell()
    {
      for (int index = 0; index < this.arguments.Count; ++index)
      {
        CommandParameterInternal parameterInternal = this.arguments[index];
        if (parameterInternal.Name == null && parameterInternal.Value1 != null && parameterInternal.Value1 is ScriptBlock)
        {
          NativeCommandProcessor.tracer.WriteLine("Argument number {0} is scriptblock. Its value is {1} ", (object) index, (object) parameterInternal.Value1.ToString());
          return true;
        }
      }
      return false;
    }

    internal static bool IsServerSide
    {
      get => NativeCommandProcessor._isServerSide;
      set => NativeCommandProcessor._isServerSide = value;
    }

    internal struct ProcessWithParentId
    {
      public Process OriginalProcessInstance;
      private int parentId;

      public int ParentId
      {
        get
        {
          if (int.MinValue == this.parentId)
            this.ConstructParentId();
          return this.parentId;
        }
      }

      public ProcessWithParentId(Process originalProcess)
      {
        this.OriginalProcessInstance = originalProcess;
        this.parentId = int.MinValue;
      }

      public static NativeCommandProcessor.ProcessWithParentId[] Construct(
        Process[] originalProcCollection)
      {
        NativeCommandProcessor.ProcessWithParentId[] processWithParentIdArray = new NativeCommandProcessor.ProcessWithParentId[originalProcCollection.Length];
        for (int index = 0; index < originalProcCollection.Length; ++index)
          processWithParentIdArray[index] = new NativeCommandProcessor.ProcessWithParentId(originalProcCollection[index]);
        return processWithParentIdArray;
      }

      private void ConstructParentId()
      {
        try
        {
          this.parentId = -1;
          using (ManagementObject managementObject = new ManagementObject(new ManagementPath(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Win32_Process.Handle=\"{0}\"", (object) this.OriginalProcessInstance.Id))))
          {
            managementObject.Get();
            this.parentId = Convert.ToInt32(managementObject["ParentProcessId"], (IFormatProvider) CultureInfo.InvariantCulture);
          }
        }
        catch (Win32Exception ex)
        {
          NativeCommandProcessor.tracer.WriteLine("Exception occured while retreiving parent id", new object[0]);
        }
        catch (InvalidOperationException ex)
        {
          NativeCommandProcessor.tracer.WriteLine("Exception occured while retreiving parent id", new object[0]);
        }
        catch (ManagementException ex)
        {
          NativeCommandProcessor.tracer.WriteLine("Exception occured while retreiving parent id", new object[0]);
        }
      }
    }

    private struct SHFILEINFO
    {
      public IntPtr hIcon;
      public IntPtr iIcon;
      public uint dwAttributes;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
      public string szDisplayName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
      public string szTypeName;
    }
  }
}
