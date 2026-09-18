using Renci.SshNet;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WRT
{
    public static class ShellStreamExtensions
    {
        public static Task<string> ExpectAsync(this ShellStream stream, Regex pattern, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<string>();
            var expectAction = new ExpectAction(pattern, text => tcs.TrySetResult(text));

            try
            {
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
