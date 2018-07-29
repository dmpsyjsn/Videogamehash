using System;
//using System.Transactions;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserRepository _repository;

        public AccountController(IUserRepository repository)
        {
            _repository = repository;
        }

        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            if (WebSecurity.IsConfirmed(model.UserName))
            {
                ModelState.AddModelError("", "The password provided is incorrect.");

            }
            else
                ModelState.AddModelError("", "The user name provided is incorrect.");
            return View(model);
        }

        //
        // POST: Account/Logoff
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOut()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/LogOff

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult LogOff()
        //{
        //    WebSecurity.Logout();

        //    return RedirectToAction("Index", "Home");
        //}

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            //return View();

            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            //if (ModelState.IsValid)
            //{
            //    // Attempt to register the user
            //    try
            //    {
            //        WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new {model.Email, model.SecurityQuestion, model.SecurityAnswer });
            //        WebSecurity.Login(model.UserName, model.Password);
            //        return RedirectToAction("Index", "Home");
            //    }
            //    catch (MembershipCreateUserException e)
            //    {
            //        ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
            //    }
            //}

            //// If we got this far, something failed, redisplay form
            //return View(model);

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : "";

            ViewBag.IsAdmin = _repository.IsInRole(User.Identity.Name, "Administrator");
            ViewBag.IsEditor = _repository.IsInRole(User.Identity.Name, "Editor");

            return View();
        }

        //
        // GET: /Account/ChangePassword
        public ActionResult ChangePassword(ManageMessageId? message)
        {
            //return View();

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(LocalPasswordModel model)
        {
            //if (ModelState.IsValid)
            //{
            //    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
            //    bool changePasswordSucceeded;
            //    try
            //    {
            //        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
            //    }
            //    catch (Exception)
            //    {
            //        changePasswordSucceeded = false;
            //    }

            //    if (changePasswordSucceeded)
            //    {
            //        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
            //    }
            //    else
            //    {
            //        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
            //    }
            //}

            //return View(model);

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Validate

        [AllowAnonymous]
        public ActionResult Validate(string recover)
        {
            //ViewBag.Recover = recover;
            //return View();

            return RedirectToAction("Index", "Home");
        }


        //
        // POST: /Account/Validate
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Validate(RecoverModel model, string recover)
        {
            //var member = _repository.GetMembershipByEmail(model.Email);

            //ViewBag.Message = String.Empty;

            //if (member != null)
            //{
            //    var security = new SecurityQuestionModel
            //    {
            //        UserId = member.UserId,
            //        SecurityQuestion = member.SecurityQuestion
            //    };

            //    if (recover == "username")
            //        return RedirectToAction("RecoverUsername", security);
            //    else
            //        return RedirectToAction("RecoverPassword", security);
            //}

            //ViewBag.Message = "Email address not found.  Please try again.";

            //return View();

            return RedirectToAction("Index", "Home");
        }
        
        //
        // GET: Account/Recover
        [AllowAnonymous]
        public ActionResult RecoverUsername(SecurityQuestionModel member)
        {
            //var security = new SecurityAnswerModel
            //{
            //    UserId = member.UserId,
            //    SecurityQuestion = member.SecurityQuestion,
            //    SecurityAnswer = String.Empty
            //};

            //return View(security);

            return RedirectToAction("Index", "Home");
        }

        //
        // POST: Account/RecoverUsername
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RecoverUsername(SecurityAnswerModel model)
        {
            //var member = _repository.GetMembershipByUserId(model.UserId);

            //ViewBag.Message = "";

            //if (member != null)
            //{
            //    if (String.Compare(member.SecurityAnswer, model.SecurityAnswer, true) == 0)
            //    {
            //        if (SendEmail(member.Email, "Username for VideoGameHash", "Your username is " + _repository.GetUserNameByUserId(model.UserId)))
            //            return RedirectToAction("RecoverResult", new { Type = "Recover user name", Message = "User name has been sent to " + member.Email });
            //        else
            //            return RedirectToAction("RecoverResult", new { Type = "Recover user name", Message = "Unable to email username.  Please try again later." });
            //    }
            //    else
            //        ViewBag.Message = "The answer provided was incorrect.  Please try again.";
            //}

            //return View(model);

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: Account/RecoverPassword
        [AllowAnonymous]
        public ActionResult RecoverPassword(SecurityQuestionModel model)
        {
            //var security = new SecurityAnswerModel
            //{
            //    UserId = model.UserId,
            //    SecurityQuestion = model.SecurityQuestion,
            //    SecurityAnswer = String.Empty
            //};

            //return View(security);

            return RedirectToAction("Index", "Home");
        }

        //
        // POST: Account/RecoverPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RecoverPassword(SecurityAnswerModel model)
        {
            //var member = _repository.GetMembershipByUserId(model.UserId);

            //ViewBag.Message = "";

            //if (member != null)
            //{
            //    if (String.Compare(member.SecurityAnswer, model.SecurityAnswer, true) == 0)
            //    {
            //        var newPassword = CreatePassword(8);
            //        _repository.ResetPassword(_repository.GetUserNameByUserId(member.UserId), newPassword);
            //        if (SendEmail(member.Email, "Password reset for VideoGameHash", "Your password has been reset.  It is now " + newPassword + "."))
            //            return RedirectToAction("RecoverResult", new { Type = "Reset Password", Message = "Password has been reset. A new password has been sent to " + member.Email + "." });
            //        else
            //            return RedirectToAction("RecoverResult", new { Type = "Reset Password", Message = "Unable to reset pasword.  Please try again later." } );
            //    }
            //    else
            //        ViewBag.Message = "The answer provided was incorrect.  Please try again.";
            //}

            //return View(model);

            return RedirectToAction("Index", "Home");
        }

        // 
        // GET: Account/RecoverResult
        [AllowAnonymous]
        public ActionResult RecoverResult(string type, string message)
        {
            //ViewBag.Type = type;
            //ViewBag.Message = message;

            //return View();

            return RedirectToAction("Index", "Home");
        }

        private bool SendEmail(string emailAddress, string subject, string body)
        {
            try
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        private static string CreatePassword(int passwordLength)
        {
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            var chars = new char[passwordLength];
            var rd = new Random();

            for (var i = 0; i < passwordLength; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }
        #endregion
    }
}
