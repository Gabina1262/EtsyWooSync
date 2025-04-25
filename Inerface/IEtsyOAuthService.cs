using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtsyWooSync.Inerface
{
   public interface IEtsyOAuthService
    {
        Task<string> GetAccessTokenAsync();
        Task<bool> RefreshTokenAsync();
        string GetAuthorizationUrl(string state);
        Task ExchangeCodeForTokenAsync(string code);
    }
}
