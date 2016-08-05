using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace VideoGameHash.Models
{
    public class UserRepository
    {
        private VGHDatabaseContainer db = new VGHDatabaseContainer();

        public int CreateUserProfile(string userName)
        {
            UserProfile user = GetUserByUserName(userName);

            if (user == null)
            {
                user = new UserProfile();
                user.UserName = userName;

                db.UserProfiles.AddObject(user);
                db.SaveChanges();

                user = GetUserByUserName(userName);

                return user.Id;
            }
            else
            {
                return -1;
            }
        }

        public UserProfile GetUserByUserId(int userId)
        {
            return db.UserProfiles.SingleOrDefault(u => u.Id == userId);
        }

        public UserProfile GetUserByUserName(string userName)
        {
            try
            {
                return db.UserProfiles.SingleOrDefault(u => u.UserName == userName);
            }
            catch
            {
                return null;
            }
        }

        public string GetUserNameByUserId(int userId)
        {
            return db.UserProfiles.SingleOrDefault(u => u.Id == userId).UserName;
        }

        public string GetPasswordByUserId(int userId)
        {
            return db.Memberships.SingleOrDefault(u => u.UserId == userId).Password;
        }

        public int GetUserIdByEmail(string email)
        {
            Membership membership = db.Memberships.SingleOrDefault(u => u.Email == email);

            if (membership != null)
                return membership.UserId;
            else
                return -1;
        }

        public void UpdatePassword(string userName, string oldPassword, string newPassword)
        {
            SaltedHash saltedHash = new SaltedHash();

            string passwordSalt = GetPasswordSalt(userName);
            string passwordHash = GetPassword(userName);

            if (saltedHash.VerifyHashString(oldPassword, passwordHash, passwordSalt))
            {
                string salt, hash;

                saltedHash.GetHashAndSaltString(newPassword, out hash, out salt);
                Membership member = GetMembershipByUsername(userName);

                member.Password = hash;
                member.PasswordSalt = salt;
                member.PasswordChangeDate = DateTime.Now;

                db.SaveChanges();
            }
            else
                throw new Exception("Password provided is incorrect.");
        }

        public void ResetPassword(string userName, string newPassword)
        {
            SaltedHash saltedHash = new SaltedHash();
            string salt, hash;

            saltedHash.GetHashAndSaltString(newPassword, out hash, out salt);

            Membership member = GetMembershipByUsername(userName);

            member.Password = hash;
            member.PasswordSalt = salt;
            member.PasswordChangeDate = DateTime.Now;

            db.SaveChanges();
        }

        public Membership GetMembershipByUsername(string userName)
        {
            UserProfile userProfile = GetUserByUserName(userName);

            return db.Memberships.SingleOrDefault(u => u.UserId == userProfile.Id);
        }

        public Membership GetMembershipByEmail(string email)
        {
            return db.Memberships.SingleOrDefault(u => u.Email == email);
        }

        public Membership GetMembershipByUserId(int userId)
        {
            return db.Memberships.SingleOrDefault(u => u.UserId == userId);
        }

        public IEnumerable<UserProfile> GetAllUserProfile()
        {
            return db.UserProfiles.AsEnumerable();
        }

        public List<string> GetUserRolesByUserName(string userName)
        {
            UserProfile user = GetUserByUserName(userName);
            List<UsersInRoles> userRoles = new List<UsersInRoles>();
            userRoles = db.UsersInRoles.Where(d => d.UserProfileId == user.Id).ToList();
            List<string> roles = new List<string>();
            for (int i = 0; i < userRoles.Count; i++)
            {
                roles.Add(GetRole(userRoles[i].RolesId));
            }

            return roles;
        }

        public List<string> GetAllRoles()
        {
            List<Roles> roles = new List<Roles>();
            roles = db.Roles.ToList();

            List<string> rolesList = new List<string>();

            foreach (Roles role in roles)
                rolesList.Add(role.RoleName);

            return rolesList;
        }

        public string GetRole(int roleId)
        {
            return db.Roles.SingleOrDefault(u => u.Id == roleId).RoleName;
        }

        public void RegisterUser(Member member)
        {
            Membership membership = new Membership();
            SaltedHash saltedHash = new SaltedHash();
            String salt, hash;
            saltedHash.GetHashAndSaltString(member.Password, out hash, out salt);

            membership.CreateDate = DateTime.Now;
            membership.IsConfirmed = true;
            membership.UserId = member.UserId;
            membership.Password = hash;
            membership.PasswordChangeDate = DateTime.Now;
            membership.PasswordSalt = salt;
            membership.Email = member.Email;
            membership.SecurityQuestion = member.SecurityQuestion;
            membership.SecurityAnswer = member.SecurityAnswer;
            db.Memberships.AddObject(membership);
            db.SaveChanges();
        }

        public string GetPassword(string userName)
        {
            UserProfile user = GetUserByUserName(userName);
            if (user != null)
            {
                string password = db.Memberships.SingleOrDefault(u => u.UserId == user.Id).Password;
                return password;
            }
            else
                return string.Empty;
        }

        public string GetPasswordSalt(string userName)
        {
            UserProfile user = GetUserByUserName(userName);
            if (user != null)
            {
                int userId = user.Id;
                return db.Memberships.SingleOrDefault(u => u.UserId == userId).PasswordSalt;
            }
            else
                return string.Empty;
        }

        public bool VerifyUser(string userName, string password)
        {
            UserProfile user = GetUserByUserName(userName);

            if (user != null)
            {
                SaltedHash saltedHash = new SaltedHash();

                string passwordSalt = GetPasswordSalt(userName);
                string passwordDb = GetPassword(userName);

                return saltedHash.VerifyHashString(password, passwordDb, passwordSalt);
            }
            else
                return false;
        }

        public bool IsInRole(string userName, string role)
        {
            List<string> roles = GetUserRolesByUserName(userName);

            bool isInRole = false;

            foreach (string userRole in roles)
            {
                if (userRole == role)
                {
                    isInRole = true;
                    break;
                }
            }

            return isInRole;
        }

        public int GetRoleIdByRoleName(string roleName)
        {
            return db.Roles.SingleOrDefault(u => u.RoleName == roleName).Id;
        }

        public void AddRole(int userId, string role)
        {
            int roleId = GetRoleIdByRoleName(role);

            UsersInRoles usersInRoles = new UsersInRoles();

            usersInRoles.UserProfileId = userId;
            usersInRoles.RolesId = roleId;

            db.UsersInRoles.AddObject(usersInRoles);
            db.SaveChanges();
        }
    }

    class SaltedHash
    {
        HashAlgorithm HashProvider;
        int SalthLength;

        /// <summary>
        /// The constructor takes a HashAlgorithm as a parameter.
        /// </summary>
        /// <param name="HashAlgorithm">
        /// A <see cref="HashAlgorithm"/> HashAlgorihm which is derived from HashAlgorithm. C# provides
        /// the following classes: SHA1Managed,SHA256Managed, SHA384Managed, SHA512Managed and MD5CryptoServiceProvider
        /// </param>

        public SaltedHash(HashAlgorithm HashAlgorithm, int theSaltLength)
        {
            HashProvider = HashAlgorithm;
            SalthLength = theSaltLength;
        }

        /// <summary>
        /// Default constructor which initialises the SaltedHash with the SHA256Managed algorithm
        /// and a Salt of 4 bytes ( or 4*8 = 32 bits)
        /// </summary>

        public SaltedHash()
            : this(new SHA256Managed(), 4)
        {
        }

        /// <summary>
        /// The actual hash calculation is shared by both GetHashAndSalt and the VerifyHash functions
        /// </summary>
        /// <param name="Data">A byte array of the Data to Hash</param>
        /// <param name="Salt">A byte array of the Salt to add to the Hash</param>
        /// <returns>A byte array with the calculated hash</returns>

        private byte[] ComputeHash(byte[] Data, byte[] Salt)
        {
            // Allocate memory to store both the Data and Salt together
            byte[] DataAndSalt = new byte[Data.Length + SalthLength];

            // Copy both the data and salt into the new array
            Array.Copy(Data, DataAndSalt, Data.Length);
            Array.Copy(Salt, 0, DataAndSalt, Data.Length, SalthLength);

            // Calculate the hash
            // Compute hash value of our plain text with appended salt.
            return HashProvider.ComputeHash(DataAndSalt);
        }

        /// <summary>
        /// Given a data block this routine returns both a Hash and a Salt
        /// </summary>
        /// <param name="Data">
        /// A <see cref="System.Byte"/>byte array containing the data from which to derive the salt
        /// </param>
        /// <param name="Hash">
        /// A <see cref="System.Byte"/>byte array which will contain the hash calculated
        /// </param>
        /// <param name="Salt">
        /// A <see cref="System.Byte"/>byte array which will contain the salt generated
        /// </param>

        public void GetHashAndSalt(byte[] Data, out byte[] Hash, out byte[] Salt)
        {
            // Allocate memory for the salt
            Salt = new byte[SalthLength];

            // Strong runtime pseudo-random number generator, on Windows uses CryptAPI
            // on Unix /dev/urandom
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();

            // Create a random salt
            random.GetNonZeroBytes(Salt);

            // Compute hash value of our data with the salt.
            Hash = ComputeHash(Data, Salt);
        }

        /// <summary>
        /// The routine provides a wrapper around the GetHashAndSalt function providing conversion
        /// from the required byte arrays to strings. Both the Hash and Salt are returned as Base-64 encoded strings.
        /// </summary>
        /// <param name="Data">
        /// A <see cref="System.String"/> string containing the data to hash
        /// </param>
        /// <param name="Hash">
        /// A <see cref="System.String"/> base64 encoded string containing the generated hash
        /// </param>
        /// <param name="Salt">
        /// A <see cref="System.String"/> base64 encoded string containing the generated salt
        /// </param>

        public void GetHashAndSaltString(string Data, out string Hash, out string Salt)
        {
            byte[] HashOut;
            byte[] SaltOut;

            // Obtain the Hash and Salt for the given string
            GetHashAndSalt(Encoding.UTF8.GetBytes(Data), out HashOut, out SaltOut);

            // Transform the byte[] to Base-64 encoded strings
            Hash = Convert.ToBase64String(HashOut);
            Salt = Convert.ToBase64String(SaltOut);
        }

        /// <summary>
        /// This routine verifies whether the data generates the same hash as we had stored previously
        /// </summary>
        /// <param name="Data">The data to verify </param>
        /// <param name="Hash">The hash we had stored previously</param>
        /// <param name="Salt">The salt we had stored previously</param>
        /// <returns>True on a succesfull match</returns>

        public bool VerifyHash(byte[] Data, byte[] Hash, byte[] Salt)
        {
            byte[] NewHash = ComputeHash(Data, Salt);

            //  No easy array comparison in C# -- we do the legwork
            if (NewHash.Length != Hash.Length) return false;

            for (int Lp = 0; Lp < Hash.Length; Lp++)
                if (!Hash[Lp].Equals(NewHash[Lp]))
                    return false;

            return true;
        }

        /// <summary>
        /// This routine provides a wrapper around VerifyHash converting the strings containing the
        /// data, hash and salt into byte arrays before calling VerifyHash.
        /// </summary>
        /// <param name="Data">A UTF-8 encoded string containing the data to verify</param>
        /// <param name="Hash">A base-64 encoded string containing the previously stored hash</param>
        /// <param name="Salt">A base-64 encoded string containing the previously stored salt</param>
        /// <returns></returns>

        public bool VerifyHashString(string Data, string Hash, string Salt)
        {
            byte[] HashToVerify = Convert.FromBase64String(Hash);
            byte[] SaltToVerify = Convert.FromBase64String(Salt);
            byte[] DataToVerify = Encoding.UTF8.GetBytes(Data);
            return VerifyHash(DataToVerify, HashToVerify, SaltToVerify);
        }

    }
}