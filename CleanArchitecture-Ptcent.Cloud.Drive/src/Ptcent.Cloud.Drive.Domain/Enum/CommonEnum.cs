using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Domain.Enum
{
    /// <summary>
    /// WebApi返回错误码
    /// </summary>
    public enum WebApiResultCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 100001,
        /// <summary>
        /// 系统异常
        /// </summary>
        SystemError = 100002,
        /// <summary>
        ///未登录
        /// </summary>
        NoLogin = 100003,

        /// <summary>
        /// 强制修改密码
        /// </summary>
        ForceModifyPassword = 100008,
    }
    /// <summary>
    /// 请求类型
    /// </summary>
    public enum Source
    {
        /// <summary>
        /// pc 请求
        /// </summary>
        PC = 1,
        /// <summary>
        /// 苹果（APP）
        /// </summary>
        IPhone = 2,
        /// <summary>
        /// 安卓（APP）
        /// </summary>
        Andriod = 3,
        /// <summary>
        /// 微信小程序（小程序）
        /// </summary>
        WeChatMiniProgram = 4,
        /// <summary>
        /// TW4
        /// </summary>
        TW4 = 5,
        /// <summary>
        /// TW5
        /// </summary>
        TW5 = 6,
        /// <summary>
        /// 国药jde演示
        /// </summary>
        JDETest = 7
    }
    public enum CacheOutDateType
    {
        /// <summary>
        /// 绝对过期
        /// </summary>
        Absolute = 1,

        /// <summary>
        /// 相对过期
        /// </summary>
        RelativeTime = 2
    }
    /// <summary>
    /// 登录
    /// </summary>
    public enum UserLoginStatus
    {
        /// <summary>
        /// 登录
        /// </summary>
        Login = 1,

        /// <summary>
        /// 退出
        /// </summary>
        LoginOut = 2
    } /// <summary>
      /// 用户状态标致
      /// </summary>
    public enum UserStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        Normal = 1,

        /// <summary>
        /// 离职
        /// </summary>
        Quit = 2
    }
    /// <summary>
    /// 用户类型
    /// </summary>
    public enum UserType
    {
        /// <summary>
        /// 管理员
        /// </summary>

        Administrators = 0,

        /// <summary>
        /// 普通用户
        /// </summary>
        OrdinaryUsers = 1
    }
    /// <summary>
    /// 文件类型
    /// </summary>
    public enum FileTypeEnum
    {
        /// <summary>
        /// 全部
        /// </summary>
        All,
        /// <summary>
        /// 图片
        /// </summary>
        Image,
        /// <summary>
        /// 文档
        /// </summary>
        Docs,
        /// <summary>
        /// 视频
        /// </summary>
        Video,
        /// <summary>
        /// 音频
        /// </summary>
        Audio,
        /// <summary>
        /// 其他
        /// </summary>
        Other,
        /// <summary>
        /// 回收
        /// </summary>
        Recycle,
        /// <summary>
        /// 分享
        /// </summary>
        Share,
        /// <summary>
        /// 文件夹
        /// </summary>
        Folder
    }
    /// <summary>
    /// 文件夹 、文件
    /// </summary>
    public enum EnumItemType
    {
        /// <summary>
        /// 表示是文件夹
        /// </summary>
        ItemFolder = 1,
        /// <summary>
        /// 表示是文件
        /// </summary>
        ItemFile = 0
    }
    /// <summary>
    /// 文件状态
    /// </summary>
    public enum FileStatsType
    {
        /// <summary>
        /// 未删除
        /// </summary>
        NoDel = 0,
        /// <summary>
        /// 删除
        /// </summary>
        Del = 1,
    }
}
