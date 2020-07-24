using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Alvianda.Software.Service.iCOMD.Extensions
{
    public class CbsRoleRequirement : TypeFilterAttribute
    {
        public CbsRoleRequirement(string claimValue) : base(typeof(RoleFilter))
        {
            Arguments = new object[] { new Claim("roles",claimValue) };
        }
    }
    
    public class RoleFilter : Microsoft.AspNetCore.Mvc.Filters.IAuthorizationFilter
    {
        readonly Claim _claim;

        public RoleFilter(Claim claim)
        {
            _claim = claim;
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

                if (!secToken.Claims.ToList<Claim>().Exists(x => x.Type == _claim.Type && x.Value == _claim.Value))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
        }
    }
}
