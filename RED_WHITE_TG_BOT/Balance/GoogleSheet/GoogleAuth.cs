using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balance.GoogleSheet
{
    public static class GoogleAuth
    {

        public static GoogleCredential Login()
        {
            using var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read);
            return GoogleCredential.FromStream(stream)
                .CreateScoped();
        }


        public static async Task<UserCredential> LoginAsync(string googleClientId, string googleClientSecret, string[] scopes)
        {
            var secrets = new ClientSecrets()
            {
                ClientId = googleClientId,
                ClientSecret = googleClientSecret
            };

            return await GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, scopes, "user", CancellationToken.None);
        }
    }
}
