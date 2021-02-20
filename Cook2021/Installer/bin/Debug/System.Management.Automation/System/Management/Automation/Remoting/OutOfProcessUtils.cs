// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.OutOfProcessUtils
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.IO;
using System.Xml;

namespace System.Management.Automation.Remoting
{
  internal static class OutOfProcessUtils
  {
    internal const string PS_OUT_OF_PROC_DATA_TAG = "Data";
    internal const string PS_OUT_OF_PROC_DATA_ACK_TAG = "DataAck";
    internal const string PS_OUT_OF_PROC_STREAM_ATTRIBUTE = "Stream";
    internal const string PS_OUT_OF_PROC_PSGUID_ATTRIBUTE = "PSGuid";
    internal const string PS_OUT_OF_PROC_CLOSE_TAG = "Close";
    internal const string PS_OUT_OF_PROC_CLOSE_ACK_TAG = "CloseAck";
    internal const string PS_OUT_OF_PROC_COMMAND_TAG = "Command";
    internal const string PS_OUT_OF_PROC_COMMAND_ACK_TAG = "CommandAck";
    internal const string PS_OUT_OF_PROC_SIGNAL_TAG = "Signal";
    internal const string PS_OUT_OF_PROC_SIGNAL_ACK_TAG = "SignalAck";
    internal const int EXITCODE_UNHANDLED_EXCEPTION = 4000;
    internal static XmlReaderSettings XmlReaderSettings = new XmlReaderSettings();

    static OutOfProcessUtils()
    {
      OutOfProcessUtils.XmlReaderSettings.CheckCharacters = false;
      OutOfProcessUtils.XmlReaderSettings.IgnoreComments = true;
      OutOfProcessUtils.XmlReaderSettings.IgnoreProcessingInstructions = true;
      OutOfProcessUtils.XmlReaderSettings.XmlResolver = (XmlResolver) null;
      OutOfProcessUtils.XmlReaderSettings.ConformanceLevel = ConformanceLevel.Fragment;
    }

    internal static string CreateDataPacket(byte[] data, DataPriorityType streamType, Guid psGuid) => string.Format((IFormatProvider) CultureInfo.InvariantCulture, "<{0} {1}='{2}' {3}='{4}'>{5}</{0}>", (object) "Data", (object) "Stream", (object) streamType.ToString(), (object) "PSGuid", (object) psGuid.ToString(), (object) Convert.ToBase64String(data));

    internal static string CreateDataAckPacket(Guid psGuid) => OutOfProcessUtils.CreatePSGuidPacket("DataAck", psGuid);

    internal static string CreateCommandPacket(Guid psGuid) => OutOfProcessUtils.CreatePSGuidPacket("Command", psGuid);

    internal static string CreateCommandAckPacket(Guid psGuid) => OutOfProcessUtils.CreatePSGuidPacket("CommandAck", psGuid);

    internal static string CreateClosePacket(Guid psGuid) => OutOfProcessUtils.CreatePSGuidPacket("Close", psGuid);

    internal static string CreateCloseAckPacket(Guid psGuid) => OutOfProcessUtils.CreatePSGuidPacket("CloseAck", psGuid);

    internal static string CreateSignalPacket(Guid psGuid) => OutOfProcessUtils.CreatePSGuidPacket("Signal", psGuid);

    internal static string CreateSignalAckPacket(Guid psGuid) => OutOfProcessUtils.CreatePSGuidPacket("SignalAck", psGuid);

    private static string CreatePSGuidPacket(string element, Guid psGuid) => string.Format((IFormatProvider) CultureInfo.InvariantCulture, "<{0} {1}='{2}' />", (object) element, (object) "PSGuid", (object) psGuid.ToString());

    internal static void ProcessData(
      string data,
      OutOfProcessUtils.DataProcessingDelegates callbacks)
    {
      if (string.IsNullOrEmpty(data))
        return;
      XmlReader xmlReader = XmlReader.Create((TextReader) new StringReader(data), OutOfProcessUtils.XmlReaderSettings);
      while (xmlReader.Read())
      {
        switch (xmlReader.NodeType)
        {
          case XmlNodeType.Element:
            OutOfProcessUtils.ProcessElement(xmlReader, callbacks);
            continue;
          case XmlNodeType.EndElement:
            continue;
          default:
            throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownNodeType, new object[3]
            {
              (object) xmlReader.NodeType.ToString(),
              (object) XmlNodeType.Element.ToString(),
              (object) XmlNodeType.EndElement.ToString()
            });
        }
      }
    }

