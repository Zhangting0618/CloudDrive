namespace Ptcent.Cloud.Drive.Application.Attributes
{
    /// <summary>
    /// 独立提交特性
    /// 标记此特性的实体/操作将立即提交，不受外层事务回滚影响
    /// 使用场景：日志记录、审计跟踪、进度记录等
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public class IndependentSubmitAttribute : Attribute
    {
        /// <summary>
        /// 描述信息
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否在事务失败后仍然提交
        /// </summary>
        public bool CommitOnFailure { get; set; } = true;
    }

    /// <summary>
    /// 标记需要独立提交的实体类型
    /// 当实体被标记此后，对该实体的所有修改都会立即提交
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class IndependentSubmitEntityAttribute : Attribute
    {
        public string? Description { get; set; }
    }
}
