using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(-1144565411)]
    public class TLKeyboardButtonRequestPoll : TLAbsKeyboardButton
    {
        public override int Constructor
        {
            get
            {
                return -1144565411;
            }
        }

             public int Flags {get;set;}
     public bool? Quiz {get;set;}
     public string Text {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
if ((Flags & 1) != 0)
Quiz = BoolUtil.Deserialize(br);
else
Quiz = null;

Text = StringUtil.Deserialize(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);
if ((Flags & 1) != 0)
BoolUtil.Serialize(Quiz.Value,bw);
StringUtil.Serialize(Text,bw);

        }
    }
}
