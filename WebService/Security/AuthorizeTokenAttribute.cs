using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using UIBuildIt.Common.Authentication;
using UIBuildIt.Models;

namespace UIBuildIt.Security
{
    public class AuthorizeTokenAttribute : AuthorizationFilterAttribute 
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext != null)
            {
                if (!AuthorizeRequest(actionContext.ControllerContext))
                {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = actionContext.ControllerContext.Request };
                }
                return;
            }
        }

        private bool AuthorizeRequest(HttpControllerContext context)
        {
            bool authorized = false;
            if (context.Request.Headers.Contains("Authorization"))
            {
                var tokenValue = context.Request.Headers.GetValues("Authorization");
                if (tokenValue.Count() == 1)
                {
                    var value = tokenValue.FirstOrDefault();
                    //Token validation logic here
                    //set authorized variable accordingly
                    using(UIBuildItContext db = new UIBuildItContext())
                    {
                        Token = db.Tokens.FirstOrDefault(t => t.Text == value);
                        authorized = Token != null && Token.Expiration > DateTime.UtcNow;
                        if(Token != null)
                        {
                            context.Request.Properties.Add("user", db.Users.FirstOrDefault(u =>u.Id == Token.UserId));
                        }
                    }
                }
            }
            return authorized;
        }

        public Token Token { get; set; }
    }
}