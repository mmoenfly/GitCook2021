// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Remoting.FragmentedRemoteObject
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

namespace System.Management.Automation.Remoting
{
  internal class FragmentedRemoteObject
  {
    internal const byte SFlag = 1;
    internal const byte EFlag = 2;
    internal const int HeaderLength = 21;
    private const int _objectIdOffset = 0;
    private const int _fragmentIdOffset = 8;
    private const int _flagsOffset = 16;
    private const int _blobLengthOffset = 17;
    private const int _blobOffset = 21;
    [TraceSource("FragObj", "FragmentedRemoteObject")]
    private static PSTraceSource _trace = PSTraceSource.GetTracer("FragObj", nameof (FragmentedRemoteObject));
    private long _objectId;
    private long _fragmentId;
    private bool _isStartFragment;
    private bool _isEndFragment;
    private byte[] _blob;
    private int _blobLength;

    internal FragmentedRemoteObject()
    {
    }

    internal FragmentedRemoteObject(
      byte[] blob,
      long objectId,
      long fragmentId,
      bool isEndFragment)
    {
      using (FragmentedRemoteObject._trace.TraceConstructor((object) this))
      {
        this._objectId = objectId;
        this._fragmentId = fragmentId;
        this._isStartFragment = fragmentId == 0L;
        this._isEndFragment = isEndFragment;
        this._blob = blob;
        this._blobLength = this._blob.Length;
      }
    }

    internal long ObjectId
    {
      get => this._objectId;
      set => this._objectId = value;
    }

    internal long FragmentId
    {
      get => this._fragmentId;
      set => this._fragmentId = value;
    }

    internal bool IsStartFragment
    {
      get => this._isStartFragment;
      set => this._isStartFragment = value;
    }

    internal bool IsEndFragment
    {
      get => this._isEndFragment;
      set => this._isEndFragment = value;
    }

    internal int BlobLength
    {
      get => this._blobLength;
      set => this._blobLength = value;
    }

    internal byte[] Blob
    {
      get => this._blob;
      set => this._blob = value;
    }

