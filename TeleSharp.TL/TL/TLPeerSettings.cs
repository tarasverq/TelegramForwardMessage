using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(1933519201)]
    public class TLPeerSettings : TLObject
    {
        public override int Constructor
        {
            get
            {
                return 1933519201;
            }
        }

             public int Flags {get;set;}
     public bool ReportSpam {get;set;}
     public bool AddContact {get;set;}
     public bool BlockContact {get;set;}
     public bool ShareContact {get;set;}
     public bool NeedContactsException {get;set;}
     public bool ReportGeo {get;set;}
     public bool Autoarchived {get;set;}
     public int? GeoDistance {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
ReportSpam = (Flags & 1) != 0;
AddContact = (Flags & 2) != 0;
BlockContact = (Flags & 4) != 0;
ShareContact = (Flags & 8) != 0;
NeedContactsException = (Flags & 16) != 0;
ReportGeo = (Flags & 32) != 0;
Autoarchived = (Flags & 128) != 0;
if ((Flags & 64) != 0)
GeoDistance = br.ReadInt32();
else
GeoDistance = null;


        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);







if ((Flags & 64) != 0)
bw.Write(GeoDistance.Value);

        }
    }
}
