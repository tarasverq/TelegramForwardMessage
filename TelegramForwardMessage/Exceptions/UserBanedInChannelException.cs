using System;
using System.Runtime.Serialization;

namespace TelegramForwardMessage.Exceptions
{
    [Serializable]
    public class UserBanedInChannelException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public UserBanedInChannelException()
        {
        }

        public UserBanedInChannelException(string message) : base(message)
        {
        }

        public UserBanedInChannelException(string message, Exception inner) : base(message, inner)
        {
        }

        protected UserBanedInChannelException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}