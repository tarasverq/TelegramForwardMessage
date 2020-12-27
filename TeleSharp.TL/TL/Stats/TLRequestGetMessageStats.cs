using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL.Stats
{
	[TLObject(-1226791947)]
    public class TLRequestGetMessageStats : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return -1226791947;
            }
        }

                public int Flags {get;set;}
        public bool Dark {get;set;}
        public TLAbsInputChannel Channel {get;set;}
        public int MsgId {get;set;}
        public Stats.TLMessageStats Response{ get; set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
Dark = (Flags & 1) != 0;
Channel = (TLAbsInputChannel)ObjectUtils.DeserializeObject(br);
MsgId = br.ReadInt32();

        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);

ObjectUtils.SerializeObject(Channel,bw);
bw.Write(MsgId);

        }
		public override void DeserializeResponse(BinaryReader br)
		{
			Response = (Stats.TLMessageStats)ObjectUtils.DeserializeObject(br);

		}
    }
}
