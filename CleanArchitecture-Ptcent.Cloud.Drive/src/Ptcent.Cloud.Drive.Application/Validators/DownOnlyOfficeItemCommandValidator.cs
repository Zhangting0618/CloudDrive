using FluentValidation;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Validators
{
    public class DownOnlyOfficeItemCommandValidator:AbstractValidator<DownOnlyOfficeItemRequestDto>
    {
        public DownOnlyOfficeItemCommandValidator()
        {
            RuleFor(c => c.FileId).NotEmpty().WithMessage("文件Id不能为空").Must(value=>value!=0).WithMessage("YourLongProperty should not be equal to 0");
            
        }
    }
}
