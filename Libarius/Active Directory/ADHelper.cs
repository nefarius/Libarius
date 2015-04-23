using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;

namespace Libarius.Active_Directory
{
    public static class AdHelper
    {
        public enum ObjectClass
        {
            User,
            Group,
            Computer
        }

        public enum ReturnType
        {
            DistinguishedName,
            ObjectGuid
        }

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
                    // TODO: improve this!
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        ///     Returns the current machines domain name.
        /// </summary>
        public static string MachineDomain
        {
            get
            {
                try
                {
                    return Domain.GetComputerDomain().ToString();
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    return null;
                }
            }
        }

        /// <summary>
        ///     Returns the current machines site name.
        /// </summary>
        public static string MachineSite
        {
            get
            {
                try
                {
                    return ActiveDirectorySite.GetComputerSite().Name;
                }
                catch (ActiveDirectoryServerDownException)
                {
                    return null;
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    return null;
                }
            }
        }

        public static string MachineSiteDescription
        {
            get
            {
                try
                {
                    // get current domain root DSE
                    var root = new DirectoryEntry("LDAP://RootDSE");
                    // get configuration DN
                    var configDn = root.Properties["configurationNamingContext"].Value.ToString();
                    // build site DN for LDAP query
                    var siteDn = string.Format("LDAP://CN={0},CN=Sites,{1}", MachineSite, configDn);
                    // query LDAP
                    var siteDescription = new DirectoryEntry(siteDn);
                    // return content of description property
                    return siteDescription.Properties["description"].Value.ToString();
                }
                    // TODO: improve this!
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        ///     Primitive check if current session is logged on to a domain.
        /// </summary>
        public static bool IsInActiveDirectory
        {
            get
            {
                try
                {
                    Domain.GetComputerDomain();
                    return true;
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    return false;
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

        /// <summary>
        ///     Searches for provided object in directory and resolves it to distinguished name.
        /// </summary>
        /// <param name="objectCls">The object class to search for.</param>
        /// <param name="returnValue">The format of the returned string.</param>
        /// <param name="objectName">The common name of the object to search for.</param>
        /// <param name="ldapDomain">The LDAP domain to search in.</param>
        /// <returns></returns>
        /// <remarks>http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C</remarks>
        public static string GetObjectDistinguishedName(ObjectClass objectCls,
            ReturnType returnValue, string objectName, string ldapDomain)
        {
            var distinguishedName = string.Empty;
            var connectionPrefix = string.Format("LDAP://{0}", ldapDomain);

            using (var entry = new DirectoryEntry(connectionPrefix))
            {
                using (var mySearcher = new DirectorySearcher(entry))
                {
                    switch (objectCls)
                    {
                        case ObjectClass.User:
                            mySearcher.Filter = string.Format("(&(objectClass=user)(|(cn={0})(sAMAccountName={0})))",
                                objectName);
                            break;
                        case ObjectClass.Group:
                            mySearcher.Filter = string.Format("(&(objectClass=group)(|(cn={0})(dn={0})))", objectName);
                            break;
                        case ObjectClass.Computer:
                            mySearcher.Filter = string.Format("(&(objectClass=computer)(|(cn={0})(dn={0})))", objectName);
                            break;
                    }

                    var result = mySearcher.FindOne();

                    if (result == null)
                    {
                        throw new NullReferenceException
                            ("unable to locate the distinguishedName for the object " +
                             objectName + " in the " + ldapDomain + " domain");
                    }

                    var directoryObject = result.GetDirectoryEntry();

                    if (returnValue.Equals(ReturnType.DistinguishedName))
                    {
                        distinguishedName = string.Format("LDAP://{0}", directoryObject.Properties
                            ["distinguishedName"].Value);
                    }

                    if (returnValue.Equals(ReturnType.ObjectGuid))
                    {
                        distinguishedName = directoryObject.Guid.ToString();
                    }
                }
            }
            return distinguishedName;
        }

        /// <remarks>http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C</remarks>
        public static string ConvertDNtoGuid(string objectDn)
        {
            //Removed logic to check existence first

            using (var directoryObject = new DirectoryEntry(objectDn))
            {
                return directoryObject.Guid.ToString();
            }
        }

        /// <remarks>http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C</remarks>
        public static string ConvertGuidToOctectString(string objectGuid)
        {
            var guid = new Guid(objectGuid);
            var byteGuid = guid.ToByteArray();
            var queryGuid = "";
            foreach (var b in byteGuid)
            {
                queryGuid += @"\" + b.ToString("x2");
            }
            return queryGuid;
        }

        /// <remarks>Currently broken!</remarks>
        public static string ConvertGuidToDn(string GUID)
        {
            using (var ent = new DirectoryEntry())
            {
                var adGuid = ent.NativeGuid;
                var x = new DirectoryEntry("LDAP://{GUID=" + adGuid + ">");
                //change the { to <>

                return x.Path.Remove(0, 7); //remove the LDAP prefix from the path
            }
        }

        /// <remarks>http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C</remarks>
        public static void EnableUserAccount(string userDn)
        {
            try
            {
                using (var user = new DirectoryEntry(userDn))
                {
                    var val = (int) user.Properties["userAccountControl"].Value;
                    user.Properties["userAccountControl"].Value = val & ~0x2; //ADS_UF_NORMAL_ACCOUNT;

                    user.CommitChanges();
                }
            }
            catch (DirectoryServicesCOMException)
            {
                //TODO: implement proper error handling
            }
        }

        /// <remarks>http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C</remarks>
        public static void DisableUserAccount(string userDn)
        {
            try
            {
                using (var user = new DirectoryEntry(userDn))
                {
                    var val = (int) user.Properties["userAccountControl"].Value;
                    user.Properties["userAccountControl"].Value = val | 0x2; //ADS_UF_ACCOUNTDISABLE;

                    user.CommitChanges();
                }
            }
            catch (DirectoryServicesCOMException)
            {
                //TODO: implement proper error handling
            }
        }

        /// <remarks>http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C</remarks>
        public static void Unlock(string userDn)
        {
            try
            {
                using (var uEntry = new DirectoryEntry(userDn))
                {
                    uEntry.Properties["LockOutTime"].Value = 0; //unlock account
                    uEntry.CommitChanges();
                }
            }
            catch (DirectoryServicesCOMException)
            {
                //TODO: implement proper error handling
            }
        }

        /// <remarks>http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C</remarks>
        public static void ResetPassword(string userDn, string password)
        {
            using (var uEntry = new DirectoryEntry(userDn))
            {
                uEntry.Invoke("SetPassword", password);
                uEntry.Properties["LockOutTime"].Value = 0; //unlock account
                uEntry.CommitChanges();
            }
        }

        /// <remarks>http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C</remarks>
        public static string CreateUserAccount(string ldapPath, string userName, string userPassword)
        {
            var oGUID = string.Empty;

            try
            {
                var connectionPrefix = string.Format("LDAP://{0}", ldapPath);

                using (var dirEntry = new DirectoryEntry(connectionPrefix))
                {
                    var newUser = dirEntry.Children.Add(string.Format("CN={0}", userName), "user");
                    newUser.Properties["samAccountName"].Value = userName;
                    newUser.CommitChanges();
                    oGUID = newUser.Guid.ToString();

                    newUser.Invoke("SetPassword", userPassword);
                    newUser.CommitChanges();
                    newUser.Close();
                }
            }
            catch (DirectoryServicesCOMException)
            {
                //TODO: implement proper error handling
            }
            return oGUID;
        }

        /// <remarks>http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C</remarks>
        private static IEnumerable<string> AttributeValuesMultiString(string attributeName, string objectDn,
            List<string> valuesCollection, bool recursive)
        {
            using (var ent = new DirectoryEntry(objectDn))
            {
                var valueCollection = ent.Properties[attributeName];
                var en = valueCollection.GetEnumerator();

                while (en.MoveNext())
                {
                    if (en.Current != null)
                    {
                        if (!valuesCollection.Contains(en.Current.ToString()))
                        {
                            valuesCollection.Add(en.Current.ToString());
                            if (recursive)
                            {
                                AttributeValuesMultiString(attributeName, string.Format("LDAP://{0}",
                                    en.Current), valuesCollection, true);
                            }
                        }
                    }
                }
            }

            return valuesCollection;
        }

        /// <remarks>http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C</remarks>
        public static string AttributeValuesSingleString
            (string attributeName, string objectDn)
        {
            using (var ent = new DirectoryEntry(objectDn))
            {
                return string.Format("{0}", ent.Properties[attributeName].Value);
            }
        }

        /// <remarks>http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C</remarks>
        public static IEnumerable<string> Groups(string userDn, bool recursive)
        {
            var groupMemberships = new List<string>();
            return AttributeValuesMultiString("memberOf", userDn,
                groupMemberships, recursive);
        }

        /// <remarks>http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C</remarks>
        public static void AddToGroup(string userDn, string groupDn)
        {
            try
            {
                using (var dirEntry = new DirectoryEntry(string.Format("LDAP://{0}", groupDn)))
                {
                    dirEntry.Properties["member"].Add(userDn);
                    dirEntry.CommitChanges();
                }
            }
            catch (DirectoryServicesCOMException)
            {
                //TODO: implement proper error handling
            }
        }
    }
}