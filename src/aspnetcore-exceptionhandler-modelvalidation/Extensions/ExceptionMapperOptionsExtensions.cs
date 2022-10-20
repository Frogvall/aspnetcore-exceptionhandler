namespace Frogvall.AspNetCore.ExceptionHandling.Mapper 
{
    public static class ExceptionMapperOptionsExtensions 
    {
        private static int _modelValidationErrorCode = 0;
        public static ExceptionMapperOptions SetModelValidationErrorCode(this ExceptionMapperOptions options, int errorCode)
        {
            _modelValidationErrorCode = errorCode;
            return options;
        }

        public static int GetModelValidationErrorCode(this ExceptionMapperOptions options)
        {
            return _modelValidationErrorCode;
        }
    }
}