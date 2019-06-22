# LoginPoC
ASP.NET Core wep app - login proof of concept - using dual auth OpenId and Jwt tokens through AWS Cognito.


Sign users up / Log users in with AWS Cognito hosted UI.
Also allow authentication against the same endpoints using JWT access token 




Three endpoints to test the authentication
/Api/Account/NotProtected
/Api/Account/Protected
/Api/Account/Logout


The "Protected" endpoint can be reached both through standard cookie authentication and with Bearer token (using for example Postman)

There is also one method /Api/Account/GetToken that takes username + password and returns tokens (to have some tokens to play with)


The key for allowing dual auth is in /Startup.cs where there is a dynamic policy scheme to choose the correct authentication scheme at runtime by lookig at the headers for a "bearer" key and forwards accordingly
Solution found at https://github.com/aspnet/Security/issues/1469 (thanks https://github.com/openidauthority)


