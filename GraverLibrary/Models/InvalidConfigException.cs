using System;

namespace GraverLibrary.Models
{
    public class InvalidConfigException : Exception
    {
        public InvalidConfigException(string message) :base(message){
        }
    }
}
