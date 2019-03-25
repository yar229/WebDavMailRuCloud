using System.Net;

using NWebDav.Server.Helpers;

namespace NWebDav.Server
{
    /// <summary>
    /// DAV status return codes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The DAV status codes are a related to the <see cref="HttpStatusCode"/>
    /// and will be the main status return code for each WebDAV operation. It
    /// is actually a subset of the <see cref="HttpStatusCode"/> and extended
    /// with some WebDAV specific return codes.
    /// </para>
    /// <para>
    /// Each <see cref="DavStatusCode"/> is annotated with a
    /// <see cref="DavStatusCodeAttribute"/> containing the human readable
    /// status. It will be sent with the response if the underlying HTTP server
    /// allows custom HTTP return codes. Clients should parse the numeric
    /// status code, because it's not guaranteed that the textual version is
    /// returned. If the human readable version is required, then it can be
    /// obtained using the <see cref="DavStatusCodeHelper"/> helper class.
    /// </para>
    /// <para>
    /// The HTTP status codes are documented on
    /// <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html"/>.
    /// A detailed list of the extended WebDAV status codes can be found on
    /// <see href="http://www.webdav.org/specs/rfc2518.html#status.code.extensions.to.http11"/>.
    /// </para>
    /// </remarks>
    public enum DavStatusCode
    {
        /// <summary>
        /// <para>
        /// The 102 (Processing) status code is an interim response used to 
        /// inform the client that the server has accepted the complete
        /// request, but has not yet completed it. This status code  should
        /// only be sent when the server has a reasonable expectation that the
        /// request will take significant time to complete. As guidance, if a
        /// method is taking longer than 20 seconds (a reasonable, but
        /// arbitrary value) to process the server should return a 102
        /// (Processing) response. The server must send a final response after
        /// the request has been completed.
        /// </para>
        /// <para>
        /// Methods can potentially take a long period of time to process,
        /// especially methods that support the Depth header. In such cases
        /// the client may time-out the connection while waiting for a
        /// response. To prevent this the server may return a 102
        /// (Processing) status code to indicate to the client that the server
        /// is still processing the method.
        /// </para>
        /// <para>
        /// This status code is a WebDAV specific result code.
        /// </para>
        /// </summary>
        [DavStatusCode("Processing")]
        Processing = 102,

        /// <summary>
        /// <para>
        /// The request has succeeded.
        /// </para>
        /// </summary>
        [DavStatusCode("OK")]
        Ok = HttpStatusCode.OK,

        /// <summary>
        /// <para>
        /// The request has been fulfilled and resulted in a new resource being
        /// created. The newly created resource can be referenced by the URI(s)
        /// returned in the entity of the response, with the most specific URI
        /// for the resource given by a 
        /// <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.30">Location</see> 
        /// header field. The response SHOULD include an entity containing a
        /// list of resource characteristics and location(s) from which the
        /// user or user agent can choose the one most appropriate. The entity
        /// format is specified by the media type given in the
        /// <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.17">Content-Type</see> 
        /// header field. The origin server MUST create the resource before
        /// returning the 201 status code. If the action cannot be carried out
        /// immediately, the server SHOULD respond with 202 (Accepted) response
        /// instead.
        /// </para>
        /// <para>
        /// A 201 response MAY contain an 
        /// <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.19">Etag</see> 
        /// response header field indicating the current value of the entity
        /// tag for the requested variant just created.
        /// </para>
        /// </summary>
        [DavStatusCode("Created")]
        Created = HttpStatusCode.Created,

