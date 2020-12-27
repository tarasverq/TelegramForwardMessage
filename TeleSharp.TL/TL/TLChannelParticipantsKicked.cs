using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(-1548400251)]
    public class TLChannelParticipantsKicked : TLAbsChannelParticipantsFilter
    {
        public override int Constructor
        {
            get
            {
                return -1548400251;
            }
        }

             public string Q {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Q = StringUtil.Deserialize(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            StringUtil.Serialize(Q,bw);

        }
    }
}
