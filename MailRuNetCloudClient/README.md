# Mail.Ru-.net-cloud-client
.Net Client for cloud.mail.ru

Main functionality with implementations:
- Create folder
  <br/>CreateFolder(string folderName, string destinationPath) - return bool result of the operation
- Copy file
  <br/>Copy(File fileObject, Entry destinationEntryObject)  - return bool result of the operation
  <br/>Copy(File fileObject, Folder destinationFolderObject) - return bool result of the operation
  <br/>Copy(File fileObject, string destinationPath) - return bool result of the operation
- Copy folder
  <br/>Copy(Folder folderObject, Entry destinationEntryObject) - return bool result of the operation
  <br/>Copy(Folder folderObject, Folder destinationFolderObject) - return bool result of the operation
  <br/>Copy(Folder folderObject, string destinationPath) - return bool result of the operation
- Download file
  <br/>GetFile(File fileObject, [bool includeProgressEvent = True]) - return byte array of the file
  <br/>GetFile(File fileObject, string destinationPathOnCopmuter, [bool includeProgressEvent = True]) - return bool result of the operation and save file in destination path on computer
- Upload file
  <br/>UploadFile(FileInfo file, string destinationPath) - return bool result of the operation
- Get list of the files and folders
  <br/>GetItems(Folder folderObject) - return Entry object with files and folders on the server (include list of the File and Folder object)
  <br/>GetItems(string pathOnServer) - return Entry object with files and folders on the server (include list of the File and Folder object)
- Get public file link (not support for large files)
  <br/>GetPublishLink(File fileObject) - return public file URL as string
- Get public folder link
  <br/>GetPublishLink(Folder folderObject) - return public folder URL as string
- Get direct file link (operation on one session)
  <br/>GetPublishDirectLink(string publicFileLink, FileType fileType) - return direct file URL as string
- Move file
  <br/>Move(File fileObject, Entry destinationEntryObject)  - return bool result of the operation
  <br/>Move(File fileObject, Folder destinationFolderObject) - return bool result of the operation
  <br/>Move(File fileObject, string destinationPath) - return bool result of the operation
- Move folder
  <br/>Move(Folder folderObject, Entry destinationEntryObject) - return bool result of the operation
  <br/>Move(Folder folderObject, Folder destinationFolderObject) - return bool result of the operation
  <br/>Move(Folder folderObject, string destinationPath) - return bool result of the operation
- Rename file
  <br/>Rename(File fileObject, string newFileName) - return bool result of the operation
- Rename folder
  <br/>Rename(Folder folderObject, string newFolderName) - return bool result of the operation
- Remove file
  <br/>Remove(File fileObject) - return bool result of the operation
- Remove folder
  <br/>Remove(Folder folderObject) - return bool result of the operation
- Disable public file link
  <br/>UnpublishLink(File fileObject) - return bool result of the operation
- Disable public folder link
  <br/>UnpublishLink(Folder folderObject) - return bool result of the operation
- Cancel all async threads
  <br/>AbortAllAsyncThreads() - return nothing, just remove all async operations to cloud

All operations supports async calls.
Upload and Download operations supports progress change event and can work with large file more than 2Gb.

Mail.ru cloud paths start with symbol "/", e.g:
- Root directory just start with "/"
- Folder in root directory as "/New folder"
- File in root directory as "/NewFileName.txt"

--------------------------------------------------
              VERY BETA VERSION
--------------------------------------------------

Distributed under the MIT license
