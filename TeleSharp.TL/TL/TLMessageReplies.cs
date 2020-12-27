using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(1093204652)]
    public class TLMessageReplies : TLObject
    {
        public override int Constructor
        {
            get
            {
                return 1093204652;
            }
        }

             public int Flags {get;set;}
     public bool Comments {get;set;}
     public int Replies {get;set;}
     public int RepliesPts {get;set;}
     public TLVector<TLAbsPeer> RecentRepliers {get;set;}
     public int? ChannelId {get;set;}
     public int? MaxId {get;set;}
     public int? ReadMaxId {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
Comments = (Flags & 1) != 0;
Replies = br.ReadInt32();
RepliesPts = br.ReadInt32();
if ((Flags & 2) != 0)
RecentRepliers = (TLVector<TLAbsPeer>)ObjectUtils.DeserializeVector<TLAbsPeer>(br);
else
RecentRepliers = null;

if ((Flags & 1) != 0)
ChannelId = br.ReadInt32();
else
ChannelId = null;

if ((Flags & 4) != 0)
MaxId = br.ReadInt32();
else
MaxId = null;

if ((Flags & 8) != 0)
ReadMaxId = br.ReadInt32();
else
ReadMaxId = null;


        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);

bw.Write(Replies);
bw.Write(RepliesPts);
if ((Flags & 2) != 0)
ObjectUtils.SerializeObject(RecentRepliers,bw);
if ((Flags & 1) != 0)
bw.Write(ChannelId.Value);
if ((Flags & 4) != 0)
bw.Write(MaxId.Value);
if ((Flags & 8) != 0)
bw.Write(ReadMaxId.Value);

        }
    }
}
