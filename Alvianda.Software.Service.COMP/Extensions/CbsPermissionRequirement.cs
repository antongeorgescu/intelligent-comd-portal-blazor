using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Alvianda.Software.Service.COMP.Extensions
{
    public class CbsPermissionRequirement : TypeFilterAttribute
    {
        public CbsPermissionRequirement(string claimValue,string source) : base(typeof(ScopeFilter))
        {
            Arguments = new object[] { new Claim("scp",claimValue),source };
        }
    }
    
    public class ScopeFilter : Microsoft.AspNetCore.Mvc.Filters.IAuthorizationFilter
    {
        readonly Claim _claim;
        readonly string _source;

        public ScopeFilter(Claim claim, string source)
        {
            _claim = claim;
            _source = source;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //if (!context.HttpContext.User.Identity.IsAuthenticated)
            //{
            //    context.Result = new UnauthorizedResult();
            //    return;
            //}

            var token = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token);
                var secToken = handler.ReadToken(token) as JwtSecurityToken;

                // return unauthorized if User Principal Name ('upn') string is missing or empty or null
                if (!secToken.Claims.ToList<Claim>().Exists(x => x.Type == "upn"))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                if (_source == "IDM")
                {
                    JObject idmperms = JObject.Parse(File.ReadAllText("idmpermissions.json"));
                    var controllernm = context.RouteData.Values["controller"];

                    if (!secToken.Claims.ToList().Exists(x => x.Type == "roles"))
                    {
                        context.Result = new UnauthorizedResult();
                        return;
                    }
                    else
                    {
                        var reqrole = secToken.Claims.ToList().First(x => x.Type == "roles").Value;

                        var allperms = idmperms["Controllers"][controllernm]["Roles"][reqrole]["Permissions"].Children();
                        if (!allperms.ToList().Exists(x => x.ToObject<string>() == _claim.Value))
                        {
                            context.Result = new UnauthorizedResult();
                            return;
                        }
                    }
                }
                
                if (_source == "AAD")
                    if (!secToken.Claims.ToList<Claim>().Exists(x => x.Type == _claim.Type && x.Value == _claim.Value))
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
            }
        }
    }
}
