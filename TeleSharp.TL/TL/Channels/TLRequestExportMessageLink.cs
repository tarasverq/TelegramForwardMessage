using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL.Channels
{
	[TLObject(-432034325)]
    public class TLRequestExportMessageLink : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return -432034325;
            }
        }

                public int Flags {get;set;}
        public bool Grouped {get;set;}
        public bool Thread {get;set;}
        public TLAbsInputChannel Channel {get;set;}
        public int Id {get;set;}
        public TLExportedMessageLink Response{ get; set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
Grouped = (Flags & 1) != 0;
Thread = (Flags & 2) != 0;
Channel = (TLAbsInputChannel)ObjectUtils.DeserializeObject(br);
Id = br.ReadInt32();

        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);


ObjectUtils.SerializeObject(Channel,bw);
bw.Write(Id);

        }
		public override void DeserializeResponse(BinaryReader br)
		{
			Response = (TLExportedMessageLink)ObjectUtils.DeserializeObject(br);

		}
    }
}