    internal byte[] GetBytes()
    {
      byte[] numArray1 = new byte[8 + 8 + 1 + 4 + this.BlobLength];
      int num1 = 0;
      int num2 = 0;
      byte[] numArray2 = numArray1;
      int index1 = num2;
      int num3 = index1 + 1;
      int num4 = (int) (byte) ((ulong) (this.ObjectId >> 56) & (ulong) sbyte.MaxValue);
      numArray2[index1] = (byte) num4;
      byte[] numArray3 = numArray1;
      int index2 = num3;
      int num5 = index2 + 1;
      int num6 = (int) (byte) ((ulong) (this.ObjectId >> 48) & (ulong) byte.MaxValue);
      numArray3[index2] = (byte) num6;
      byte[] numArray4 = numArray1;
      int index3 = num5;
      int num7 = index3 + 1;
      int num8 = (int) (byte) ((ulong) (this.ObjectId >> 40) & (ulong) byte.MaxValue);
      numArray4[index3] = (byte) num8;
      byte[] numArray5 = numArray1;
      int index4 = num7;
      int num9 = index4 + 1;
      int num10 = (int) (byte) ((ulong) (this.ObjectId >> 32) & (ulong) byte.MaxValue);
      numArray5[index4] = (byte) num10;
      byte[] numArray6 = numArray1;
      int index5 = num9;
      int num11 = index5 + 1;
      int num12 = (int) (byte) ((ulong) (this.ObjectId >> 24) & (ulong) byte.MaxValue);
      numArray6[index5] = (byte) num12;
      byte[] numArray7 = numArray1;
      int index6 = num11;
      int num13 = index6 + 1;
      int num14 = (int) (byte) ((ulong) (this.ObjectId >> 16) & (ulong) byte.MaxValue);
      numArray7[index6] = (byte) num14;
      byte[] numArray8 = numArray1;
      int index7 = num13;
      int num15 = index7 + 1;
      int num16 = (int) (byte) ((ulong) (this.ObjectId >> 8) & (ulong) byte.MaxValue);
      numArray8[index7] = (byte) num16;
      byte[] numArray9 = numArray1;
      int index8 = num15;
      num1 = index8 + 1;
      int num17 = (int) (byte) ((ulong) this.ObjectId & (ulong) byte.MaxValue);
      numArray9[index8] = (byte) num17;
      int num18 = 8;
      byte[] numArray10 = numArray1;
      int index9 = num18;
      int num19 = index9 + 1;
      int num20 = (int) (byte) ((ulong) (this.FragmentId >> 56) & (ulong) sbyte.MaxValue);
      numArray10[index9] = (byte) num20;
      byte[] numArray11 = numArray1;
      int index10 = num19;
      int num21 = index10 + 1;
      int num22 = (int) (byte) ((ulong) (this.FragmentId >> 48) & (ulong) byte.MaxValue);
      numArray11[index10] = (byte) num22;
      byte[] numArray12 = numArray1;
      int index11 = num21;
      int num23 = index11 + 1;
      int num24 = (int) (byte) ((ulong) (this.FragmentId >> 40) & (ulong) byte.MaxValue);
      numArray12[index11] = (byte) num24;
      byte[] numArray13 = numArray1;
      int index12 = num23;
      int num25 = index12 + 1;
      int num26 = (int) (byte) ((ulong) (this.FragmentId >> 32) & (ulong) byte.MaxValue);
      numArray13[index12] = (byte) num26;
      byte[] numArray14 = numArray1;
      int index13 = num25;
      int num27 = index13 + 1;
      int num28 = (int) (byte) ((ulong) (this.FragmentId >> 24) & (ulong) byte.MaxValue);
      numArray14[index13] = (byte) num28;
      byte[] numArray15 = numArray1;
      int index14 = num27;
      int num29 = index14 + 1;
      int num30 = (int) (byte) ((ulong) (this.FragmentId >> 16) & (ulong) byte.MaxValue);
      numArray15[index14] = (byte) num30;
      byte[] numArray16 = numArray1;
      int index15 = num29;
      int num31 = index15 + 1;
      int num32 = (int) (byte) ((ulong) (this.FragmentId >> 8) & (ulong) byte.MaxValue);
      numArray16[index15] = (byte) num32;
      byte[] numArray17 = numArray1;
      int index16 = num31;
      num1 = index16 + 1;
      int num33 = (int) (byte) ((ulong) this.FragmentId & (ulong) byte.MaxValue);
      numArray17[index16] = (byte) num33;
      int num34 = 16;
      byte num35 = this.IsStartFragment ? (byte) 1 : (byte) 0;
      byte num36 = this.IsEndFragment ? (byte) 2 : (byte) 0;
      byte[] numArray18 = numArray1;
      int index17 = num34;
      num1 = index17 + 1;
      int num37 = (int) (byte) ((uint) num35 | (uint) num36);
      numArray18[index17] = (byte) num37;
      int num38 = 17;
      byte[] numArray19 = numArray1;
      int index18 = num38;
      int num39 = index18 + 1;
      int num40 = (int) (byte) (this.BlobLength >> 24 & (int) byte.MaxValue);
      numArray19[index18] = (byte) num40;
      byte[] numArray20 = numArray1;
      int index19 = num39;
      int num41 = index19 + 1;
      int num42 = (int) (byte) (this.BlobLength >> 16 & (int) byte.MaxValue);
      numArray20[index19] = (byte) num42;
      byte[] numArray21 = numArray1;
      int index20 = num41;
      int num43 = index20 + 1;
      int num44 = (int) (byte) (this.BlobLength >> 8 & (int) byte.MaxValue);
      numArray21[index20] = (byte) num44;
      byte[] numArray22 = numArray1;
      int index21 = num43;
      num1 = index21 + 1;
      int num45 = (int) (byte) (this.BlobLength & (int) byte.MaxValue);
      numArray22[index21] = (byte) num45;
      Array.Copy((Array) this._blob, 0, (Array) numArray1, 21, this.BlobLength);
      return numArray1;
    }

