namespace WebDAVSharp.Server.Utilities
{
    /// <summary>
    /// Contains the values of status codes defined for Http (WebDav).
    /// </summary>
    internal enum WebDavStatusCode
    {
        /// <summary>
        /// Equivalent to Http status 207 (WebDav).
        /// WebDAVSharp.Server.Utilities.WebDavStatusCode.MultiStatus provides status for multiple independent operations.
        /// </summary>
        /// <remarks>
        /// For more information, see <see href="http://www.webdav.org/specs/rfc2518.html#STATUS_207" />.
        /// </remarks>
        MultiStatus = 207,
        /// <summary>
        /// Equivalent to Http status 422 (WebDav).
        /// WebDAVSharp.Server.Utilities.WebDavStatusCode.UnprocessableEntity means the server understands the content type of the request entity (hence a 415 (<see cref="System.Net.HttpStatusCode.UnsupportedMediaType"/>) status code is inappropriate), and the syntax of the request entity is correct (thus a 400 (<see cref="System.Net.HttpStatusCode.BadRequest"/>) status code is inappropriate) but was unable to process the contained instructions.
        /// </summary>
        /// <remarks>
        /// For more information, see <see href="http://www.webdav.org/specs/rfc2518.html#STATUS_422"/>.
        /// </remarks>
        UnprocessableEntity = 422,
        /// <summary>
        /// Equivalent to Http status 423 (WebDav).
        /// WebDAVSharp.Server.Utilities.WebDavStatusCode.Locked means the source or destination resource of a method is locked.
        /// </summary>
        /// <remarks>
        /// For more information, see <see href="http://www.webdav.org/specs/rfc2518.html#STATUS_423"/>.
        /// </remarks>
        Locked = 423,
        /// <summary>
        /// Equivalent to Http status 424 (WebDav).
        /// WebDAVSharp.Server.Utilities.WebDavStatusCode.FailedDependency means that the method could not be performed on the resource because the requested action depended on another action and that action failed.
        /// </summary>
        /// <remarks>
        /// For more information, see <see href="http://www.webdav.org/specs/rfc2518.html#STATUS_424"/>.
        /// </remarks>
        FailedDependency = 424,
        /// <summary>
        /// Equivalent to Http status 507 (WebDav).
        /// WebDAVSharp.Server.Utilities.WebDavStatusCode.InsufficientStorage means the method could not be performed on the resource because the server is unable to store the representation needed to successfully complete the request.
        /// </summary>
        /// <remarks>
        /// For more information, see <see href="http://www.webdav.org/specs/rfc2518.html#STATUS_507" />.
        /// </remarks>
        InsufficientStorage = 507,
    }
}