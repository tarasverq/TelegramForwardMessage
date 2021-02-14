using System;
using System.Runtime.Serialization;

namespace TelegramForwardMessage.Exceptions
{
    [Serializable]
    public class NoLinkedChatException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public string Username { get; set; }
        public  int ChatId { get; set; }
        public NoLinkedChatException()
        {
        }

        public NoLinkedChatException(string message) : base(message)
        {
        }

        public NoLinkedChatException(string message, Exception inner) : base(message, inner)
        {
        }

        protected NoLinkedChatException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}