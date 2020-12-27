using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL.Messages
{
	[TLObject(-1594999949)]
    public class TLRequestGetDialogs : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return -1594999949;
            }
        }

                public int Flags {get;set;}
        public bool ExcludePinned {get;set;}
        public int? FolderId {get;set;}
        public int OffsetDate {get;set;}
        public int OffsetId {get;set;}
        public TLAbsInputPeer OffsetPeer {get;set;}
        public int Limit {get;set;}
        public int Hash {get;set;}
        public Messages.TLAbsDialogs Response{ get; set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
ExcludePinned = (Flags & 1) != 0;
if ((Flags & 2) != 0)
FolderId = br.ReadInt32();
else
FolderId = null;

OffsetDate = br.ReadInt32();
OffsetId = br.ReadInt32();
OffsetPeer = (TLAbsInputPeer)ObjectUtils.DeserializeObject(br);
Limit = br.ReadInt32();
Hash = br.ReadInt32();

        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);

if ((Flags & 2) != 0)
bw.Write(FolderId.Value);
bw.Write(OffsetDate);
bw.Write(OffsetId);
ObjectUtils.SerializeObject(OffsetPeer,bw);
bw.Write(Limit);
bw.Write(Hash);

        }
		public override void DeserializeResponse(BinaryReader br)
		{
			Response = (Messages.TLAbsDialogs)ObjectUtils.DeserializeObject(br);

		}
    }
}
