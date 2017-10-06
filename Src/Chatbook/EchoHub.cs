using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using FacebookClone.Models.Data;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;

namespace FacebookClone
{
    [HubName("echo")]
    public class EchoHub : Hub
    {
        public void Hello(string message)
        {
            Trace.WriteLine(message);

            Clients.All.test("This is a test");
        }

        public void Notify(string friend)
        {
            //Init DB
            Db db = new Db();

            //Get friend's id
            UserDTO userDTO = db.Users.Where(v => v.UserName == friend).FirstOrDefault();
            int friendId = userDTO.Id;

            //Get friend count
            var frCount = db.Friends.Count(v => v.User2 == friendId && v.Active == false);

            //Set Clients
            var clients = Clients.Others;

            //call js function
            clients.frnotify(friend, frCount);

        }

        public void GetFrcount()
        {
            //Init Db
            Db db = new Db();

            //Get UserId
            UserDTO userDto = db.Users.Where(v => v.UserName.Equals(Context.User.Identity.Name)).FirstOrDefault();
            var userId = userDto.Id;

            //Get Friend Count
            var FrCount = db.Friends.Count(v => v.User2 == userId && v.Active == false);

            //Set Clients
            var clients = Clients.Caller;

            //Call js function
            clients.frcount(Context.User.Identity.Name, FrCount);
        }

        public void GetFcount(int friendId)
        {
            //Init Db
            Db db = new Db();

            //Get UserId
            UserDTO userDto = db.Users.Where(v => v.UserName.Equals(Context.User.Identity.Name)).FirstOrDefault();
            var userId = userDto.Id;

            //Get user1 Friend Count
            var FrCount1 = db.Friends.Count(v => v.User2 == userId && v.Active == true ||
                                                 v.User1 == userId && v.Active == true);

            //Get user2 username
            UserDTO userDto2 = db.Users.Where(v => v.Id == friendId).FirstOrDefault();
            var userName2 = userDto2.UserName;


            //get user2 friend count
            var FrCount2 = db.Friends.Count(v => v.User2 == friendId && v.Active == true ||
                                                 v.User1 == friendId && v.Active == true);
            //Updatechat
            UpdateChat();

            //Set Clients
            var clients = Clients.All;

            //Call js function
            clients.fcount(Context.User.Identity.Name, userName2, FrCount1, FrCount2);
        }

        public void NotifyOfMessage(string friend)
        {
             //Init Db
             Db db = new Db();

            //Get friend object
            var userDTO = db.Users.Where(v => v.UserName.Equals(friend)).FirstOrDefault();
            var friendId= userDTO.Id;

             var messageCount = db.Messages.Count(v => v.To == friendId && v.Read == false);

            var clients = Clients.Others;
            clients.msgCount(friend, messageCount);
        }

        public void NotifyOfMessage()
        {
            //Init Db
            Db db = new Db();

            //Get friend object
            var userDTO = db.Users.Where(v => v.UserName.Equals(Context.User.Identity.Name)).FirstOrDefault();
            var userId = userDTO.Id;

            var messageCount = db.Messages.Count(v => v.To == userId && v.Read == false);

            var clients = Clients.Others;
            clients.msgCount(Context.User.Identity.Name, messageCount);
        }

