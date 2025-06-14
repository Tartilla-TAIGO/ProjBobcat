using System.Net.Sockets;

namespace TAIGO.ECore.MLibs.Handler;

public class RetryHandler : DelegatingHandler
{
    readonly int _maxRetries = 5;

    public RetryHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
    }

    public RetryHandler(HttpMessageHandler innerHandler, int maxRetries) : base(innerHandler)
    {
        _maxRetries = maxRetries;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = null;

        for (var i = 0; i < _maxRetries; i++)
        {
            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (Exception e)
            {
                if (IsNetworkError(e))
                    continue;

                throw;
            }

            return response;
        }

        return response!;
    }

    static bool IsNetworkError(Exception ex)
    {
        while (true)
        {
            if (ex is SocketException) return true;
            if (ex.InnerException == null) return false;

            ex = ex.InnerException;
        }
    }
}