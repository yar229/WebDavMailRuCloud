## **WebDAV emulator for Mail.ru Cloud**<br>
[download latest release binaries](https://github.com/yar229/WebDavMailRuCloud/releases/latest) <a href="https://ci.appveyor.com/project/yar229/webdavmailrucloud-k21bq/branch/master"><img src="https://ci.appveyor.com/api/projects/status/3gejunv39gqed3tp/branch/master?svg=true" align="right"></a>
<a href="https://github.com/yar229/WebDavMailRuCloud/releases/latest"><img src="https://github-basic-badges.herokuapp.com/release/yar229/WebDavMailRuCloud.svg" align="right"></a>

**Requirements:**
* Windows - Microsoft .NET Framework 4.5 (WebDavMailRuCloud_*.zip)
* Linux - Mono Stable 4.6.2.16 (wdmrc-mono-*.tar.gz)

**Usage:**
``` 	
	-p, --port        (Default: 801) WebDAV server port
	-h, --host		  (Default: "http://127.0.0.1") WebDAV server host with protocol
	--maxthreads      (Default: 5) Maximum concurrent connections to cloud.mail.ru
	--user-agent      "browser" user-agent
	--help            Display this help screen.
	--version         Display version information.
```
Connect with (almost any) file manager that supports WebDAV using Basic authentification with no encryption and your cloud.mail.ru email and password (it's safe when you opens server on 127.0.0.1)

***Hints***
* Clone shared cloud.mail.ru file/folder to your account:	make folder with name `>>SHARED_FOLDER_LINK`
* Automatic split/join when uploading/downloading files large than cloud allows

**Windows**
<br>Using from explorer requires enabled Basic Auth for WebDAV:
* Press Win+R, type `regedit`, click OK
* HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WebClient\Parameters
* Right click on the BasicAuthLevel and click Modify
* In the Value data box, type 2, and then click OK.
* Reset computer (or run `cmd` with admin rights and then `net stop webclient`, `net start webclient`)

Use as Windows disk: <br>
``` 
	net use <disk>: http://127.0.0.1:<port> <your password for cloud.mail.ru> /USER:<your_email@mail.ru>
``` 


Windows 7 client might perform very bad when connecting to any WebDAV server. This is caused, because it tries to auto-detect any proxy server before any request. Refer to KB2445570 for more information.

Faster WebDAV Performance in Windows 7<br>
* In Internet Explorer, open the Tools menu, then click Internet Options.
* Select the Connections tab.
* Click the LAN Settings button.
* Uncheck the “Automatically detect settings” box.
* Click OK until you’re out of dialog.

By default, Windows limits file size to 5000000 bytes, you can increase it up to 4Gb:
* Press Win+R, type `regedit`, click OK
* HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WebClient\Parameters
* Right click on the FileSizeLimitInBytes and click Modify
* Click on Decimal
* In the Value data box, type 4294967295, and then click OK.
* Reset computer (or run `cmd` with admin rights and then `net stop webclient`, `net start webclient`)

[Wrong disk size when mapped as Windows drive](https://support.microsoft.com/en-us/kb/2386902)<br>
Microsoft says - "It's not a bug, it's by design"

**Linux**
<br>I've tested it only under [Elementary OS](https://elementary.io)
* download [latest](https://github.com/yar229/WebDavMailRuCloud/releases/latest) `wdmrc-mono-*.tar.gz`
* `sudo apt install apt mono-complete`
* `mono wdmrc.exe -p <port>`

[alternative way](https://toster.ru/q/375448) from [Алексей Немиро](https://toster.ru/user/AlekseyNemiro) (obsolete a bit)

**Big thanks** to
* [Ramon de Klein](https://github.com/ramondeklein) for [nwebdav server](https://github.com/ramondeklein/nwebdav)
* [Erast Korolev](https://github.com/erastmorgan) for [Mail.Ru.net-cloud-client](https://github.com/erastmorgan/Mail.Ru-.net-cloud-client)
* [C-A-T](https://github.com/C-A-T9LIFE) for testing and essential information


**Remarks**
* Using within [**Total Commander**](http://www.ghisler.com/) requires to update `WebDAV plugin` to [v.2.9](http://ghisler.fileburst.com/fsplugins/webdav.zip)
* Avoid using #, %, +  in file and folder names
* If you have paid account - you can remove 2Gb filesize limitation using `--user-agent "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.57 Safari/537.17/TCWFX(x64)"` (taken from [pozitronik/CloudMailRu]( https://github.com/pozitronik/CloudMailRu), no any guarantees, speed limit may exist)

**See also**
* [Mail.Ru.net-cloud-client](https://github.com/erastmorgan/Mail.Ru-.net-cloud-client)
* [Total Commander plugin for cloud.mail.ru service](https://github.com/pozitronik/CloudMailRu)
* [MARC-FS - FUSE filesystem attempt for Mail.Ru Cloud](https://gitlab.com/Kanedias/MARC-FS)
