using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(569137759)]
    public class TLSecurePlainEmail : TLAbsSecurePlainData
    {
        public override int Constructor
        {
            get
            {
                return 569137759;
            }
        }

             public string Email {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Email = StringUtil.Deserialize(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            StringUtil.Serialize(Email,bw);

        }
    }
}
