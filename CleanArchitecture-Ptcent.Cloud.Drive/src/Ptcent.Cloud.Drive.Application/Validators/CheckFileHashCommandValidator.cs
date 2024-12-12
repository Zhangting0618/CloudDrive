using FluentValidation;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Validators
{
    public class CheckFileHashCommandValidator : AbstractValidator<CheckFileHashRequestDto>
    {
        public CheckFileHashCommandValidator()
        {
            RuleFor(c => c.FileHash).NotEmpty().WithMessage("文件Hash不能为空");
        }
      
    }
}
