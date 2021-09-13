using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace daBoot.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public virtual ICollection<UserProject> TeamMembers { get; set; }

        public Project(string projectname)
        {
            Name = projectname;
        }
        public Project()
        {
        }

    }

    public class Role
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string RoleName { get; set; }
        public virtual ICollection<UserProject> UserProject { get; set; }
    }

    public class UserProject
    {
        [Required]
        public int UserId { get; set; }
        [Required] 
        public int ProjectId { get; set; }
        [Required] 
        public int RoleId { get; set; }
        [Required] 
        public virtual Account User { get; set; }
        [Required] 
        public virtual Project Project { get; set; }
        [Required] 
        public virtual Role Role { get; set; }

        public UserProject(int userid, int projectid, int roleid)
        {
            UserId = userid;
            ProjectId = projectid;
            RoleId = roleid;
        }
        public UserProject()
        {
        }
    }
}
