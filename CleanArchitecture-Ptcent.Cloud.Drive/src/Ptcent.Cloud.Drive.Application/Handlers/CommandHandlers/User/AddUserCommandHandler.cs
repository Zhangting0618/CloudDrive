using MediatR;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Shared.SecurityUtil;
using Yitter.IdGenerator;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.User
{
    public class AddUserCommandHandler : IRequestHandler<RegistrationAccountRequestDto, ResponseMessageDto<bool>>
    {
        private readonly IUserRepository userRepository;
        private readonly IIdGenerator idGenerator;
        public AddUserCommandHandler(IUserRepository userRepository, IIdGenerator idGenerator)
        {
            this.userRepository = userRepository;
            this.idGenerator = idGenerator;
        }
        public async Task<ResponseMessageDto<bool>> Handle(RegistrationAccountRequestDto request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool>();
            var phoneIsExit = await userRepository.Any(a => a.Phone == request.Phone);
            if (phoneIsExit)
            {
                response.IsSuccess = false;
                response.Message = "手机号不能重复";
                return response;
            }
            var userEntity = new UserEntity();// AutoMapperConfig.Map<RegistrationAccountRequestDto, UserEntity>(request);
            userEntity.Id = idGenerator.NewLong();
            userEntity.UpdateDate = DateTime.Now;
            userEntity.CreateDate = DateTime.Now;
            userEntity.CreateBy = userEntity.Id;
            userEntity.UpdateBy = userEntity.Id;
            userEntity.Sex = 0;
            userEntity.Password = MD5Util.ComputeMD5(request.PassWord, request.Phone);
            userEntity.IsDel = 0;
            userEntity.RegisterTime = DateTime.Now;
            userEntity.Salt = request.Phone;
            userEntity.UserName = request.UserName;
            response.IsSuccess = await userRepository.Add(userEntity);
            response.Message = "注册成功";
            return response;
        }
    }
}
