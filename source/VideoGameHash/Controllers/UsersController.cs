//using System.Transactions;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {

        private readonly UserRepository _repository;

        public UsersController(UserRepository repository)
        {
            _repository = repository;
        }

        //
        // GET: /Users/
        public ActionResult Index()
        {
            return View(_repository.GetAllUserProfile());
        }

        //
        // GET: Users/Edit
        public ActionResult Edit(int id)
        {
            var userMaster = new UserMasterModel();
            var user = _repository.GetUserByUserId(id);

            userMaster.UserProfile = user;

            userMaster.UserMembership = _repository.GetMembershipByUserId(user.Id);

            userMaster.UsersRoles = _repository.GetUserRolesByUserName(user.UserName);

            return View(userMaster);
        }

        //
        // GET: Users/AddRole
        public ActionResult AddRole(int id)
        {
            var rolesModel = new Models.RolesModel();
            var rolesList = _repository.GetAllRoles();
            var userRoles = _repository.GetUserRolesByUserName(_repository.GetUserNameByUserId(id));
            var rolesAvailable = new List<string>();

            foreach (var role in rolesList)
            {
                if (!userRoles.Contains(role))
                    rolesAvailable.Add(role);
            }

            ViewData["AvailableRoles"] = new SelectList(rolesAvailable, rolesModel.RoleName);
            ViewBag.UserId = id;

            return View(rolesModel);
        }

        //
        // POST: Users/AddRole
        [HttpPost]
        public ActionResult AddRole(int id, FormCollection collection)
        {
            var value = collection.Get("AvailableRoles");

            _repository.AddRole(id, value);

            return RedirectToAction("Edit", new { Id = id });
        }

    }
}
