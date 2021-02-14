using System;
using System.Runtime.Serialization;

namespace TelegramForwardMessage.Exceptions
{
    [Serializable]
    public class NoDiscussionsException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //
        public string MessageText { get; set; }
        public string Username { get; set; }
        public  int ChatId { get; set; }
        public  int MessageId { get; set; }
        public NoDiscussionsException()
        {
        }

        public NoDiscussionsException(string message) : base(message)
        {
        }

        public NoDiscussionsException(string message, Exception inner) : base(message, inner)
        {
        }

        protected NoDiscussionsException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}