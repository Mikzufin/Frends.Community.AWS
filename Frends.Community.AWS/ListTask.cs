﻿using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Newtonsoft.Json.Linq;

namespace Frends.Community.AWS
{
    /// <summary>
    /// Lists objects in between prefix and delimiter.
    /// </summary>
    public class ListTask
    {
        /// <summary>
        /// Lists keys from specified S3 Bucket.
        /// You can return full response from S3, or just keys.
        /// They're located in "S3Objects"-array in resulting JObject.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>JObect { JArray("S3Objects"), JProperty }</returns>
        public static async Task<JToken> ListObjectsAsync(
            [PropertyTab] ListInput input,
            [PropertyTab] Parameters parameters,
            [PropertyTab] ListOptions options,
            CancellationToken cancellationToken
        )
        {
            if (!parameters.UseDefaultCredentials && parameters.AwsCredentials == null) parameters.IsAnyNullOrWhiteSpaceThrow();

            ListObjectsV2Response response;

            using (var client = Utilities.GetS3Client(parameters))
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = parameters.BucketName,
                    Delimiter = string.IsNullOrWhiteSpace(input.Delimiter) ? null : input.Delimiter,
                    Encoding = null,
                    FetchOwner = false,
                    MaxKeys = input.MaxKeys,

                    // Added ternary to account for Frends not including null as parameter by default.
                    Prefix = string.IsNullOrWhiteSpace(input.Prefix) ? null : input.Prefix,
                    ContinuationToken = string.IsNullOrWhiteSpace(input.ContinuationToken) ? null : input.ContinuationToken,
                    StartAfter = string.IsNullOrWhiteSpace(input.StartAfter) ? null : input.StartAfter
                };
                response = await client.ListObjectsV2Async(request, cancellationToken);
            }

            // If option is true and array has no objects.
            if (options.ThrowErrorIfNoFilesFound && response.S3Objects.Count == 0)
                throw new ArgumentException($"No objects found with supplied parameters: {nameof(input.Prefix)}, {nameof(input.Delimiter)}, {nameof(input.StartAfter)}.");

            return options.FullResponse ? JToken.FromObject(response) : JToken.FromObject(response)["S3Objects"];
        }
    }
}
