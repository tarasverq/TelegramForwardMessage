using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL.Help
{
	[TLObject(-1014526429)]
    public class TLCountry : TLObject
    {
        public override int Constructor
        {
            get
            {
                return -1014526429;
            }
        }

             public int Flags {get;set;}
     public bool Hidden {get;set;}
     public string Iso2 {get;set;}
     public string DefaultName {get;set;}
     public string Name {get;set;}
     public TLVector<Help.TLCountryCode> CountryCodes {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
Hidden = (Flags & 1) != 0;
Iso2 = StringUtil.Deserialize(br);
DefaultName = StringUtil.Deserialize(br);
if ((Flags & 2) != 0)
Name = StringUtil.Deserialize(br);
else
Name = null;

CountryCodes = (TLVector<Help.TLCountryCode>)ObjectUtils.DeserializeVector<Help.TLCountryCode>(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);

StringUtil.Serialize(Iso2,bw);
StringUtil.Serialize(DefaultName,bw);
if ((Flags & 2) != 0)
StringUtil.Serialize(Name,bw);
ObjectUtils.SerializeObject(CountryCodes,bw);

        }
    }
}
