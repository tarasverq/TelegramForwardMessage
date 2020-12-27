using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(-1901828938)]
    public class TLStatsGraph : TLAbsStatsGraph
    {
        public override int Constructor
        {
            get
            {
                return -1901828938;
            }
        }

             public int Flags {get;set;}
     public TLDataJSON Json {get;set;}
     public string ZoomToken {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
Json = (TLDataJSON)ObjectUtils.DeserializeObject(br);
if ((Flags & 1) != 0)
ZoomToken = StringUtil.Deserialize(br);
else
ZoomToken = null;


        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);
ObjectUtils.SerializeObject(Json,bw);
if ((Flags & 1) != 0)
StringUtil.Serialize(ZoomToken,bw);

        }
    }
}
