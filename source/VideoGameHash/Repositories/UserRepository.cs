using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VideoGameHash.Models;

namespace VideoGameHash.Repositories
{
    public class UserRepository
    {
        private readonly VGHDatabaseContainer _db;

        public UserRepository(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public int CreateUserProfile(string userName)
        {
            var user = GetUserByUserName(userName);

            if (user == null)
            {
                user = new UserProfile
                {
                    UserName = userName
                };

                _db.UserProfiles.AddObject(user);
                _db.SaveChanges();

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
            return _db.UserProfiles.SingleOrDefault(u => u.Id == userId);
        }

        public UserProfile GetUserByUserName(string userName)
        {
            try
            {
                return _db.UserProfiles.SingleOrDefault(u => u.UserName == userName);
            }
            catch
            {
                return null;
            }
        }

        public string GetUserNameByUserId(int userId)
        {
            return _db.UserProfiles.SingleOrDefault(u => u.Id == userId).UserName;
        }

        public string GetPasswordByUserId(int userId)
        {
            return _db.Memberships.SingleOrDefault(u => u.UserId == userId).Password;
        }

        public int GetUserIdByEmail(string email)
        {
            var membership = _db.Memberships.SingleOrDefault(u => u.Email == email);

            if (membership != null)
                return membership.UserId;
            else
                return -1;
        }

        public void UpdatePassword(string userName, string oldPassword, string newPassword)
        {
            var saltedHash = new SaltedHash();

            var passwordSalt = GetPasswordSalt(userName);
            var passwordHash = GetPassword(userName);

            if (saltedHash.VerifyHashString(oldPassword, passwordHash, passwordSalt))
            {
                string salt, hash;

                saltedHash.GetHashAndSaltString(newPassword, out hash, out salt);
                var member = GetMembershipByUsername(userName);

                member.Password = hash;
                member.PasswordSalt = salt;
                member.PasswordChangeDate = DateTime.Now;

                _db.SaveChanges();
            }
            else
                throw new Exception("Password provided is incorrect.");
        }

        public void ResetPassword(string userName, string newPassword)
        {
            var saltedHash = new SaltedHash();
            string salt, hash;

            saltedHash.GetHashAndSaltString(newPassword, out hash, out salt);

            var member = GetMembershipByUsername(userName);

            member.Password = hash;
            member.PasswordSalt = salt;
            member.PasswordChangeDate = DateTime.Now;

            _db.SaveChanges();
        }

        public Membership GetMembershipByUsername(string userName)
        {
            var userProfile = GetUserByUserName(userName);

            return _db.Memberships.SingleOrDefault(u => u.UserId == userProfile.Id);
        }

        public Membership GetMembershipByEmail(string email)
        {
            return _db.Memberships.SingleOrDefault(u => u.Email == email);
        }

        public Membership GetMembershipByUserId(int userId)
        {
            return _db.Memberships.SingleOrDefault(u => u.UserId == userId);
        }

        public IEnumerable<UserProfile> GetAllUserProfile()
        {
            return _db.UserProfiles.AsEnumerable();
        }

        public List<string> GetUserRolesByUserName(string userName)
        {
            var user = GetUserByUserName(userName);
            var userRoles = new List<UsersInRoles>();
            userRoles = _db.UsersInRoles.Where(d => d.UserProfileId == user.Id).ToList();
            var roles = new List<string>();
            for (var i = 0; i < userRoles.Count; i++)
            {
                roles.Add(GetRole(userRoles[i].RolesId));
            }

            return roles;
        }

        public List<string> GetAllRoles()
        {
            var roles = new List<Roles>();
            roles = _db.Roles.ToList();

            var rolesList = new List<string>();

            foreach (var role in roles)
                rolesList.Add(role.RoleName);

            return rolesList;
        }

        public string GetRole(int roleId)
        {
            return _db.Roles.SingleOrDefault(u => u.Id == roleId).RoleName;
        }

        public void RegisterUser(Member member)
        {
            var membership = new Membership();
            var saltedHash = new SaltedHash();
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
            _db.Memberships.AddObject(membership);
            _db.SaveChanges();
        }

        public string GetPassword(string userName)
        {
            var user = GetUserByUserName(userName);
            if (user != null)
            {
                var password = _db.Memberships.SingleOrDefault(u => u.UserId == user.Id).Password;
                return password;
            }
            else
                return string.Empty;
        }

        public string GetPasswordSalt(string userName)
        {
            var user = GetUserByUserName(userName);
            if (user != null)
            {
                var userId = user.Id;
                return _db.Memberships.SingleOrDefault(u => u.UserId == userId).PasswordSalt;
            }
            else
                return string.Empty;
        }

        public bool VerifyUser(string userName, string password)
        {
            var user = GetUserByUserName(userName);

            if (user != null)
            {
                var saltedHash = new SaltedHash();

                var passwordSalt = GetPasswordSalt(userName);
                var passwordDb = GetPassword(userName);

                return saltedHash.VerifyHashString(password, passwordDb, passwordSalt);
            }
            else
                return false;
        }

        public bool IsInRole(string userName, string role)
        {
            var roles = GetUserRolesByUserName(userName);

            var isInRole = false;

            foreach (var userRole in roles)
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
            return _db.Roles.SingleOrDefault(u => u.RoleName == roleName).Id;
        }

        public void AddRole(int userId, string role)
        {
            var roleId = GetRoleIdByRoleName(role);

            var usersInRoles = new UsersInRoles
            {
                UserProfileId = userId,
                RolesId = roleId
            };

            _db.UsersInRoles.AddObject(usersInRoles);
            _db.SaveChanges();
        }
    }

    class SaltedHash
    {
        HashAlgorithm _hashProvider;
        int _salthLength;

        /// <summary>
        /// The constructor takes a HashAlgorithm as a parameter.
        /// </summary>
        /// <param name="hashAlgorithm">
        /// A <see cref="HashAlgorithm"/> HashAlgorihm which is derived from HashAlgorithm. C# provides
        /// the following classes: SHA1Managed,SHA256Managed, SHA384Managed, SHA512Managed and MD5CryptoServiceProvider
        /// </param>

        public SaltedHash(HashAlgorithm hashAlgorithm, int theSaltLength)
        {
            _hashProvider = hashAlgorithm;
            _salthLength = theSaltLength;
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
        /// <param name="data">A byte array of the Data to Hash</param>
        /// <param name="salt">A byte array of the Salt to add to the Hash</param>
        /// <returns>A byte array with the calculated hash</returns>

        private byte[] ComputeHash(byte[] data, byte[] salt)
        {
            // Allocate memory to store both the Data and Salt together
            var dataAndSalt = new byte[data.Length + _salthLength];

            // Copy both the data and salt into the new array
            Array.Copy(data, dataAndSalt, data.Length);
            Array.Copy(salt, 0, dataAndSalt, data.Length, _salthLength);

            // Calculate the hash
            // Compute hash value of our plain text with appended salt.
            return _hashProvider.ComputeHash(dataAndSalt);
        }

        /// <summary>
        /// Given a data block this routine returns both a Hash and a Salt
        /// </summary>
        /// <param name="data">
        /// A <see cref="System.Byte"/>byte array containing the data from which to derive the salt
        /// </param>
        /// <param name="hash">
        /// A <see cref="System.Byte"/>byte array which will contain the hash calculated
        /// </param>
        /// <param name="salt">
        /// A <see cref="System.Byte"/>byte array which will contain the salt generated
        /// </param>

        public void GetHashAndSalt(byte[] data, out byte[] hash, out byte[] salt)
        {
            // Allocate memory for the salt
            salt = new byte[_salthLength];

            // Strong runtime pseudo-random number generator, on Windows uses CryptAPI
            // on Unix /dev/urandom
            var random = new RNGCryptoServiceProvider();

            // Create a random salt
            random.GetNonZeroBytes(salt);

            // Compute hash value of our data with the salt.
            hash = ComputeHash(data, salt);
        }

        /// <summary>
        /// The routine provides a wrapper around the GetHashAndSalt function providing conversion
        /// from the required byte arrays to strings. Both the Hash and Salt are returned as Base-64 encoded strings.
        /// </summary>
        /// <param name="data">
        /// A <see cref="System.String"/> string containing the data to hash
        /// </param>
        /// <param name="hash">
        /// A <see cref="System.String"/> base64 encoded string containing the generated hash
        /// </param>
        /// <param name="salt">
        /// A <see cref="System.String"/> base64 encoded string containing the generated salt
        /// </param>

        public void GetHashAndSaltString(string data, out string hash, out string salt)
        {
            byte[] hashOut;
            byte[] saltOut;

            // Obtain the Hash and Salt for the given string
            GetHashAndSalt(Encoding.UTF8.GetBytes(data), out hashOut, out saltOut);

            // Transform the byte[] to Base-64 encoded strings
            hash = Convert.ToBase64String(hashOut);
            salt = Convert.ToBase64String(saltOut);
        }

        /// <summary>
        /// This routine verifies whether the data generates the same hash as we had stored previously
        /// </summary>
        /// <param name="data">The data to verify </param>
        /// <param name="hash">The hash we had stored previously</param>
        /// <param name="salt">The salt we had stored previously</param>
        /// <returns>True on a succesfull match</returns>

        public bool VerifyHash(byte[] data, byte[] hash, byte[] salt)
        {
            var newHash = ComputeHash(data, salt);

            //  No easy array comparison in C# -- we do the legwork
            if (newHash.Length != hash.Length) return false;

            for (var lp = 0; lp < hash.Length; lp++)
                if (!hash[lp].Equals(newHash[lp]))
                    return false;

            return true;
        }

        /// <summary>
        /// This routine provides a wrapper around VerifyHash converting the strings containing the
        /// data, hash and salt into byte arrays before calling VerifyHash.
        /// </summary>
        /// <param name="data">A UTF-8 encoded string containing the data to verify</param>
        /// <param name="hash">A base-64 encoded string containing the previously stored hash</param>
        /// <param name="salt">A base-64 encoded string containing the previously stored salt</param>
        /// <returns></returns>

        public bool VerifyHashString(string data, string hash, string salt)
        {
            var hashToVerify = Convert.FromBase64String(hash);
            var saltToVerify = Convert.FromBase64String(salt);
            var dataToVerify = Encoding.UTF8.GetBytes(data);
            return VerifyHash(dataToVerify, hashToVerify, saltToVerify);
        }

    }
}