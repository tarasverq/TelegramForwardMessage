using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(-1495959709)]
    public class TLMessageReplyHeader : TLObject
    {
        public override int Constructor
        {
            get
            {
                return -1495959709;
            }
        }

             public int Flags {get;set;}
     public int ReplyToMsgId {get;set;}
     public TLAbsPeer ReplyToPeerId {get;set;}
     public int? ReplyToTopId {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
ReplyToMsgId = br.ReadInt32();
if ((Flags & 1) != 0)
ReplyToPeerId = (TLAbsPeer)ObjectUtils.DeserializeObject(br);
else
ReplyToPeerId = null;

if ((Flags & 2) != 0)
ReplyToTopId = br.ReadInt32();
else
ReplyToTopId = null;


        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);
bw.Write(ReplyToMsgId);
if ((Flags & 1) != 0)
ObjectUtils.SerializeObject(ReplyToPeerId,bw);
if ((Flags & 2) != 0)
bw.Write(ReplyToTopId.Value);

        }
    }
}
