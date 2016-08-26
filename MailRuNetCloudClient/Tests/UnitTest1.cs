using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MailRuCloudApi;
using System.IO;
using System.ComponentModel;
using System.Threading;
using System;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        //private const string Login = "asdf@asdf.ru";
        //private const string Password = "asdfsvba";
        //private Account account = new Account(Login, Password);

        //[TestMethod]
        //public void A1LoginTest()
        //{
        //    account.Login();
        //    Assert.IsNotNull(account.AuthToken);
        //}

        //[TestMethod]
        //public void GetItemsTest()
        //{
        //    var api = new MailRuCloud();
        //    api.Account = this.account;
        //    var items = api.GetItems("/Camera Uploads/");
        //    Assert.IsNotNull(items);
        //    Assert.IsTrue(items.Files.Count == items.NumberOfFiles || items.Folders.Count == items.NumberOfFolders);

        //    var percent = 0;
        //    api.ChangingProgressEvent += delegate(object sender, ProgressChangedEventArgs e)
        //    {
        //        percent = e.ProgressPercentage;
        //    };

        //    var fileToDownload = items.Files.First(t => t.Size.DefaultValue <= 1 * 1024 * 1024);
        //    var task = api.GetFileAsync(fileToDownload);
        //    Assert.IsNotNull(task.Result);
        //    Assert.IsTrue(percent == 100);

        //    percent = 0;
        //    var task2 = api.GetFileAsync(fileToDownload, @"C:\Development\MailRuCloudApi\");
        //    Assert.IsTrue(task2.Result);
        //    Assert.IsTrue(percent == 100);

        //    var fileInfo = new FileInfo(@"C:\Development\MailRuCloudApi\" + fileToDownload.Name);
        //    Assert.IsTrue(fileInfo.Exists, "File is not created.");
        //    Assert.IsTrue(fileInfo.Length > 0, "File size in not retrieved.");
        //}

        //[TestMethod]
        //public void UploadFileTest()
        //{
        //    var api = new MailRuCloud();
        //    api.Account = this.account;

        //    var percent = 0;
        //    api.ChangingProgressEvent += delegate(object sender, ProgressChangedEventArgs e)
        //    {
        //        percent = e.ProgressPercentage;
        //    };

        //    var task = api.UploadFileAsync(new FileInfo(@"C:\Development\MailRuCloudApi\1.txt"), "/");
        //    Assert.IsTrue(task.Result);
        //    Assert.IsTrue(percent == 100);
        //    Thread.Sleep(5000);
        //}

        //[TestMethod]
        //public void GetPublishDirectLinkTest()
        //{
        //    var api = new MailRuCloud();
        //    api.Account = this.account;

        //    var items = api.GetItems("/Camera Uploads");
        //    var fileToDownload = items.Files.First(t => t.Size.DefaultValue <= 1 * 1024 * 1024);
        //    var publicFileLink = api.GetPublishLink(fileToDownload);

        //    Assert.IsTrue(!string.IsNullOrEmpty(publicFileLink));

        //    var directLink = api.GetPublishDirectLink(publicFileLink);

        //    Assert.IsTrue(!string.IsNullOrEmpty(directLink));

        //    var unpublishFile = api.UnpublishLink(fileToDownload);
        //    Assert.IsTrue(unpublishFile);
        //}

        //[TestMethod]
        //public void PublicUnpublishLink()
        //{
        //    var api = new MailRuCloud();
        //    api.Account = this.account;

        //    var items = api.GetItems("/Camera Uploads");
        //    var fileToDownload = items.Files.First(t => t.Size.DefaultValue <= 1 * 1024 * 1024);
        //    var publicFileLink = api.GetPublishLink(fileToDownload);

        //    var folder = new Folder(
        //        0, 
        //        0, 
        //        "Camera Uploads", 
        //        new FileSize()
        //        {
        //            DefaultValue = 0
        //        }, 
        //        "/Camera Uploads");
        //    var publishFolderLink = api.GetPublishLink(folder);

        //    Assert.IsTrue(!string.IsNullOrEmpty(publicFileLink));
        //    Assert.IsTrue(!string.IsNullOrEmpty(publishFolderLink));

        //    var unpublishFile = api.UnpublishLink(fileToDownload);
        //    var unpublishFolder = api.UnpublishLink(folder);

        //    Assert.IsTrue(unpublishFile);
        //    Assert.IsTrue(unpublishFolder);
        //}

        ////[TestMethod]
        ////public void GetShardTest()
        ////{
        ////    var objToTestPrivateMethod = new PrivateObject(typeof(MailRuCloud));
        ////    objToTestPrivateMethod.SetFieldOrProperty("Account", this.account);
        ////    var result = objToTestPrivateMethod.Invoke("GetShardInfo", ShardType.Get);

        ////    Assert.IsNotNull(result);
        ////    Assert.IsTrue(!string.IsNullOrEmpty((result as ShardInfo).Url));
        ////}

        //[TestMethod]
        //public void RemoveFileFolderTest()
        //{
        //    var api = new MailRuCloud();
        //    api.Account = this.account;

        //    var result = api.UploadFileAsync(new FileInfo(@"C:\Development\MailRuCloudApi\1.txt"), "/");
        //    var file = api.GetItems("/").Files.First(x => x.Name == "1.txt");
        //    api.Remove(file);

        //    api.CreateFolder("new test folder", "/");
        //    var folder = api.GetItems("/").Folders.First(x => x.Name == "new test folder");
        //    api.Remove(folder);
        //}

        //[TestMethod]
        //public void MoveFileFolderTest()
        //{
        //    var api = new MailRuCloud();
        //    api.Account = this.account;

        //    var result = api.UploadFileAsync(new FileInfo(@"D:\1.stl"), "/");
        //    if (result.Result)
        //    {
        //        var file = api.GetItems("/").Files.First(x => x.Name == "1.stl");
        //        api.Move(file, "/Misuc");

        //        api.CreateFolder("new test folder", "/");
        //        var folder = api.GetItems("/").Folders.First(x => x.Name == "new test folder");
        //        api.Move(folder, "/Misuc");

        //        var entry = api.GetItems("/Misuc");

        //        Assert.IsNotNull(entry.Folders.FirstOrDefault(x => x.Name == "new test folder"));
        //        Assert.IsNotNull(entry.Files.FirstOrDefault(x => x.Name == "1.stl"));
        //    }
        //}

        //[TestMethod]
        //public void CopyFileFolderTest()
        //{
        //    var api = new MailRuCloud();
        //    api.Account = this.account;

        //    var result = api.UploadFileAsync(new FileInfo(@"D:\1.stl"), "/");
        //    if (result.Result)
        //    {
        //        var file = api.GetItems("/").Files.First(x => x.Name == "1.stl");
        //        api.Copy(file, "/Misuc");

        //        api.CreateFolder("new test folder", "/");
        //        var folder = api.GetItems("/").Folders.First(x => x.Name == "new test folder");
        //        api.Copy(folder, "/Misuc");

        //        var entry = api.GetItems("/Misuc");

        //        Assert.IsNotNull(entry.Folders.FirstOrDefault(x => x.Name == folder.Name));
        //        Assert.IsNotNull(entry.Files.FirstOrDefault(x => x.Name == file.Name));
        //    }
        //}

        //[TestMethod]
        //public void RenameTest()
        //{
        //    var api = new MailRuCloud();
        //    api.Account = this.account;

        //    var result = api.UploadFileAsync(new FileInfo(@"D:\1.stl"), "/");
        //    if (result.Result)
        //    {
        //        var file = api.GetItems("/").Files.First(x => x.Name == "1.stl");
        //        api.Rename(file, "rename stl test.stl");

        //        api.CreateFolder("new test folder", "/");
        //        var folder = api.GetItems("/").Folders.First(x => x.Name == "new test folder");
        //        api.Rename(folder, "rename folder test");

        //        var entry = api.GetItems("/");

        //        Assert.IsNotNull(entry.Folders.FirstOrDefault(x => x.Name == "rename folder test"));
        //        Assert.IsNotNull(entry.Files.FirstOrDefault(x => x.Name == "rename stl test.stl"));
        //    }
        //}
    }
}
