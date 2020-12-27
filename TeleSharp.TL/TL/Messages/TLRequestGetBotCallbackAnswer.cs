using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL.Messages
{
	[TLObject(-1824339449)]
    public class TLRequestGetBotCallbackAnswer : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return -1824339449;
            }
        }

                public int Flags {get;set;}
        public bool Game {get;set;}
        public TLAbsInputPeer Peer {get;set;}
        public int MsgId {get;set;}
        public byte[] Data {get;set;}
        public TLAbsInputCheckPasswordSRP Password {get;set;}
        public Messages.TLBotCallbackAnswer Response{ get; set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
Game = (Flags & 2) != 0;
Peer = (TLAbsInputPeer)ObjectUtils.DeserializeObject(br);
MsgId = br.ReadInt32();
if ((Flags & 1) != 0)
Data = BytesUtil.Deserialize(br);
else
Data = null;

if ((Flags & 4) != 0)
Password = (TLAbsInputCheckPasswordSRP)ObjectUtils.DeserializeObject(br);
else
Password = null;


        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);

ObjectUtils.SerializeObject(Peer,bw);
bw.Write(MsgId);
if ((Flags & 1) != 0)
BytesUtil.Serialize(Data,bw);
if ((Flags & 4) != 0)
ObjectUtils.SerializeObject(Password,bw);

        }
		public override void DeserializeResponse(BinaryReader br)
		{
			Response = (Messages.TLBotCallbackAnswer)ObjectUtils.DeserializeObject(br);

		}
    }
}
