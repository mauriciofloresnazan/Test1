using System;
using System.Security.Claims;
using System.Security.Principal;

namespace Ppgz.Web.Infraestructure
{
    public static class ExtendedMethods_Principal
    {
        public static bool HasPermission(this IPrincipal _principal, string _requiredPermission)
        {
            bool _retVal = false;
           /* try
            {
                if (_principal != null && _principal.Identity != null && _principal.Identity.IsAuthenticated)
                {
                    var ci = _principal.Identity as ClaimsIdentity;
                    string _userId = ci != null ? ci.FindFirstValue(ClaimTypes.NameIdentifier) : null;

                    if (!string.IsNullOrEmpty(_userId))
                    {
                        ApplicationUser _authenticatedUser = ApplicationUserManager.GetUser(int.Parse(_userId));
                        _retVal = _authenticatedUser.IsPermissionInUserRoles(_requiredPermission);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }*/
            return _retVal;
        }

        public static bool IsSysAdmin(this IPrincipal _principal)
        {
            bool _retVal = false;
            /*try
            {
                if (_principal != null && _principal.Identity != null && _principal.Identity.IsAuthenticated)
                {
                    var ci = _principal.Identity as ClaimsIdentity;
                    string _userId = ci != null ? ci.FindFirstValue(ClaimTypes.NameIdentifier) : null;

                    if (!string.IsNullOrEmpty(_userId))
                    {
                        ApplicationUser _authenticatedUser = ApplicationUserManager.GetUser(int.Parse(_userId));
                        _retVal = _authenticatedUser.IsSysAdmin();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }*/
            return _retVal;
        }

        public static string FindFirstValue(this ClaimsIdentity identity, string claimType)
        {
            string _retVal = string.Empty;
            try
            {
                if (identity != null)
                {
                    var claim = identity.FindFirst(claimType);
                    _retVal = claim != null ? claim.Value : null;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return _retVal;
        }
    }
}