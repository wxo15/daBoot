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

        /* Username + password constructor*/
        public Account(string username, string password, string firstName, string lastName, string emailAddress)
        {
            Username = username;
            Password = password;
            FirstName = firstName;
            LastName = lastName;
            EmailAddress = emailAddress;
        }

        /* OAuth constructor*/
        public Account(string username, string firstName, string lastName, string emailAddress)
        {
            Username = username;
            FirstName = firstName;
            LastName = lastName;
            EmailAddress = emailAddress;
        }
    }
}
