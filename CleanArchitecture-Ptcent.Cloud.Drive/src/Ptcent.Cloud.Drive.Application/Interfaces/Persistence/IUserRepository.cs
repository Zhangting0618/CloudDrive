using Ptcent.Cloud.Drive.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Interfaces.Persistence
{
    public interface IUserRepository : IBaseRepository<UserEntity>
    {
        /// <summary>
        /// 获取用户Id
        /// </summary>
        /// <returns></returns>
        Task<long> UserId();
        /// <summary>
        /// 获取用户手机号
        /// </summary>
        /// <returns></returns>
        Task<string> Phone();
        /// <summary>
        /// 获取用户名
        /// </summary>
        /// <returns></returns>
        Task<string> UserName();
        /// <summary>
        /// 获取用户邮箱
        /// </summary>
        /// <returns></returns>
        Task<string> UserMail();
    }
}
