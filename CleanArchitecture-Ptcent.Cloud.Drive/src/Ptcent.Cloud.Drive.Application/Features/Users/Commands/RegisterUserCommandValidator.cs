using FluentValidation;

namespace Ptcent.Cloud.Drive.Application.Features.Users.Commands
{
    /// <summary>
    /// 用户注册命令验证器
    /// </summary>
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("用户名不能为空")
                .Length(2, 50).WithMessage("用户名长度必须在 2-50 个字符之间");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("手机号不能为空")
                .Matches(@"^1[3-9]\d{9}$").WithMessage("手机号格式不正确");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("密码不能为空")
                .MinimumLength(6).WithMessage("密码长度不能少于 6 位");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("邮箱不能为空")
                .EmailAddress().WithMessage("邮箱格式不正确");
        }
    }
}
