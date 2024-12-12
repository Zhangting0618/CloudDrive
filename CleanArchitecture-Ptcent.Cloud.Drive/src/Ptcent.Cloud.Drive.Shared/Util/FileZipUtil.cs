using ICSharpCode.SharpZipLib.Zip;
using SharpCompress.Common;
using System.IO.Compression;
using System.Text;
using Ptcent.Cloud.Drive.Shared.Util;
using SharpCompress.Archives;
namespace Ptcent.Cloud.Drive.Shared.Util
{
    /// <summary>
    /// 压缩文件扩展方法
    /// </summary>
    public class FileZipUtil
    {
        /// <summary>
        /// 100MB写入一次
        /// </summary>
        public static int avg = 1024 * 1024 * 100;
        /// <summary>
        /// 压缩项
        /// </summary>
        public static event Action<int> ItemCompressed;
        /// <summary>
        /// 每一项的个数
        /// </summary>
        public static int itemCount = 0;
        /// <summary>
        /// 压缩目录
        /// </summary>
        /// <param name="filePath">文件所在目录</param>
        /// <param name="saveZipPath">压缩包目录</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static bool ZipFileDictory(string filePath, string saveZipPath)
        {
            bool flag = true;
            if (!Directory.Exists(filePath))
            {
                return false;
            }
            if (string.IsNullOrEmpty(saveZipPath))
            {
                //如果压缩后的文件路径为空 则文件名称为 文件名 + .zip
                saveZipPath += ".zip";
            }
            //实例化一个zip文件流
            LogUtil.Info("实例化一个zip文件流");
            ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(saveZipPath));
            LogUtil.Error("实例化一个zip文件流错误");
            zipOutputStream.SetLevel(6); //设置压缩级别为6级
                                         //调用方法 递归进行压缩
            flag = ZipFileDictory(filePath, zipOutputStream, "");
            zipOutputStream.Finish();
            zipOutputStream.Close();
            return flag;
        }


        /// <summary>
        /// 递归压缩文件夹方法
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="s">zip文件流</param>
        /// <param name="ParentFolderName">父类文件名称</param>
        /// <returns></returns>
        public static bool ZipFileDictory(string filePath, ZipOutputStream s, string ParentFolderName)
        {
            bool flag = true;
            string[] folders, filenames;
            ZipEntry entry = null; //压缩后的文件目录
            FileStream fs = null;  //文件数据流
                                   //Crc32 crc = new Crc32();
            try
            {
                LogUtil.Info("开始压缩");
                //创建当前文件夹
                entry = new ZipEntry(Path.Combine(ParentFolderName, Path.GetFileName(filePath) + "/"));  //加上 “/” 才会当成是文件夹创建
                LogUtil.Info("ParentFolderName:" + ParentFolderName);
                LogUtil.Info("压缩文件目录:" + Path.Combine(ParentFolderName, Path.GetFileName(filePath) + "/"));
                s.PutNextEntry(entry);
                s.Flush();
                //先压缩文件，再递归压缩文件夹
                filenames = Directory.GetFiles(filePath);
                foreach (string file in filenames)
                {
                    itemCount++;
                    ItemCompressed?.Invoke(itemCount);
                    //打开压缩文件
                    fs = File.OpenRead(file);
                    byte[] buffer = new byte[avg];
                    LogUtil.Info("打开压缩文件:" + Path.Combine(ParentFolderName, Path.GetFileName(filePath) + "/" + Path.GetFileName(file)));
                    entry = new ZipEntry(Path.Combine(ParentFolderName, Path.GetFileName(filePath) + "/" + Path.GetFileName(file)))
                    {
                        DateTime = DateTime.Now,
                        Size = fs.Length
                    };
                    s.PutNextEntry(entry);
                    for (int i = 0; i < fs.Length; i += avg)
                    {
                        if (i + avg > fs.Length)
                        {
                            //不足100MB的部分写剩余部分
                            buffer = new byte[fs.Length - i];
                        }
                        fs.Read(buffer, 0, buffer.Length);
                        s.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.Error("压缩文件失败:" + ex.Message);
                flag = false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
                GC.Collect();
                GC.Collect(1);
            }

            folders = Directory.GetDirectories(filePath);
            foreach (string folder in folders)
            {
                if (!ZipFileDictory(folder, s, Path.Combine(ParentFolderName, Path.GetFileName(filePath))))
                    return false;
            }
            return flag;
        }

        /// <summary>
        /// 解压文件夹
        /// </summary>
        /// <param name="TargetFile">要解压的文件路径</param>
        /// <param name="fileDir">解压后的文件夹路径</param>
        /// <returns></returns>
        public static string unZipFileOld(string TargetFile, string fileDir)
        {
            string rootFile = " ";
            try
            {
                FileStream fileStream = new FileStream(TargetFile, FileMode.Open);
                LogUtil.Info("TargetFile" + TargetFile);
                //读取压缩文件(zip文件),准备解压缩
                ZipInputStream s = new ZipInputStream(fileStream);
                LogUtil.Info("读取压缩文件");
                ZipEntry theEntry;
                string path = fileDir;
                //解压出来的文件保存的路径

                string rootDir = " ";
                //根目录下的第一个子文件夹的名称
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    rootDir = Path.GetDirectoryName(theEntry.Name);
                    LogUtil.Info("压缩文件" + rootDir);
                    //得到根目录下的第一级子文件夹的名称
                    if (rootDir.IndexOf("/") >= 0)
                    {
                        rootDir = rootDir.Substring(0, rootDir.IndexOf("/") + 1);
                    }
                    string dir = Path.GetDirectoryName(theEntry.Name);
                    //根目录下的第一级子文件夹的下的文件夹的名称
                    string fileName = Path.GetFileName(theEntry.Name);
                    LogUtil.Info("压缩文件名称" + fileName);
                    //根目录下的文件名称
                    if (dir != " ")
                    //创建根目录下的子文件夹,不限制级别
                    {
                        if (!Directory.Exists(fileDir + "/" + dir))
                        {
                            path = fileDir + "/" + dir;
                            //在指定的路径创建文件夹
                            Directory.CreateDirectory(path);
                        }
                    }
                    else if (dir == " " && fileName != "")
                    //根目录下的文件
                    {
                        path = fileDir;
                        rootFile = fileName;
                    }
                    else if (dir != " " && fileName != "")
                    //根目录下的第一级子文件夹下的文件
                    {
                        if (dir.IndexOf("/") > 0)
                        //指定文件保存的路径
                        {
                            path = fileDir + "/" + dir;
                        }
                    }

                    if (dir == rootDir)
                    //判断是不是需要保存在根目录下的文件
                    {
                        path = fileDir + "/" + rootDir;
                    }

                    //以下为解压缩zip文件的基本步骤
                    //基本思路就是遍历压缩文件里的所有文件,创建一个相同的文件。
                    if (fileName != String.Empty)
                    {
                        FileStream streamWriter = File.Create(path + "/" + fileName);
                        LogUtil.Info("压缩文件创建路径" + path);
                        int size = 2048;
                        byte[] data = new byte[1024 * 1024 * 5];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }

                        streamWriter.Close();
                    }
                }
                s.Close();

                return rootFile;
            }
            catch (Exception ex)
            {
                return "1; " + ex.Message;
            }
        }



