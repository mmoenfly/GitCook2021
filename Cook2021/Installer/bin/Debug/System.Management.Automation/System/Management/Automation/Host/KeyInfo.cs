// Decompiled with JetBrains decompiler
// Type: System.Management.Automation.Host.KeyInfo
// Assembly: System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: A866DF73-FE6E-416D-AD3E-EC8D3078BB68
// Assembly location: C:\windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35\System.Management.Automation.dll

using System.Globalization;

namespace System.Management.Automation.Host
{
  public struct KeyInfo
  {
    private int virtualKeyCode;
    private char character;
    private ControlKeyStates controlKeyState;
    private bool keyDown;

    public int VirtualKeyCode
    {
      get => this.virtualKeyCode;
      set => this.virtualKeyCode = value;
    }

    public char Character
    {
      get => this.character;
      set => this.character = value;
    }

    public ControlKeyStates ControlKeyState
    {
      get => this.controlKeyState;
      set => this.controlKeyState = value;
    }

    public bool KeyDown
    {
      get => this.keyDown;
      set => this.keyDown = value;
    }

    public KeyInfo(int virtualKeyCode, char ch, ControlKeyStates controlKeyState, bool keyDown)
    {
      this.virtualKeyCode = virtualKeyCode;
      this.character = ch;
      this.controlKeyState = controlKeyState;
      this.keyDown = keyDown;
    }

    public override string ToString() => string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0},{1},{2},{3}", (object) this.VirtualKeyCode, (object) this.Character, (object) this.ControlKeyState, (object) this.KeyDown);

    public override bool Equals(object obj)
    {
      bool flag = false;
      if (obj is KeyInfo keyInfo)
        flag = this == keyInfo;
      return flag;
    }

    public override int GetHashCode() => ((this.KeyDown ? 268435456U : 0U) | (uint) this.ControlKeyState << 16 | (uint) this.VirtualKeyCode).GetHashCode();

    public static bool operator ==(KeyInfo first, KeyInfo second) => (int) first.Character == (int) second.Character && first.ControlKeyState == second.ControlKeyState && first.KeyDown == second.KeyDown && first.VirtualKeyCode == second.VirtualKeyCode;

    public static bool operator !=(KeyInfo first, KeyInfo second) => !(first == second);
  }
}
