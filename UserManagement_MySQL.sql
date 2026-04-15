ALTER TABLE `User`
ADD COLUMN `UserType` INT NOT NULL DEFAULT 1 COMMENT '用户类型 0-管理员 1-普通用户' AFTER `IsDel`;

UPDATE `User`
SET `UserType` = 1
WHERE `UserType` IS NULL;

-- 将下面的手机号替换成你的管理员账号手机号，再执行一次
UPDATE `User`
SET `UserType` = 0
WHERE `Phone` = '请替换为管理员手机号';
