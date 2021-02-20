// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.RemoteSessionCapability
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;

namespace System.Management.Automation.Remoting
{
  internal class RemoteSessionCapability
  {
    private Version _psversion;
    private Version _serversion;
    private Version _protocolVersion;
    private RemotingDestination _remotingDestination;
    private TimeZone _timeZone;
    private static byte[] _timeZoneInByteFormat;

    internal Version ProtocolVersion
    {
      get => this._protocolVersion;
      set => this._protocolVersion = value;
    }

    internal Version PSVersion => this._psversion;

    internal Version SerializationVersion => this._serversion;

    internal RemotingDestination RemotingDestination => this._remotingDestination;

    internal RemoteSessionCapability(RemotingDestination remotingDestination)
    {
      this._protocolVersion = RemotingConstants.ProtocolVersion;
      this._psversion = PSVersionInfo.PSVersion;
      this._serversion = PSVersionInfo.SerializationVersion;
      this._remotingDestination = remotingDestination;
    }

    internal RemoteSessionCapability(
      RemotingDestination remotingDestination,
      Version protocolVersion,
      Version psVersion,
      Version serVersion)
    {
      this._protocolVersion = protocolVersion;
      this._psversion = psVersion;
      this._serversion = serVersion;
      this._remotingDestination = remotingDestination;
    }

    internal static RemoteSessionCapability CreateClientCapability() => new RemoteSessionCapability(RemotingDestination.Server);

    internal static RemoteSessionCapability CreateServerCapability() => new RemoteSessionCapability(RemotingDestination.Client);

    internal static byte[] GetCurrentTimeZoneInByteFormat()
    {
      if (RemoteSessionCapability._timeZoneInByteFormat == null)
      {
        Exception exception = (Exception) null;
        try
        {
          using (MemoryStream memoryStream = new MemoryStream())
          {
            new BinaryFormatter().Serialize((Stream) memoryStream, (object) TimeZone.CurrentTimeZone);
            memoryStream.Seek(0L, SeekOrigin.Begin);
            byte[] buffer = new byte[memoryStream.Length];
            memoryStream.Read(buffer, 0, (int) memoryStream.Length);
            RemoteSessionCapability._timeZoneInByteFormat = buffer;
          }
        }
        catch (ArgumentNullException ex)
        {
          exception = (Exception) ex;
        }
        catch (SerializationException ex)
        {
          exception = (Exception) ex;
        }
        catch (SecurityException ex)
        {
          exception = (Exception) ex;
        }
        if (exception != null)
          RemoteSessionCapability._timeZoneInByteFormat = new byte[0];
      }
      return RemoteSessionCapability._timeZoneInByteFormat;
    }

    internal static TimeZone ConvertFromByteToTimeZone(byte[] data)
    {
      TimeZone result = (TimeZone) null;
      if (data == null)
        return result;
      try
      {
        LanguagePrimitives.TryConvertTo<TimeZone>(new BinaryFormatter().Deserialize((Stream) new MemoryStream(data)), out result);
        return result;
      }
      catch (ArgumentNullException ex)
      {
      }
      catch (SerializationException ex)
      {
      }
      catch (SecurityException ex)
      {
      }
      return result;
    }

    internal TimeZone TimeZone
    {
      get => this._timeZone;
      set => this._timeZone = value;
    }
  }
}
