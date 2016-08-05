using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using WebMatrix.WebData;
using VideoGameHash.Helpers;


namespace VideoGameHash.Models
{
    public class CustomMembershipProvider : ExtendedMembershipProvider
    {
        private UserRepository repository = new UserRepository();

        public override bool ConfirmAccount(string accountConfirmationToken)
        {
            throw new NotImplementedException();
        }

        public override bool ConfirmAccount(string userName, string accountConfirmationToken)
        {
            throw new NotImplementedException();
        }

        public override string CreateAccount(string userName, string password, bool requireConfirmationToken)
        {
            throw new NotImplementedException();
        }

        public override string CreateUserAndAccount(string userName, string password, bool requireConfirmation, IDictionary<string, object> values)
        {
            MembershipCreateStatus status = MembershipCreateStatus.Success;
            MembershipUser user = CreateUser(userName, password, values["Email"].ToString(), values["SecurityQuestion"].ToString(), values["SecurityAnswer"].ToString(), true, null, out status);

            if (user == null)
            {
                throw new MembershipCreateUserException(status);
            }

            return null;
        }

        public override bool DeleteAccount(string userName)
        {
            throw new NotImplementedException();
        }

        public override string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow)
        {
            throw new NotImplementedException();
        }

        public override ICollection<OAuthAccountData> GetAccountsForUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetCreateDate(string userName)
        {
            Membership membership = repository.GetMembershipByUsername(userName);
            
            return membership.CreateDate;
        }

        public override DateTime GetLastPasswordFailureDate(string userName)
        {
            Membership membership = repository.GetMembershipByUsername(userName);

            return membership.LastPasswordFailureDate ?? membership.CreateDate;
        }

        public override DateTime GetPasswordChangedDate(string userName)
        {
            Membership membership = repository.GetMembershipByUsername(userName);

            return membership.PasswordChangeDate ?? membership.CreateDate;
        }

        public override int GetPasswordFailuresSinceLastSuccess(string userName)
        {
            Membership membership = repository.GetMembershipByUsername(userName);

            return membership.PasswordFailuresSinceLastSuccess ?? 0;
        }

        public override int GetUserIdFromPasswordResetToken(string token)
        {
            throw new NotImplementedException();
        }

        public override bool IsConfirmed(string userName)
        {
            UserProfile user = repository.GetUserByUserName(userName);

            if (user != null)
                return true;
            else
                return false;
        }

        public override bool ResetPasswordWithToken(string token, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            repository.UpdatePassword(username, oldPassword, newPassword);

            return true;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            ValidatePasswordEventArgs args =
           new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (RequiresUniqueEmail && GetUserNameByEmail(email) != string.Empty)
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            RegexUtilities utility = new RegexUtilities();

            if (!utility.IsValidEmail(email))
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }

            int userId = repository.CreateUserProfile(username);

            if (userId != -1)
            {
                Member member = new Member();

                member.UserId = userId;
                member.Password = password;
                member.Email = email;
                member.SecurityQuestion = passwordQuestion;
                member.SecurityAnswer = passwordAnswer;

                repository.RegisterUser(member);

                status = MembershipCreateStatus.Success;

                return GetUser(username, true);
            }
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
            }

            return null;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override bool EnablePasswordReset
        {
            get { return true; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            UserProfile user = repository.GetUserByUserName(username);

            if (user != null)
                return new MembershipUser("CustomMembershipProvider", username, null, null, String.Empty, String.Empty, true, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now);
            else
                return null;
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            int userId = repository.GetUserIdByEmail(email);

            if (userId != -1)
                return repository.GetUserNameByUserId(userId);
            else
                return string.Empty;
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredPasswordLength
        {
            get { throw new NotImplementedException(); }
        }

        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
            get { return true; }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            return repository.VerifyUser(username, password);
        }
    }

    public class Member
    {
        private int userId;
        public int UserId
        {
            get { return userId; }
            set { userId = value; }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        private string email;
        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        private string securityQuestion;
        public string SecurityQuestion
        {
            get { return securityQuestion; }
            set { securityQuestion = value; }
        }

        private string securityAnswer;
        public string SecurityAnswer
        {
            get { return securityAnswer; }
            set { securityAnswer = value; }
        }
    }
}