using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(-531931925)]
    public class TLChannelParticipantsMentions : TLAbsChannelParticipantsFilter
    {
        public override int Constructor
        {
            get
            {
                return -531931925;
            }
        }

             public int Flags {get;set;}
     public string Q {get;set;}
     public int? TopMsgId {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
if ((Flags & 1) != 0)
Q = StringUtil.Deserialize(br);
else
Q = null;

if ((Flags & 2) != 0)
TopMsgId = br.ReadInt32();
else
TopMsgId = null;


        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);
if ((Flags & 1) != 0)
StringUtil.Serialize(Q,bw);
if ((Flags & 2) != 0)
bw.Write(TopMsgId.Value);

        }
    }
}
