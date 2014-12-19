using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;

namespace Libarius.Active_Directory
{
    public static class AdHelper
    {
        /// <summary>
        /// Returns a list of groups the supplied user is member of.
        /// </summary>
        /// <param name="userName">Username</param>
        /// <returns>A list of groups on success, otherwise an empty list</returns>
        public static List<GroupPrincipal> GetGroups(string userName)
        {
            var result = new List<GroupPrincipal>();
            // establish domain context
            PrincipalContext yourDomain = null;

            try
            {
                // establish domain context
                yourDomain = new PrincipalContext(ContextType.Domain);
            }
            catch (PrincipalServerDownException) { return result; }

            // find your user
            var user = UserPrincipal.FindByIdentity(yourDomain, userName);

            // if found - grab its groups
            if (user != null)
            {
                PrincipalSearchResult<Principal> groups = user.GetAuthorizationGroups();

                // iterate over all groups
                foreach (Principal p in groups)
                {
                    // make sure to add only group principals
                    if (p is GroupPrincipal)
                    {
                        result.Add((GroupPrincipal)p);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if the current user is member of the supplied group.
        /// </summary>
        /// <param name="gUid">The groups unique identifier.</param>
        /// <returns>True if the user is a mamber, false otherwise.</returns>
        public static bool IsUserInGroup(Guid gUid)
        {
            return IsUserInGroup(Environment.UserName, gUid);
        }

        /// <summary>
        /// Checks if the current user is member of the supplied group.
        /// </summary>
        /// <param name="gUid">The groups unique identifier.</param>
        /// <returns>True if the user is a mamber, false otherwise.</returns>
        public static bool IsUserInGroup(string gUid)
        {
            return IsUserInGroup(Environment.UserName, gUid);
        }

        /// <summary>
        /// Checks if the provided user is member of the supplied group.
        /// </summary>
        /// <param name="userName">The logon name of the user.</param>
        /// <param name="gUid">The groups unique identifier.</param>
        /// <returns></returns>
        public static bool IsUserInGroup(string userName, string gUid)
        {
            return IsUserInGroup(userName, Guid.Parse(gUid));
        }

        /// <summary>
        /// Checks if the provided user is member of the supplied group.
        /// </summary>
        /// <param name="userName">The logon name of the user.</param>
        /// <param name="gUid">The groups unique identifier.</param>
        /// <returns></returns>
        public static bool IsUserInGroup(string userName, Guid gUid)
        {
            return (GetGroups(userName).Find(group => group.Guid == gUid) == null);
        }
    }
}
