using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL.Help
{
	[TLObject(125807007)]
    public class TLRequestDismissSuggestion : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return 125807007;
            }
        }

                public string Suggestion {get;set;}
        public bool Response{ get; set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Suggestion = StringUtil.Deserialize(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            StringUtil.Serialize(Suggestion,bw);

        }
		public override void DeserializeResponse(BinaryReader br)
		{
			Response = BoolUtil.Deserialize(br);

		}
    }
}