    internal static long GetObjectId(byte[] fragmentBytes, int startIndex)
    {
      int num1 = startIndex;
      byte[] numArray1 = fragmentBytes;
      int index1 = num1;
      int num2 = index1 + 1;
      long num3 = (long) numArray1[index1] << 56 & 9151314442816847872L;
      byte[] numArray2 = fragmentBytes;
      int index2 = num2;
      int num4 = index2 + 1;
      long num5 = (long) numArray2[index2] << 48 & 71776119061217280L;
      long num6 = num3 + num5;
      byte[] numArray3 = fragmentBytes;
      int index3 = num4;
      int num7 = index3 + 1;
      long num8 = (long) numArray3[index3] << 40 & 280375465082880L;
      long num9 = num6 + num8;
      byte[] numArray4 = fragmentBytes;
      int index4 = num7;
      int num10 = index4 + 1;
      long num11 = (long) numArray4[index4] << 32 & 1095216660480L;
      long num12 = num9 + num11;
      byte[] numArray5 = fragmentBytes;
      int index5 = num10;
      int num13 = index5 + 1;
      long num14 = (long) numArray5[index5] << 24 & 4278190080L;
      long num15 = num12 + num14;
      byte[] numArray6 = fragmentBytes;
      int index6 = num13;
      int num16 = index6 + 1;
      long num17 = (long) numArray6[index6] << 16 & 16711680L;
      long num18 = num15 + num17;
      byte[] numArray7 = fragmentBytes;
      int index7 = num16;
      int num19 = index7 + 1;
      long num20 = (long) numArray7[index7] << 8 & 65280L;
      long num21 = num18 + num20;
      byte[] numArray8 = fragmentBytes;
      int index8 = num19;
      int num22 = index8 + 1;
      long num23 = (long) numArray8[index8] & (long) byte.MaxValue;
      return num21 + num23;
    }

    internal static long GetFragmentId(byte[] fragmentBytes, int startIndex)
    {
      int num1 = startIndex + 8;
      byte[] numArray1 = fragmentBytes;
      int index1 = num1;
      int num2 = index1 + 1;
      long num3 = (long) numArray1[index1] << 56 & 9151314442816847872L;
      byte[] numArray2 = fragmentBytes;
      int index2 = num2;
      int num4 = index2 + 1;
      long num5 = (long) numArray2[index2] << 48 & 71776119061217280L;
      long num6 = num3 + num5;
      byte[] numArray3 = fragmentBytes;
      int index3 = num4;
      int num7 = index3 + 1;
      long num8 = (long) numArray3[index3] << 40 & 280375465082880L;
      long num9 = num6 + num8;
      byte[] numArray4 = fragmentBytes;
      int index4 = num7;
      int num10 = index4 + 1;
      long num11 = (long) numArray4[index4] << 32 & 1095216660480L;
      long num12 = num9 + num11;
      byte[] numArray5 = fragmentBytes;
      int index5 = num10;
      int num13 = index5 + 1;
      long num14 = (long) numArray5[index5] << 24 & 4278190080L;
      long num15 = num12 + num14;
      byte[] numArray6 = fragmentBytes;
      int index6 = num13;
      int num16 = index6 + 1;
      long num17 = (long) numArray6[index6] << 16 & 16711680L;
      long num18 = num15 + num17;
      byte[] numArray7 = fragmentBytes;
      int index7 = num16;
      int num19 = index7 + 1;
      long num20 = (long) numArray7[index7] << 8 & 65280L;
      long num21 = num18 + num20;
      byte[] numArray8 = fragmentBytes;
      int index8 = num19;
      int num22 = index8 + 1;
      long num23 = (long) numArray8[index8] & (long) byte.MaxValue;
      return num21 + num23;
    }

    internal static bool GetIsStartFragment(byte[] fragmentBytes, int startIndex) => ((int) fragmentBytes[startIndex + 16] & 1) != 0;

    internal static bool GetIsEndFragment(byte[] fragmentBytes, int startIndex) => ((int) fragmentBytes[startIndex + 16] & 2) != 0;

    internal static int GetBlobLength(byte[] fragmentBytes, int startIndex)
    {
      int num1 = 0;
      int num2 = startIndex + 17;
      int num3 = num1;
      byte[] numArray1 = fragmentBytes;
      int index1 = num2;
      int num4 = index1 + 1;
      int num5 = (int) numArray1[index1] << 24 & 2130706432;
      int num6 = num3 + num5;
      byte[] numArray2 = fragmentBytes;
      int index2 = num4;
      int num7 = index2 + 1;
      int num8 = (int) numArray2[index2] << 16 & 16711680;
      int num9 = num6 + num8;
      byte[] numArray3 = fragmentBytes;
      int index3 = num7;
      int num10 = index3 + 1;
      int num11 = (int) numArray3[index3] << 8 & 65280;
      int num12 = num9 + num11;
      byte[] numArray4 = fragmentBytes;
      int index4 = num10;
      int num13 = index4 + 1;
      int num14 = (int) numArray4[index4] & (int) byte.MaxValue;
      return num12 + num14;
    }
  }
}
