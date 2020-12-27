using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL.Contacts
{
	[TLObject(698914348)]
    public class TLRequestBlockFromReplies : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return 698914348;
            }
        }

                public int Flags {get;set;}
        public bool DeleteMessage {get;set;}
        public bool DeleteHistory {get;set;}
        public bool ReportSpam {get;set;}
        public int MsgId {get;set;}
        public TLAbsUpdates Response{ get; set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
DeleteMessage = (Flags & 1) != 0;
DeleteHistory = (Flags & 2) != 0;
ReportSpam = (Flags & 4) != 0;
MsgId = br.ReadInt32();

        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);



bw.Write(MsgId);

        }
		public override void DeserializeResponse(BinaryReader br)
		{
			Response = (TLAbsUpdates)ObjectUtils.DeserializeObject(br);

		}
    }
}
