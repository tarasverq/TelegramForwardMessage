using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(1601666510)]
    public class TLMessageFwdHeader : TLObject
    {
        public override int Constructor
        {
            get
            {
                return 1601666510;
            }
        }

             public int Flags {get;set;}
     public TLAbsPeer FromId {get;set;}
     public string FromName {get;set;}
     public int Date {get;set;}
     public int? ChannelPost {get;set;}
     public string PostAuthor {get;set;}
     public TLAbsPeer SavedFromPeer {get;set;}
     public int? SavedFromMsgId {get;set;}
     public string PsaType {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
if ((Flags & 1) != 0)
FromId = (TLAbsPeer)ObjectUtils.DeserializeObject(br);
else
FromId = null;

if ((Flags & 32) != 0)
FromName = StringUtil.Deserialize(br);
else
FromName = null;

Date = br.ReadInt32();
if ((Flags & 4) != 0)
ChannelPost = br.ReadInt32();
else
ChannelPost = null;

if ((Flags & 8) != 0)
PostAuthor = StringUtil.Deserialize(br);
else
PostAuthor = null;

if ((Flags & 16) != 0)
SavedFromPeer = (TLAbsPeer)ObjectUtils.DeserializeObject(br);
else
SavedFromPeer = null;

if ((Flags & 16) != 0)
SavedFromMsgId = br.ReadInt32();
else
SavedFromMsgId = null;

if ((Flags & 64) != 0)
PsaType = StringUtil.Deserialize(br);
else
PsaType = null;


        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);
if ((Flags & 1) != 0)
ObjectUtils.SerializeObject(FromId,bw);
if ((Flags & 32) != 0)
StringUtil.Serialize(FromName,bw);
bw.Write(Date);
if ((Flags & 4) != 0)
bw.Write(ChannelPost.Value);
if ((Flags & 8) != 0)
StringUtil.Serialize(PostAuthor,bw);
if ((Flags & 16) != 0)
ObjectUtils.SerializeObject(SavedFromPeer,bw);
if ((Flags & 16) != 0)
bw.Write(SavedFromMsgId.Value);
if ((Flags & 64) != 0)
StringUtil.Serialize(PsaType,bw);

        }
    }
}
