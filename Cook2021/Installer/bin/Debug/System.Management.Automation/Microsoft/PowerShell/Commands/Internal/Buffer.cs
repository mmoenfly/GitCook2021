// Decompiled with JetBrains decompiler
// Type: Microsoft.PowerShell.Commands.Internal.Buffer
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands.Internal
{
  [ComVisible(true)]
  internal static class Buffer
  {
    internal static unsafe int IndexOfByte(byte* src, byte value, int index, int count)
    {
      byte* numPtr;
      for (numPtr = src + index; ((int) numPtr & 3) != 0; ++numPtr)
      {
        if (count == 0)
          return -1;
        if ((int) *numPtr == (int) value)
          return (int) (numPtr - src);
        --count;
      }
      uint num1 = ((uint) value << 8) + (uint) value;
      uint num2 = (num1 << 16) + num1;
      while (count > 3)
      {
        uint num3 = *(uint*) numPtr ^ num2;
        uint num4 = 2130640639U + num3;
        if (((num3 ^ uint.MaxValue ^ num4) & 2164326656U) != 0U)
        {
          int num5 = (int) (numPtr - src);
          if ((int) *numPtr == (int) value)
            return num5;
          if ((int) numPtr[1] == (int) value)
            return num5 + 1;
          if ((int) numPtr[2] == (int) value)
            return num5 + 2;
          if ((int) numPtr[3] == (int) value)
            return num5 + 3;
        }
        count -= 4;
        numPtr += 4;
      }
      while (count > 0)
      {
        if ((int) *numPtr == (int) value)
          return (int) (numPtr - src);
        --count;
        ++numPtr;
      }
      return -1;
    }

    internal static unsafe void ZeroMemory(byte* src, long len)
    {
      while (len-- > 0L)
        src[len] = (byte) 0;
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    internal static unsafe void memcpy(
      byte* src,
      int srcIndex,
      byte[] dest,
      int destIndex,
      int len)
    {
      if (len == 0)
        return;
      fixed (byte* numPtr = dest)
        Buffer.memcpyimpl(src + srcIndex, numPtr + destIndex, len);
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    internal static unsafe void memcpy(
      byte[] src,
      int srcIndex,
      byte* pDest,
      int destIndex,
      int len)
    {
      if (len == 0)
        return;
      fixed (byte* numPtr = src)
        Buffer.memcpyimpl(numPtr + srcIndex, pDest + destIndex, len);
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    internal static unsafe void memcpy(
      char* pSrc,
      int srcIndex,
      char* pDest,
      int destIndex,
      int len)
    {
      if (len == 0)
        return;
      Buffer.memcpyimpl((byte*) (pSrc + srcIndex), (byte*) (pDest + destIndex), len * 2);
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    internal static unsafe void memcpyimpl(byte* src, byte* dest, int len)
    {
      if (len >= 16)
      {
        do
        {
          *(int*) dest = *(int*) src;
          *(int*) (dest + 4) = *(int*) (src + 4);
          *(int*) (dest + 8) = *(int*) (src + 8);
          *(int*) (dest + 12) = *(int*) (src + 12);
          dest += 16;
          src += 16;
        }
        while ((len -= 16) >= 16);
      }
      if (len <= 0)
        return;
      if ((len & 8) != 0)
      {
        *(int*) dest = *(int*) src;
        *(int*) (dest + 4) = *(int*) (src + 4);
        dest += 8;
        src += 8;
      }
      if ((len & 4) != 0)
      {
        *(int*) dest = *(int*) src;
        dest += 4;
        src += 4;
      }
      if ((len & 2) != 0)
      {
        *(short*) dest = *(short*) src;
        dest += 2;
        src += 2;
      }
      if ((len & 1) == 0)
        return;
      *dest++ = *src++;
    }
  }
}
