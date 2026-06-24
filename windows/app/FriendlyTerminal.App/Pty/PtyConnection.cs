using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace FriendlyTerminal.App.Pty;

internal sealed class PtyConnection : IDisposable
{
    private readonly string _command;
    private readonly int _width;
    private readonly int _height;

    private PseudoConsole? _console;
    private SafeFileHandle? _inputWrite;
    private SafeFileHandle? _outputRead;
    private FileStream? _writer;
    private Thread? _reader;
    private volatile bool _running;

    public event Action<byte[]>? OutputReceived;

    public PtyConnection(string command, int width, int height)
    {
        _command = command;
        _width = width;
        _height = height;
    }

    public void Start()
    {
        CreatePipe(out var inputRead, out var inputWrite);
        CreatePipe(out var outputRead, out var outputWrite);

        _console = PseudoConsole.Create(inputRead, outputWrite, _width, _height);
        _inputWrite = inputWrite;
        _outputRead = outputRead;

        inputRead.Dispose();
        outputWrite.Dispose();

        StartProcess(_console.Handle, _command);

        _writer = new FileStream(_inputWrite, FileAccess.Write);
        _running = true;
        _reader = new Thread(ReadLoop) { IsBackground = true };
        _reader.Start();
    }

    public void WriteInput(string text)
    {
        if (_writer is null) return;
        var bytes = Encoding.UTF8.GetBytes(text);
        _writer.Write(bytes, 0, bytes.Length);
        _writer.Flush();
    }

    public void Resize(int width, int height) => _console?.Resize(width, height);

    private void ReadLoop()
    {
        using var stream = new FileStream(_outputRead!, FileAccess.Read);
        var buffer = new byte[4096];
        while (_running)
        {
            int read;
            try { read = stream.Read(buffer, 0, buffer.Length); }
            catch { break; }
            if (read <= 0) break;
            var chunk = new byte[read];
            Array.Copy(buffer, chunk, read);
            OutputReceived?.Invoke(chunk);
        }
    }

    private static void CreatePipe(out SafeFileHandle read, out SafeFileHandle write)
    {
        if (!NativeMethods.CreatePipe(out read, out write, IntPtr.Zero, 0))
            throw new InvalidOperationException("CreatePipe failed");
    }

    private static void StartProcess(IntPtr console, string command)
    {
        var attrSize = IntPtr.Zero;
        NativeMethods.InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref attrSize);
        var attrList = Marshal.AllocHGlobal(attrSize);

        if (!NativeMethods.InitializeProcThreadAttributeList(attrList, 1, 0, ref attrSize))
            throw new InvalidOperationException("InitializeProcThreadAttributeList failed");

        if (!NativeMethods.UpdateProcThreadAttribute(attrList, 0,
                (IntPtr)NativeMethods.PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE,
                console, (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero))
            throw new InvalidOperationException("UpdateProcThreadAttribute failed");

        var startup = new NativeMethods.STARTUPINFOEX();
        startup.StartupInfo.cb = Marshal.SizeOf<NativeMethods.STARTUPINFOEX>();
        startup.lpAttributeList = attrList;

        if (!NativeMethods.CreateProcess(null, command, IntPtr.Zero, IntPtr.Zero, false,
                NativeMethods.EXTENDED_STARTUPINFO_PRESENT, IntPtr.Zero, null,
                ref startup, out var proc))
            throw new InvalidOperationException("CreateProcess failed");

        NativeMethods.CloseHandle(proc.hThread);
        NativeMethods.CloseHandle(proc.hProcess);
        NativeMethods.DeleteProcThreadAttributeList(attrList);
        Marshal.FreeHGlobal(attrList);
    }

    public void Dispose()
    {
        _running = false;
        _writer?.Dispose();
        _outputRead?.Dispose();
        _inputWrite?.Dispose();
        _console?.Dispose();
    }
}
