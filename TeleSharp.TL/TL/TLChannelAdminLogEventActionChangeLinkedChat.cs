using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(-1569748965)]
    public class TLChannelAdminLogEventActionChangeLinkedChat : TLAbsChannelAdminLogEventAction
    {
        public override int Constructor
        {
            get
            {
                return -1569748965;
            }
        }

             public int PrevValue {get;set;}
     public int NewValue {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            PrevValue = br.ReadInt32();
NewValue = br.ReadInt32();

        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(PrevValue);
bw.Write(NewValue);

        }
    }
}
