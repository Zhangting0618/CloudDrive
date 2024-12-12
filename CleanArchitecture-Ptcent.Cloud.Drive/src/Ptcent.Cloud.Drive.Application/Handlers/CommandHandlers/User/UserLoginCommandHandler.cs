using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Ptcent.Cloud.Drive.Application.Dto.Common;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Domain.Constants;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Infrastructure.Cache;
using Ptcent.Cloud.Drive.Infrastructure.IRespository;
using Ptcent.Cloud.Drive.Shared.SecurityUtil;
using Ptcent.Cloud.Drive.Shared.Util;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.User
{
    public class UserLoginCommandHandler : IRequestHandler<LoginUserRequestDto, ResponseMessageDto<string>>
    {
        private readonly IUserRepository userRepository;
        private readonly IConfiguration config;
        public UserLoginCommandHandler(IUserRepository userRepository, IConfiguration config)
        {
            this.userRepository = userRepository;
            this.config = config;
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ResponseMessageDto<string>> Handle(LoginUserRequestDto request, CancellationToken cancellationToken)
        {
            var respone = new ResponseMessageDto<string>() { IsSuccess = true };
            var userEntity = (await userRepository.WhereAsync(a => a.Phone == request.Phone)).FirstOrDefault();
            if (userEntity != null)
            {
                //查询用户状态
                if (userEntity.IsDel == 1)
                {
                    respone.IsSuccess = false;
                    respone.Message = "用户已被禁用";
                    return respone;
                }
                string pwd = MD5Util.ComputeMD5(request.PassWord, request.Phone);
                if (!string.Equals(userEntity.Password, pwd, StringComparison.InvariantCultureIgnoreCase))
                {
                    respone.IsSuccess = false;
                    respone.Message = "密码错误!";
                    return respone;
                }
                var loginInfoResult = CreateToken(userEntity);
                if (loginInfoResult.IsSuccess)
                {
                    //将信息记录在Redis
                    respone.Data = loginInfoResult.Data;
                    RedisClient.Insert(CacheKey.Ptcent_Cloud_Drive_WebApi_User_Login_Status + userEntity.Id, UserLoginStatus.Login.GetHashCode().ToString(), new TimeSpan(Convert.ToInt32(config["UserLoingExpires"] ?? "30"), 0, 0, 0));
                }
                else
                {
                    respone.IsSuccess = false;
                    respone.Message = "登录失败";
                    return respone;
                }
            }

            if (userEntity == null)
            {
                respone.IsSuccess = false;
                respone.Message = "用户不存在!";
                return respone;
            }
            return respone;
        }
        public ResponseMessageDto<string> CreateToken(UserEntity userEntity)
        {
            var response = new ResponseMessageDto<string>();
            try
            {

                var loginUserDto = new LoginUserDto();
                loginUserDto.UserId = userEntity.Id;
                loginUserDto.Phone = userEntity.Phone;
                loginUserDto.UserName = userEntity.UserName;
                loginUserDto.UserMail = userEntity.Email;
                var claims = new List<Claim>();
                claims.Add(new Claim("jwt", JsonConvert.SerializeObject(loginUserDto)));
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Authentication:SecretKey"]));
                var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(Convert.ToDouble(config["UserLoingExpires"] ?? "15")),
                    signingCredentials: signingCredentials);
                var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
                response.IsSuccess = true;
                response.Data = tokenStr;
                return response;
            }
            catch (Exception ex)
            {
                LogUtil.Error("Token生成错误!" + ex.Message);
                response.IsSuccess = false;
                response.Message = ex.Message;
                return response;
            }
        }
    }
}
