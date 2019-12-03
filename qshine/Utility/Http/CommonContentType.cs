using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Utility.Http
{
    /// <summary>
    /// Define common html content type.
    /// The completed list of context type could be found in 
    /// <a href="https://www.iana.org/assignments/media-types/media-types.xhtml">IANA</a>
    /// </summary>
    public enum CommonContentType
    {
        /// <summary>
        /// Undefined
        /// </summary>
        [StringValue("-")]
        UnDefined,
        /// <summary>
        /// Default - undefined type
        /// </summary>
        [StringValue("")]
        Unknown,
        /// <summary>
        /// Json content type
        /// </summary>
        [StringValue("text/plain")]
        PlainText,

        /// <summary>
        /// Json content type
        /// </summary>
        [StringValue("application/json")]
        Json,

        /// <summary>
        /// XML content type
        /// </summary>
        [StringValue("application/xml")]
        Xml,

        /// <summary>
        /// XML Utf-8 encoded content type
        /// </summary>
        [StringValue("application/xml; charset=utf-8")]
        XmlUtf8,

        /// <summary>
        /// XHTML content type
        /// </summary>
        [StringValue("application/xhtml+xml")]
        Xhtml,

        /// <summary>
        /// Binary content type
        /// </summary>
        [StringValue("application/octet-stream")]
        Binary,

        /// <summary>
        /// Jpeg image content type
        /// </summary>
        [StringValue("image/jpeg")]
        Jpeg,

        /// <summary>
        /// Png image content type
        /// </summary>
        [StringValue("image/png")]
        Png,

        /// <summary>
        /// PDF content type
        /// </summary>
        [StringValue("application/pdf")]
        Pdf,

        /// <summary>
        /// MS Excel (xls) content type
        /// </summary>
        [StringValue("application/vnd.ms-excel")]
        MsExcel,

        /// <summary>
        /// MS Word (doc) content type
        /// </summary>
        [StringValue("application/msword")]
        MsWord,

        /// <summary>
        /// MS Powerpoint (ppt) content type
        /// </summary>
        [StringValue("application/vnd.ms-powerpoint")]
        MsPpt,

        /// <summary>
        /// MS/Open Excel (xlsx) content type
        /// </summary>
        [StringValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        OpenExcel,

        /// <summary>
        /// MS/Open Word (docx) content type
        /// </summary>
        [StringValue("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
        OpenWord,

        /// <summary>
        /// MS/Open Powerpoint (pptx) content type
        /// </summary>
        [StringValue("application/vnd.openxmlformats-officedocument.presentationml.presentation")]
        OpenPpt,

        /// <summary>
        /// multipart form data content type 
        /// <a href="https://tools.ietf.org/html/rfc7578">[RFC7578]</a>
        /// It's best to send binary data along with form data.
        /// You must choose boundary string text uniquely that does not occur in any of the data.
        /// </summary>
        [StringValue("multipart/form-data")]
        MultipartFormData,

        /// <summary>
        /// Default application form urlencoded context type.
        /// This is the most form format for text and binary.
        /// </summary>
        [StringValue("application/x-www-form-urlencoded")]
        FormUrlencoded,

        /// <summary>
        /// multipart mixed data content type. One of common multipart message 
        /// <a href="https://tools.ietf.org/html/rfc2045">[RFC2045]</a>
        /// <a href="https://tools.ietf.org/html/rfc2046">[RFC2046]</a>
        /// </summary>
        [StringValue("multipart/mixed")]
        MultipartMixed,
    }
}
