using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL.Contacts
{
	[TLObject(-750207932)]
    public class TLRequestGetLocated : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return -750207932;
            }
        }

                public int Flags {get;set;}
        public bool Background {get;set;}
        public TLAbsInputGeoPoint GeoPoint {get;set;}
        public int? SelfExpires {get;set;}
        public TLAbsUpdates Response{ get; set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
Background = (Flags & 2) != 0;
GeoPoint = (TLAbsInputGeoPoint)ObjectUtils.DeserializeObject(br);
if ((Flags & 1) != 0)
SelfExpires = br.ReadInt32();
else
SelfExpires = null;


        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);

ObjectUtils.SerializeObject(GeoPoint,bw);
if ((Flags & 1) != 0)
bw.Write(SelfExpires.Value);

        }
		public override void DeserializeResponse(BinaryReader br)
		{
			Response = (TLAbsUpdates)ObjectUtils.DeserializeObject(br);

		}
    }
}
