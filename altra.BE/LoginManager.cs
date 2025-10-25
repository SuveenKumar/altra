using KiteConnect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace altra.BE
{
    public class LoginManager
    {
        private Kite _kite;

        public LoginManager(Kite kite)
        {
            _kite = kite;
        }

        public string GetLoginURL()
        {
            return _kite.GetLoginURL();

        }
        public async Task<string> ProcessLoginWithRequestToken(string requestToken)
        {
            try
            {
                var user = await Task.Run(() => _kite.GenerateSession(requestToken, TradingConstants.APISECRET));
                if (ProcessLoginWithAccessToken(user.AccessToken).Result) 
                {
                    return user.AccessToken;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public async Task<bool> ProcessLoginWithAccessToken(string accessToken)
        {
            try
            {
                _kite.SetAccessToken(accessToken);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

}
