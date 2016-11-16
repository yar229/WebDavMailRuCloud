**WebDAV emulator for Mail.ru Cloud**<br>
[download latest release binaries](https://github.com/yar229/WebDavMailRuCloud/releases/latest)

Usage:
``` 	
	-p, --port        Required. WebDAV server port
	-l, --login       Required. Login to Mail.ru Cloud
	-s, --password    Required. Password to Mail.ru Cloud
	--help            Display this help screen.
	--version         Display version information.
```

Use as Windows disk: <br>
``` 
	net use z: http://127.0.0.1:<port>
``` 	

Windows 7 client might perform very bad when connecting to any WebDAV server. This is caused, because it tries to auto-detect any proxy server before any request. Refer to KB2445570 for more information.

By default, Windows limits file size to 5000000 bytes, you can increase it up to 4Gb:
* HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WebClient\Parameters
* Right click on the FileSizeLimitInBytes and click Modify
* Click on Decimal
* In the Value data box, type 4294967295, and then click OK.


Bad news: Total Commander [WebDAV plugin](http://www.ghisler.com/plugins.htm) does not work...

**Big thanks** to
* [Ramon de Klein](https://github.com/ramondeklein) for [nwebdav server](https://github.com/ramondeklein/nwebdav)
* [Erast Korolev](https://github.com/erastmorgan) for [Mail.Ru.net-cloud-client](https://github.com/erastmorgan/Mail.Ru-.net-cloud-client)
* [C-A-T](https://github.com/C-A-T9LIFE) for help with testing


Remarks
* beware of names contains "#"