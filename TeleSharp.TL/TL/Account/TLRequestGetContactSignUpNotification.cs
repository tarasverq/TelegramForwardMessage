using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL.Account
{
	[TLObject(-1626880216)]
    public class TLRequestGetContactSignUpNotification : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return -1626880216;
            }
        }

                public bool Response{ get; set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            
        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            
        }
		public override void DeserializeResponse(BinaryReader br)
		{
			Response = BoolUtil.Deserialize(br);

		}
    }
}
