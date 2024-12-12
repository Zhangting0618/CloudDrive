using MediatR;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Domain.Constants;
using Ptcent.Cloud.Drive.Infrastructure.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.User
{
    public class UserOutCommandHandler : IRequestHandler<UserQueryRequestDto, ResponseMessageDto<bool>>
    {
        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ResponseMessageDto<bool>> Handle(UserQueryRequestDto request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool>() { IsSuccess = true };
            string cacheKey = CacheKey.Ptcent_Cloud_Drive_WebApi_User_Login_Status + request.UserId;
            RedisClient.Remove(cacheKey);
            response.Message = "退出成功";
            return response;
        }
    }
}
