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
    public class DownLoadFileCommandValidator : AbstractValidator<DownLoadFileRequestDto>
    {
        public DownLoadFileCommandValidator()
        {
            RuleFor(c => c.FileIds).NotEmpty().WithMessage("下载文件Id不能为空!");
        }
    }
}
