using MediatR;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Infrastructure.IRespository;
using Ptcent.Cloud.Drive.Shared.Extensions;
using Ptcent.Cloud.Drive.Shared.Util;
using System.Transactions;
using Yitter.IdGenerator;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.File
{
    public class MergeFileCommandHandler : IRequestHandler<MergeChunkFileRequestDto, ResponseMessageDto<bool>>
    {
        private readonly IIdGenerator idGenerator;
        private readonly IUserRepository userRepository;
        private readonly IFileRepository fileRepository;
        public MergeFileCommandHandler(IIdGenerator idGenerator, IUserRepository userRepository, IFileRepository fileRepository)
        {
            this.idGenerator = idGenerator;
            this.userRepository = userRepository;
            this.fileRepository = fileRepository;
        }

        public async Task<ResponseMessageDto<bool>> Handle(MergeChunkFileRequestDto request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool>() { IsSuccess = true };
            long fileId = idGenerator.NewLong();
            long versionId = idGenerator.NewLong();
            string mergeFilePath=string.Empty; 
            try
            {
                var exName = Path.GetExtension(request.FileName);
                var fileType = CommonExtension.JudgmentFileType(exName);
                FileEntity fileEntity = new FileEntity();
                fileEntity.Id = fileId;
                fileEntity.VersionId = versionId;
                fileEntity.LeafName = request.FileName;
                fileEntity.FileType = (int)Enum.Parse(typeof(FileTypeEnum), fileType.ToString());
                fileEntity.Extension = exName;
                fileEntity.ParentFolderId = request.ParentFloderId == null ? null : request.ParentFloderId;
                fileEntity.IsFolder = (int)EnumItemType.ItemFile;
                fileEntity.FileSize = request.FileSize;
                //存在 PhysicalDirectory 字段下 如果编辑该文件了 则需要更新 ItemFileMapUrl字段
                if (fileEntity.ParentFolderId == null)
                {
                    //证明是在最顶层创建的
                    fileEntity.Path = $"/{request.FileName}";
                    fileEntity.Idpath = $"/{fileId}";
                }
                else
                {
                    //证明是在某个文件夹下方 则需要查询 该文件的Path IdPath
                    var parentFileEntity = await fileRepository.FirstOrDefaultAsync(a => a.Id == fileEntity.ParentFolderId);
                    fileEntity.Path = parentFileEntity.Path + $"/{request.FileName}";
                    fileEntity.Idpath = parentFileEntity.Idpath + $"/{fileId}";
                }
                fileEntity.ItemHash = request.FileHash;
                string filePath = "SourceFiles" + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy-MM-dd") + Path.DirectorySeparatorChar + fileEntity.Id.ToString() + Path.DirectorySeparatorChar + fileEntity.VersionId.ToString() + Path.DirectorySeparatorChar + fileEntity.Id.ToString() + fileEntity.Extension;
                fileEntity.PhysicalDirectory = filePath;
                //获取 合并目录下面的数据
                 mergeFilePath = Path.Combine(ConfigUtil.GetValue("FileRootPath"), "TempFile",request.FileHash);
                string prefix = request.FileHash;
                string[] files = Directory.GetFiles(mergeFilePath, $"{prefix}*");
                // 按 "_数字" 后缀升序排序
                var sortedFiles = files.OrderBy(file => ExtractNumberFromFileName(file));
               string dirPath = Path.Combine(ConfigUtil.GetValue("FileRootPath"), filePath.Replace(fileEntity.Id.ToString() + fileEntity.Extension,""));
                filePath= Path.Combine(ConfigUtil.GetValue("FileRootPath"), filePath);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                //合并文件
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    foreach (var sortedFile in sortedFiles)
                    {
                        var bytes = await System.IO.File.ReadAllBytesAsync(sortedFile);
                        await fileStream.WriteAsync(bytes, 0, bytes.Length);
                    }
                }

                //同步更新数据
                using (TransactionScope scope=new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                   await fileRepository.SaveFileEntity(fileEntity, await userRepository.UserId(), true);
                    Directory.Delete(mergeFilePath, true);
                    scope.Complete();
                }
                #region 待定
                //await Policy.Handle<IOException>()  待定
                //         .RetryForeverAsync()
                //         .ExecuteAsync(async () =>
                //         {
                //             foreach (FileSort fileSort in mergeFileSorts)
                //             {
                //                 using FileStream fileChunk =
                //                     new FileStream(fileSort.FileName, FileMode.Open,
                //                     FileAccess.Read, FileShare.Read);

                //                 await fileChunk.CopyToAsync(fileStream);
                //             }
                //         }); 
                #endregion
            }
            catch (Exception ex)
            {
               LogUtil.Error("合并文件出错MergeFileCommandHandler:" + ex.Message);
                //删除数据
                await  fileRepository.DeleteBatch(a => a.Id == fileId);
                //删除合并文件夹
                Directory.Delete(mergeFilePath, true);
            }
            return response;
        }
        /// <summary>
        /// 提取数字
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public int ExtractNumberFromFileName(string fileName)
        {
            int underscoreIndex = fileName.LastIndexOf('_');
            if (underscoreIndex != -1 && underscoreIndex < fileName.Length - 1)
            {
                string numberString = fileName.Substring(underscoreIndex + 1);
                if (int.TryParse(numberString, out int number))
                {
                    return number;
                }
            }
            return 0; // 默认返回0，如果提取失败
        }
    }
}
