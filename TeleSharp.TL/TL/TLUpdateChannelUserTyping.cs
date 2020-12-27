using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(-13975905)]
    public class TLUpdateChannelUserTyping : TLAbsUpdate
    {
        public override int Constructor
        {
            get
            {
                return -13975905;
            }
        }

             public int Flags {get;set;}
     public int ChannelId {get;set;}
     public int? TopMsgId {get;set;}
     public int UserId {get;set;}
     public TLAbsSendMessageAction Action {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
ChannelId = br.ReadInt32();
if ((Flags & 1) != 0)
TopMsgId = br.ReadInt32();
else
TopMsgId = null;

UserId = br.ReadInt32();
Action = (TLAbsSendMessageAction)ObjectUtils.DeserializeObject(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);
bw.Write(ChannelId);
if ((Flags & 1) != 0)
bw.Write(TopMsgId.Value);
bw.Write(UserId);
ObjectUtils.SerializeObject(Action,bw);

        }
    }
}
