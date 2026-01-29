using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;

namespace Ptcent.Cloud.Drive.Web.Controllers
{
    /// <summary>
    /// Item模块
    /// </summary>
    public class ItemController : BaseController
    {
        private readonly IConfiguration config;
        private readonly IMediator mediator;
        private readonly IHttpContextAccessor httpContextAccessor;
        /// <summary>
        /// 注入
        /// </summary>
        /// <param name="config"></param>
        /// <param name="mediator"></param>
        /// <param name="httpContextAccessor"></param>
        public ItemController(IConfiguration config, IMediator mediator, IHttpContextAccessor httpContextAccessor) : base(config)
        {
            this.config = config;
            this.mediator = mediator;
            this.httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="createFolderRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseMessageDto<bool>> CreateFolder(CreateFolderRequestDto createFolderRequest)
        {
            return await mediator.Send(createFolderRequest);
        }
        /// <summary>
        /// 检查文件Hash
        /// </summary>
        /// <param name="fileHash"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ResponseMessageDto<bool>> CheckFileHash(string fileHash) =>  await mediator.Send(new CheckFileHashRequestDto { FileHash=fileHash});
        /// <summary>
        /// 分块上传
        /// </summary>
        /// <param name="upLoadFileRequestDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseMessageDto<bool>> BlockUpload()
        {
            UpLoadFileRequestDto upLoadFileRequestDto = new UpLoadFileRequestDto { FormFiles = HttpContext.Request.Form.Files };
            return await mediator.Send(upLoadFileRequestDto);
        }
        /// <summary>
        /// 合并
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseMessageDto<bool>> MergeChunkFile(MergeChunkFileRequestDto mergeChunkFileRequestDto) => await mediator.Send(mergeChunkFileRequestDto);
        /// <summary>
        /// 查询文件列表
        /// </summary>
        /// <param name="fileRequestDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseMessageDto<List<ItemResponseDto>>> GetFiles(FileRequestDto fileRequestDto) => await mediator.Send(fileRequestDto);
        /// <summary>
        /// 文件详情
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseMessageDto<FileDetailsResponseDto>> GetFileDetails(long fileId) => await mediator.Send(new FileDetailsRequestDto { FileId = fileId });
    }
}
