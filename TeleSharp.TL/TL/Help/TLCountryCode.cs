using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL.Help
{
	[TLObject(1107543535)]
    public class TLCountryCode : TLObject
    {
        public override int Constructor
        {
            get
            {
                return 1107543535;
            }
        }

             public int Flags {get;set;}
     public string CountryCode {get;set;}
     public TLVector<string> Prefixes {get;set;}
     public TLVector<string> Patterns {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
CountryCode = StringUtil.Deserialize(br);
if ((Flags & 1) != 0)
Prefixes = (TLVector<string>)ObjectUtils.DeserializeVector<string>(br);
else
Prefixes = null;

if ((Flags & 2) != 0)
Patterns = (TLVector<string>)ObjectUtils.DeserializeVector<string>(br);
else
Patterns = null;


        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);
StringUtil.Serialize(CountryCode,bw);
if ((Flags & 1) != 0)
ObjectUtils.SerializeObject(Prefixes,bw);
if ((Flags & 2) != 0)
ObjectUtils.SerializeObject(Patterns,bw);

        }
    }
}
