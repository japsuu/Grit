
using System.Runtime.InteropServices;

using var game = new Grit.Grit();
AllocConsole();
game.Run();
FreeConsole();

[DllImport("kernel32.dll", SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool AllocConsole();

[DllImport("kernel32.dll", SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool FreeConsole();