        /// <summary>
        /// 解压文件夹
        /// </summary>
        /// <param name="TargetFile">要解压的文件路径</param>
        /// <param name="fileDir">解压后的文件夹路径</param>
        /// <returns></returns>
        public static bool unZipFile(string TargetFile, string fileDir)
        {
            bool flag = false;
            try
            {
                if (!Directory.Exists(fileDir))
                {
                    Directory.CreateDirectory(fileDir);
                }
                using (var reader = SharpCompress.Archives.Zip.ZipArchive.Open(TargetFile))
                {
                    foreach (var entry in reader.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            entry.WriteToDirectory(fileDir, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }
                //获取该文件下面的文件名称
                var list = GetDirectories(fileDir, new List<string>());
                if (list.Count() == 0)
                {
                    //说明正常什么也不干
                }
                else
                {
                    //编码格式不正确
                    //删除目录下的所有文件和文件夹  重新导入
                    Directory.Delete(fileDir, true);
                    if (!Directory.Exists(fileDir))
                    {
                        Directory.CreateDirectory(fileDir);
                    }
                    SharpCompress.Readers.ReaderOptions options = new SharpCompress.Readers.ReaderOptions();
                    options.ArchiveEncoding.Default = Encoding.GetEncoding("GBK");
                    using (var reader = SharpCompress.Archives.Zip.ZipArchive.Open(TargetFile, options))
                    {
                        foreach (var entry in reader.Entries)
                        {
                            if (!entry.IsDirectory)
                            {
                                entry.WriteToDirectory(fileDir, new ExtractionOptions()
                                {
                                    ExtractFullPath = true,
                                    Overwrite = true
                                });
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogUtil.Error("解压文件夹失败\t" + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 获取目录下面的所有文件名称
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        /// <summary>
        /// 获取目录下面的所有文件名称
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static List<string> GetDirectories(string folderPath, List<string> fileNames)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            FileSystemInfo[] filesArray = directoryInfo.GetFileSystemInfos();
            foreach (var item in filesArray)
            {
                if (item.Attributes == FileAttributes.Directory)
                {
                    if (item.Name.Contains("�"))
                    {
                        fileNames.Add(item.Name);
                        break;
                    }
                    GetDirectories(item.FullName, fileNames);
                }
                else
                {
                    if (item.Name.Contains("�"))
                    {
                        fileNames.Add(item.Name);
                        break;
                    }
                }
            }
            return fileNames;
        }
    }
}

/// <summary>
/// 解压Rar文件扩展方法
/// </summary>
public class FileRarUtil
{
    /// <summary>
    /// 解压RAR文件
    /// </summary>
    /// <param name="rarFilePath">解压文件源路径</param>
    /// <param name="extractPath">解压文件目标路径</param>
    public static bool ExtractRAR(string rarFilePath, string extractPath)
    {
        bool flag = false;
        try
        {
            if (!Directory.Exists(extractPath))
            {
                Directory.CreateDirectory(extractPath);
            }
            SharpCompress.Readers.ReaderOptions options = new SharpCompress.Readers.ReaderOptions();
            options.ArchiveEncoding.Default = Encoding.GetEncoding("GBK");
            IArchive archive = ArchiveFactory.Open(rarFilePath, options);
            foreach (IArchiveEntry archiveEntry in archive.Entries)
            {
                if (!archiveEntry.IsDirectory)
                {
                    archiveEntry.WriteToDirectory(extractPath, new ExtractionOptions
                    {
                        ExtractFullPath = true,
                        Overwrite = true,
                    });
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            flag = false;
            LogUtil.Error("ExtractRAR:" + $"解压失败路径为{rarFilePath}" + ex.Message);
            return flag;
        }
    }
}
