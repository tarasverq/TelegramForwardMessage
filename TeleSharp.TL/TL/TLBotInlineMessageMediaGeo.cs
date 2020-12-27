using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(85477117)]
    public class TLBotInlineMessageMediaGeo : TLAbsBotInlineMessage
    {
        public override int Constructor
        {
            get
            {
                return 85477117;
            }
        }

             public int Flags {get;set;}
     public TLAbsGeoPoint Geo {get;set;}
     public int? Heading {get;set;}
     public int? Period {get;set;}
     public int? ProximityNotificationRadius {get;set;}
     public TLAbsReplyMarkup ReplyMarkup {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
Geo = (TLAbsGeoPoint)ObjectUtils.DeserializeObject(br);
if ((Flags & 1) != 0)
Heading = br.ReadInt32();
else
Heading = null;

if ((Flags & 2) != 0)
Period = br.ReadInt32();
else
Period = null;

if ((Flags & 8) != 0)
ProximityNotificationRadius = br.ReadInt32();
else
ProximityNotificationRadius = null;

if ((Flags & 4) != 0)
ReplyMarkup = (TLAbsReplyMarkup)ObjectUtils.DeserializeObject(br);
else
ReplyMarkup = null;


        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);
ObjectUtils.SerializeObject(Geo,bw);
if ((Flags & 1) != 0)
bw.Write(Heading.Value);
if ((Flags & 2) != 0)
bw.Write(Period.Value);
if ((Flags & 8) != 0)
bw.Write(ProximityNotificationRadius.Value);
if ((Flags & 4) != 0)
ObjectUtils.SerializeObject(ReplyMarkup,bw);

        }
    }
}
