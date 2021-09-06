using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace daBoot.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        public string Password { get; set; }
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public string ProfilePicURL { get; set; }
        public virtual ICollection<Relation> TeamMembers { get; set; }
        public virtual ICollection<Relation> OthersTeamMember { get; set; }



        /* Username + password constructor*/
        public Account(string username, string password, string firstName, string lastName, string emailAddress, string profpic)
        {
            Username = username;
            Password = password;
            FirstName = firstName;
            LastName = lastName;
            EmailAddress = emailAddress;
            if (profpic is null || profpic == "")
            {
                profpic = "https://github.com/wxo15/daBoot/blob/main/default-avatar.png?raw=true";
            }
            ProfilePicURL = profpic;
        }

        /* OAuth constructor*/
        public Account(string username, string firstName, string lastName, string emailAddress, string profpic)
        {
            Username = username;
            FirstName = firstName;
            LastName = lastName;
            EmailAddress = emailAddress;
            if (profpic is null || profpic == "")
            {
                profpic = "https://github.com/wxo15/daBoot/blob/main/default-avatar.png?raw=true";
            }
            ProfilePicURL = profpic;
        }

        //Empty constructor
        public Account()
        {
        }
    }

    public class Relation
    {
        public int UserId { get; set; }
        public int TeamMemberId { get; set; }

        public virtual Account User { get; set; }
        public virtual Account TeamMember { get; set; }
    }
}
