WebDAVSharp.Server
==================

A WebDAV server, coded in C#, which can be used for various WebDAV .NET applications.

WebDAV# is developed in the .NET Framework 4.5

## Develop notice.

This version was forked by original implementation found in [https://github.com/WebDAVSharp/WebDAVSharp.Server](https://github.com/WebDAVSharp/WebDAVSharp.Server) then modified to fix some bug and to adapt to internal usage. 

To verify that the implementation is OK you can install [litmus](https://github.com/tolsen/litmus) on a linux box and verify that WebDav calls are ok. Once litmus is downloaded you can compile with

```
./configure
make install
```

Then you can use to test your implementation

```
./litmus "http://10.0.0.1:40001/My%20Documents/Books/LitmusTest" username password

```

#This is the original README of the original library.

WebDAVSharp.Server
==================

A WebDAV server, coded in C#, which can be used for various WebDAV .NET applications.

WebDAV# is developed in the .NET Framework 4.5

## Developed by ##

The WebDAV server implementation was based on another open source project, also called WebDAV#.
The maintainer of the project is [Lasse V. Karlsen][1] and the code can be found on [CodePlex][2].

This version of WebDAV# has been developed by [Lieven Janssen][3] and [Ewout Merckx][4] at [Xplore+][5].


  [1]: http://www.vkarlsen.no/
  [2]: https://webdavsharp.codeplex.com/
  [3]: https://www.linkedin.com/in/lievenjanssen/
  [4]: https://www.linkedin.com/in/ewoutmerckx
  [5]: http://xploreplus.com/
