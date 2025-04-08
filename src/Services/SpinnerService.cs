// Mugs/Services/SpinnerService.cs

namespace Mugs.Services
{
    public static class SpinnerService
    {
        private static readonly char[] _spinnerSequence = new[] { '|', '/', '-', '\\' };
        private static CancellationTokenSource _cts;
        private static Task _spinnerTask;
        private static int _spinnerPosition;
        private const int SpinnerDelayMs = 300;
        private static bool _isWaitingForInput = false;

        public static void Start()
        {
            if (_isWaitingForInput) return;

            Stop();

            Console.CursorVisible = false;
            _cts = new CancellationTokenSource();
            _spinnerPosition = Console.CursorLeft;

            _spinnerTask = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(SpinnerDelayMs, _cts.Token);

                    if (_cts.Token.IsCancellationRequested || _isWaitingForInput)
                        return;

                    var counter = 0;
                    while (!_cts.Token.IsCancellationRequested && !_isWaitingForInput)
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

        public static void Stop()
        {
            _cts?.Cancel();
            _spinnerTask?.Wait();
            _spinnerTask?.Dispose();
            _spinnerTask = null;
            _cts?.Dispose();
            _cts = null;

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

        public static IDisposable PauseForInput()
        {
            _isWaitingForInput = true;
            Stop();
            return new InputActivity();
        }

        private class DisposableActivity : IDisposable
        {
            public void Dispose()
            {
                Stop();
            }
        }

        private class InputActivity : IDisposable
        {
            public void Dispose()
            {
                _isWaitingForInput = false;
            }
        }
    }
}