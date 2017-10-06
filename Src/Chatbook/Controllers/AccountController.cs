using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.MappingViews;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using FacebookClone.Models.Data;
using FacebookClone.Models.ViewModels.Account;
using FacebookClone.Models.ViewModels.Profile;

namespace FacebookClone.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        
        public ActionResult Index()
        {

            string userName = User.Identity.Name;
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("~/" + userName);

            }

            //if (!string.IsNullOrEmpty(userName))
            //{
                
            //}
            return View();
        }

        //POST: Account/CreateAccount
        [HttpPost]
        public ActionResult CreateAccount(UserVM model, HttpPostedFileBase file)
        {
            //Init Db
            Db db = new Db();
            //check model state
            if (!ModelState.IsValid )
            {
                return View("Index",model);
            }

            //Make sure user name is unique
            if (db.Users.Any(v=>v.UserName.Equals(model.UserName)))
            {
                ModelState.AddModelError("","User Name " + model.UserName + " is taken.");
                model.UserName = "";
                return View("Index", model);
            }

            //Create UserDto
            UserDTO userDto = new UserDTO()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailAddress =  model.EmailAddress,
                UserName = model.UserName,
                Password = model.Password
            };

            //Add UserDto
            db.Users.Add(userDto);

            //Save
            db.SaveChanges();

            //Get inserted user id
            var userId = userDto.Id;

            //Login User
            FormsAuthentication.SetAuthCookie(model.UserName,false);

            //Set upload directories
            var uploadsDir = new DirectoryInfo(string.Format("{0}Uploads",Server.MapPath(@"\")));

            //Check file was uploaded
            if (file != null && file.ContentLength > 0)
            {
                //Get extension
                string ext = file.ContentType.ToLower();

                //Verify extension
                if (ext!= "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/png" &&
                    ext != "image/x-png"  )
                {
                    ModelState.AddModelError("","The image was not uploaded - wrong image extension. ");
                    return View("Index", model);
                }
                //Get Image Name
                string imageName = userId + ".jpg";

                //Set image Path
                string path = string.Format("{0}\\{1}", uploadsDir, imageName);

                //Save Image
                file.SaveAs(path);

                //Create wall Dto

               WallDTO wall = new WallDTO();
                wall.Id = userId;
                wall.Message = "";
                wall.DateEdited = DateTime.Now;
                db.Wall.Add(wall);
                db.SaveChanges();

            }
            //redirect
            return Redirect("~/" + model.UserName);

        }
        [Authorize]
        public ActionResult Username(string userName = "")
        {
            //if (User.Identity.Name != userName)
            //{
            //    return Redirect("~/" + User.Identity.Name);
            //}

            ViewBag.Username = userName;
            var user = User.Identity.Name;
            //Init Db
            Db db = new Db();

            //User Full Name
            UserDTO userDto = db.Users.Where(v => v.UserName.Equals(User.Identity.Name)).FirstOrDefault();
            ViewBag.FullName = userDto.FirstName + " " + userDto.LastName;
            ViewBag.UserId = userDto.Id;
            var userId = userDto.Id;

            //Viewbag user walls
            List<int> friendIds1 = db.Friends.Where(v => v.User1 == userId && v.Active)
                .ToArray()
                .Select(x => x.User2)
                .ToList();
            List<int> friendIds2 = db.Friends.Where(v => v.User2 == userId && v.Active)
                .ToArray()
                .Select(x => x.User1)
                .ToList();
            List<int> allFriends = friendIds1.Concat(friendIds2).ToList();

            List<WallVM> walls = db.Wall.Where(v => allFriends.Contains(v.Id))
                .ToArray()
                .OrderByDescending(x => x.DateEdited)
                .Select(t => new WallVM(t))
                .ToList();
            ViewBag.Walls = walls;
            //Viewing User Full Name
            UserDTO userDto2 = db.Users.Where(v => v.UserName.Equals(userName)).FirstOrDefault();
            ViewBag.ViewingFullName = userDto2.FirstName + " " + userDto2.LastName;
            var otherUserId = userDto2.Id;

            //Get Accepted Friend Count
            var FrCount = db.Friends.Count(v => v.User1 == otherUserId && v.Active == true || v.User2 == otherUserId && v.Active == true);
            ViewBag.FCount = FrCount;
            //Find Viewing User image
            ViewBag.UserImage = userDto2.Id + ".jpg";

            //Count Message for the user
            var msgCount = db.Messages.Count(v => v.To == userId && v.Read == false);
            ViewBag.MsgCount = msgCount;
            
            //User type
            string userType = "guest";
            //Check which user
            if (userName.Equals(user))
            {
                 userType = "owner";
            }
            ViewBag.UserType = userType;
            //Check if they are friends
            if (userType == "guest")
            {
                int id1 = userDto.Id;
                int id2 = userDto2.Id;

                FriendDTO f1 = db.Friends.Where(v => v.User1 == id1 && v.User2 == id2).FirstOrDefault();
                FriendDTO f2 = db.Friends.Where(v => v.User1 == id2 && v.User2 == id1).FirstOrDefault();

                if (f1 == null && f2 == null)
                {
                    ViewBag.NotFriends = "True";
                }

                if (f1 != null)
                {
                    if (!f1.Active)
                    {
                        ViewBag.NotFriends = "Pending";
                    }
                }
                if (f2 != null)
                {
                    if (!f2.Active)
                    {
                        ViewBag.NotFriends = "Pending";
                    }
                }
            }

            //Wall message
            WallDTO wallDto = new WallDTO();
            ViewBag.WallMessage = db.Wall.Where(v => v.Id == userId).Select(t => t.Message).FirstOrDefault();


            //Get Friends count
            var friendCount = db.Friends.Count(v => v.User2 == userId && v.Active == false);

            if (friendCount >0)
            {
                ViewBag.FRCount = friendCount;
            }

            return View();
        }
        [Authorize]
        public ActionResult Logout()
        {
            //sign out
            FormsAuthentication.SignOut();

            //Redirect
            return Redirect("~/");
        }

        public ActionResult LoginPartial()
        {
            return PartialView();
        }

        public string Login(string userName , string password)
        {
            Db db = new Db();
            if (db.Users.Any(v=>v.UserName.Equals(userName)) 
                && db.Users.Any(v => v.Password.Equals(password)))
            {
                FormsAuthentication.SetAuthCookie(userName,true);
                return "OK";
            }
            else
            {
                return "Problem";
            }
        }
        [Authorize]
        public ActionResult ShowFriends(string username)
        {
            //Init Db
            Db db = new Db();

            //Get user id
            UserDTO userDto = db.Users.Where(v => v.UserName == username).FirstOrDefault();
            int userId = userDto.Id;

            //Get Friend list
            List<int> friend1 = db.Friends.Where(v => v.User1 == userId && v.Active).Select(t => t.User2).ToList();
            List<int> friend2 = db.Friends.Where(v => v.User2 == userId && v.Active).Select(t => t.User1).ToList();

            //final friend list
            List<int> friends = friend1.Concat(friend2).ToList();

            //Get Friends userDTO object
           List<UserDTO> userDtoList = db.Users.Where(v => friends.Contains(v.Id)).ToList();
            return View(userDtoList);
        }
        
    }
}