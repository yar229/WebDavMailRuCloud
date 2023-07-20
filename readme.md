## **WebDAV emulator for Cloud.mail.ru / Yandex.Disk**<br>

<a href="https://github.com/ZZZConsulting/WebDavMailRuCloud/releases/latest">
<img src="https://img.shields.io/github/v/release/ZZZConsulting/WebDavMailRuCloud?include_prereleases">
</a>
<img src="https://img.shields.io/github/last-commit/ZZZConsulting/WebDavMailRuCloud" target="_blank">
<img src="https://img.shields.io/github/downloads/ZZZConsulting/WebDavMailRuCloud/total" align="right" target="_blank">


```diff
Перестали скачиваться файлы с Яндекс.Диска - FIXED
```

----
Этот fork проекта [https://github.com/yar229/WebDavMailRuCloud](https://github.com/yar229/WebDavMailRuCloud).

Дополнительно сделан вход на Яндекс.Диск с помощью браузера.<br>
Поддерживается всё разнообразие вариантов аутентификации, включая СМС-коды и QR-коды.

Версия .NET обновлена до 7.0, включая поддержку установки сервисом Windows.

----
# **Помните!**
**Авторы не гарантируют и не несут ответственность
за правильную и постоянную работу программы,
не гарантируют и не несут ответственность
за сохранность и неизменность данных
(как передаваемых, так и хранимых на серверах)!**

**Вы используете программу на свой страх и риск!**

**Используя YandexAuthBrowser.exe, Вы принимаете все риски возможного стороннего доступа к ресурсам
в случае, если кто-то получит доступ к файлам, хранящим данные аутентификации,
создаваемым при входе в хранилище с Вашими учетными данными!**

<hr>
<br>
<br>

#### Requirements <img src="https://habrastorage.org/files/72e/83b/159/72e83b159c2446b9adcdaa03b9bb5c55.png" width=200 align="right"/>
* [Windows](#windows)  - .NET Framework 4.8 / [.NET 3.1 / 5.0 / 6.0 / 7.0](https://dotnet.microsoft.com/download?initial-os=windows)
* [Linux](#linux) - Mono 6.8 / [.NET 3.1 / 5.0 / 6.0 / 7.0](https://dotnet.microsoft.com/download?initial-os=linux)
* [OS X](#mac-os-x) - Mono 6.8 / [.NET 3.1 / 5.0 / 6.0 / 7.0](https://dotnet.microsoft.com/download?initial-os=macos)

#### Usage
```
  -p, --port            (Default: 801) WebDAV server port or several ports separated by `,`
  -h, --host            (Default: "http://127.0.0.1") WebDAV server host with protocol (http://* for http://0.0.0.0)
  --maxthreads          (Default: 5) Maximum concurrent connections to cloud.mail.ru / disk.yandex.ru
  --use-locks           use locking feature
  --cache-listing       (Default: 30) Cache folders listing, sec
  --cache-listing-depth (Default: 1) Cache folders listing depth.
                        If large folder browsing is extremely slow, set to 2

  --protocol            (Default: WebM1Bin) Cloud protocol
                        * WebM1Bin  - (Cloud.Mail.Ru) mix of mobile and DiskO protocols
                        * WebV2     - (Cloud.Mail.Ru) [deprecated] desktop browser protocol
                        * YadWeb    - (Yandex.Disk) desktop browser protocol, see Yandex.Disk readme section
                        * YadWebV2  - (Yandex.Disk) desktop browser protocol with browser authentication, see Yandex.Disk readme section

  --use-deduplicate     Enable deduplication (upload speedup, put by hash), see Using deduplication readme section

  --install <servicename>          Install as windows service (Windows .Net 4.8/7.0 versions only)
  --install-display <displayname>  Display name for Windows service (Windows .Net 4.8/7.0 versions only)
  --uninstall <servicename>        Uninstall windows service (Windows .Net 4.8/7.0 versions only)

  --proxy-address <socks|https|http>://<address>:<port>   Use proxy
  --proxy-user <username>                                 Proxy user name
  --proxy-password <password>                             Proxy password

  --help                Display this help screen.
  --version             Display version information.

  -user-agent           overrides default 'user-agent' string in request headers while accessing clouds.
  -sec-ch-ua            overrides default 'sec-ch-ua' string in request headers while accessing clouds.
```

#### Hasher.exe usage

Calculating hashes for local files

```
  --files            (Group: sources) Filename(s)/wildcard(s) separated by space

  --lists            (Group: sources) Text files with wildcards/filenames separated by space

  --protocol         (Default: WebM1Bin) Cloud protocol to determine hasher

  -r, --recursive    (Default: false) Perform recursive directories scan

  --help             Display this help screen.

  --version          Display version information.
```

### Using deduplication (upload speedup, put by hash)

Edit `<Deduplicate>` section in `wdmrc.config`:

```
  <Deduplicate>
    <!-- Path for disk file cache -->
    <Disk Path = "d:\Temp\WDMRC_Cache" />

    <!--
      Cache: on disk or in-memory file caching
      Target:  path with filename in cloud, .NET regular expression,
               see https://docs.microsoft.com/ru-ru/dotnet/standard/base-types/regular-expressions
      MinSize: minimum file size
      MaxSize: maximum file size
      -->
    <Rules>
      <!-- cache any path/file contains "EUREKA" in disk cache-->
      <Rule Cache="Disk" Target = "EUREKA" MinSize = "0" MaxSize = "0" />

      <!-- small files less than 15000000 bytes will be cached in memory -->
      <Rule Cache="Memory" Target = "" MinSize = "0" MaxSize = "15000000" />

      <!-- files larger than 15000000 bytes will be cached on disk -->
      <Rule Cache="Disk" Target = "" MinSize = "15000000" MaxSize = "0" />
    </Rules>
  </Deduplicate>
```
Then run with `--use-deduplicate` command line key.



### Yandex.Disk

(download [latest Release](https://github.com/ZZZConsulting/WebDavMailRuCloud/releases/), use `--protocol YadWebV2` command line key)

Yandex.Disk WebDAV issues

* It seems Yandex.Disk WebDAV is limited by speed now.
* After file uploading Yandex servers calculating hash.
  E.g. for a 10GB file it may take ~1..2 minutes depending on server load.
  So most of WebDAV clients drops connection on timeout.
* There's no WebDAV info in official help now. WTF?

This solution allow to bypass that limits using unofficial Yandex.Disk Web API.

Yandex.Disk WebDAV authentication

There are 2 ways to get into Yandex.Disk:
* Use login & password only.
  The option to use login & password only must be selected in account settings.
  In this case login (e.g. John or John@yandex.ru)
  and main account password should be used in authentication fields.
  Do not use an Application password or any other special password generated by Yandex,
  the main original account password must be used. And no codes from SMS!
* Use standard browser authentication.
  Use the YandexAuthBrowser application.
  In this case you can use any available Yandex authentication
  including SMS and QR codes.
  For more details read the YandexAuthBrowser section.


***YandexAuthBrowser*** <br/>

1. Download the package with YandexAuthBrowser.

2. Choose a folder for the program.
* The access to folder must be __unrestricted__ for the program!<br>
The program writes into sub-folders, so it must have full unrestricted rights on the folder and sub-folders.<br>
C:\\, C:\Program Files and so on is not a good choice, the program will not work properly.
The better choice is "%userprofile%\AppData\Local\YandexAuthBrowser\" folder.
* The folder must be __secured__ from anyone else!<br>
The sub-folders contain cookies and other sensitive information
for easy access any of your data inside on your Yandex.Disk!<br>
Please keep program sub-folders from other's dirty hands!

3. Unpack YandexAuthBrowser into selected secured folder.
Run the YandexAuthBrowser.<br>
Choose a port number to listen to incoming authentication request on.<br>
Set a password or use link to generate GUID as a new password.<br>
Also you can test the authentication process using Test button.

4. Open `wdmrc.config` and edit `<BrowserAuthenticator>` tag
(add the tag if it's missing).<br>
Attributes:<br>
  `Url`="http://localhost:`<port>`/" - address of the PC running YandexAuthBrowser application,
  `port` means port number you selected on previous step in YandexAuthBrowser application.<br>
  `Password`="`E86A63FC-9BF0-4351-AD51-A5F806BA38EF`" - password or GUID
  you selected on previous step in YandexAuthBrowser application.<br>
  The `E86A63FC-9BF0-4351-AD51-A5F806BA38EF` is an example of password, use your own please.
  The privacy of your password prevents intruders from accessing your private data on Yandex.Disk!<br>
  `CacheDir`="`full path to a secured folder`" - full path to a folder with cache files
  containing browser cookies after authenticated access to your Yandex.Disk.
  This folder must be placed in restricted location.
  Only you and your BrowserAuthenticator must have access to the folder!
  __If anyone gets files from the folder he or she may easily take your data on Yandex.Disk!__
  So keep the folder secured!
  If you do not trust or if you do not want to keep cache, keep `CacheDir` attribute empty.
  Also you may delete any sub-folder but 'runtimes' in the BrowserAuthenticator application's folder.
  If you trust you PC enough, you can use a folder like "%userprofile%\AppData\Local\YandexAuthBrowser\"
  for the cache.
  Replace the %userprofile% with actual path.

5. When you connect to a webdav:
* Fill the `login` field with short (John) or full (John@yandex.ru) name of the account.<br>
* Leave the `password` field empty (or put one or more spaces in case your program disallows empty passwords).<br>
If `password` is empty or space(s) only the `Password` from `<BrowserAuthenticator>` tag is used
to authenticate the request to the BrowserAuthenticator program.
If passwords are not matched the BrowserAuthenticator rejects the request.<br>
* Fill the `password` field with `Password` taken from BrowserAuthenticator application.<br>
In this case the `Password` from `<BrowserAuthenticator>` tag is not used.
Also you may remove `Password` attribute in `<BrowserAuthenticator>` tag.

6. Press Win+R and run `shell:startup`. Put shortcut of the BrowserAuthenticator program in startup folder,
so the BrowserAuthenticator program will be started everytime you login the Windows.
The program stays in system tray until you reboot or manually exit it using menu on tray icon.



***How to use encryption***

Using XTS AES-256 on-the-fly encryption/decryption

* Set (en/de)cryption password
  * with `>>crypt passwd` special command <br/>
	or
  * Add `#` and separator string to your login: `login@mail.ru#_SEP_`
  * After your mail.ru password add separator string and password for encrypting: `MyLoginPassword_SEP_MyCryptingPassword`

* Mark folder as encrypted using `>>crypt init` command
* After that files uploaded to this folder will be encrypted

***Commands*** <br/>
Commands executed by making directory with special name.<br/>
Parameters with spaces must be screened by quotes.
* `>>join SHARED_FOLDER_LINK` Clone shared cloud.mail.ru file/folder to your account
* `>>join #filehash filesize [/][path]filename` Clone cloud.mail.ru file to your account by known hash and size
* `>>link SHARED_FOLDER_LINK [linkname]` Link shared folder without wasting your space (or manually edit file /item.links.wdmrc)
* `>>link check` Remove all dead links (may take time if there's a lot of links)
* `>>move` `/full/path/from /full/path/to` Fast move (if your client moves inner items recursively)
* `>>copy` `/full/path/from /full/path/to` Fast copy (if your client copies inner items recursively)
* `>>lcopy` `x:/local/path/from /full/server/path/to` If file already in cloud, add it by hash without uploading
* `>>rlist` [[/]path] [list_filename]	list [path] to [list_filename]
* `>>del [[/]path]` Fast delete (if your client makes recursive deletions of inner items)
* `>>share [[/]path]` Make file/folder public <br/>
  - and create `.share.wdmrc` file with links
* `>>sharev [[/]path] [resolution]` Make media file public <br/>
  - `resolution` = `0p` (all), `240p`, `360p`, `480p`, `720p`, `1080p`
  - and create `.share.wdmrc` file with public and direct play links
* `>>pl [[/]path]  [resolution]` Make media file public <br/>
  - `resolution` = `0p` (all), `240p`, `360p`, `480p`, `720p`, `1080p`
  - and create `.share.wdmrc` file with public and direct play links <br/>
  - and create `.m3u8` playlist file
* `>>crypt init` Mark current folder as encrypted
* `>>crypt passwd password_for_encryption_decryption` Set password for encryption/decryption

***Settings*** in `wdmrc.exe.config`
* Logging <br/>
	`<config><log4net>` <br/>
	It's standard [Apache log4net](https://logging.apache.org/log4net/) configurations, take a look for [examples](https://logging.apache.org/log4net/release/config-examples.html)
* Default video resolution for generated m3u playlists
    `<config><DefaultSharedVideoResolution>` <br/>
    Values:
      `0p`      auto, m3u contains links to all available resolutions 
      `240p`    ~ 352 x 240
      `360p`    ~ 480 x 360
      `480p`    ~ 858 x 480
      `720p`    ~ 1280 x 720
      `1080p`   ~ 1920 x 1080
* Default User-Agent <br/>
	`<config><DefaultUserAgent>` <br/>
	Default user-agent for web requests to cloud.
* Special command prefix <br/>
	`<config><AdditionalSpecialCommandPrefix>` <br/>
	custom special command prefix instead of `>>`. Make possible to use special commands if client doesn't allow `>>`.
* Enable/disable WebDAV properties <br/>
	`<config><WebDAVProps>` <br/>
	set `false` on properties you don't need to speedup listing on large catalogs / slow connections.
* 2 Factor Authentication <br/>
	At this time you can use
	* `<TwoFactorAuthHandler Name = "AuthCodeConsole"/>` - asks for authcode in application console
	* `<TwoFactorAuthHandler Name = "AuthCodeWindow"/>` - asks for authcode in GUI window (only for .NET Framework releases)
	* 
		```
		<TwoFactorAuthHandler Name = "AuthCodeFile">
			<Param Name = "Directory" Value = "d:"/>
			<Param Name = "FilenamePrefix" Value = "wdmrc_2FA_"/>
		</TwoFactorAuthHandler>
		```
	   user must write authcode to file. For example, user `test@mail.ru` writes code to `d:\wdmrc_2FA_test@mail.ru`.
	
	
	Be careful, this methods does not usable when application started as a service/daemon. <br>
	You can make your own 2FA handlers inherited from `ITwoFaHandler` and put it in separate dll which name starts with `MailRuCloudApi.TwoFA`
	
Connect with (almost any) file manager that supports WebDAV using Basic authentication with no encryption and
* your cloud.mail.ru email and password
* or `anonymous` login if only public links list/download required ([WinSCP script example](https://github.com/yar229/WebDavMailRuCloud/issues/146#issuecomment-448978833))

Automatically split/join when uploading/downloading files larger than cloud allows.

[Russian FAQ](https://gist.github.com/yar229/4b702af114503546be1fe221bb098f27) <br/>
[geektimes.ru - Снова про WebDAV и Облако Mail.Ru](https://geektimes.ru/post/285520/) <br/>
[glashkoff.com - Как бесплатно подключить Облако Mail.Ru через WebDAV](https://glashkoff.com/blog/manual/webdav-cloudmailru/) <br/>
[manjaro.ru - Облако Mail.Ru подключаем через эмулятор WebDav как сетевой диск](https://manjaro.ru/how-to/oblako-mailru-podklyuchaem-cherez-emulyator-webdav-kak-setevoy-disk.html) <br/>


#### Windows

Using as windows service
* Run `cmd` with Administrator rights
* Then, for example, `wdmrc.exe --install wdmrc -p 801 --maxthreads 15` <br/>
* `net start wdmrc`

<br/>

<details> 
<summary>Using from explorer requires enabled Basic Auth for WebDAV </summary>
* Press Win+R, type `regedit`, click OK
* HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WebClient\Parameters
* Right click on the BasicAuthLevel and click Modify
* In the Value data box, type 2, and then click OK.
* Reset computer (or run `cmd` with admin rights and then `net stop webclient`, `net start webclient`)
</details>

<details> 
<summary>Use as Windows disk </summary>
```
net use ^disk^: http://^address^:^port^ ^your_mailru_password^ /USER:^your_mailru_email^
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

(tested under [Elementary OS](https://elementary.io) and [Lubuntu](http://lubuntu.net/))
* download and unzip [latest](https://github.com/yar229/WebDavMailRuCloud/releases/latest) release  <sub><sup>([obsolete alternative way](https://toster.ru/q/375448) from [Алексей Немиро](https://toster.ru/user/AlekseyNemiro) )</sup></sub>
* .Net Framework (WebDAVCloudMailRu-*-dotNet45.zip)
  * `sudo apt install apt mono-complete`
  * `mono wdmrc.exe -p <port>`
* .Net Core (WebDAVCloudMailRu-*-dotNetCore20.zip)
  * install [.NET Core](https://www.microsoft.com/net/core#linuxredhat)
  * `dotnet wdmrc.dll <params>`


See also 
* [Package for Gentoo Linux](https://github.com/yar229/WebDavMailRuCloud/issues/66) by [powerman](https://github.com/powerman)
* Docker image by [slothds](https://github.com/slothds) ([DockerHub](https://hub.docker.com/r/slothds/wdmrc-proxy/), [GitHub](https://github.com/slothds/wdmrc-proxy))
* Docker image by [ivang7](https://github.com/ivang7) HTTP & HTTPS [DockerHub](https://hub.docker.com/r/ivang7/webdav-mailru-cloud)




Mount with davfs2
* `mkdir /mnt/<folder>`
* edit `/etc/davfs2/davfs2.conf` set `use_locks       0`
* `sudo mount --rw -t davfs http://<address>:<port> /mnt/<folder>/ -o uid=<current_linux_user>`

As a service (daemon)
* https://github.com/yar229/WebDavMailRuCloud/issues/214


CERTIFICATE_VERIFY_FAILED exception
[Issue 56](https://github.com/yar229/WebDavMailRuCloud/issues/56)
[default installation of Mono doesn’t trust anyone](http://www.mono-project.com/docs/faq/security/)

In short:
```
# cat /etc/ssl/certs/* >ca-bundle.crt
# cert-sync ca-bundle.crt
# rm ca-bundle.crt
```

#### Mac OS X

* download and unzip [latest](https://github.com/yar229/WebDavMailRuCloud/releases/latest) release  <sub><sup>([obsolete alternative way](https://toster.ru/q/375448) from [Алексей Немиро](https://toster.ru/user/AlekseyNemiro) )</sup></sub>
* .Net Framework (WebDAVCloudMailRu-*-dotNet45.zip)
  * `brew install mono` (how to install [brew](https://brew.sh/))
  * `mono wdmrc.exe -p <port>`
* .Net Core (WebDAVCloudMailRu-*-dotNetCore20.zip)
  * install [.NET Core](https://www.microsoft.com/net/core#macos)
  * `dotnet wdmrc.dll <params>`

Use any client supports webdav.


#### Remarks
* [**RaiDrive**](https://www.raidrive.com/)
* [**NetDrive**](http://www.netdrive.net/)
* [**rclone mount**](https://rclone.org/)
* [**Total Commander**](http://www.ghisler.com/): 
  - requires to update `WebDAV plugin` to [v.2.9](http://ghisler.fileburst.com/fsplugins/webdav.zip)
  - turn on `(connection properties) -> Send\Receive accents in URLs as UTF-8 Unicode`
* [**WebDrive**](https://southrivertech.com/products/webdrive/): 
  - disable `(disk properties) -> HTTP Settings -> Do chunked upload for large files.`
* [**CarotDAV**](http://rei.to/carotdav_en.html): 
  - check `(connection properties) -> Advanced -> Don't update property.`
* avoid using Unicode non-printing characters such as [right-to-left mark](https://en.wikipedia.org/wiki/Right-to-left_mark) in file/folder names


#### Big thanks
* [Ramon de Klein](https://github.com/ramondeklein) for [nwebdav server](https://github.com/ramondeklein/nwebdav)
* [Erast Korolev](https://github.com/erastmorgan) for [Mail.Ru.net-cloud-client](https://github.com/erastmorgan/Mail.Ru-.net-cloud-client)
* [Gareth Lennox](https://bitbucket.org/garethl/) for [XTSSharp](https://bitbucket.org/garethl/xtssharp)
* [C-A-T](https://github.com/C-A-T9LIFE) for testing and essential information


#### See also<br>
*  Official client [Disk-O:](https://disk-o.cloud/)
*  [Total Commander plugin for cloud.mail.ru service](https://github.com/pozitronik/CloudMailRu)<br>
*  [MARC-FS - FUSE filesystem attempt for Mail.Ru Cloud](https://gitlab.com/Kanedias/MARC-FS)<br>




<a href="https://www.donationalerts.com/r/yar229"><img src="https://hangoverbarandgrill.com/files/2019/12/002-beer.png" height="20"></a> [Buy me a beer](https://www.donationalerts.com/r/yar229)
