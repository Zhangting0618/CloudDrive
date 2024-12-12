using MediatR;
using Microsoft.Extensions.Configuration;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Shared.Extensions;
using Ptcent.Cloud.Drive.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.File
{
    public class BlockUpLoadCommandHandler : IRequestHandler<UpLoadFileRequestDto, ResponseMessageDto<bool>>
    {
        private readonly IConfiguration config;
        public BlockUpLoadCommandHandler(IConfiguration config)
        {
            this.config = config;
        }
        /// <summary>
        /// 分片上传
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ResponseMessageDto<bool>> Handle(UpLoadFileRequestDto request, CancellationToken cancellationToken)
        {
           var response=new ResponseMessageDto<bool>() { IsSuccess=true};

            if (request.FormFiles==null||request.FormFiles.Count()==0)
            {
                response.IsSuccess = false;
                response.Data = false;
                response.Message = "文件为空!";
                return response;
            }

            var allowUploadFileType = config["AllowUploadFileType"];
            if (string.IsNullOrEmpty(allowUploadFileType))
            {
                response.IsSuccess = false;
                response.Data = false;
                response.Message = $"allowUploadFileType配置节点不存在";
                return response;
            }
            var allowUploadFileTypes = allowUploadFileType.Split(',').ToList();
            var fileTypes = request.FormFiles.Select(a => Path.GetExtension(a.FileName)).ToList();
            var excludeFileTypes = allowUploadFileTypes.Intersect(fileTypes).ToList(); 
            if (!excludeFileTypes.IsNull())
            {
                string excludeFileType = string.Join(",", excludeFileTypes);
                response.IsSuccess = false;
                response.Data = false;
                response.Message = $"{excludeFileType}类型文件不允许上传";
                return response;
            }
            //获取文件名的
            // 查找下划线的索引
            foreach (var file in request.FormFiles)
            {
                var fileName = Path.GetFileName(file.FileName);
                int underscoreIndex = fileName.IndexOf('-');
                string prefix = underscoreIndex != -1 ? fileName.Substring(0, underscoreIndex) : fileName;
                //创建临时文件夹
                string savePath = Path.Combine(ConfigUtil.GetValue("FileRootPath"), "TempFile", prefix);
                string filePath =savePath + Path.DirectorySeparatorChar + fileName;
                //创建文件夹
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                Stream stream = file.OpenReadStream();
                using (var chunkFileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    // await file.CopyToAsync(chunkFileStream);
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(chunkFileStream);
                }
            }
            response.Message = "上传成功";
            response.Data = true;
            return response;
        }
    }
}
