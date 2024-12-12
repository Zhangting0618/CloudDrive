using FluentValidation;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Validators
{
    public class LoginUserCommandValidator:AbstractValidator<LoginUserRequestDto>
    {
        public LoginUserCommandValidator()
        {
            RuleFor(c=>c.Phone).NotEmpty().WithMessage("手机号不能为空").Must(CommonExtension.IsMobile).WithMessage("手机号格式欲有误");
        }
    }
}
