// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.RemoteHostMethodInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Security;

namespace System.Management.Automation.Remoting
{
  internal class RemoteHostMethodInfo
  {
    private Type _interfaceType;
    private string _name;
    private Type _returnType;
    private Type[] _parameterTypes;

    internal Type InterfaceType => this._interfaceType;

    internal string Name => this._name;

    internal Type ReturnType => this._returnType;

    internal Type[] ParameterTypes => this._parameterTypes;

    internal RemoteHostMethodInfo(
      Type interfaceType,
      string name,
      Type returnType,
      Type[] parameterTypes)
    {
      this._interfaceType = interfaceType;
      this._name = name;
      this._returnType = returnType;
      this._parameterTypes = parameterTypes;
    }

    internal static RemoteHostMethodInfo LookUp(RemoteHostMethodId methodId)
    {
      switch (methodId)
      {
        case RemoteHostMethodId.GetName:
          return new RemoteHostMethodInfo(typeof (PSHost), "get_Name", typeof (string), new Type[0]);
        case RemoteHostMethodId.GetVersion:
          return new RemoteHostMethodInfo(typeof (PSHost), "get_Version", typeof (Version), new Type[0]);
        case RemoteHostMethodId.GetInstanceId:
          return new RemoteHostMethodInfo(typeof (PSHost), "get_InstanceId", typeof (Guid), new Type[0]);
        case RemoteHostMethodId.GetCurrentCulture:
          return new RemoteHostMethodInfo(typeof (PSHost), "get_CurrentCulture", typeof (CultureInfo), new Type[0]);
        case RemoteHostMethodId.GetCurrentUICulture:
          return new RemoteHostMethodInfo(typeof (PSHost), "get_CurrentUICulture", typeof (CultureInfo), new Type[0]);
        case RemoteHostMethodId.SetShouldExit:
          return new RemoteHostMethodInfo(typeof (PSHost), "SetShouldExit", typeof (void), new Type[1]
          {
            typeof (int)
          });
        case RemoteHostMethodId.EnterNestedPrompt:
          return new RemoteHostMethodInfo(typeof (PSHost), "EnterNestedPrompt", typeof (void), new Type[0]);
        case RemoteHostMethodId.ExitNestedPrompt:
          return new RemoteHostMethodInfo(typeof (PSHost), "ExitNestedPrompt", typeof (void), new Type[0]);
        case RemoteHostMethodId.NotifyBeginApplication:
          return new RemoteHostMethodInfo(typeof (PSHost), "NotifyBeginApplication", typeof (void), new Type[0]);
        case RemoteHostMethodId.NotifyEndApplication:
          return new RemoteHostMethodInfo(typeof (PSHost), "NotifyEndApplication", typeof (void), new Type[0]);
        case RemoteHostMethodId.ReadLine:
          return new RemoteHostMethodInfo(typeof (PSHostUserInterface), "ReadLine", typeof (string), new Type[0]);
        case RemoteHostMethodId.ReadLineAsSecureString:
          return new RemoteHostMethodInfo(typeof (PSHostUserInterface), "ReadLineAsSecureString", typeof (SecureString), new Type[0]);
        case RemoteHostMethodId.Write1:
          return new RemoteHostMethodInfo(typeof (PSHostUserInterface), "Write", typeof (void), new Type[1]
          {
            typeof (string)
          });
        case RemoteHostMethodId.Write2:
          return new RemoteHostMethodInfo(typeof (PSHostUserInterface), "Write", typeof (void), new Type[3]
          {
            typeof (ConsoleColor),
            typeof (ConsoleColor),
            typeof (string)
          });
        case RemoteHostMethodId.WriteLine1:
          return new RemoteHostMethodInfo(typeof (PSHostUserInterface), "WriteLine", typeof (void), new Type[0]);
        case RemoteHostMethodId.WriteLine2:
          return new RemoteHostMethodInfo(typeof (PSHostUserInterface), "WriteLine", typeof (void), new Type[1]
          {
            typeof (string)
          });
        case RemoteHostMethodId.WriteLine3:
          return new RemoteHostMethodInfo(typeof (PSHostUserInterface), "WriteLine", typeof (void), new Type[3]
          {
            typeof (ConsoleColor),
            typeof (ConsoleColor),
            typeof (string)
          });
        case RemoteHostMethodId.WriteErrorLine:
          return new RemoteHostMethodInfo(typeof (PSHostUserInterface), "WriteErrorLine", typeof (void), new Type[1]
          {
            typeof (string)
          });
        case RemoteHostMethodId.WriteDebugLine:
          return new RemoteHostMethodInfo(typeof (PSHostUserInterface), "WriteDebugLine", typeof (void), new Type[1]
          {
            typeof (string)
          });
        case RemoteHostMethodId.WriteProgress:
          return new RemoteHostMethodInfo(typeof (PSHostUserInterface), "WriteProgress", typeof (void), new Type[2]
          {
            typeof (long),
            typeof (ProgressRecord)
          });
        case RemoteHostMethodId.WriteVerboseLine:
          return new RemoteHostMethodInfo(typeof (PSHostUserInterface), "WriteVerboseLine", typeof (void), new Type[1]
          {
            typeof (string)
          });
        case RemoteHostMethodId.WriteWarningLine:
          return new RemoteHostMethodInfo(typeof (PSHostUserInterface), "WriteWarningLine", typeof (void), new Type[1]
          {
            typeof (string)
          });
        case RemoteHostMethodId.Prompt:
          return new RemoteHostMethodInfo(typeof (PSHostUserInterface), "Prompt", typeof (Dictionary<string, PSObject>), new Type[3]
          {
            typeof (string),
            typeof (string),
            typeof (Collection<FieldDescription>)
          });
        case RemoteHostMethodId.PromptForCredential1:
          return new RemoteHostMethodInfo(typeof (PSHostUserInterface), "PromptForCredential", typeof (PSCredential), new Type[4]
          {
            typeof (string),
            typeof (string),
            typeof (string),
            typeof (string)
          });
        case RemoteHostMethodId.PromptForCredential2:
          return new RemoteHostMethodInfo(typeof (PSHostUserInterface), "PromptForCredential", typeof (PSCredential), new Type[6]
          {
            typeof (string),
            typeof (string),
            typeof (string),
            typeof (string),
            typeof (PSCredentialTypes),
            typeof (PSCredentialUIOptions)
          });
        case RemoteHostMethodId.PromptForChoice:
          return new RemoteHostMethodInfo(typeof (PSHostUserInterface), "PromptForChoice", typeof (int), new Type[4]
          {
            typeof (string),
            typeof (string),
            typeof (Collection<ChoiceDescription>),
            typeof (int)
          });
        case RemoteHostMethodId.GetForegroundColor:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "get_ForegroundColor", typeof (ConsoleColor), new Type[0]);
        case RemoteHostMethodId.SetForegroundColor:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "set_ForegroundColor", typeof (void), new Type[1]
          {
            typeof (ConsoleColor)
          });
        case RemoteHostMethodId.GetBackgroundColor:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "get_BackgroundColor", typeof (ConsoleColor), new Type[0]);
        case RemoteHostMethodId.SetBackgroundColor:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "set_BackgroundColor", typeof (void), new Type[1]
          {
            typeof (ConsoleColor)
          });
        case RemoteHostMethodId.GetCursorPosition:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "get_CursorPosition", typeof (Coordinates), new Type[0]);
        case RemoteHostMethodId.SetCursorPosition:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "set_CursorPosition", typeof (void), new Type[1]
          {
            typeof (Coordinates)
          });
        case RemoteHostMethodId.GetWindowPosition:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "get_WindowPosition", typeof (Coordinates), new Type[0]);
        case RemoteHostMethodId.SetWindowPosition:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "set_WindowPosition", typeof (void), new Type[1]
          {
            typeof (Coordinates)
          });
        case RemoteHostMethodId.GetCursorSize:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "get_CursorSize", typeof (int), new Type[0]);
        case RemoteHostMethodId.SetCursorSize:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "set_CursorSize", typeof (void), new Type[1]
          {
            typeof (int)
          });
        case RemoteHostMethodId.GetBufferSize:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "get_BufferSize", typeof (Size), new Type[0]);
        case RemoteHostMethodId.SetBufferSize:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "set_BufferSize", typeof (void), new Type[1]
          {
            typeof (Size)
          });
        case RemoteHostMethodId.GetWindowSize:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "get_WindowSize", typeof (Size), new Type[0]);
        case RemoteHostMethodId.SetWindowSize:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "set_WindowSize", typeof (void), new Type[1]
          {
            typeof (Size)
          });
        case RemoteHostMethodId.GetWindowTitle:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "get_WindowTitle", typeof (string), new Type[0]);
        case RemoteHostMethodId.SetWindowTitle:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "set_WindowTitle", typeof (void), new Type[1]
          {
            typeof (string)
          });
        case RemoteHostMethodId.GetMaxWindowSize:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "get_MaxWindowSize", typeof (Size), new Type[0]);
        case RemoteHostMethodId.GetMaxPhysicalWindowSize:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "get_MaxPhysicalWindowSize", typeof (Size), new Type[0]);
        case RemoteHostMethodId.GetKeyAvailable:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "get_KeyAvailable", typeof (bool), new Type[0]);
        case RemoteHostMethodId.ReadKey:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "ReadKey", typeof (KeyInfo), new Type[1]
          {
            typeof (ReadKeyOptions)
          });
        case RemoteHostMethodId.FlushInputBuffer:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "FlushInputBuffer", typeof (void), new Type[0]);
        case RemoteHostMethodId.SetBufferContents1:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "SetBufferContents", typeof (void), new Type[2]
          {
            typeof (Rectangle),
            typeof (BufferCell)
          });
        case RemoteHostMethodId.SetBufferContents2:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "SetBufferContents", typeof (void), new Type[2]
          {
            typeof (Coordinates),
            typeof (BufferCell[,])
          });
        case RemoteHostMethodId.GetBufferContents:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "GetBufferContents", typeof (BufferCell[,]), new Type[1]
          {
            typeof (Rectangle)
          });
        case RemoteHostMethodId.ScrollBufferContents:
          return new RemoteHostMethodInfo(typeof (PSHostRawUserInterface), "ScrollBufferContents", typeof (void), new Type[4]
          {
            typeof (Rectangle),
            typeof (Coordinates),
            typeof (Rectangle),
            typeof (BufferCell)
          });
        case RemoteHostMethodId.PushRunspace:
          return new RemoteHostMethodInfo(typeof (IHostSupportsInteractiveSession), "PushRunspace", typeof (void), new Type[1]
          {
            typeof (Runspace)
          });
        case RemoteHostMethodId.PopRunspace:
          return new RemoteHostMethodInfo(typeof (IHostSupportsInteractiveSession), "PopRunspace", typeof (void), new Type[0]);
        case RemoteHostMethodId.GetIsRunspacePushed:
          return new RemoteHostMethodInfo(typeof (IHostSupportsInteractiveSession), "get_IsRunspacePushed", typeof (bool), new Type[0]);
        case RemoteHostMethodId.GetRunspace:
          return new RemoteHostMethodInfo(typeof (IHostSupportsInteractiveSession), "get_Runspace", typeof (Runspace), new Type[0]);
        case RemoteHostMethodId.PromptForChoiceMultipleSelection:
          return new RemoteHostMethodInfo(typeof (IHostUISupportsMultipleChoiceSelection), "PromptForChoice", typeof (Collection<int>), new Type[4]
          {
            typeof (string),
            typeof (string),
            typeof (Collection<ChoiceDescription>),
            typeof (IEnumerable<int>)
          });
        default:
          return (RemoteHostMethodInfo) null;
      }
    }
  }
}
