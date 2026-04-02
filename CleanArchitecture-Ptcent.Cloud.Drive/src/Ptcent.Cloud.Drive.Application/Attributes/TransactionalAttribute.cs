namespace Ptcent.Cloud.Drive.Application.Attributes
{
    /// <summary>
    /// 事务特性标记
    /// 用于标记需要事务管理的 Command/Query
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class TransactionalAttribute : Attribute
    {
        /// <summary>
        /// 事务隔离级别
        /// </summary>
        public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;

        /// <summary>
        /// 事务超时时间（秒），默认 30 秒
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// 是否启用事务
        /// </summary>
        public bool Enabled { get; set; } = true;
    }

    /// <summary>
    /// 事务隔离级别
    /// </summary>
    public enum IsolationLevel
    {
        /// <summary>
        /// 使用数据库默认的隔离级别
        /// </summary>
        Unspecified,

        /// <summary>
        /// 允许读取未提交的数据（可能脏读）
        /// </summary>
        ReadUncommitted,

        /// <summary>
        /// 只能读取已提交的数据（防止脏读）
        /// </summary>
        ReadCommitted,

        /// <summary>
        /// 在读取的数据上保持共享锁（防止脏读和不可重复读）
        /// </summary>
        RepeatableRead,

        /// <summary>
        /// 在读取的数据上保持范围锁（防止脏读、不可重复读和幻读）
        /// </summary>
        Serializable,

        /// <summary>
        /// 使用快照隔离，读取数据的版本
        /// </summary>
        Snapshot
    }
}
