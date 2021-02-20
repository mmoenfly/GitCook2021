// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.RemoteHostCall
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation.Host;
using System.Management.Automation.Internal.Host;
using System.Reflection;
using System.Security;

namespace System.Management.Automation.Remoting
{
  internal class RemoteHostCall
  {
    private RemoteHostMethodId _methodId;
    private object[] _parameters;
    private RemoteHostMethodInfo _methodInfo;
    private long _callId;
    private string _computerName;

    internal string MethodName => this._methodInfo.Name;

    internal RemoteHostMethodId MethodId => this._methodId;

    internal object[] Parameters => this._parameters;

    internal long CallId => this._callId;

    internal RemoteHostCall(long callId, RemoteHostMethodId methodId, object[] parameters)
    {
      this._callId = callId;
      this._methodId = methodId;
      this._parameters = parameters;
      this._methodInfo = RemoteHostMethodInfo.LookUp(methodId);
    }

    private static PSObject EncodeParameters(object[] parameters)
    {
      ArrayList arrayList = new ArrayList();
      for (int index = 0; index < parameters.Length; ++index)
      {
        object obj = parameters[index] == null ? (object) null : RemoteHostEncoder.EncodeObject(parameters[index]);
        arrayList.Add(obj);
      }
      return new PSObject((object) arrayList);
    }

    private static object[] DecodeParameters(PSObject parametersPSObject, Type[] parameterTypes)
    {
      ArrayList baseObject = (ArrayList) parametersPSObject.BaseObject;
      List<object> objectList = new List<object>();
      for (int index = 0; index < baseObject.Count; ++index)
      {
        object obj = baseObject[index] == null ? (object) null : RemoteHostEncoder.DecodeObject(baseObject[index], parameterTypes[index]);
        objectList.Add(obj);
      }
      return objectList.ToArray();
    }

