using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FacebookClone.Models.Data;
using FacebookClone.Models.ViewModels.Profile;

namespace FacebookClone.Controllers
{
    public class ProfileController : Controller
    {
      
        // GET: Profile
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JsonResult LiveSearch(string searchVal)
        {
            //Init DB
            Db db = new Db();

           List<LiveSearchUserVM> userNames =  db.Users
                .Where(v => v.UserName.Contains(searchVal) && v.UserName != User.Identity.Name)
                .ToArray()
                .Select(x => new LiveSearchUserVM(x))
                .ToList();
            return Json(userNames);
        }

        [HttpPost]
        public void AddFriend(string friend)
        {
            //Init db
            Db  db = new Db();

            //Get user id
            UserDTO userDto = db.Users.Where(v => v.UserName.Equals(User.Identity.Name)).FirstOrDefault();
            int userId = userDto.Id;

            //Get friend id
            UserDTO userDto2 = db.Users.Where(v => v.UserName.Equals(friend)).FirstOrDefault();
            int friendId = userDto2.Id;

            //Add FriendDTO
            FriendDTO friendDTO = new FriendDTO();
            friendDTO.User1 = userId;
            friendDTO.User2 = friendId;
            friendDTO.Active = false;

            db.Friends.Add(friendDTO);
            db.SaveChanges();
        }

        public JsonResult DisplayFriendRequest()
        {
            //Init db
            Db db = new Db();

            //Get userId
            var userId = db.Users.Where(v => v.UserName == User.Identity.Name).FirstOrDefault().Id;
            //Get Pending Friend Requests
            List<FriendRequestVM> list = db.Friends
                .Where(v => v.User2 == userId && v.Active == false)
                .ToArray()
                .Select(t=> new FriendRequestVM(t))
                .ToList();

            List<UserDTO> users = new List<UserDTO>();

            foreach (var item in list)
            {
               UserDTO user = db.Users.Where(v => v.Id == item.User1).FirstOrDefault();
               users.Add(user);
            }

            return Json(users);
        }


        [HttpPost]
        public void AcceptFriendRequest(int friendId)
        {
            //Init Db
            Db db = new Db();

            //user id get
            var userDTO = db.Users.Where(v => v.UserName.Equals(User.Identity.Name)).FirstOrDefault();
            var userId = userDTO.Id;

            //Accept friend request
            FriendDTO friendDto = db.Friends.Where(v => v.User1 == friendId && v.User2 == userId).FirstOrDefault();

            friendDto.Active = true;

            db.SaveChanges();
        }

        [HttpPost]
        public void DeclineFriendRequest(int friendId)
        {
            //Init Db
            Db db = new Db();

            //user id get
            var userDTO = db.Users.Where(v => v.UserName.Equals(User.Identity.Name)).FirstOrDefault();
            var userId = userDTO.Id;

            //Accept friend request
            FriendDTO friendDto = db.Friends.Where(v => v.User1 == friendId && v.User2 == userId).FirstOrDefault();

            db.Friends.Remove(friendDto);

            db.SaveChanges();
        }

        // POST: Profile/SendMessage
        [HttpPost]
        public void SendMessage(string friend, string message)
        {
            // Init db
            Db db = new Db();

            // Get user id
            UserDTO userDTO = db.Users.Where(x => x.UserName.Equals(User.Identity.Name)).FirstOrDefault();
            int userId = userDTO.Id;

            // Get friend id
            UserDTO userDTO2 = db.Users.Where(x => x.UserName.Equals(friend)).FirstOrDefault();
            int userId2 = userDTO2.Id;

            // Save message

            MessageDTO dto = new MessageDTO();

            dto.From = userId;
            dto.To = userId2;
            dto.Message = message;
            dto.DateSent = DateTime.Now;
            dto.Read = false;

            db.Messages.Add(dto);
            db.SaveChanges();
        }

        [HttpPost]
        public ActionResult DisplayUnreadMessages()
        {
            //Init db
            Db db = new Db();

            //Get userDto and id
            UserDTO userDTO = db.Users.Where(v => v.UserName.Equals(User.Identity.Name)).FirstOrDefault();
            var userId = userDTO.Id;

            //Init message view model
            List<MessageVM> msgVMList = new List<MessageVM>();

            //Get the messagelist of the user
            msgVMList = db.Messages.Where(v => v.To == userId && v.Read == false)
                .ToArray()
                .Select(x => new MessageVM(x))
                .ToList();

            //Set Read == false
            db.Messages.Where(v=>v.Id==userId && v.Read == false).ToList().ForEach(x=>x.Read =true);
            db.SaveChanges();
            

            return Json(msgVMList);

        }
        [Route("Profile/Demo")]
        [Authorize(Roles = "admin")]
        public ActionResult Demo()
        {
            return View();
        }

        [HttpPost]
        public void UpdateWallMessage(int id, string message)
        {
            Db db = new Db();

            WallDTO wall = db.Wall.Find(id);
            wall.Message = message;
            wall.DateEdited = DateTime.Now;
            db.SaveChanges();
        }
    }
}