        public override Task OnConnected()
        {
           //Init Db
           Db db = new Db();
            //GetUser Id
           var userDTO = db.Users.Where(v => v.UserName.Equals(Context.User.Identity.Name)).FirstOrDefault();
            int userId = userDTO.Id;
            //Get Conn Id
            string connId = Context.ConnectionId;
            //Add OnlineDTO
            if (!db.Online.Any(v => v.Id == userId))
            {
                OnlineDTO onlineDTO = new OnlineDTO();
                onlineDTO.Id = userId;
                onlineDTO.ConnId = connId;
                db.Online.Add(onlineDTO);
                db.SaveChanges();
            }

            //Get all Online Ids
           var onlineIds = db.Online.ToArray().Select(v => v.Id).ToList();
            //Get friend Ids
            List<int> friendIds1 = db.Friends.Where(v => v.User1 == userId && v.Active).Select(t => t.User2).ToList();
            List<int> friendIds2 = db.Friends.Where(v => v.User2 == userId && v.Active).Select(t => t.User1).ToList();
            List<int> allfriends = friendIds1.Concat(friendIds2).ToList();
            //Get final set of Ids
            List<int> onlinefriends = onlineIds.Where(v => allfriends.Contains(v)).ToList();
            //Create dict o friendIds and usernames
            Dictionary<int,string> dictfriends = new Dictionary<int, string>();

            foreach (var id in onlinefriends)
            {
              var user =   db.Users.Find(id);
                string friend = user.UserName;
                if (!dictfriends.ContainsKey(id))
                {
                    dictfriends.Add(id,friend);
                }
            }

            var transformed = from key in dictfriends.Keys
                select new {id = key, friend = dictfriends[key]};
            var json = JsonConvert.SerializeObject(transformed);
            //Set Clients
            var clients = Clients.Caller;
            //Call js function
            clients.getonlinefriends(Context.User.Identity.Name,json);

            //Update chat
            UpdateChat();
            return  base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            // Log
            Trace.WriteLine("gone - " + Context.ConnectionId + " " + Context.User.Identity.Name);

            // Init db
            Db db = new Db();

            // Get user id
            UserDTO userDTO = db.Users.Where(x => x.UserName.Equals(Context.User.Identity.Name)).FirstOrDefault();
            int userId = userDTO.Id;

            // Remove from db
            if (db.Online.Any(x => x.Id == userId))
            {
                OnlineDTO online = db.Online.Find(userId);
                db.Online.Remove(online);
                db.SaveChanges();
            }

            // Update chat
            UpdateChat();

            // Return
            return base.OnDisconnected(stopCalled);
        }

        public void UpdateChat()
        {
            // Init db
            Db db = new Db();

            // Get all online ids
            List<int> onlineIds = db.Online.ToArray().Select(x => x.Id).ToList();

            // Loop thru onlineids and get friends
            foreach (var userId in onlineIds)
            {
                // Get username
                UserDTO user = db.Users.Find(userId);
                string username = user.UserName;

                // Get all friend ids

                List<int> friendIds1 = db.Friends.Where(x => x.User1 == userId && x.Active == true).ToArray().Select(x => x.User2).ToList();

                List<int> friendIds2 = db.Friends.Where(x => x.User2 == userId && x.Active == true).ToArray().Select(x => x.User1).ToList();

                List<int> allFriendsIds = friendIds1.Concat(friendIds2).ToList();

                // Get final set of ids
                List<int> resultList = onlineIds.Where((i) => allFriendsIds.Contains(i)).ToList();

                // Create a dict of friend ids and usernames

                Dictionary<int, string> dictFriends = new Dictionary<int, string>();

                foreach (var id in resultList)
                {
                    var users = db.Users.Find(id);
                    string friend = users.UserName;

                    if (!dictFriends.ContainsKey(id))
                    {
                        dictFriends.Add(id, friend);
                    }
                }

                var transformed = from key in dictFriends.Keys
                    select new { id = key, friend = dictFriends[key] };

                string json = JsonConvert.SerializeObject(transformed);

                // Set clients
                var clients = Clients.All;

                // Call js function
                clients.updatechat(username, json);
            }

        }

        public void SendChat(int friendId, string friendUsername, string message)
        {
            //Init Db
            Db db = new Db();

            // Get user id
            UserDTO userDTO = db.Users.Where(x => x.UserName.Equals(Context.User.Identity.Name)).FirstOrDefault();
            int userId = userDTO.Id;

            //set clients
            var clients = Clients.All;

            //call js function
            clients.sendchat(userId, Context.User.Identity.Name, friendId, friendUsername, message);
        }
    }
}