    private static void ProcessElement(
      XmlReader xmlReader,
      OutOfProcessUtils.DataProcessingDelegates callbacks)
    {
      switch (xmlReader.LocalName)
      {
        case "Data":
          string stream = xmlReader.AttributeCount == 2 ? xmlReader.GetAttribute("Stream") : throw new PSRemotingTransportException(PSRemotingErrorId.IPCWrongAttributeCountForDataElement, new object[3]
          {
            (object) "Stream",
            (object) "PSGuid",
            (object) "Data"
          });
          Guid psGuid1 = new Guid(xmlReader.GetAttribute("PSGuid"));
          if (!xmlReader.Read())
            throw new PSRemotingTransportException(PSRemotingErrorId.IPCInsufficientDataforElement, new object[1]
            {
              (object) "Data"
            });
          byte[] rawData = xmlReader.NodeType == XmlNodeType.Text ? Convert.FromBase64String(xmlReader.Value) : throw new PSRemotingTransportException(PSRemotingErrorId.IPCOnlyTextExpectedInDataElement, new object[3]
          {
            (object) xmlReader.NodeType,
            (object) "Data",
            (object) XmlNodeType.Text
          });
          callbacks.DataPacketReceived(rawData, stream, psGuid1);
          break;
        case "DataAck":
          Guid psGuid2 = xmlReader.AttributeCount == 1 ? new Guid(xmlReader.GetAttribute("PSGuid")) : throw new PSRemotingTransportException(PSRemotingErrorId.IPCWrongAttributeCountForElement, new object[2]
          {
            (object) "PSGuid",
            (object) "DataAck"
          });
          callbacks.DataAckPacketReceived(psGuid2);
          break;
        case "Command":
          Guid psGuid3 = xmlReader.AttributeCount == 1 ? new Guid(xmlReader.GetAttribute("PSGuid")) : throw new PSRemotingTransportException(PSRemotingErrorId.IPCWrongAttributeCountForElement, new object[2]
          {
            (object) "PSGuid",
            (object) "Command"
          });
          callbacks.CommandCreationPacketReceived(psGuid3);
          break;
        case "CommandAck":
          Guid psGuid4 = xmlReader.AttributeCount == 1 ? new Guid(xmlReader.GetAttribute("PSGuid")) : throw new PSRemotingTransportException(PSRemotingErrorId.IPCWrongAttributeCountForElement, new object[2]
          {
            (object) "PSGuid",
            (object) "CommandAck"
          });
          callbacks.CommandCreationAckReceived(psGuid4);
          break;
        case "Close":
          Guid psGuid5 = xmlReader.AttributeCount == 1 ? new Guid(xmlReader.GetAttribute("PSGuid")) : throw new PSRemotingTransportException(PSRemotingErrorId.IPCWrongAttributeCountForElement, new object[2]
          {
            (object) "PSGuid",
            (object) "Close"
          });
          callbacks.ClosePacketReceived(psGuid5);
          break;
        case "CloseAck":
          Guid psGuid6 = xmlReader.AttributeCount == 1 ? new Guid(xmlReader.GetAttribute("PSGuid")) : throw new PSRemotingTransportException(PSRemotingErrorId.IPCWrongAttributeCountForElement, new object[2]
          {
            (object) "PSGuid",
            (object) "CloseAck"
          });
          callbacks.CloseAckPacketReceived(psGuid6);
          break;
        case "Signal":
          Guid psGuid7 = xmlReader.AttributeCount == 1 ? new Guid(xmlReader.GetAttribute("PSGuid")) : throw new PSRemotingTransportException(PSRemotingErrorId.IPCWrongAttributeCountForElement, new object[2]
          {
            (object) "PSGuid",
            (object) "Signal"
          });
          callbacks.SignalPacketReceived(psGuid7);
          break;
        case "SignalAck":
          Guid psGuid8 = xmlReader.AttributeCount == 1 ? new Guid(xmlReader.GetAttribute("PSGuid")) : throw new PSRemotingTransportException(PSRemotingErrorId.IPCWrongAttributeCountForElement, new object[2]
          {
            (object) "PSGuid",
            (object) "SignalAck"
          });
          callbacks.SignalAckPacketReceived(psGuid8);
          break;
        default:
          throw new PSRemotingTransportException(PSRemotingErrorId.IPCUnknownElementReceived, new object[1]
          {
            (object) xmlReader.LocalName
          });
      }
    }

    internal delegate void DataPacketReceived(byte[] rawData, string stream, Guid psGuid);

    internal delegate void DataAckPacketReceived(Guid psGuid);

    internal delegate void CommandCreationPacketReceived(Guid psGuid);

    internal delegate void CommandCreationAckReceived(Guid psGuid);

    internal delegate void ClosePacketReceived(Guid psGuid);

    internal delegate void CloseAckPacketReceived(Guid psGuid);

    internal delegate void SignalPacketReceived(Guid psGuid);

    internal delegate void SignalAckPacketReceived(Guid psGuid);

    internal struct DataProcessingDelegates
    {
      internal OutOfProcessUtils.DataPacketReceived DataPacketReceived;
      internal OutOfProcessUtils.DataAckPacketReceived DataAckPacketReceived;
      internal OutOfProcessUtils.CommandCreationPacketReceived CommandCreationPacketReceived;
      internal OutOfProcessUtils.CommandCreationAckReceived CommandCreationAckReceived;
      internal OutOfProcessUtils.SignalPacketReceived SignalPacketReceived;
      internal OutOfProcessUtils.SignalAckPacketReceived SignalAckPacketReceived;
      internal OutOfProcessUtils.ClosePacketReceived ClosePacketReceived;
      internal OutOfProcessUtils.CloseAckPacketReceived CloseAckPacketReceived;
    }
  }
}
