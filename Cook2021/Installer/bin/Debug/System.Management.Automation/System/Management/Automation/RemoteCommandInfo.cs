// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.RemoteCommandInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Collections.ObjectModel;

namespace System.Management.Automation
{
  public class RemoteCommandInfo : CommandInfo
  {
    private string definition;

    private RemoteCommandInfo(string name, CommandTypes type)
      : base(name, type)
    {
    }

    public override string Definition => this.definition;

    internal static RemoteCommandInfo FromPSObjectForRemoting(PSObject psObject)
    {
      RemoteCommandInfo remoteCommandInfo = (RemoteCommandInfo) null;
      if (SerializationUtilities.GetPsObjectPropertyBaseObject(psObject, "CommandInfo_CommandType") != null)
      {
        CommandTypes propertyValue = RemotingDecoder.GetPropertyValue<CommandTypes>(psObject, "CommandInfo_CommandType");
        remoteCommandInfo = new RemoteCommandInfo(RemotingDecoder.GetPropertyValue<string>(psObject, "CommandInfo_Name"), propertyValue);
        remoteCommandInfo.definition = RemotingDecoder.GetPropertyValue<string>(psObject, "CommandInfo_Definition");
        remoteCommandInfo.Visibility = RemotingDecoder.GetPropertyValue<SessionStateEntryVisibility>(psObject, "CommandInfo_Visibility");
      }
      return remoteCommandInfo;
    }

    internal static void ToPSObjectForRemoting(CommandInfo commandInfo, PSObject psObject)
    {
      if (commandInfo == null)
        return;
      RemotingEncoder.AddNoteProperty<CommandTypes>(psObject, "CommandInfo_CommandType", (RemotingEncoder.ValueGetterDelegate<CommandTypes>) (() => commandInfo.CommandType));
      RemotingEncoder.AddNoteProperty<string>(psObject, "CommandInfo_Definition", (RemotingEncoder.ValueGetterDelegate<string>) (() => commandInfo.Definition));
      RemotingEncoder.AddNoteProperty<string>(psObject, "CommandInfo_Name", (RemotingEncoder.ValueGetterDelegate<string>) (() => commandInfo.Name));
      RemotingEncoder.AddNoteProperty<SessionStateEntryVisibility>(psObject, "CommandInfo_Visibility", (RemotingEncoder.ValueGetterDelegate<SessionStateEntryVisibility>) (() => commandInfo.Visibility));
    }

    public override ReadOnlyCollection<PSTypeName> OutputType => (ReadOnlyCollection<PSTypeName>) null;
  }
}
