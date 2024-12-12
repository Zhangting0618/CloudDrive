using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Infrastructure.IRespository;
using Ptcent.Cloud.Drive.Shared.Extensions;
using Ptcent.Cloud.Drive.Shared.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yitter.IdGenerator;

namespace Ptcent.Cloud.Drive.Application.Handlers.QueryHandlers.File
{
    public class DownLoadFileQueryHandler : IRequestHandler<DownLoadFileRequestDto, ResponseMessageDto<(string filePath, string fileName)>>
    {
        private readonly IFileRepository fileRepository;
        private readonly IConfiguration config;
        private readonly IIdGenerator idGenerator;
        public DownLoadFileQueryHandler(IFileRepository fileRepository, IConfiguration config, IIdGenerator idGenerator)
        {
            this.fileRepository = fileRepository;
            this.config = config;
            this.idGenerator = idGenerator;
        }
        /// <summary>
        /// 下载文件夹/文件
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ResponseMessageDto<(string filePath, string fileName)>> Handle(DownLoadFileRequestDto request, CancellationToken cancellationToken)
        {
           var response=new ResponseMessageDto<(string filePath, string fileName)>() { IsSuccess=true};
            bool flag = false;
            string filePath = string.Empty;
            string zipPath = Path.Combine(config["FileRootPath"], "TempZipFile");
            zipPath = zipPath + Path.DirectorySeparatorChar + idGenerator.NewLong() + ".zip";
            string path = Path.Combine(config["FileRootPath"], "TempCopyFile");
            //临时复制文件地址
            string tempCopyFile = path + Path.DirectorySeparatorChar + idGenerator.NewLong();
            //处理下载逻辑 只返回路径即可
            var fileEntitys = (await fileRepository.WhereAsync(a => request.FileIds.Contains(a.Id))).ToList();
            if (request.FileIds.Count() == 1)//表明是单个文件或者文件夹
            {
                var fileEntity= fileEntitys.FirstOrDefault(a => a.Id == request.FileIds[0]);
                if (fileEntity == null)
                {
                    response.IsSuccess = false;
                    response.Data = (string.Empty,string.Empty);
                    response.Message = $"文件{request.FileIds[0]}不存在";
                    return response;
                }
                if (fileEntity.IsFolder==(int)EnumItemType.ItemFolder)
                {
                    //表示是文件夹 
                    flag= await CopyTempItem(tempCopyFile, fileEntity.Id, fileEntity.Idpath, new List<FileEntity>(), true);
                   var zipFileName = $"archive-{DateTime.Now:yyyy_MM_dd-HH_mm_ss.fff}.zip";
                    if (flag)
                    {
                        bool isSuccess = FileZipUtil.ZipFileDictory(tempCopyFile, zipPath);
                        if (isSuccess)
                        {
                            response.Data = (filePath, zipFileName);
                            return response;
                        }
                    }
                    else
                    {
                        response.IsSuccess= false;
                        response.Message = "文件夹压缩失败";
                        response.Data = (string.Empty,string.Empty) ;
                        return response;
                    }
                }
                if (fileEntity.IsFolder == (int)EnumItemType.ItemFile)
                {
                    //查询源文件是否存在
                    filePath = Path.Combine(config["FileRootPath"],fileEntity.ItemFileMapUrl.IsNullOrWhiteSpace()?fileEntity.PhysicalDirectory: fileEntity.ItemFileMapUrl);
                    //判断文件是否存在
                    if (!System.IO.File.Exists(filePath))
                    {
                        response.IsSuccess = false;
                        response.Message = "源文件不存在!";
                        LogUtil.Error($"DownLoadFile:源文件{filePath}不存在,参数为:"+request.ToJson());
                        return response;
                    }
                    response.Data = (filePath, fileEntity.LeafName);
                    return response;
                } 
            }
            else
            {
                //证明是多个文件夹 则需要多次传入
                foreach (var fileId in request.FileIds)
                {
                    var fileEntity = fileEntitys.FirstOrDefault(a => a.Id == fileId);
                    flag = await CopyTempItem(tempCopyFile, fileId, fileEntity.Idpath, new List<FileEntity>(), true);
                }
                var zipFileName = $"archive-{DateTime.Now:yyyy_MM_dd-HH_mm_ss.fff}.zip";
                if (flag)
                {
                    bool isSuccess = FileZipUtil.ZipFileDictory(tempCopyFile, zipPath);
                    if (isSuccess)
                    {
                        response.Data = (filePath, zipFileName);
                        return response;
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "文件夹压缩失败";
                    response.Data = (string.Empty, string.Empty);
                    return response;
                }
            }
            return response;
        }
        /// <summary>
        /// 复制临时文件
        /// </summary>
        /// <param name="tempCopyItemPath"></param>
        /// <param name="fileId"></param>
        /// <param name="idPath"></param>
        /// <param name="fileEntitys"></param>
        /// <param name="itemFileFlag"></param>
        /// <returns></returns>
        public async Task<bool> CopyTempItem(string tempCopyItemPath, long fileId, string idPath, List<FileEntity> fileEntitys, bool itemFileFlag = true)
        {
            if (itemFileFlag)
            {
                var fileInfo = await fileRepository.GetById(fileId);
                fileEntitys = (await fileRepository.WhereAsync(a => EF.Functions.Like(a.Idpath, $"{idPath}%") && a.IsDel == (int)FileStatsType.NoDel)).Select(a => new FileEntity
                {
                    Id = a.Id,
                    Idpath = a.Idpath,
                    IsFolder = a.IsFolder,
                    ParentFolderId = a.ParentFolderId,
                    LeafName = a.LeafName
                }).ToList();
                if (fileInfo.IsFolder == (int)EnumItemType.ItemFile)
                {
                    if (!Directory.Exists(tempCopyItemPath))
                    {
                        Directory.CreateDirectory(tempCopyItemPath);
                    }
                    tempCopyItemPath = tempCopyItemPath + Path.DirectorySeparatorChar + fileInfo.LeafName;
                    if (!System.IO.File.Exists(tempCopyItemPath))
                    {
                        //string itemFilePath = Path.Combine(FileRootPath, itemInfo.SourcePath);
                        var filePath = Path.Combine(config["FileRootPath"],fileInfo.ItemFileMapUrl.IsNullOrWhiteSpace()?fileInfo.PhysicalDirectory:fileInfo.ItemFileMapUrl);
                        if (System.IO.File.Exists(filePath))
                        {
                            // System.IO.File.Copy(filePath, tempCopyItemPath, true);
                            await CopyFileAsync(filePath, tempCopyItemPath);
                            return true;
                        }
                    }
                }
                if (fileInfo.IsFolder == (int)EnumItemType.ItemFolder)
                {
                    tempCopyItemPath = tempCopyItemPath + Path.DirectorySeparatorChar + fileInfo.LeafName;
                    if (!Directory.Exists(tempCopyItemPath))
                    {
                        Directory.CreateDirectory(tempCopyItemPath);
                    }
                }
            }
            var parentItemEntitys = fileEntitys.Where(a => a.ParentFolderId == fileId).ToList();

            foreach (var fileEntity in parentItemEntitys)
            {
                if (fileEntity.IsFolder == (int)EnumItemType.ItemFolder)
                {
                    string tempPath = tempCopyItemPath;
                    var folderPath = tempPath + Path.DirectorySeparatorChar + fileEntity.LeafName;
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    await CopyTempItem(folderPath, fileEntity.Id, fileEntity.Idpath, fileEntitys, false);
                }
                else
                {
                    string tempPath = tempCopyItemPath;
                    if (!Directory.Exists(tempPath))
                    {
                        Directory.CreateDirectory(tempPath);
                    }
                    var fileSourcePath = Path.Combine(config["FileRootPath"], fileEntity.ItemFileMapUrl.IsNullOrWhiteSpace() ? fileEntity.PhysicalDirectory : fileEntity.ItemFileMapUrl);
                    if (System.IO.File.Exists(fileSourcePath))
                    {
                        var filePath = tempPath + "/" + fileEntity.LeafName;
                       // System.IO.File.Copy(fileSourcePath, filePath, true);
                      await  CopyFileAsync(fileSourcePath, filePath);
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// 异步复制文件
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationFilePath"></param>
        /// <returns></returns>
        public async Task CopyFileAsync(string sourceFilePath, string destinationFilePath)
        {
            using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            {
                using (var destinationStream = new FileStream(destinationFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }
            }
        }
    }
}