        /// <summary>
        /// <para>
        /// The request has been accepted for processing, but the processing
        /// has not been completed. The request might or might not eventually
        /// be acted upon, as it might be disallowed when processing actually
        /// takes place. There is no facility for re-sending a status code
        /// from an asynchronous operation such as this. 
        /// </para>
        /// <para>
        /// The 202 response is intentionally non-committal. Its purpose is
        /// to allow a server to accept a request for some other process
        /// (perhaps a batch-oriented process that is only run once per day)
        /// without requiring that the user agent's connection to the server
        /// persist until the process is completed. The entity returned with
        /// this response SHOULD include an indication of the request's current
        /// status and either a pointer to a status monitor or some estimate of
        /// when the user can expect the request to be fulfilled. 
        /// </para>
        /// </summary>
        [DavStatusCode("Accepted")]
        Accepted = HttpStatusCode.Accepted,

        /// <summary>
        /// <para>
        /// The server has fulfilled the request but does not need to return an
        /// entity-body, and might want to return updated meta information. The
        /// response MAY include new or updated meta information in the form of
        /// entity-headers, which if present SHOULD be associated with the
        /// requested variant.
        /// </para>
        /// <para>
        /// If the client is a user agent, it SHOULD NOT change its document
        /// view from that which caused the request to be sent. This response
        /// is primarily intended to allow input for actions to take place
        /// without causing a change to the user agent's active document view,
        /// although any new or updated meta information SHOULD be applied to
        /// the document currently in the user agent's active view.
        /// </para>
        /// <para>
        /// The 204 response MUST NOT include a message-body, and thus is
        /// always terminated by the first empty line after the header fields.
        /// </para>
        /// </summary>
        [DavStatusCode("No Content")]
        NoContent = HttpStatusCode.NoContent,

        /// <summary>
        /// <para>
        /// The server has fulfilled the partial GET request for the resource.
        /// The request MUST have included a
        /// <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.35">Range</see>
        /// header field indicating the desired range, and MAY have included an
        /// <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.27">If-Range</see>
        /// header field to make the request conditional. 
        /// </para>
        /// </summary>
        [DavStatusCode("Partial Content")]
        PartialContent = HttpStatusCode.PartialContent,

        /// <summary>
        /// <para>
        /// The 207 (Multi-Status) status code provides status for multiple
        /// independent operations.
        /// </para>
        /// <para>
        /// The default 207 (Multi-Status) response body is a text/xml or
        /// application/xml HTTP entity that contains a single XML element
        /// called multi-status, which contains a set of XML elements called
        /// response which contain 200, 300, 400, and 500 series status codes
        /// generated during the method invocation. 100 series status codes
        /// should not be recorded in a response XML element.
        /// </para>
        /// <para>
        /// This status code is a WebDAV specific result code.
        /// </para>
        /// </summary>
        [DavStatusCode("Multi-Status")]
        MultiStatus = 207,

        /// <summary>
        /// <para>
        /// This is used for caching purposes. It is telling to client that
        /// response has not been modified. So, client can continue to use
        /// same cached version of response.
        /// </para>
        /// </summary>
        [DavStatusCode("Not Modified")]
        NotModified = HttpStatusCode.NotModified,

        /// <summary>
        /// <para>
        /// The request could not be understood by the server due to malformed
        /// syntax. The client SHOULD NOT repeat the request without
        /// modifications.
        /// </para>
        /// </summary>
        [DavStatusCode("Bad Request")]
        BadRequest = HttpStatusCode.BadRequest,

        /// <summary>
        /// <para>
        /// The request requires user authentication. The response MUST include a
        /// <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.47">WWW-Authenticate</see>
        /// header field containing a challenge applicable to the requested
        /// resource. The client MAY repeat the request with a suitable
        /// <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.8">Authorization</see>
        /// header field. If the request already included Authorization
        /// credentials, then the 401 response indicates that authorization has
        /// been refused for those credentials. If the 401 response contains
        /// the same challenge as the prior response, and the user agent has
        /// already attempted authentication at least once, then the user
        /// SHOULD be presented the entity that was given in the response,
        /// since that entity might include relevant diagnostic information.
        /// </para>
        /// <para>
        /// HTTP access authentication is explained in
        /// "<see href="http://www.ietf.org/rfc/rfc2617.txt">HTTP Authentication: Basic and Digest Access Authentication</see>".
        /// </para>
        /// </summary>
        [DavStatusCode("Unauthorized")]
        Unauthorized = HttpStatusCode.Unauthorized,

