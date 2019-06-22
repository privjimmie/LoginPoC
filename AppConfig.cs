using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoginPoC
{
    public static class AppConfig
    {
        //TODO: Move to appsettings

        public static readonly string Region = "eu-west-1";
        public static readonly string UserPoolId = "eu-west-1_xxx";
        public static readonly string ClientId = "xxx";
        public static readonly string ClientSecret = "xxx";
        public static readonly string ResponseType = "code";
        public static readonly string MetadataAddress = $"https://cognito-idp.{Region}.amazonaws.com/{UserPoolId}/.well-known/openid-configuration";
        public static readonly string Domain = "xxx.auth.eu-west-1.amazoncognito.com";
        public static readonly string Authority = $"https://cognito-idp.{Region}.amazonaws.com/{UserPoolId}";
    }
}
