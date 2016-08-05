//using System.Transactions;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;
using VideoGameHash.Models;

namespace VideoGameHash.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {

        private UserRepository repository = new UserRepository();
        
        //
        // GET: /Users/
        public ActionResult Index()
        {
            return View(repository.GetAllUserProfile());
        }

        //
        // GET: Users/Edit
        public ActionResult Edit(int Id)
        {
            UserMasterModel userMaster = new UserMasterModel();
            UserProfile user = repository.GetUserByUserId(Id);

            userMaster._UserProfile = user;

            userMaster._UserMembership = repository.GetMembershipByUserId(user.Id);

            userMaster._UsersRoles = repository.GetUserRolesByUserName(user.UserName);

            return View(userMaster);
        }

        //
        // GET: Users/AddRole
        public ActionResult AddRole(int Id)
        {
            RolesModel rolesModel = new Models.RolesModel();
            List<string> rolesList = repository.GetAllRoles();
            List<string> userRoles = repository.GetUserRolesByUserName(repository.GetUserNameByUserId(Id));
            List<string> rolesAvailable = new List<string>();

            foreach (string role in rolesList)
            {
                if (!userRoles.Contains(role))
                    rolesAvailable.Add(role);
            }

            ViewData["AvailableRoles"] = new SelectList(rolesAvailable, rolesModel.RoleName);
            ViewBag.UserId = Id;

            return View(rolesModel);
        }

        //
        // POST: Users/AddRole
        [HttpPost]
        public ActionResult AddRole(int Id, FormCollection collection)
        {
            string value = collection.Get("AvailableRoles");

            repository.AddRole(Id, value);

            return RedirectToAction("Edit", new { Id = Id });
        }

    }
}
