using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

// Talks to a Stockfish process over the UCI protocol. The best move is computed on a
// background thread and delivered back on the main thread via the onMove callback.
public partial class StockfishEngine : Node
{
    private Process? _process;
    private Action<string>? _pending;
    private volatile string? _result;
    private bool _ready;

    public bool IsReady => _ready;

    public override void _Ready()
    {
        string path = ResolveEnginePath();
        if (!System.IO.File.Exists(path))
        {
            GD.PushError($"[Stockfish] binary not found at {path}");
            return;
        }

        try
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };
            _process.Start();
            Send("uci");
            ReadUntil("uciok");
            Send("isready");
            ReadUntil("readyok");
            _ready = true;
            GD.Print("[Stockfish] ready");
        }
        catch (Exception e)
        {
            GD.PushError($"[Stockfish] failed to start: {e.Message}");
        }
    }

    private static string ResolveEnginePath()
    {
        // Dev / editor: the project's engine folder.
        string dev = ProjectSettings.GlobalizePath("res://engine/stockfish.exe");
        if (System.IO.File.Exists(dev)) return dev;

        // Exported build: stockfish.exe sits next to the game executable.
        string exeDir = System.IO.Path.GetDirectoryName(OS.GetExecutablePath()) ?? ".";
        return System.IO.Path.Combine(exeDir, "stockfish.exe");
    }

    public override void _Process(double delta)
    {
        if (_result == null) return;
        string move = _result;
        _result = null;
        Action<string>? cb = _pending;
        _pending = null;
        cb?.Invoke(move);
    }

    public void RequestBestMove(string fen, int skillLevel, int moveTimeMs, Action<string> onMove)
    {
        if (!_ready || _process == null) { onMove(""); return; }

        _pending = onMove;
        Task.Run(() =>
        {
            try
            {
                Send($"setoption name Skill Level value {skillLevel}");
                Send($"position fen {fen}");
                Send($"go movetime {moveTimeMs}");

                string best = "";
                string? line;
                while ((line = _process!.StandardOutput.ReadLine()) != null)
                {
                    if (line.StartsWith("bestmove"))
                    {
                        string[] parts = line.Split(' ');
                        if (parts.Length > 1) best = parts[1];
                        break;
                    }
                }
                _result = best;
            }
            catch (Exception e)
            {
                GD.PushError($"[Stockfish] error: {e.Message}");
                _result = "";
            }
        });
    }

    private void Send(string command)
    {
        _process!.StandardInput.WriteLine(command);
        _process.StandardInput.Flush();
    }

    private void ReadUntil(string token)
    {
        string? line;
        while ((line = _process!.StandardOutput.ReadLine()) != null)
            if (line.StartsWith(token)) return;
    }

    public override void _ExitTree()
    {
        try
        {
            if (_process is { HasExited: false })
            {
                Send("quit");
                _process.WaitForExit(500);
                if (!_process.HasExited) _process.Kill();
            }
        }
        catch { /* shutting down */ }
        _process?.Dispose();
    }
}
