using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL.Payments
{
	[TLObject(-666824391)]
    public class TLPaymentVerificationNeeded : TLAbsPaymentResult
    {
        public override int Constructor
        {
            get
            {
                return -666824391;
            }
        }

             public string Url {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Url = StringUtil.Deserialize(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            StringUtil.Serialize(Url,bw);

        }
    }
}