        /// <summary>
        /// <para>
        /// The server understood the request, but is refusing to fulfill it.
        /// Authorization will not help and the request SHOULD NOT be
        /// repeated. If the request method was not HEAD and the server wishes
        /// to make public why the request has not been fulfilled, it SHOULD
        /// describe the reason for the refusal in the entity. If the server
        /// does not wish to make this information available to the client,
        /// the status code 404 (<see cref="NotFound">Not Found</see>) can be
        /// used instead.
        /// </para>
        /// </summary>
        [DavStatusCode("Forbidden")]
        Forbidden = HttpStatusCode.Forbidden,

        /// <summary>
        /// <para>
        /// The server has not found anything matching the Request-URI. No
        /// indication is given of whether the condition is temporary or
        /// permanent. The 410 (Gone) status code SHOULD be used if the
        /// server knows, through some internally configurable mechanism,
        /// that an old resource is permanently unavailable and has no
        /// forwarding address. This status code is commonly used when the
        /// server does not wish to reveal exactly why the request has been
        /// refused, or when no other response is applicable.
        /// </para>
        /// </summary>
        [DavStatusCode("Not Found")]
        NotFound = HttpStatusCode.NotFound,

        /// <summary>
        /// <para>
        /// The request could not be completed due to a conflict with the
        /// current state of the resource. This code is only allowed in
        /// situations where it is expected that the user might be able to
        /// resolve the conflict and resubmit the request. The response body
        /// SHOULD include enough information for the user to recognize the
        /// source of the conflict. Ideally, the response entity would
        /// include enough information for the user or user agent to fix the
        /// problem; however, that might not be possible and is not required.
        /// </para>
        /// <para>
        /// Conflicts are most likely to occur in response to a PUT request.
        /// For example, if versioning were being used and the entity being
        /// PUT included changes to a resource which conflict with those made
        /// by an earlier (third-party) request, the server might use the
        /// 409 response to indicate that it can't complete the request. In
        /// this case, the response entity would likely contain a list of the
        /// differences between the two versions in a format defined by the
        /// response 
        /// <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.17">Content-Type</see>.
        /// </para>
        /// </summary>
        [DavStatusCode("Conflict")]
        Conflict = HttpStatusCode.Conflict,

        /// <summary>
        /// <para>
        /// The requested resource is no longer available at the server and no
        /// forwarding address is known. This condition is expected to be
        /// considered permanent. Clients with link editing capabilities SHOULD
        /// delete references to the Request-URI after user approval. If the
        /// server does not know, or has no facility to determine, whether or
        /// not the condition is permanent, the status code 404
        /// (<see cref="NotFound">Not Found</see>) SHOULD be used instead. This
        /// response is cacheable unless indicated otherwise.
        /// </para>
        /// <para>
        /// The 410 response is primarily intended to assist the task of web
        /// maintenance by notifying the recipient that the resource is
        /// intentionally unavailable and that the server owners desire that
        /// remote links to that resource be removed. Such an event is common
        /// for limited-time, promotional services and for resources belonging
        /// to individuals no longer working at the server's site. It is not
        /// necessary to mark all permanently unavailable resources as "gone"
        /// or to keep the mark for any length of time -- that is left to the
        /// discretion of the server owner.
        /// </para>
        /// </summary>
        [DavStatusCode("Gone")]
        Gone = HttpStatusCode.Gone,

        /// <summary>
        /// <para>
        /// The precondition given in one or more of the request-header fields
        /// evaluated to false when it was tested on the server. This response
        /// code allows the client to place preconditions on the current
        /// resource meta information (header field data) and thus prevent the
        /// requested method from being applied to a resource other than the
        /// one intended.
        /// </para>
        /// </summary>
        [DavStatusCode("Precondition Failed")]
        PreconditionFailed = HttpStatusCode.PreconditionFailed,

