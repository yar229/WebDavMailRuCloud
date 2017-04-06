## **WebDAV emulator for Mail.ru Cloud**<br>

<a href="https://github.com/yar229/WebDavMailRuCloud/releases/latest"><img src="https://mybadges.herokuapp.com/github/filewlogo/windows-logo-silhouette/WebDavMailRuCloud__version_.zip%20%20%20%20%20/yar229/WebDavMailRuCloud.svg?colorA=abcdef&colorB=cccdce&style=social"></a>
 <a href="https://github.com/yar229/WebDavMailRuCloud/releases"><img src="https://mybadges.herokuapp.com/github/downloads/yar229/WebDavMailRuCloud/total.svg" align="right"></a>

#### Requirements <img src="https://habrastorage.org/files/72e/83b/159/72e83b159c2446b9adcdaa03b9bb5c55.png" width=200 align="right"/>
* [Windows](#windows)  - Microsoft .NET Framework 4.5 
* [Linux](#linux) - Mono 4.6 +
* OS X - Mono 4.6 + (buddy said it works)

#### Usage
``` 	
	-p, --port        (Default: 801) WebDAV server port
	-h, --host		  (Default: "http://127.0.0.1") WebDAV server host with protocol (http://* for htpp://0.0.0.0)
	--maxthreads      (Default: 5) Maximum concurrent connections to cloud.mail.ru
	--user-agent      "browser" user-agent
	--help            Display this help screen.
	--version         Display version information.
```
Connect with (almost any) file manager that supports WebDAV using Basic authentification with no encryption and your cloud.mail.ru email and password (it's safe when you opens server on 127.0.0.1)

***Hints***
* Clone shared cloud.mail.ru file/folder to your account:	make folder with name `>>SHARED_FOLDER_LINK`
* Automatic split/join when uploading/downloading files larger than cloud allows


#### Windows

<details> 
<summary>Using from explorer requires enabled Basic Auth for WebDAV </summary>
* Press Win+R, type `regedit`, click OK
* HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WebClient\Parameters
* Right click on the BasicAuthLevel and click Modify
* In the Value data box, type 2, and then click OK.
* Reset computer (or run `cmd` with admin rights and then `net stop webclient`, `net start webclient`)
</details>

<details> 
<summary>Use as Windows disk</summary>
``` 
	net use <disk>: http://127.0.0.1:<port> <your password for cloud.mail.ru> /USER:<your_email@mail.ru>
```
</details>

<details>
<summary>Faster WebDAV Performance in Windows 7</summary>
Windows 7 client might perform very bad when connecting to any WebDAV server. This is caused, because it tries to auto-detect any proxy server before any request. Refer to KB2445570 for more information.

* In Internet Explorer, open the Tools menu, then click Internet Options.
* Select the Connections tab.
* Click the LAN Settings button.
* Uncheck the “Automatically detect settings” box.
* Click OK until you’re out of dialog.
</details>

<details>
<summary>By default, Windows limits file size to 5000000 bytes, you can increase it up to 4Gb</summary>
* Press Win+R, type `regedit`, click OK
* HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WebClient\Parameters
* Right click on the FileSizeLimitInBytes and click Modify
* Click on Decimal
* In the Value data box, type 4294967295, and then click OK.
* Reset computer (or run `cmd` with admin rights and then `net stop webclient`, `net start webclient`)
</details>

<details>
<summary>Wrong disk size when mapped as Windows drive</summary>
[Microsoft says - "It's not a bug, it's by design"](https://support.microsoft.com/en-us/kb/2386902)
</details>


#### Linux

You can simply use [Package for Gentoo Linux](https://github.com/yar229/WebDavMailRuCloud/issues/66) by [powerman](https://github.com/powerman)

...or by hands (tested under [Elementary OS](https://elementary.io) and [Lubuntu](http://lubuntu.net/))
* download [latest](https://github.com/yar229/WebDavMailRuCloud/releases/latest) `wdmrc-mono-*.tar.gz`  <sub><sup>([obsolete alternative way](https://toster.ru/q/375448) from [Алексей Немиро](https://toster.ru/user/AlekseyNemiro) )</sup></sub>
* `sudo apt install apt mono-complete`
* `mono wdmrc.exe -p <port>`

<details>
<summary>Mount with davfs2</summary>
* `mkdir /mnt/<folder>`
* edit `/etc/davfs2/davfs2.conf` set `use_locks       0`
* `sudo mount --rw -t davfs http://<address>:<port> /mnt/<folder>/ -o uid=<current_linux_user>`
</details>

<details>
<summary>CERTIFICATE_VERIFY_FAILED exception</summary>
[Issue 56](https://github.com/yar229/WebDavMailRuCloud/issues/56)
[default installation of Mono doesn’t trust anyone](http://www.mono-project.com/docs/faq/security/)

In short:
```
# cat /etc/ssl/certs/* >ca-bundle.crt
# cert-sync ca-bundle.crt
# rm ca-bundle.crt
```
</details>


#### Big thanks
* [Ramon de Klein](https://github.com/ramondeklein) for [nwebdav server](https://github.com/ramondeklein/nwebdav)
* [Erast Korolev](https://github.com/erastmorgan) for [Mail.Ru.net-cloud-client](https://github.com/erastmorgan/Mail.Ru-.net-cloud-client)
* [C-A-T](https://github.com/C-A-T9LIFE) for testing and essential information

<br>
#### Remarks
* [Discussion on geektimes.ru](https://geektimes.ru/post/285520/)
* Using within [**Total Commander**](http://www.ghisler.com/) requires to update `WebDAV plugin` to [v.2.9](http://ghisler.fileburst.com/fsplugins/webdav.zip)
* Avoid using #, %, +  in file and folder names
* If you have paid account - you can remove 2Gb filesize limitation using `--user-agent "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.57 Safari/537.17/TCWFX(x64)"` (taken from [pozitronik/CloudMailRu]( https://github.com/pozitronik/CloudMailRu), no any guarantees, speed limit may exist)

<br>
#### See also<br>
*  [Mail.Ru.net-cloud-client](https://github.com/erastmorgan/Mail.Ru-.net-cloud-client)<br>
*  [Total Commander plugin for cloud.mail.ru service](https://github.com/pozitronik/CloudMailRu)<br>
*  [MARC-FS - FUSE filesystem attempt for Mail.Ru Cloud](https://gitlab.com/Kanedias/MARC-FS)<br>


