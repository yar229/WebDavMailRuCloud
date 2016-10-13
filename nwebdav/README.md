# NWebDAV
.NET implementation of a WebDAV server.

## Overview
I needed a WebDAV server implementation for C#, but I couldn't find an
implementation that fulfilled my needs. That's why I wrote
my own.

__Requirements__

* Fast, scalable, robust with moderate memory usage.
* Abstract data store, so it can be used for directories/files but also for any
  other data.
* Abstract locking providers, so it can use in-memory locking (small servers)
  or Redis (large installations).
* Flexible and extensible property management.
* Fully supports .NET framework, Mono and the Core CLR.
* Allows for various HTTP authentication and SSL support (basic authentication works).

## WebDAV client on Windows Vista/7
The Windows Vista/7 WebDAV client is implemented poorly. We have support for
this client since the 0.1.7 release.

* It required the 'D' namespace prefix on all DAV related XML nodes. XML
  namespaces without prefixes are not supported.
* It cannot deal with XML date time format (ISO 8601) in a decent manner. It
  processes the fraction part as milliseconds, which is wrong. Milliseconds
  can be between 0 and 999, where a fraction can have more than 3 digits. The
  difference is subtle. __2016-04-14T01:02:03.12__ denotes 120ms, but it could
  be parsed as 12ms by Windows 7 clients. __2016-04-14T01:02:03.1234__ denotes
  123.4ms, but cannot be parsed when using integers. Windows 7 clients don't
  accept this format. For that reason we will not output more than 3 digits
  for the fraction part.

Windows 7 client might perform very bad when connecting to any WebDAV server
(not related to this specific implementation). This is caused, because it tries
to auto-detect any proxy server before __any__ request. Refer to
[KB2445570](https://support.microsoft.com/en-us/kb/2445570) for more information.

## Work in progress
This module is currently work-in-progress and shouldn't be used for production use yet. If you want to help, then let me know...
The following features are currently missing:

* Only the in-memory locking provider has been implemented yet.
* Check if each call responds with the proper status codes (as defined in the WebDAV specification).
* Recursive locking is not supported yet.
* We should have compatibility flags that can be used to implement quirks
  for bad WebDAV clients. We can detect the client based on the User-Agent
  and provide support for it.

The current version seems to work fine to serve files using WebDAV on both Windows and OS X.
