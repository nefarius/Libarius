using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;

namespace Libarius.AD
{
    public static class ADHelper
    {
        /// <summary>
        /// Returns a list of groups the supplied user is member of.
        /// </summary>
        /// <param name="userName">Username</param>
        /// <returns>A list of groups on success, otherwise an empty list</returns>
        public static List<GroupPrincipal> GetGroups(string userName)
        {
            List<GroupPrincipal> result = new List<GroupPrincipal>();
            // establish domain context
            PrincipalContext yourDomain = null;

            try
            {
                // establish domain context
                yourDomain = new PrincipalContext(ContextType.Domain);
            }
            catch (PrincipalServerDownException) { return result; }

            // find your user
            UserPrincipal user = UserPrincipal.FindByIdentity(yourDomain, userName);

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
        /// <param name="gUID">The groups unique identifier.</param>
        /// <returns>True if the user is a mamber, false otherwise.</returns>
        public static bool IsUserInGroup(Guid gUID)
        {
            return IsUserInGroup(Environment.UserName, gUID);
        }

        /// <summary>
        /// Checks if the current user is member of the supplied group.
        /// </summary>
        /// <param name="gUID">The groups unique identifier.</param>
        /// <returns>True if the user is a mamber, false otherwise.</returns>
        public static bool IsUserInGroup(string gUID)
        {
            return IsUserInGroup(Environment.UserName, gUID);
        }

        /// <summary>
        /// Checks if the provided user is member of the supplied group.
        /// </summary>
        /// <param name="userName">The logon name of the user.</param>
        /// <param name="gUID">The groups unique identifier.</param>
        /// <returns></returns>
        public static bool IsUserInGroup(string userName, string gUID)
        {
            return IsUserInGroup(userName, Guid.Parse(gUID));
        }

        /// <summary>
        /// Checks if the provided user is member of the supplied group.
        /// </summary>
        /// <param name="userName">The logon name of the user.</param>
        /// <param name="gUID">The groups unique identifier.</param>
        /// <returns></returns>
        public static bool IsUserInGroup(string userName, Guid gUID)
        {
            return ADHelper.GetGroups(userName).Find(group => group.Guid == gUID) == null ? false : true;
        }
    }
}
