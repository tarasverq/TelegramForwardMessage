using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(1163625789)]
    public class TLMessageViews : TLObject
    {
        public override int Constructor
        {
            get
            {
                return 1163625789;
            }
        }

             public int Flags {get;set;}
     public int? Views {get;set;}
     public int? Forwards {get;set;}
     public TLMessageReplies Replies {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
if ((Flags & 1) != 0)
Views = br.ReadInt32();
else
Views = null;

if ((Flags & 2) != 0)
Forwards = br.ReadInt32();
else
Forwards = null;

if ((Flags & 4) != 0)
Replies = (TLMessageReplies)ObjectUtils.DeserializeObject(br);
else
Replies = null;


        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);
if ((Flags & 1) != 0)
bw.Write(Views.Value);
if ((Flags & 2) != 0)
bw.Write(Forwards.Value);
if ((Flags & 4) != 0)
ObjectUtils.SerializeObject(Replies,bw);

        }
    }
}