        /// <summary>
        /// <para>
        /// The 422 (Unprocessable Entity) status code means the server
        /// understands the content type of the request entity (hence a 415
        /// (Unsupported Media Type) status code is inappropriate), and the
        /// syntax of the request entity is correct (thus a 400 (Bad Request)
        /// status code is inappropriate) but was unable to process the
        /// contained instructions. For example, this error condition may occur
        /// if an XML request body contains well-formed (i.e., syntactically
        /// correct), but semantically erroneous XML instructions.
        /// </para>
        /// <para>
        /// This status code is a WebDAV specific result code.
        /// </para>
        /// </summary>
        [DavStatusCode("Unprocessable Entity")]
        UnprocessableEntity = 422,

        /// <summary>
        /// <para>
        /// The 423 (Locked) status code means the source or destination
        /// resource of a method is locked.
        /// </para>
        /// <para>
        /// This status code is a WebDAV specific result code.
        /// </para>
        /// </summary>
        [DavStatusCode("Locked")]
        Locked = 423,

        /// <summary>
        /// <para>
        /// The 424 (Failed Dependency) status code means that the method could
        /// not be performed on the resource because the requested action
        /// depended on another action and that action failed. For example, if
        /// a command in a PROPPATCH method fails then, at minimum, the rest of
        /// the commands will also fail with 424 (Failed Dependency).
        /// </para>
        /// <para>
        /// This status code is a WebDAV specific result code.
        /// </para>
        /// </summary>
        [DavStatusCode("Failed Dependency")]
        FailedDependency = 424,

        /// <summary>
        /// <para>
        /// The server encountered an unexpected condition which prevented it
        /// from fulfilling the request.
        /// </para>
        /// </summary>
        [DavStatusCode("Internal Server Error")]
        InternalServerError = HttpStatusCode.InternalServerError,

        /// <summary>
        /// <para>
        /// The server does not support the functionality required to fulfill
        /// the request. This is the appropriate response when the server does
        /// not recognize the request method and is not capable of supporting
        /// it for any resource.
        /// </para>
        /// </summary>
        [DavStatusCode("Not Implemented")]
        NotImplemented = HttpStatusCode.NotImplemented,

        /// <summary>
        /// <para>
        /// The server, while acting as a gateway or proxy, received an invalid
        /// response from the upstream server it accessed in attempting to
        /// fulfill the request.
        /// </para>
        /// </summary>
        [DavStatusCode("Bad Gateway")]
        BadGateway = HttpStatusCode.BadGateway,

        /// <summary>
        /// <para>
        /// The server is currently unable to handle the request due to a
        /// temporary overloading or maintenance of the server. The implication
        /// is that this is a temporary condition which will be alleviated
        /// after some delay. If known, the length of the delay MAY be
        /// indicated in a 
        /// <see href="https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.37">Retry-After</see>
        /// header. If no Retry-After is given, the client SHOULD handle the
        /// response as it would for a 500 response 
        /// <see cref="InternalServerError">(Internal Server Error)</see>.
        /// </para>
        /// <para>
        /// The existence of the 503 status code does not imply that a server
        /// must use it when becoming overloaded. Some servers may wish to
        /// simply refuse the connection.
        /// </para>
        /// </summary>
        [DavStatusCode("Service Unavailable")]
        ServiceUnavailable = HttpStatusCode.ServiceUnavailable,

        /// <summary>
        /// <para>
        /// The 507 (Insufficient Storage) status code means the method could
        /// not be performed on the resource because the server is unable to
        /// store the representation needed to successfully complete the
        /// request. This condition is considered to be temporary. If the 
        /// request which received this status code was the result of a user 
        /// action, the request must not be repeated until it is requested by
        /// a separate user action.
        /// </para>
        /// <para>
        /// This status code is a WebDAV specific result code.
        /// </para>
        /// </summary>
        [DavStatusCode("Insufficient Storage")]
        InsufficientStorage = 507
    }
}
