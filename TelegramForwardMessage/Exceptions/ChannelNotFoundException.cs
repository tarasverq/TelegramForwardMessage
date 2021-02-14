using System;
using System.Runtime.Serialization;

namespace TelegramForwardMessage.Exceptions
{
    [Serializable]
    public class ChannelNotFoundException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public string ChannelName { get; set; }
        public ChannelNotFoundException()
        {
        }

        public ChannelNotFoundException(string message) : base(message)
        {
        }

        public ChannelNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ChannelNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}