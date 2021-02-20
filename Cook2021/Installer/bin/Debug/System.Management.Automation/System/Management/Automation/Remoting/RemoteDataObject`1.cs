// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.RemoteDataObject`1
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;
using System.IO;

namespace System.Management.Automation.Remoting
{
  internal class RemoteDataObject<T>
  {
    private const int destinationOffset = 0;
    private const int dataTypeOffset = 4;
    private const int rsPoolIdOffset = 8;
    private const int psIdOffset = 24;
    private const int headerLength = 40;
    private const int SessionMask = 65536;
    private const int RunspacePoolMask = 135168;
    private const int PowerShellMask = 266240;
    private RemotingDestination destination;
    private RemotingDataType dataType;
    private Guid runspacePoolId;
    private Guid powerShellId;
    private T data;

    protected RemoteDataObject(
      RemotingDestination destination,
      RemotingDataType dataType,
      Guid runspacePoolId,
      Guid powerShellId,
      T data)
    {
      this.destination = destination;
      this.dataType = dataType;
      this.runspacePoolId = runspacePoolId;
      this.powerShellId = powerShellId;
      this.data = data;
    }

    internal RemotingDestination Destination => this.destination;

    internal RemotingTargetInterface TargetInterface
    {
      get
      {
        int dataType = (int) this.dataType;
        if ((dataType & 266240) == 266240)
          return RemotingTargetInterface.PowerShell;
        if ((dataType & 135168) == 135168)
          return RemotingTargetInterface.RunspacePool;
        return (dataType & 65536) == 65536 ? RemotingTargetInterface.Session : RemotingTargetInterface.InvalidTargetInterface;
      }
    }

    internal RemotingDataType DataType => this.dataType;

    internal Guid RunspacePoolId => this.runspacePoolId;

    internal Guid PowerShellId => this.powerShellId;

    internal T Data => this.data;

    internal static RemoteDataObject<T> CreateFrom(
      RemotingDestination destination,
      RemotingDataType dataType,
      Guid runspacePoolId,
      Guid powerShellId,
      T data)
    {
      return new RemoteDataObject<T>(destination, dataType, runspacePoolId, powerShellId, data);
    }

    internal static RemoteDataObject<T> CreateFrom(
      Stream serializedDataStream,
      Fragmentor defragmentor)
    {
      if (serializedDataStream.Length - serializedDataStream.Position < 40L)
        throw new PSRemotingTransportException(PSRemotingErrorId.NotEnoughHeaderForRemoteDataObject, new object[1]
        {
          (object) 61
        });
      RemotingDestination destination = (RemotingDestination) RemoteDataObject<T>.DeserializeUInt(serializedDataStream);
      RemotingDataType dataType = (RemotingDataType) RemoteDataObject<T>.DeserializeUInt(serializedDataStream);
      Guid runspacePoolId = RemoteDataObject<T>.DeserializeGuid(serializedDataStream);
      Guid powerShellId = RemoteDataObject<T>.DeserializeGuid(serializedDataStream);
      object valueToConvert = (object) null;
      if (serializedDataStream.Length - 40L > 0L)
        valueToConvert = (object) defragmentor.DeserializeToPSObject(serializedDataStream);
      T data = (T) LanguagePrimitives.ConvertTo(valueToConvert, typeof (T), (IFormatProvider) CultureInfo.CurrentCulture);
      return new RemoteDataObject<T>(destination, dataType, runspacePoolId, powerShellId, data);
    }

    internal virtual void Serialize(Stream streamToWriteTo, Fragmentor fragmentor)
    {
      this.SerializeHeader(streamToWriteTo);
      if ((object) this.data == null)
        return;
      fragmentor.SerializeToBytes((object) this.data, streamToWriteTo);
    }

    private void SerializeHeader(Stream streamToWriteTo)
    {
      this.SerializeUInt((uint) this.Destination, streamToWriteTo);
      this.SerializeUInt((uint) this.DataType, streamToWriteTo);
      this.SerializeGuid(this.runspacePoolId, streamToWriteTo);
      this.SerializeGuid(this.powerShellId, streamToWriteTo);
    }

    private void SerializeUInt(uint data, Stream streamToWriteTo)
    {
      byte[] buffer = new byte[4];
      int num1 = 0;
      byte[] numArray1 = buffer;
      int index1 = num1;
      int num2 = index1 + 1;
      int num3 = (int) (byte) (data & (uint) byte.MaxValue);
      numArray1[index1] = (byte) num3;
      byte[] numArray2 = buffer;
      int index2 = num2;
      int num4 = index2 + 1;
      int num5 = (int) (byte) (data >> 8 & (uint) byte.MaxValue);
      numArray2[index2] = (byte) num5;
      byte[] numArray3 = buffer;
      int index3 = num4;
      int num6 = index3 + 1;
      int num7 = (int) (byte) (data >> 16 & (uint) byte.MaxValue);
      numArray3[index3] = (byte) num7;
      byte[] numArray4 = buffer;
      int index4 = num6;
      int num8 = index4 + 1;
      int num9 = (int) (byte) (data >> 24 & (uint) byte.MaxValue);
      numArray4[index4] = (byte) num9;
      streamToWriteTo.Write(buffer, 0, 4);
    }

    private static uint DeserializeUInt(Stream serializedDataStream) => (uint) (0 | serializedDataStream.ReadByte() & (int) byte.MaxValue) | (uint) (serializedDataStream.ReadByte() << 8 & 65280) | (uint) (serializedDataStream.ReadByte() << 16 & 16711680) | (uint) (serializedDataStream.ReadByte() << 24 & -16777216);

    private void SerializeGuid(Guid guid, Stream streamToWriteTo)
    {
      byte[] byteArray = guid.ToByteArray();
      streamToWriteTo.Write(byteArray, 0, byteArray.Length);
    }

    private static Guid DeserializeGuid(Stream serializedDataStream)
    {
      byte[] b = new byte[16];
      for (int index = 0; index < 16; ++index)
        b[index] = (byte) serializedDataStream.ReadByte();
      return new Guid(b);
    }
  }
}
