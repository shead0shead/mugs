// Mugs/Services/SpinnerService.cs

using Mugs.Models;

using System.Text;

namespace Mugs.Services
{
    public static class SpinnerService
    {
        private static readonly char[] _spinnerSequence = new[] { '|', '/', '-', '\\' };
        private static CancellationTokenSource _cts;
        private static Task _spinnerTask;
        private static int _spinnerPosition;
        private const int SpinnerDelayMs = 300;
        private static TextReader _originalInput;
        private static TextWriter _originalOutput;

        public static void Start()
        {
            if (AppSettings.EnableSpinnerAnimation)
            {
                Stop();

                Console.CursorVisible = false;
                _cts = new CancellationTokenSource();
                _spinnerPosition = Console.CursorLeft;

                _originalInput = Console.In;
                _originalOutput = Console.Out;

                Console.SetIn(new InputInterceptor(Console.In));
                Console.SetOut(new OutputInterceptor(Console.Out));

                _spinnerTask = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(SpinnerDelayMs, _cts.Token);

                        if (_cts.Token.IsCancellationRequested)
                            return;

                        var counter = 0;
                        while (!_cts.Token.IsCancellationRequested)
                        {
                            var spinChar = _spinnerSequence[counter++ % _spinnerSequence.Length];
                            UpdateSpinner(spinChar);
                            await Task.Delay(100, _cts.Token);
                        }
                    }
                    catch (TaskCanceledException)
                    {

                    }
                }, _cts.Token);
            }
        }

        public static void Stop()
        {
            _cts?.Cancel();
            _spinnerTask?.Wait();
            _spinnerTask?.Dispose();
            _spinnerTask = null;
            _cts?.Dispose();
            _cts = null;

            if (_originalInput != null)
            {
                Console.SetIn(_originalInput);
                _originalInput = null;
            }

            if (_originalOutput != null)
            {
                Console.SetOut(_originalOutput);
                _originalOutput = null;
            }

            Console.CursorVisible = true;
        }

        private static void UpdateSpinner(char spinChar)
        {
            try
            {
                var currentLeft = Console.CursorLeft;
                var currentTop = Console.CursorTop;

                Console.SetCursorPosition(_spinnerPosition, Console.CursorTop);
                Console.Write(spinChar);
                Console.SetCursorPosition(currentLeft, currentTop);
            }
            catch
            {
                
            }
        }

        public static IDisposable StartActivity()
        {
            Start();
            return new DisposableActivity();
        }

        private class DisposableActivity : IDisposable
        {
            public void Dispose()
            {
                Stop();
            }
        }

        private class InputInterceptor : TextReader
        {
            private readonly TextReader _originalReader;

            public InputInterceptor(TextReader originalReader)
            {
                _originalReader = originalReader;
            }

            public override int Read()
            {
                Stop();
                return _originalReader.Read();
            }

            public override string ReadLine()
            {
                Stop();
                return _originalReader.ReadLine();
            }
        }

        private class OutputInterceptor : TextWriter
        {
            private readonly TextWriter _originalWriter;

            public OutputInterceptor(TextWriter originalWriter)
            {
                _originalWriter = originalWriter;
            }

            public override Encoding Encoding => _originalWriter.Encoding;

            public override void Write(char value)
            {
                _originalWriter.Write(value);
            }
        }
    }
}