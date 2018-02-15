﻿using System;
using System.IO;
using System.ComponentModel;
using Frends.Tasks.Attributes;
using Newtonsoft.Json.Linq;

namespace Frends.Community.AWS
{
    #region Download argument classes
    /// <summary>
    ///     Input class, you can download whole directories or single files.
    /// </summary>
    [DisplayName("Input")]
    public class DownloadInput
    {
        /// <summary>
        ///     Uses different method to download. Whole directory gets all objects recursively.
        /// </summary>
        
        public Boolean DownloadWholeDirectory { get; set; }

        /// <summary>
        ///     Downloads all objects with this prefix. Creates folder structure.
        ///     Examples: folder/, this/is/prefix/
        /// </summary>
        [ConditionalDisplay(nameof(DownloadWholeDirectory), true)]
        [DefaultDisplayType(DisplayType.Text)]
        public string SourcePrefix { get; set; }

        /// <summary>
        ///     Downloads single object (file).
        ///     Example: folder/file.txt, this/is/prefix/file
        /// </summary>
        [ConditionalDisplay(nameof(DownloadWholeDirectory), false)]
        [DefaultDisplayType(DisplayType.Text)]
        public string SourcePrefixAndKey { get; set; }

        /// <summary>
        ///     Directory to create folders and files to.
        ///     Use trailing backlash ( \ ).
        /// </summary>
        [ConditionalDisplay(nameof(DownloadWholeDirectory), true)]
        [DefaultDisplayType(DisplayType.Text)]
        public string DestinationPath { get; set; }

        /// <summary>
        ///     Folder to write file.
        ///     You can use different filename.
        /// </summary>
        [ConditionalDisplay(nameof(DownloadWholeDirectory), false)]
        [DefaultDisplayType(DisplayType.Text)]
        public string DestinationPathAndFilename { get; set; }
    }
    #endregion

    #region Download Result Tokens
    /// <summary>
    ///     Result toke
    /// </summary>
    internal class DownloadResultToken
    {
        internal string ObjectKey { get; set; }
        internal long Size { get; set; }
        private string filePath;
        internal string FilePath
        {
            get { return filePath; }
            set {
                // just to ensure our string.replace from objectkey has created file and path is correct
                if (File.Exists(value))
                    filePath = value;
                else
                    throw new Exception($"AWS Download File Error; Cannot find {value} from filesystem. ");
            }
        }

        internal DownloadResultToken() { }
        internal DownloadResultToken(string ObjectKey, string FilePath, long Size)
        {
            this.ObjectKey = ObjectKey;
            filePath = FilePath;
            this.Size = Size;
        }

        // newtonsofts style of jobject creation.
        internal JToken ToJToken()
        {
            return (JObject)JToken.FromObject(this);
        }
    }    
    #endregion

    #region List
    /// <summary>
    ///     Input parameter class for ListObjectsAsync
    /// </summary>
    [DisplayName("Input")]
    public class ListInput
    {
        /// <summary>
        ///     Object prefix ( folder path ).
        ///     Use this to set prefix for each file.
        ///     Default: null
        /// </summary>
        [DefaultValue(null)]
        [DefaultDisplayType(DisplayType.Text)]
        public string Prefix { get; set; }

        /// <summary>
        ///     Delimiter.
        ///     Use in conjuction with prefix to limit results to specific level of the flat namespace hierarchy.
        ///     See: http://docs.aws.amazon.com/AmazonS3/latest/dev/ListingKeysHierarchy.html
        /// </summary>
        [DefaultValue(null)]
        [DefaultDisplayType(DisplayType.Text)]
        public string Delimiter { get; set; }

        /// <summary>
        ///     Max number of keys returned.
        /// </summary>
        [DefaultValue(100)]
        [DefaultDisplayType(DisplayType.Expression)]
        public int MaxKeys { get; set; }

        /// <summary>
        ///     A key to start the listing from.
        ///     Default: null
        /// </summary>
        [DefaultValue(null)]
        [DefaultDisplayType(DisplayType.Text)]
        public string StartAfter { get; set; }

