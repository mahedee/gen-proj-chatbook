using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FacebookClone.Models.Data;

namespace FacebookClone.Models.ViewModels.Profile
{
    public class MessageVM
    {
        public MessageVM(MessageDTO row)
        {
            From = row.From;
            To = row.To;
            Message = row.Message;
            DateSent = row.DateSent;
            Read = row.Read;

            FromId = row.FromUsers.Id;
            FromUserName = row.FromUsers.UserName;
            FromFirstName = row.FromUsers.FirstName;
            FromLastName = row.FromUsers.LastName;
        }

        public MessageVM()
        {
            
        }


        public int Id { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public string Message { get; set; }
        public DateTime DateSent { get; set; }
        public bool Read { get; set; }

        public int FromId { get; set; }
        public string FromUserName { get; set; }
        public string FromFirstName { get; set; }
        public string FromLastName { get; set; }
    }
}