// Mugs/Services/SpinnerService.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Mugs.Models;

namespace Mugs.Services
{
    public static class SpinnerService
    {
        private static readonly char[] _spinnerSequence = { '|', '/', '-', '\\' };
        private static volatile CancellationTokenSource _cts;
        private static Task _spinnerTask;
        private static int _spinnerPosition;
        private const int SpinnerDelayMs = 300;
        private static TextReader _originalInput;
        private static readonly object _lock = new object();

        public static void Start()
        {
            if (!AppSettings.EnableSpinnerAnimation) return;

            lock (_lock)
            {
                Stop();

                Console.CursorVisible = false;
                _cts = new CancellationTokenSource();
                _spinnerPosition = Console.CursorLeft;

                _originalInput = Console.In;
                Console.SetIn(new InputInterceptor(_originalInput));

                var token = _cts.Token;
                _spinnerTask = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(SpinnerDelayMs, token);
                        if (token.IsCancellationRequested) return;

                        var counter = 0;
                        while (!token.IsCancellationRequested)
                        {
                            var spinChar = _spinnerSequence[counter++ % _spinnerSequence.Length];
                            UpdateSpinner(spinChar);
                            await Task.Delay(100, token);
                        }
                    }
                    catch (OperationCanceledException) { }
                }, token);
            }
        }

        public static void Stop()
        {
            lock (_lock)
            {
                try
                {
                    if (_cts != null)
                    {
                        _cts.Cancel();
                        _cts.Dispose();
                        _cts = null;
                    }

                    _spinnerTask?.Wait(50);
                    _spinnerTask?.Dispose();
                    _spinnerTask = null;

                    if (_originalInput != null)
                    {
                        Console.SetIn(_originalInput);
                        _originalInput = null;
                    }

                    Console.CursorVisible = true;
                }
                catch { }
            }
        }

        private static void UpdateSpinner(char spinChar)
        {
            lock (_lock)
            {
                try
                {
                    var (left, top) = (Console.CursorLeft, Console.CursorTop);
                    Console.SetCursorPosition(_spinnerPosition, top);
                    Console.Write(spinChar);
                    Console.SetCursorPosition(left, top);
                }
                catch { }
            }
        }

        public static IDisposable StartActivity()
        {
            Start();
            return new DisposableActivity();
        }

        private class DisposableActivity : IDisposable
        {
            public void Dispose() => Stop();
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
                SpinnerService.Stop();
                return _originalReader.Read();
            }

            public override string ReadLine()
            {
                SpinnerService.Stop();
                return _originalReader.ReadLine();
            }
        }
    }
}