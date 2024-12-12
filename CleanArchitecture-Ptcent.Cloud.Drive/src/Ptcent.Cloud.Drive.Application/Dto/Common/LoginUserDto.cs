using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Dto.Common
{
    /// <summary>
    ///登录用户信息
    /// </summary>
    //[ProtoContract]
    public class LoginUserDto
    {
        /// <summary>
        /// 用户id
        /// </summary>
        //[ProtoMember(1)]
        public long UserId { get; set; }
        /// <summary>
        /// 登录名
        /// </summary>
        //[ProtoMember(2)]
        public string Phone { get; set; }
        /// <summary>
        /// 员工姓名
        /// </summary>
        //[ProtoMember(3)]
        public string UserName { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        //[ProtoMember(4)]
        public string? UserMail { get; set; }
        public DateTime TokenCreateTime { get; set; }
    }
}
