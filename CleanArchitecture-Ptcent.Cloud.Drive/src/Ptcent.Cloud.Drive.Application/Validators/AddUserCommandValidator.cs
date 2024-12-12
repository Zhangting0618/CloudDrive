using FluentValidation;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Shared.Extensions;
using Ptcent.Cloud.Drive.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Validators
{
    public class AddUserCommandValidator : AbstractValidator<RegistrationAccountRequestDto>
    {
        public AddUserCommandValidator()
        {
            RuleFor(c => c.Phone).NotEmpty().WithMessage("手机号不能为空").Must(CommonExtension.IsMobile).WithMessage("手机号格式不正确");
            RuleFor(c => c.Email).NotEmpty().WithMessage("邮箱不能为空").Must(CommonExtension.IsEmail).WithMessage("邮箱格式不正确");
            RuleFor(c => c.PassWord).NotEmpty().WithMessage("密码不能为空");
        }
    }
}
