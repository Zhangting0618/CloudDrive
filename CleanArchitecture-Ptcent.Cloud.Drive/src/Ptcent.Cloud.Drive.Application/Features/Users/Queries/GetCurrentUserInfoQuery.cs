using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Users.Queries
{
    /// <summary>
    /// 获取当前用户信息查询
    /// </summary>
    public record GetCurrentUserInfoQuery() : IRequest<ResponseMessageDto<CurrentUserDto>>;

    /// <summary>
    /// 当前用户信息 DTO
    /// </summary>
    public class CurrentUserDto
    {
        public long Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int? Sex { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? RegisterTime { get; set; }
    }
}
