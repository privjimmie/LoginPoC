using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LoginPoC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly CognitoUserPool _userPool;
        private readonly IAmazonCognitoIdentityProvider _identityProvider;

        public AccountController(CognitoUserPool userPool, IAmazonCognitoIdentityProvider identityProvider)
        {
            _userPool = userPool;
            _identityProvider = identityProvider;
        }


        [HttpGet("NotProtected")]
        [AllowAnonymous]
        public async Task<ActionResult> NotProtected()
        {
            return new JsonResult("Not protected endpoint");
        }


        [HttpGet("Protected")]
        public async Task<ActionResult> Protected()
        {
            var data = new List<string>() { "Protected endpoint" };

            if (HttpContext.User != null)
            {
                foreach (var claim in HttpContext.User.Claims)
                {
                    data.Add($"Claim.{claim.Type}: {claim.Value}");
                }
            }

            return new JsonResult(data);
        }


        [HttpGet("Logout")]
        [AllowAnonymous]
        public async Task<ActionResult> Logout(string code)
        {
            await HttpContext.SignOutAsync();

            var redirectToUrlAfter = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/Account/NotProtected";
            var url = $"https://{AppConfig.Domain}/logout?client_id={AppConfig.ClientId}&response_type={AppConfig.ResponseType}&logout_uri={HttpUtility.UrlEncode(redirectToUrlAfter)}&redirect_uri={HttpUtility.UrlEncode(redirectToUrlAfter)}";
            return Redirect(url);
        }



        public class GetTokenRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        [HttpPost("GetToken")]
        [AllowAnonymous]
        public async Task<ActionResult> GetToken(GetTokenRequest input)
        {
            try
            {
                var user = new CognitoUser(input.username, AppConfig.ClientId, _userPool, _identityProvider, AppConfig.ClientSecret);
                var authRequest = new InitiateSrpAuthRequest() { Password = input.password };

                var authResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);

                var authenticationResultSerialized = JsonConvert.SerializeObject(authResponse.AuthenticationResult);
                return new JsonResult(authenticationResultSerialized);
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, e.Message);
            }

        }
    }
}
