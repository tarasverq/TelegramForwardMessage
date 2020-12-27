using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(-668906175)]
    public class TLPhotoPathSize : TLAbsPhotoSize
    {
        public override int Constructor
        {
            get
            {
                return -668906175;
            }
        }

             public string Type {get;set;}
     public byte[] Bytes {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Type = StringUtil.Deserialize(br);
Bytes = BytesUtil.Deserialize(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            StringUtil.Serialize(Type,bw);
BytesUtil.Serialize(Bytes,bw);

        }
    }
}
