using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace Libarius.Active_Directory
{
    public static class AdHelper
    {
        /// <summary>
        ///     Gets the current users full name from the domains directory.
        /// </summary>
        public static string FullName
        {
            get
            {
                try
                {
                    var de = new DirectoryEntry(string.Format("WinNT://{0}/{1}",
                        Environment.UserDomainName, Environment.UserName));
                    return de.Properties["fullName"].Value.ToString();
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        ///     Returns a list of groups the supplied user is member of.
        /// </summary>
        /// <param name="userName">Username</param>
        /// <returns>A list of groups on success, otherwise an empty list</returns>
        public static List<GroupPrincipal> GetGroups(string userName)
        {
            IEnumerable<GroupPrincipal> result = null;
            // establish domain context
            PrincipalContext yourDomain;

            try
            {
                // establish domain context
                yourDomain = new PrincipalContext(ContextType.Domain);
            }
            catch (PrincipalServerDownException)
            {
                return null;
            }

            // find your user
            var user = UserPrincipal.FindByIdentity(yourDomain, userName);

            // if found - grab its groups
            if (user != null)
            {
                result = from p in user.GetAuthorizationGroups()
                    where p is GroupPrincipal
                    select p as GroupPrincipal;
            }

            return (result != null) ? result.ToList() : null;
        }

        /// <summary>
        ///     Checks if the current user is member of the supplied group.
        /// </summary>
        /// <param name="gUid">The groups unique identifier.</param>
        /// <returns>True if the user is a mamber, false otherwise.</returns>
        public static bool IsUserInGroup(Guid gUid)
        {
            return IsUserInGroup(Environment.UserName, gUid);
        }

        /// <summary>
        ///     Checks if the current user is member of the supplied group.
        /// </summary>
        /// <param name="gUid">The groups unique identifier.</param>
        /// <returns>True if the user is a mamber, false otherwise.</returns>
        public static bool IsUserInGroup(string gUid)
        {
            return IsUserInGroup(Environment.UserName, gUid);
        }

        /// <summary>
        ///     Checks if the provided user is member of the supplied group.
        /// </summary>
        /// <param name="userName">The logon name of the user.</param>
        /// <param name="gUid">The groups unique identifier.</param>
        /// <returns></returns>
        public static bool IsUserInGroup(string userName, string gUid)
        {
            return IsUserInGroup(userName, Guid.Parse(gUid));
        }

        /// <summary>
        ///     Checks if the provided user is member of the supplied group.
        /// </summary>
        /// <param name="userName">The logon name of the user.</param>
        /// <param name="gUid">The groups unique identifier.</param>
        /// <returns></returns>
        public static bool IsUserInGroup(string userName, Guid gUid)
        {
            return (GetGroups(userName).Find(group => group.Guid == gUid) != null);
        }
    }
}