using System;

namespace GraverLibrary.Services.Common
{
    public class GraverServiceException : Exception
    {
        public GraverServiceException(string message) : base(message){ }
    }

    }
