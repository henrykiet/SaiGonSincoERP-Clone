using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportSystem01.Shared
{
    public class UserGroupDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserGroupId { get; set; }

        [Required(ErrorMessage = "Group Name bắt buộc nhập")]
        [MaxLength(100, ErrorMessage = "Độ dài vượt quá quy định")]
        public string GroupName { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        [Required(ErrorMessage = "Danh sách User bắt buộc nhập")]
        [MaxLength(1024, ErrorMessage = "Độ dài List User vượt quá quy định")]
        public string ListUser {  get; set; }
        public string TreeViewPermissions { get; set; } = string.Empty;
    }
}
