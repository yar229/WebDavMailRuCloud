using System;
using System.Collections.Generic;
using System.IO;
using Fsp;
using YaR.MailRuCloud.Api.Base;
using Path = YaR.MailRuCloud.Fs.Path;

namespace YaR.MailRuCloud.Fs
{
    class FileNodeMap
    {
        public FileNodeMap(Boolean caseInsensitive)
        {
            CaseInsensitive = caseInsensitive;
            StringComparer comparer = caseInsensitive ?
                StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            _set = new SortedSet<String>(comparer);
            _map = new Dictionary<String, FileNode>(comparer);
        }
        public UInt32 Count()
        {
            return (UInt32)_map.Count;
        }
        public FileNode Get(String fileName)
        {
            return _map.TryGetValue(fileName, out var fileNode) ? fileNode : null;
        }
        public FileNode GetMain(String fileName)
        {
            int index = fileName.IndexOf(':');
            if (0 > index)
                return null;
            return _map.TryGetValue(fileName.Substring(0, index), out var fileNode) ? fileNode : null;
        }
        public FileNode GetParent(String fileName, ref Int32 result)
        {
            _map.TryGetValue(Path.GetDirectoryName(fileName), out var fileNode);
            if (null == fileNode)
            {
                result = FileSystemBase.STATUS_OBJECT_PATH_NOT_FOUND;
                return null;
            }
            if (0 == (fileNode.FileInfo.FileAttributes & (UInt32)FileAttributes.Directory))
            {
                result = FileSystemBase.STATUS_NOT_A_DIRECTORY;
                return null;
            }
            return fileNode;
        }
        public void TouchParent(FileNode fileNode)
        {
            if ("\\" == fileNode.FileName)
                return;
            Int32 result = FileSystemBase.STATUS_SUCCESS;
            FileNode parent = GetParent(fileNode.FileName, ref result);
            if (null == parent)
                return;
            parent.FileInfo.LastAccessTime =
                parent.FileInfo.LastWriteTime =
                    parent.FileInfo.ChangeTime = (UInt64)DateTime.Now.ToFileTimeUtc();
        }
        public void Insert(FileNode fileNode)
        {
            _set.Add(fileNode.FileName);
            _map.Add(fileNode.FileName, fileNode);
            TouchParent(fileNode);
        }
        public void Remove(FileNode fileNode)
        {
            if (_set.Remove(fileNode.FileName))
            {
                _map.Remove(fileNode.FileName);
                TouchParent(fileNode);
            }
        }
        public Boolean HasChild(FileNode fileNode)
        {
            foreach (String unused in GetChildrenFileNames(fileNode, null))
                return true;
            return false;
        }
        public IEnumerable<String> GetChildrenFileNames(FileNode fileNode, String marker)
        {
            String minName = "\\";
            String maxName = "]";
            if ("\\" != fileNode.FileName)
            {
                minName = fileNode.FileName + "\\";
                maxName = fileNode.FileName + "]";
            }
            if (null != marker)
                minName += marker;
            foreach (String name in _set.GetViewBetween(minName, maxName))
                if (name != minName &&
                    name.Length > maxName.Length && -1 == name.IndexOfAny(Delimiters, maxName.Length))
                    yield return name;
        }
        public IEnumerable<String> GetStreamFileNames(FileNode fileNode)
        {
            String minName = fileNode.FileName + ":";
            String maxName = fileNode.FileName + ";";
            foreach (String name in _set.GetViewBetween(minName, maxName))
                if (name.Length > minName.Length)
                    yield return name;
        }
        public IEnumerable<String> GetDescendantFileNames(FileNode fileNode)
        {
            yield return fileNode.FileName;
            String minName = fileNode.FileName + ":";
            String maxName = fileNode.FileName + ";";
            foreach (String name in _set.GetViewBetween(minName, maxName))
                if (name.Length > minName.Length)
                    yield return name;
            minName = "\\";
            maxName = "]";
            if ("\\" != fileNode.FileName)
            {
                minName = fileNode.FileName + "\\";
                maxName = fileNode.FileName + "]";
            }
            foreach (String name in _set.GetViewBetween(minName, maxName))
                if (name.Length > minName.Length)
                    yield return name;
        }

        private static readonly Char[] Delimiters = { '\\', ':' };
        public readonly Boolean CaseInsensitive;
        private readonly SortedSet<String> _set;
        private readonly Dictionary<String, FileNode> _map;
    }
}