    internal PSObject Encode()
    {
      PSObject emptyPsObject = RemotingEncoder.CreateEmptyPSObject();
      PSObject psObject = RemoteHostCall.EncodeParameters(this._parameters);
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("ci", (object) this._callId));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("mi", (object) this._methodId));
      emptyPsObject.Properties.Add((PSPropertyInfo) new PSNoteProperty("mp", (object) psObject));
      return emptyPsObject;
    }

    internal static RemoteHostCall Decode(PSObject data)
    {
      long propertyValue1 = RemotingDecoder.GetPropertyValue<long>(data, "ci");
      PSObject propertyValue2 = RemotingDecoder.GetPropertyValue<PSObject>(data, "mp");
      RemoteHostMethodId propertyValue3 = RemotingDecoder.GetPropertyValue<RemoteHostMethodId>(data, "mi");
      RemoteHostMethodInfo remoteHostMethodInfo = RemoteHostMethodInfo.LookUp(propertyValue3);
      object[] parameters = RemoteHostCall.DecodeParameters(propertyValue2, remoteHostMethodInfo.ParameterTypes);
      return new RemoteHostCall(propertyValue1, propertyValue3, parameters);
    }

    internal bool IsVoidMethod => this._methodInfo.ReturnType == typeof (void);

    internal void ExecuteVoidMethod(PSHost clientHost)
    {
      RemoteRunspace remoteRunspace = (RemoteRunspace) null;
      if (this.IsSetShouldExitOrPopRunspace)
        remoteRunspace = this.GetRemoteRunspaceToClose(clientHost);
      try
      {
        this.MyMethodBase.Invoke(this.SelectTargetObject(clientHost), this._parameters);
      }
      finally
      {
        remoteRunspace?.Close();
      }
    }

    private RemoteRunspace GetRemoteRunspaceToClose(PSHost clientHost)
    {
      if (!(clientHost is IHostSupportsInteractiveSession interactiveSession) || !interactiveSession.IsRunspacePushed)
        return (RemoteRunspace) null;
      return !(interactiveSession.Runspace is RemoteRunspace runspace) || !runspace.ShouldCloseOnPop ? (RemoteRunspace) null : runspace;
    }

    private MethodBase MyMethodBase => (MethodBase) this._methodInfo.InterfaceType.GetMethod(this._methodInfo.Name, this._methodInfo.ParameterTypes);

    internal RemoteHostResponse ExecuteNonVoidMethod(PSHost clientHost) => this.ExecuteNonVoidMethodOnObject(this.SelectTargetObject(clientHost));

    private RemoteHostResponse ExecuteNonVoidMethodOnObject(object instance)
    {
      Exception exception = (Exception) null;
      object returnValue = (object) null;
      try
      {
        if (this._methodId == RemoteHostMethodId.GetBufferContents)
          throw new PSRemotingDataStructureException(PSRemotingErrorId.RemoteHostGetBufferContents, new object[1]
          {
            (object) this._computerName.ToUpper(CultureInfo.CurrentCulture)
          });
        returnValue = this.MyMethodBase.Invoke(instance, this._parameters);
      }
      catch (Exception ex)
      {
        CommandProcessorBase.CheckForSevereException(ex);
        exception = ex.InnerException;
      }
      return new RemoteHostResponse(this._callId, this._methodId, returnValue, exception);
    }

    private object SelectTargetObject(PSHost host)
    {
      if (host == null || host.UI == null)
        return (object) null;
      if (this._methodInfo.InterfaceType == typeof (PSHost))
        return (object) host;
      if (this._methodInfo.InterfaceType == typeof (IHostSupportsInteractiveSession))
        return (object) host;
      if (this._methodInfo.InterfaceType == typeof (PSHostUserInterface))
        return (object) host.UI;
      if (this._methodInfo.InterfaceType == typeof (IHostUISupportsMultipleChoiceSelection))
        return (object) host.UI;
      if (this._methodInfo.InterfaceType == typeof (PSHostRawUserInterface))
        return (object) host.UI.RawUI;
      throw RemoteHostExceptions.NewUnknownTargetClassException(this._methodInfo.InterfaceType.ToString());
    }

    internal bool IsSetShouldExit => this._methodId == RemoteHostMethodId.SetShouldExit;

    internal bool IsSetShouldExitOrPopRunspace => this._methodId == RemoteHostMethodId.SetShouldExit || this._methodId == RemoteHostMethodId.PopRunspace;

    internal Collection<RemoteHostCall> PerformSecurityChecksOnHostMessage(
      string computerName)
    {
      this._computerName = computerName;
      Collection<RemoteHostCall> collection = new Collection<RemoteHostCall>();
      if (this._methodId == RemoteHostMethodId.PromptForCredential1 || this._methodId == RemoteHostMethodId.PromptForCredential2)
      {
        string str1 = this.ModifyCaption((string) this._parameters[0]);
        string str2 = this.ModifyMessage((string) this._parameters[1], computerName);
        this._parameters[0] = (object) str1;
        this._parameters[1] = (object) str2;
      }
      else if (this._methodId == RemoteHostMethodId.Prompt)
      {
        if (this._parameters.Length == 3)
        {
          foreach (FieldDescription field in (Collection<FieldDescription>) this._parameters[2])
          {
            field.IsFromRemoteHost = true;
            Type fieldType = InternalHostUserInterface.GetFieldType(field);
            if (fieldType != null)
            {
              if (fieldType == typeof (PSCredential))
              {
                string str1 = this.ModifyCaption((string) this._parameters[0]);
                string str2 = this.ModifyMessage((string) this._parameters[1], computerName);
                this._parameters[0] = (object) str1;
                this._parameters[1] = (object) str2;
                field.ModifiedByRemotingProtocol = true;
              }
              else if (fieldType == typeof (SecureString))
                collection.Add(this.ConstructWarningMessageForSecureString(computerName, PSRemotingErrorId.RemoteHostPromptSecureStringPrompt));
            }
          }
        }
      }
      else if (this._methodId == RemoteHostMethodId.ReadLineAsSecureString)
        collection.Add(this.ConstructWarningMessageForSecureString(computerName, PSRemotingErrorId.RemoteHostReadLineAsSecureStringPrompt));
      else if (this._methodId == RemoteHostMethodId.GetBufferContents)
        collection.Add(this.ConstructWarningMessageForGetBufferContents(computerName));
      return collection;
    }

    private string ModifyCaption(string caption)
    {
      string resourceString = ResourceManagerCache.GetResourceString("CredUI", "PromptForCredential_DefaultCaption");
      if (caption.Equals(resourceString, StringComparison.OrdinalIgnoreCase))
        return caption;
      return PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.RemoteHostPromptForCredentialModifiedCaption, (object) caption);
    }

    private string ModifyMessage(string message, string computerName) => PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.RemoteHostPromptForCredentialModifiedMessage, (object) computerName.ToUpper(CultureInfo.CurrentCulture), (object) message);

    private RemoteHostCall ConstructWarningMessageForSecureString(
      string computerName,
      PSRemotingErrorId errorId)
    {
      return new RemoteHostCall(-100L, RemoteHostMethodId.WriteWarningLine, new object[1]
      {
        (object) PSRemotingErrorInvariants.FormatResourceString(errorId, (object) computerName.ToUpper(CultureInfo.CurrentCulture))
      });
    }

    private RemoteHostCall ConstructWarningMessageForGetBufferContents(
      string computerName)
    {
      return new RemoteHostCall(-100L, RemoteHostMethodId.WriteWarningLine, new object[1]
      {
        (object) PSRemotingErrorInvariants.FormatResourceString(PSRemotingErrorId.RemoteHostGetBufferContents, (object) computerName.ToUpper(CultureInfo.CurrentCulture))
      });
    }
  }
}
