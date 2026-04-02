namespace Ptcent.Cloud.Drive.Application.Attributes
{
    /// <summary>
    /// 要求新事务特性
    /// 标记此特性的方法将在一个新的事务中执行，独立于外层事务
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public class RequireNewTransactionAttribute : Attribute
    {
        /// <summary>
        /// 新事务的隔离级别
        /// </summary>
        public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;

        /// <summary>
        /// 描述信息
        /// </summary>
        public string? Description { get; set; }
    }
}