        /// <summary>
        ///     If previous response is truncated, use the ContinuationToken from that response here, to continue listing.
        ///     Default: null
        /// </summary>
        [DefaultValue(null)]
        [DefaultDisplayType(DisplayType.Text)]
        public string ContinuationToken { get; set; }
    }



    /// <summary>
    ///     Options class.
    /// </summary>
    [DisplayName("Options")]
    public class ListOptions
    {
        /// <summary>
        ///     True will return full response, false will return object keys only.
        /// </summary>
        [DefaultValue(false)]
        public bool FullResponse { get; set; }

        /// <summary>
        ///     Throw error if reponse has no items in "S3Objects" array.
        /// </summary>
        [DefaultValue(true)]
        public bool ThrowErrorIfNoFilesFound { get; set; }
    }
    #endregion

    #region Upload
    /// <summary>
    ///     Input filepath and filemask.
    /// </summary>
    [DisplayName("Input")]
    public class UploadInput
    {
        /// <summary>
        ///     Path to folder.
        ///     ( c:\temp\ , \\network\folder )
        /// </summary>
        [DefaultValue(@"c:\temp\")]
        [DefaultDisplayType(DisplayType.Text)]
        public string FilePath { get; set; }

        /// <summary>
        ///     Windows-style filemask, ( *.* , ?_file.*, foo_*.txt ).
        ///     Empty field = *.*
        /// </summary>
        [DefaultValue(@"*.*")]
        [DefaultDisplayType(DisplayType.Text)]
        public string FileMask { get; set; }
        /// <summary>
        ///     S3 prefix for files.
        /// </summary>
        [DefaultValue(@"")]
        [DefaultDisplayType(DisplayType.Text)]
        public string Prefix { get; set; }
    }

    /// <summary>
    ///     Task behaviour.
    ///     Defaults work fine.
    /// </summary>
    [DisplayName("Options")]
    public class UploadOptions
    {
        /// <summary>
        ///     If there are no files in the path matching the filemask supplied,
        ///     throw error.
        /// </summary>
        [DefaultValue(true)]
        public bool ThrowErrorIfNoMatch { get; set; }

        /// <summary>
        ///     If you wish, you can return object keys from S3
        ///     ( format: prefix/prefix/filename )
        /// </summary>
        [DefaultValue(false)]
        public bool ReturnListOfObjectKeys { get; set; }

        /// <summary>
        ///     You can specify Storage Class for uploaded files.
        ///     Standard is default.
        ///     Consult AWS S3 Documentation for others.
        /// </summary>
        public StorageClasses StorageClass { get; set; }
    }
    #endregion

    #region Parameters for all!
    /// <summary>
    ///     Parameter class with username and keys.
    /// </summary>
    public class Parameters
    {
        /// <summary>
        ///     AWS Bucket name with Path
        ///     Example: bucketname/path/to/directory or #env.variable.
        /// </summary>
        [DefaultDisplayType(DisplayType.Text)]
        public string BucketName { get; set; }

        /// <summary>
        ///     Key name for Amazon s3 File transfer aws_access_key_id
        ///     Use #env.variable.
        /// </summary>
        [PasswordPropertyText(true)]
        [DefaultDisplayType(DisplayType.Expression)]
        public string AWSAccessKeyID { get; set; }

        /// <summary>
        ///     Secret  key name for Amazon s3 File transfer aws_secret_access_key
        ///     Use #env.variable.
        /// </summary>
        [PasswordPropertyText(true)]
        [DefaultDisplayType(DisplayType.Expression)]
        public string AWSSecretAccessKey { get; set; }

        /// <summary>
        ///     Region selection, default EUWest1.
        /// </summary>
        [DisplayName("Region")]
        public Regions Region { get; set; }
    }
    #endregion

    #region Enumerations

    #pragma warning disable 1591
    public enum Regions
    {
        EUWest1, EUWest2, EUCentral1,
        APNortheast1, APNortheast2, APSouth1, APSoutheast1, APSoutheast2,
        CACentral1, CNNorth1, SAEast1,
        USEast1, USEast2, USGovCloudWest1, USWest1, USWest2
    }
    public enum StorageClasses
    {
        Standard, StandardInfrequent, Reduced, Glacier
    }
    #pragma warning restore 1591
    #endregion
}
