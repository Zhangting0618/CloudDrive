using MediatR;
using Microsoft.Extensions.Logging;
using Ptcent.Cloud.Drive.Application.Attributes;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Files.Commands;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Domain.Enum;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.File
{
    /// <summary>
    /// 添加收藏命令处理器
    /// </summary>
    [Transactional]
    public class AddToCollectionCommandHandler : IRequestHandler<AddToCollectionCommand, ResponseMessageDto<bool>>
    {
        private readonly ICollectionRepository _collectionRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AddToCollectionCommandHandler> _logger;

        public AddToCollectionCommandHandler(
            ICollectionRepository collectionRepository,
            IFileRepository fileRepository,
            IUserRepository userRepository,
            ILogger<AddToCollectionCommandHandler> logger)
        {
            _collectionRepository = collectionRepository;
            _fileRepository = fileRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ResponseMessageDto<bool>> Handle(AddToCollectionCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool> { IsSuccess = true };

            try
            {
                var userId = await _userRepository.UserId();

                // 检查文件是否存在
                var file = await _fileRepository.GetByIdAsync(request.FileId, cancellationToken);
                if (file == null || file.IsDel == (int)FileStatsType.Del)
                {
                    response.IsSuccess = false;
                    response.Message = "文件不存在";
                    return response;
                }

                // 检查是否已收藏
                if (await _collectionRepository.ExistsAsync(userId, request.FileId))
                {
                    response.IsSuccess = false;
                    response.Message = "已收藏该文件";
                    return response;
                }

                // 添加收藏
                var collection = new CollectionEntity
                {
                    UserId = userId,
                    FileId = request.FileId,
                    CreateTime = DateTime.Now
                };

                await _collectionRepository.AddAsync(collection, cancellationToken);

                response.Message = "收藏成功";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加收藏失败：FileId={FileId}, Error={Error}", request.FileId, ex.Message);

                response.IsSuccess = false;
                response.Message = $"添加收藏失败：{ex.Message}";
                return response;
            }
        }
    }

    /// <summary>
    /// 取消收藏命令处理器
    /// </summary>
    public class RemoveFromCollectionCommandHandler : IRequestHandler<RemoveFromCollectionCommand, ResponseMessageDto<bool>>
    {
        private readonly ICollectionRepository _collectionRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<RemoveFromCollectionCommandHandler> _logger;

        public RemoveFromCollectionCommandHandler(
            ICollectionRepository collectionRepository,
            IUserRepository userRepository,
            ILogger<RemoveFromCollectionCommandHandler> logger)
        {
            _collectionRepository = collectionRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ResponseMessageDto<bool>> Handle(RemoveFromCollectionCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool> { IsSuccess = true };

            try
            {
                var userId = await _userRepository.UserId();

                // 查找收藏记录
                var collection = await _collectionRepository.FirstOrDefaultAsync(
                    c => c.UserId == userId && c.FileId == request.FileId,
                    cancellationToken
                );

                if (collection == null)
                {
                    response.IsSuccess = false;
                    response.Message = "未找到收藏记录";
                    return response;
                }

                // 删除收藏
                await _collectionRepository.DeleteAsync(collection, cancellationToken);

                response.Message = "取消收藏成功";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消收藏失败：FileId={FileId}, Error={Error}", request.FileId, ex.Message);

                response.IsSuccess = false;
                response.Message = $"取消收藏失败：{ex.Message}";
                return response;
            }
        }
    }

    /// <summary>
    /// 获取收藏列表查询处理器
    /// </summary>
    public class GetCollectionsQueryHandler : IRequestHandler<GetCollectionsQuery, ResponseMessageDto<List<CollectionItemDto>>>
    {
        private readonly ICollectionRepository _collectionRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IUserRepository _userRepository;

        public GetCollectionsQueryHandler(
            ICollectionRepository collectionRepository,
            IFileRepository fileRepository,
            IUserRepository userRepository)
        {
            _collectionRepository = collectionRepository;
            _fileRepository = fileRepository;
            _userRepository = userRepository;
        }

        public async Task<ResponseMessageDto<List<CollectionItemDto>>> Handle(GetCollectionsQuery request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<List<CollectionItemDto>> { IsSuccess = true };

            try
            {
                var userId = await _userRepository.UserId();

                // 获取收藏列表
                var collections = await _collectionRepository.GetUserCollectionsAsync(userId, request.PageIndex, request.PageSize);

                var result = new List<CollectionItemDto>();

                foreach (var collection in collections)
                {
                    var file = await _fileRepository.GetByIdAsync(collection.FileId);

                    result.Add(new CollectionItemDto
                    {
                        CollectionId = collection.Id,
                        FileId = collection.FileId,
                        FileName = file?.LeafName ?? "已删除文件",
                        IsFolder = file?.IsFolder == 1,
                        FileSize = file?.FileSize,
                        Extension = file?.Extension,
                        CreatedDate = file?.CreatedDate,
                        CollectionTime = collection.CreateTime
                    });
                }

                response.Data = result;
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"获取收藏列表失败：{ex.Message}";
                return response;
            }
        }
    }
}
