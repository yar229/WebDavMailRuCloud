https://bitbucket.org/garethl/xtssharp/downloads


XTSSharp is an implementation of XTS-AES 128/256 for .NET

XTSSharp is copyright (c) 2010 Gareth Lennox (garethl@dwakn.com)
All rights reserved.

See license.txt for licensing information. 

----------

XTS-AES is a cipher meant to be used for encryption of block-level devices. It allows
a block to be encrypted/decrypted without depending on previous blocks.

This means that you effectively have random access (or very close to it) to the data.

The library provides a stream that simulates random access - encrypting and decrypting
blocks as data is read/written. It also supports seeking.

For more information on the XTS-AES algorithm, see the following links:

	http://en.wikipedia.org/wiki/Disk_encryption_theory#XTS
	http://axelkenzo.ru/downloads/1619-2007-NIST-Submission.pdf


Main entry point is the XtsAes128 and XtsAes256 classes, as well as the XtsStream
class. It should be pretty self explanatory. See the unit tests for more information.

The XtsStream will always write a full "sector". I.e. if you write 1 byte, it will write
the full sector size (default of 512 bytes) with the other bytes zeroed (but encrypted before
writing). Reading it back again will result in the full 512 bytes being available.

Note that the solution file is in Visual Studio 2010 format.