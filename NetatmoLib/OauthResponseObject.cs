using System.Collections.Generic;

namespace netatmo
{
    public class OauthResponseObject
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public List<string> scope { get; set; }
        public int expires_in { get; set; }
        public int expire_in { get; set; }
        public bool isValid { get; set; } = false;
        public long timestamp { get; set; }
    }
}