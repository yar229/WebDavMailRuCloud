## **WebDAV emulator for Mail.ru Cloud**<br>
[download latest release binaries](https://github.com/yar229/WebDavMailRuCloud/releases/latest) <a href="https://ci.appveyor.com/project/yar229/webdavmailrucloud-k21bq/branch/master"><img src="https://ci.appveyor.com/api/projects/status/3gejunv39gqed3tp/branch/master?svg=true" align="right"></a>

**Requirements:**
* Microsoft .NET Framework 4.5

**Usage:**
``` 	
	-p, --port        Required. WebDAV server port
	-l, --login       Required. Login to Mail.ru Cloud
	-s, --password    Required. Password to Mail.ru Cloud
	--maxthreads      (Default: 5) Maximum concurrent connections to cloud.mail.ru
	--user-agent      "browser" user-agent
	--help            Display this help screen.
	--version         Display version information.
```
Use as Windows disk: <br>
``` 
	net use <disk>: http://127.0.0.1:<port>
``` 	

Windows 7 client might perform very bad when connecting to any WebDAV server. This is caused, because it tries to auto-detect any proxy server before any request. Refer to KB2445570 for more information.

By default, Windows limits file size to 5000000 bytes, you can increase it up to 4Gb:
* Press Win+R, type `regedit`, click OK
* HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WebClient\Parameters
* Right click on the FileSizeLimitInBytes and click Modify
* Click on Decimal
* In the Value data box, type 4294967295, and then click OK.


Bad news: Total Commander [WebDAV plugin](http://www.ghisler.com/plugins.htm) does not work... ([sources](http://ghisler.fileburst.com/fsplugins/webdav_src.zip) found occasionally, but no time...)

**Big thanks** to
* [Ramon de Klein](https://github.com/ramondeklein) for [nwebdav server](https://github.com/ramondeklein/nwebdav)
* [Erast Korolev](https://github.com/erastmorgan) for [Mail.Ru.net-cloud-client](https://github.com/erastmorgan/Mail.Ru-.net-cloud-client)
* [C-A-T](https://github.com/C-A-T9LIFE) for testing and essential information


**Remarks**
* [How to compile for Linux](https://toster.ru/q/375448) from [Алексей Немиро](https://toster.ru/user/AlekseyNemiro) (yep, I'm lazy ass...)
* Avoid using #, %, +  in file and folder names
* If you have paid account - you can remove 2Gb filesize limitation using `--user-agent "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.57 Safari/537.17/TCWFX(x64)"` (taken from [pozitronik/CloudMailRu]( https://github.com/pozitronik/CloudMailRu), no any guarantees, speed limit may exist)
