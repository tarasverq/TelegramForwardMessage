using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL.Messages
{
	[TLObject(-170029155)]
    public class TLDiscussionMessage : TLObject
    {
        public override int Constructor
        {
            get
            {
                return -170029155;
            }
        }

             public int Flags {get;set;}
     public TLVector<TLAbsMessage> Messages {get;set;}
     public int? MaxId {get;set;}
     public int? ReadInboxMaxId {get;set;}
     public int? ReadOutboxMaxId {get;set;}
     public TLVector<TLAbsChat> Chats {get;set;}
     public TLVector<TLAbsUser> Users {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
Messages = (TLVector<TLAbsMessage>)ObjectUtils.DeserializeVector<TLAbsMessage>(br);
if ((Flags & 1) != 0)
MaxId = br.ReadInt32();
else
MaxId = null;

if ((Flags & 2) != 0)
ReadInboxMaxId = br.ReadInt32();
else
ReadInboxMaxId = null;

if ((Flags & 4) != 0)
ReadOutboxMaxId = br.ReadInt32();
else
ReadOutboxMaxId = null;

Chats = (TLVector<TLAbsChat>)ObjectUtils.DeserializeVector<TLAbsChat>(br);
Users = (TLVector<TLAbsUser>)ObjectUtils.DeserializeVector<TLAbsUser>(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);
ObjectUtils.SerializeObject(Messages,bw);
if ((Flags & 1) != 0)
bw.Write(MaxId.Value);
if ((Flags & 2) != 0)
bw.Write(ReadInboxMaxId.Value);
if ((Flags & 4) != 0)
bw.Write(ReadOutboxMaxId.Value);
ObjectUtils.SerializeObject(Chats,bw);
ObjectUtils.SerializeObject(Users,bw);

        }
    }
}
