using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(415136107)]
    public class TLPrivacyValueAllowChatParticipants : TLAbsPrivacyRule
    {
        public override int Constructor
        {
            get
            {
                return 415136107;
            }
        }

             public TLVector<int> Chats {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Chats = (TLVector<int>)ObjectUtils.DeserializeVector<int>(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            ObjectUtils.SerializeObject(Chats,bw);

        }
    }
}
