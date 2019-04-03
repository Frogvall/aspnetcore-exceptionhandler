using System;
using Amazon.XRay.Recorder.Core;

namespace Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling
{
    public class AwsXRayExceptionListener
    {
        public static void AddExceptionMetadataToAwsXRay(Exception exception)
        {
            try
            {
                AWSXRayRecorder.Instance.AddException(exception);
            }
            catch
            {
                //Best effort, do not panic on failure.
            }
        }
    }
}