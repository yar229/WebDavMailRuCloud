WebDAV emulator for Mail.ru Cloud

Usage:

	-p, --port        Required. WebDAV server port
	-l, --login       Required. Login to Mail.ru Cloud
	-s, --password    Required. Password to Mail.ru Cloud
	--help            Display this help screen.
	--version         Display version information.


Use as Windows disk:
	net use http://127.0.0.1: &lt;port&gt;

Windows 7 client might perform very bad when connecting to any WebDAV server. This is caused, because it tries to auto-detect any proxy server before any request. Refer to KB2445570 for more information.

By default, Windows limits file size to 5000000 bytes, you can increase it up to 4Gb:
HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WebClient\Parameters
	Right click on the FileSizeLimitInBytes and click Modify
	Click on Decimal
	In the Value data box, type 4294967295, and then click OK.


Bad news: Total Commander WebDAV plugin (http://www.ghisler.com/plugins.htm) does not work...

You can find latest release binaries here - https://github.com/yar229/WebDavMailRuCloud/releases/latest