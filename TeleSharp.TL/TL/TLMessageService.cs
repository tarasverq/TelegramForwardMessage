using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(678405636)]
    public class TLMessageService : TLAbsMessage
    {
        public override int Constructor
        {
            get
            {
                return 678405636;
            }
        }

             public int Flags {get;set;}
     public bool Out {get;set;}
     public bool Mentioned {get;set;}
     public bool MediaUnread {get;set;}
     public bool Silent {get;set;}
     public bool Post {get;set;}
     public bool Legacy {get;set;}
     public int Id {get;set;}
     public TLAbsPeer FromId {get;set;}
     public TLAbsPeer PeerId {get;set;}
     public TLMessageReplyHeader ReplyTo {get;set;}
     public int Date {get;set;}
     public TLAbsMessageAction Action {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
Out = (Flags & 2) != 0;
Mentioned = (Flags & 16) != 0;
MediaUnread = (Flags & 32) != 0;
Silent = (Flags & 8192) != 0;
Post = (Flags & 16384) != 0;
Legacy = (Flags & 524288) != 0;
Id = br.ReadInt32();
if ((Flags & 256) != 0)
FromId = (TLAbsPeer)ObjectUtils.DeserializeObject(br);
else
FromId = null;

PeerId = (TLAbsPeer)ObjectUtils.DeserializeObject(br);
if ((Flags & 8) != 0)
ReplyTo = (TLMessageReplyHeader)ObjectUtils.DeserializeObject(br);
else
ReplyTo = null;

Date = br.ReadInt32();
Action = (TLAbsMessageAction)ObjectUtils.DeserializeObject(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);






bw.Write(Id);
if ((Flags & 256) != 0)
ObjectUtils.SerializeObject(FromId,bw);
ObjectUtils.SerializeObject(PeerId,bw);
if ((Flags & 8) != 0)
ObjectUtils.SerializeObject(ReplyTo,bw);
bw.Write(Date);
ObjectUtils.SerializeObject(Action,bw);

        }
    }
}
