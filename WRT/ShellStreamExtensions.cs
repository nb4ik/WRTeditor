using Renci.SshNet;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WRT
{
    // 1. Класс ОБЯЗАТЕЛЬНО должен быть public static
    public static class ShellStreamExtensions
    {
        // 2. Метод ОБЯЗАТЕЛЬНО должен быть public static Task<string>
        // 3. Перед ShellStream ОБЯЗАТЕЛЬНО должно стоять слово "this"
        public static Task<string> ExpectAsync(this ShellStream stream, Regex pattern, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<string>();
            var expectAction = new ExpectAction(pattern, text => tcs.TrySetResult(text));

            try
            {
                // ИСПРАВЛЕНО: Вызываем перегрузку BeginExpect, которая принимает ТОЛЬКО массив ExpectAction.
                // Это убирает ошибку CS1502 / CS1503 и корректно запускает асинхронный сканер.
                stream.BeginExpect(new[] { expectAction });
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            var cancellationTokenSource = new CancellationTokenSource(timeout);
            cancellationTokenSource.Token.Register(() => tcs.TrySetCanceled());

            return tcs.Task;
        }
    }
}
